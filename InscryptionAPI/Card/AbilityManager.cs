using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Card;

/// <summary>
/// This manager class handles the creation and management of mod-added abilities (sigils).
/// </summary>
[HarmonyPatch]
public static class AbilityManager
{
    private class AbilityExt
    {
        public readonly Dictionary<Type, object> TypeMap = new();
        public readonly Dictionary<string, string> StringMap = new();
    }
    private static ConditionalWeakTable<AbilityInfo, AbilityExt> AbilityExtensionProperties = new();

    /// <summary>
    /// A utility class that holds all of the required information about an ability in order to be able to use it in-game
    /// </summary>
    public class FullAbility
    {
        /// <summary>
        /// The unique ID for this ability
        /// </summary>
        public readonly Ability Id;

        /// <summary>
        /// The description object for this ability
        /// </summary>        
        public readonly AbilityInfo Info;

        /// <summary>
        /// A subclass of AbilityBehaviour that implements the logic for the ability
        /// </summary>
        public readonly Type AbilityBehavior;

        /// <summary>
        /// A 49x49 texture for the ability icon
        /// </summary>
        /// <value></value>
        public Texture Texture { get; internal set; }

        /// <summary>
        /// A 49x49 texture for the ability icon, used when the card belongs to the opponent
        /// </summary>
        /// <value></value>
        public Texture CustomFlippedTexture { get; internal set; }

        internal static ConditionalWeakTable<AbilityInfo, FullAbility> ReverseMapper = new();

        /// <summary>
        /// Creates a new instance of FullAbility and registers its behaviour type with the [TypeManager](InscryptionAPI.Guid.TypeManager).
        /// </summary>
        /// <param name="id">The unique ID for this ability</param>
        /// <param name="info">The description object for this ability</param>
        /// <param name="behaviour">A subclass of AbilityBehaviour that implements the logic for the ability</param>
        /// <param name="texture">A 49x49 texture  for the ability icon</param>
        public FullAbility(Ability id, AbilityInfo info, Type behaviour, Texture texture)
        {
            Id = id;
            Info = info;
            AbilityBehavior = behaviour;
            Texture = texture;

            ReverseMapper.Add(info, this);

            TypeManager.Add(id.ToString(), behaviour);
        }

        /// <summary>
        /// Makes a deep copy of the current FullAbility object
        /// </summary>
        public FullAbility Clone()
        {
            AbilityInfo clonedInfo = ScriptableObject.CreateInstance<AbilityInfo>();
            clonedInfo.ability = Info.ability;
            clonedInfo.abilityLearnedDialogue = Info.abilityLearnedDialogue;
            clonedInfo.activated = Info.activated;
            clonedInfo.canStack = Info.canStack;
            clonedInfo.colorOverride = Info.colorOverride;
            clonedInfo.conduit = Info.conduit;
            clonedInfo.conduitCell = Info.conduitCell;
            clonedInfo.customFlippedIcon = Info.customFlippedIcon;
            clonedInfo.customFlippedPixelIcon = Info.customFlippedPixelIcon;
            clonedInfo.flipYIfOpponent = Info.flipYIfOpponent;
            clonedInfo.hasColorOverride = Info.hasColorOverride;
            clonedInfo.keywordAbility = Info.keywordAbility;
            clonedInfo.mesh3D = Info.mesh3D;
            clonedInfo.metaCategories = new(Info.metaCategories);
            clonedInfo.name = Info.name;
            clonedInfo.opponentUsable = Info.opponentUsable;
            clonedInfo.passive = Info.passive;
            clonedInfo.pixelIcon = Info.pixelIcon;
            clonedInfo.powerLevel = Info.powerLevel;
            clonedInfo.rulebookDescription = Info.rulebookDescription;
            clonedInfo.rulebookName = Info.rulebookName;
            clonedInfo.triggerText = Info.triggerText;

            AbilityExtensionProperties.Add(clonedInfo, AbilityExtensionProperties.GetOrCreateValue(Info));

            return new FullAbility(this.Id, clonedInfo, this.AbilityBehavior, this.Texture) { CustomFlippedTexture = this.CustomFlippedTexture };
        }
    }

    /// <summary>
    /// All of the vanilla game's abilities
    /// </summary>
    public readonly static ReadOnlyCollection<FullAbility> BaseGameAbilities = new(GenBaseGameAbilityList());
    internal readonly static ObservableCollection<FullAbility> NewAbilities = new();

    /// <summary>
    /// The current processed list of all abilities in the game, including vanilla and mod-added abilities.
    /// </summary>
    public static List<FullAbility> AllAbilities { get; private set; } = BaseGameAbilities.ToList();

    /// <summary>
    /// The current processed list of all AbilityInfos in the game, including vanilla and mod-added abilities.
    /// </summary>
    public static List<AbilityInfo> AllAbilityInfos { get; private set; } = BaseGameAbilities.Select(x => x.Info).ToList();

    /// <summary>
    /// A hook for modders to add custom code to modify the ability list dynamically
    /// </summary>
    /// <remarks>There are two primary use cases for this hook:
    /// 
    /// - Making changes to abilities that may be added after your plugin initializes
    /// - Making context-aware changes to abilities.
    /// 
    /// The way this operates is as follows:
    /// 
    /// 1. The AbilityManager makes a copy of all FullAbility objects. Note that unlike the similar code in CardManager, this is actually a *deep* copy of the AbilityInfo.
    /// 2. The code in ModifyAbiltyList is executed on the copy of those abilities.
    /// 3. The modified list becomes the game's new official list of abilities.
    /// 
    /// The reason the abilities are cloned before processing is so that you can make any change to them you wish
    /// without affecting the original ability. This means that you do not need to try to track changes or remember
    /// what the original version of the ability looked like before you started changing it; the next time 
    /// [SyncAbilityList](xref:InscryptionAPI.AbilityManager.SyncAbilityList) is called, all changes will be reverted
    /// and then re-applied.
    /// 
    /// To use this, you need to add a delegate that accepts the current list of all abilities, then returns
    /// that same list back.
    /// 
    /// ```c#
    /// AbilityManager.ModifyAbilityList += delegate(List<FullAbility> abilities)
    /// {
    ///     // Add the rulebook metacategories to every single ability
    ///     foreach (var ability in abilities)
    ///         ability.info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Rulebook);
    ///     return abilities;
    /// }
    /// ```
    /// </remarks>
    public static event Func<List<FullAbility>, List<FullAbility>> ModifyAbilityList;

    /// <summary>
    /// Resynchronizes the ablity list
    /// </summary>
    /// <remarks>Most importantly, this re-executes all custom code that was added to the [ModifyAbilityList](xref:InscryptionAPI.AbilityManager.ModifyAbilityList)
    /// event. If you are doing any sort of context-aware processing of the ability list, you may need to manually
    /// call this method in order to make sure that your code executes correctly. However, this automatically gets called
    /// every time that the game transitions from either the main menu into the base game, or whenever a new run is 
    /// started inside of Kaycee's Mod. Only in extreme edge cases should you need to manually call this.</remarks>
    public static void SyncAbilityList()
    {
        AllAbilities = BaseGameAbilities.Concat(NewAbilities).Select(a => a.Clone()).ToList();
        AllAbilities = ModifyAbilityList?.Invoke(AllAbilities) ?? AllAbilities;
        AllAbilityInfos = AllAbilities.Select(x => x.Info).ToList();
    }

    static AbilityManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(AbilityInfo))
            {
                ScriptableObjectLoader<AbilityInfo>.allData = AllAbilityInfos;
            }
        };
        NewAbilities.CollectionChanged += static (_, _) =>
        {
            SyncAbilityList();
        };
    }

    private static List<FullAbility> GenBaseGameAbilityList()
    {
        bool useReversePatch = true;
        try
        {
            OriginalLoadAbilityIcon(Ability.AllStrike.ToString());
        }
        catch (NotImplementedException)
        {
            useReversePatch = false;
        }

        List<FullAbility> baseGame = new();
        var gameAsm = typeof(AbilityInfo).Assembly;
        foreach (var ability in Resources.LoadAll<AbilityInfo>("Data/Abilities"))
        {
            var name = ability.ability.ToString();
            baseGame.Add(new FullAbility
            (
                ability.ability,
                ability,
                gameAsm.GetType($"DiskCardGame.{name}"),
                useReversePatch ? OriginalLoadAbilityIcon(name) : AbilitiesUtil.LoadAbilityIcon(name)
            ));
        }
        return baseGame;
    }

    /// <summary>
    /// Creates a new ability and registers it to be able to be added to cards
    /// </summary>
    /// <param name="guid">The guid of the mod adding the ability</param>
    /// <param name="info">An instance of AbilityInfo describing the ability</param>
    /// <param name="behavior">A subclass of AbilityBehaviour that implements the logic for the ability</param>
    /// <param name="tex">The ability icon as a 49x49 texture</param>
    /// <returns>An instance of AbilityInfo describing the new ability</returns>
    /// <remarks>The actual unique identifier for the new ability will be found in the 'ability' field of 
    /// the returned AbilityInfo object. There is no way for the modder to create a specific ability ID; it will
    /// always be assigned by the API.
    /// 
    /// **NOTE**: Even if you manually create an Ability identifier for your ability and attach it to the AbilityInfo 
    /// object before you pass it to the API, the API will *still* generate a unique ability ID for you, which may
    /// or may not be the same as the ID you created for yourself. As such, it is best practice to *not* set the 
    /// ablity ID yourself and leave it as its default value.</remarks>
    public static FullAbility Add(string guid, AbilityInfo info, Type behavior, Texture tex)
    {
        FullAbility full = new(GuidManager.GetEnumValue<Ability>(guid, info.rulebookName), info, behavior, tex);
        full.Info.ability = full.Id;
        info.name = $"{guid}_{info.rulebookName}";
        NewAbilities.Add(full);
        return full;
    }

    /// <summary>
    /// Creates a new ability and registers it to be able to be added to cards
    /// </summary>
    /// <param name="guid">The guid of the mod adding the ability</param>
    /// <param name="rulebookName">The name of the ability</param>
    /// <param name="rulebookDescription">The description as it appears in the game's rulebook</param>
    /// <param name="behavior">A subclass of AbilityBehaviour that implements the logic for the ability</param>
    /// <param name="pathToArt">Path to the ability texture on disk</param>
    /// <returns>An instance of AbilityInfo describing the new ability</returns>
    /// <remarks>The actual unique identifier for the new ability will be found in the 'ability' field of 
    /// the returned AbilityInfo object. There is no way for the modder to create a specific ability ID; it will
    /// always be assigned by the API.</remarks>
    public static AbilityInfo New(string guid, string rulebookName, string rulebookDescription, Type behavior, string pathToArt)
    {
        return New(guid, rulebookName, rulebookDescription, behavior, TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Creates a new ability and registers it to be able to be added to cards
    /// </summary>
    /// <param name="guid">The guid of the mod adding the ability</param>
    /// <param name="rulebookName">The name of the ability</param>
    /// <param name="rulebookDescription">The description as it appears in the game's rulebook</param>
    /// <param name="behavior">A subclass of AbilityBehaviour that implements the logic for the ability</param>
    /// <param name="tex">The ability icon as a 49x49 texture</param>
    /// <returns>An instance of AbilityInfo describing the new ability</returns>
    /// <remarks>The actual unique identifier for the new ability will be found in the 'ability' field of 
    /// the returned AbilityInfo object. There is no way for the modder to create a specific ability ID; it will
    /// always be assigned by the API.</remarks>
    public static AbilityInfo New(string guid, string rulebookName, string rulebookDescription, Type behavior, Texture tex)
    {
        AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
        info.rulebookName = rulebookName;
        info.rulebookDescription = rulebookDescription;
        Add(guid, info, behavior, tex);
        return info;
    }

    /// <summary>
    /// Removes an ability from the game based on ability ID. Can only remove mod-added abilities, not vanilla abilities.
    /// </summary>
    /// <param name="id">The unique ID of the ability to remove</param>
    public static void Remove(Ability id) => NewAbilities.Remove(NewAbilities.FirstOrDefault(x => x.Id == id));

    /// <summary>
    /// Removes an ability from the game based on ability ID. Can only remove mod-added abilities, not vanilla abilities.
    /// </summary>
    /// <param name="ability">The instance of the ability to remove</param>
    public static void Remove(FullAbility ability) => NewAbilities.Remove(ability);

    internal static Dictionary<string, string> GetAbilityExtensionTable(this AbilityInfo info)
    {
        return AbilityExtensionProperties.GetOrCreateValue(info).StringMap;
    }


    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(AbilitiesUtil), nameof(AbilitiesUtil.LoadAbilityIcon))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Texture OriginalLoadAbilityIcon(string abilityName, bool fillerAbility = false, bool something = false) { throw new NotImplementedException(); }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbilitiesUtil), nameof(AbilitiesUtil.LoadAbilityIcon))]
    private static bool LoadAbilityIconReplacement(string abilityName, ref Texture __result)
    {
        if (string.IsNullOrEmpty(abilityName))
            return true;

        bool normalTexture = true;
        if (abilityName.EndsWith("_opponent"))
        {
            normalTexture = false;
            abilityName = abilityName.Replace("_opponent", "");
        }

        if (int.TryParse(abilityName, out int abilityId))
        {
            FullAbility ability = AllAbilities.FirstOrDefault(x => x.Id == (Ability)abilityId);
            __result = (normalTexture || ability.CustomFlippedTexture == null) ? ability.Texture : ability.CustomFlippedTexture;
            return false;
        }

        if (Enum.TryParse<Ability>(abilityName, out Ability abilityEnum))
        {
            FullAbility ability = AllAbilities.FirstOrDefault(x => x.Id == abilityEnum);
            __result = (normalTexture || ability.CustomFlippedTexture == null) ? ability.Texture : ability.CustomFlippedTexture;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbilitiesUtil), nameof(AbilitiesUtil.GetLearnedAbilities))]
    private static bool GetLearnedAbilitesReplacement(bool opponentUsable, int minPower, int maxPower, AbilityMetaCategory categoryCriteria, ref List<Ability> __result)
    {
        __result = new();

        foreach (var ability in AllAbilityInfos)
        {
            bool canUse = true;
            canUse &= !opponentUsable || ability.opponentUsable;
            canUse &= minPower <= ability.powerLevel && maxPower >= ability.powerLevel;
            canUse &= ability.metaCategories.Contains(categoryCriteria);
            canUse &= ProgressionData.LearnedAbility(ability.ability);

            if (canUse)
            {
                __result.Add(ability.ability);
            }
        }

        return false;
    }

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(CardTriggerHandler), nameof(CardTriggerHandler.AddAbility), new[] { typeof(Ability) })]
    // private static bool AddAbilityReplacement(CardTriggerHandler __instance, Ability ability)
    // {
    //     var full = AllAbilities.FirstOrDefault(x => x.Id == ability);
    //     if (!__instance.triggeredAbilities.Exists(x => x.Item1 == ability) || full.Info.canStack && !full.Info.passive)
    //     {
    //         var reciever = (AbilityBehaviour)__instance.gameObject.GetComponent(full.AbilityBehavior);
    //         if (!reciever)
    //         {
    //             reciever = (AbilityBehaviour)__instance.gameObject.AddComponent(full.AbilityBehavior);
    //         }
    //         __instance.triggeredAbilities.Add(new Tuple<Ability, AbilityBehaviour>(ability, reciever));
    //     }

    //     return false;
    // }

    [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] { typeof(AbilityMetaCategory) })]
    [HarmonyPostfix]
    private static void FixRulebook(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
    {
        //InscryptionAPIPlugin.Logger.LogInfo($"In rulebook patch: I see {NewAbilities.Count}");
        if (NewAbilities.Count > 0)
        {
            foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
            {
                // regular abilities
                if (pageRangeInfo.type == PageRangeType.Abilities)
                {
                    int insertPosition = __result.FindLastIndex(rbi => rbi.pagePrefab == pageRangeInfo.rangePrefab) + 1;
                    int curPageNum = (int)Ability.NUM_ABILITIES;
                    List<FullAbility> abilitiesToAdd = NewAbilities.Where(x => __instance.AbilityShouldBeAdded((int)x.Id, metaCategory)).ToList();
                    //InscryptionAPIPlugin.Logger.LogInfo($"Adding {abilitiesToAdd.Count} out of {NewAbilities.Count} abilities to rulebook");
                    foreach (FullAbility fab in abilitiesToAdd)
                    {
                        RuleBookPageInfo info = new();
                        info.pagePrefab = pageRangeInfo.rangePrefab;
                        info.headerText = string.Format(Localization.Translate("APPENDIX XII, SUBSECTION I - MOD ABILITIES {0}"), curPageNum);
                        __instance.FillAbilityPage(info, pageRangeInfo, (int)fab.Id);
                        __result.Insert(insertPosition, info);
                        curPageNum += 1;
                        insertPosition += 1;
                    }
                }
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TransformIntoCard))]
    private static IEnumerator TriggerStacksOnceAfterEvolve(IEnumerator enumerator, PlayableCard __instance)
    {
        yield return enumerator;

        List<Ability> abilities = new();

        for (int i = 0; i < __instance.TriggerHandler.triggeredAbilities.Count; i++)
        {
            // get info
            AbilityInfo info = AllAbilityInfos.AbilityByID(__instance.TriggerHandler.triggeredAbilities[i].Item1);

            // if can stack and triggers once
            if (info.canStack && info.GetTriggersOncePerStack())
            {
                // add to list if not in it
                if (!abilities.Contains(__instance.TriggerHandler.triggeredAbilities[i].Item1))
                    abilities.Add(__instance.TriggerHandler.triggeredAbilities[i].Item1);

                // remove trigger
                __instance.TriggerHandler.triggeredAbilities.Remove(__instance.TriggerHandler.triggeredAbilities[i]);
            }
        }

        foreach (Ability ab in abilities)
            __instance.TriggerHandler.AddAbility(ab);
    }

    [HarmonyPatch(typeof(GlobalTriggerHandler), nameof(GlobalTriggerHandler.TriggerCardsOnBoard))]
    [HarmonyPostfix]
    private static IEnumerator WaterborneFix(IEnumerator result, Trigger trigger, bool triggerFacedown, params object[] otherArgs)
    {
        yield return result;
        if (!triggerFacedown)
        {
            bool RespondsToTrigger(CardTriggerHandler r, Trigger trigger, params object[] otherArgs)
            {
                foreach (TriggerReceiver receiver in r.GetAllReceivers())
                {
                    if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs) && ((receiver is IActivateWhenFacedown && (receiver as IActivateWhenFacedown).ShouldTriggerWhenFaceDown(trigger, otherArgs))))
                    {
                        return true;
                    }
                }
                return false;
            }
            IEnumerator OnTrigger(CardTriggerHandler r, Trigger trigger, params object[] otherArgs)
            {
                foreach (TriggerReceiver receiver in r.GetAllReceivers())
                {
                    if (GlobalTriggerHandler.ReceiverRespondsToTrigger(trigger, receiver, otherArgs) && ((receiver is IActivateWhenFacedown && (receiver as IActivateWhenFacedown).ShouldTriggerWhenFaceDown(trigger, otherArgs))))
                    {
                        yield return Singleton<GlobalTriggerHandler>.Instance.TriggerSequence(trigger, receiver, otherArgs);
                    }
                }
                yield break;
            }
            List<PlayableCard> list = new(Singleton<BoardManager>.Instance.CardsOnBoard);
            foreach (PlayableCard playableCard in list)
            {
                if (playableCard != null && playableCard.FaceDown && RespondsToTrigger(playableCard.TriggerHandler, trigger, otherArgs))
                {
                    yield return OnTrigger(playableCard.TriggerHandler, trigger, otherArgs);
                }
            }
        }
        yield break;
    }
}