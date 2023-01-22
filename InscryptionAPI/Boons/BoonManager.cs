using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InscryptionAPI.Boons;

/// <summary>
/// This manager handles the creation of new Boons and ensures they are properly attached to battles
/// </summary>
[HarmonyPatch]
public static class BoonManager
{
    /// <summary>
    /// Maps custom BoonData objects to the trigger handler that manages them
    /// </summary>
    public class FullBoon
    {
        /// <summary>
        /// Describes the boon
        /// </summary>
        public BoonData boon;

        /// <summary>
        /// A subclass of [BoonBehaviour](xref:InscryptionAPI.Boons.BoonBehaviour) that implements the boon's behaviour
        /// </summary>
        public Type boonHandlerType;

        /// <summary>
        /// Indicates if the boon should appear in the rulebook under the Boons section
        /// </summary>
        public bool appearInRulebook;

        /// <summary>
        /// Indicates if the player can have multiple instances of this boon in their deck
        /// </summary>
        public bool stacks;
    }

    /// <summary>
    /// All boons that come as part of the vanilla game
    /// </summary>
    public static readonly ReadOnlyCollection<BoonData> BaseGameBoons = new(Resources.LoadAll<BoonData>("Data/Boons"));

    internal static readonly ObservableCollection<FullBoon> NewBoons = new();

    /// <summary>
    /// All boons, including vanilla and mod-added boons
    /// </summary>
    public static List<BoonData> AllBoonsCopy { get; private set; } = BaseGameBoons.ToList();

    /// <summary>
    /// Creates a new boon and adds it to the list of boons the player can receive
    /// </summary>
    /// <param name="guid">The guid of the mod adding the boon</param>
    /// <param name="name">The name of the boom as it should appear in the rulebook</param>
    /// <param name="boonHandlerType">A subclass of [BoonBehaviour](xref:InscryptionAPI.Boons.BoonBehaviour) that implements the boon's behaviour</param>
    /// <param name="rulebookDescription">The description of this boon in the rulebook</param>
    /// <param name="icon">The icon that appears in the center of the card art and in the rulebook</param>
    /// <param name="cardArt">The art that surrounds the boon</param>
    /// <param name="stackable">Indicates if the player can have multiple instances of this boon</param>
    /// <param name="appearInLeshyTrials">Indicates if the boon should appear in Leshy trials (the boon trials that happen right before battling Leshy in a non-Kaycee's mod run</param>
    /// <param name="appearInRulebook">Indicates if the boon should appear in the rulebook</param>
    /// <returns>The unique identifier for this boon</returns>
    public static BoonData.Type New(string guid, string name, Type boonHandlerType, string rulebookDescription, Texture icon, Texture cardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
        appearInRulebook = true)
    {
        FullBoon fb = new();
        BoonData data = ScriptableObject.CreateInstance<BoonData>();
        data.name = name;
        data.displayedName = name;
        data.description = rulebookDescription;
        data.icon = icon;
        data.cardArt = cardArt;
        data.minorEffect = !appearInLeshyTrials;
        data.type = GuidManager.GetEnumValue<BoonData.Type>(guid, name);
        fb.appearInRulebook = appearInRulebook;
        fb.boon = data;
        fb.boonHandlerType = boonHandlerType;
        fb.stacks = stackable;
        NewBoons.Add(fb);
        return data.type;
    }

    /// <summary>
    /// Creates a new boon and adds it to the list of boons the player can receive
    /// </summary>
    /// <param name="guid">The guid of the mod adding the boon</param>
    /// <param name="name">The name of the boom as it should appear in the rulebook</param>
    /// <param name="rulebookDescription">The description of this boon in the rulebook</param>
    /// <param name="icon">The icon that appears in the center of the card art and in the rulebook</param>
    /// <param name="cardArt">The art that surrounds the boon</param>
    /// <param name="stackable">Indicates if the player can have multiple instances of this boon</param>
    /// <param name="appearInLeshyTrials">Indicates if the boon should appear in Leshy trials (the boon trials that happen right before battling Leshy in a non-Kaycee's mod run</param>
    /// <param name="appearInRulebook">Indicates if the boon should appear in the rulebook</param>
    /// <typeparam name="T">A subclass of [BoonBehaviour](xref:InscryptionAPI.Boons.BoonBehaviour) that implements the boon's behaviour</typeparam>
    /// <returns>The unique identifier for this boon</returns>
    public static BoonData.Type New<T>(string guid, string name, string rulebookDescription, Texture icon, Texture cardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
        appearInRulebook = true) where T : BoonBehaviour
    {
        return New(guid, name, typeof(T), rulebookDescription, icon, cardArt, stackable, appearInLeshyTrials, appearInRulebook);
    }

    /// <summary>
    /// Creates a new boon and adds it to the list of boons the player can receive
    /// </summary>
    /// <param name="guid">The guid of the mod adding the boon</param>
    /// <param name="name">The name of the boom as it should appear in the rulebook</param>
    /// <param name="boonHandlerType">A subclass of [BoonBehaviour](xref:InscryptionAPI.Boons.BoonBehaviour) that implements the boon's behaviour</param>
    /// <param name="rulebookDescription">The description of this boon in the rulebook</param>
    /// <param name="pathToIcon">Path to the icon that appears in the center of the card art and in the rulebook</param>
    /// <param name="pathToCardArt">Path to the art that surrounds the boon</param>
    /// <param name="stackable">Indicates if the player can have multiple instances of this boon</param>
    /// <param name="appearInLeshyTrials">Indicates if the boon should appear in Leshy trials (the boon trials that happen right before battling Leshy in a non-Kaycee's mod run</param>
    /// <param name="appearInRulebook">Indicates if the boon should appear in the rulebook</param>
    /// <returns>The unique identifier for this boon</returns>
    public static BoonData.Type New(string guid, string name, Type boonHandlerType, string rulebookDescription, string pathToIcon, string pathToCardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
        appearInRulebook = true)
    {
        return New(guid, name, boonHandlerType, rulebookDescription, TextureHelper.GetImageAsTexture(pathToIcon), TextureHelper.GetImageAsTexture(pathToCardArt), stackable, appearInLeshyTrials, appearInRulebook);
    }

    /// <summary>
    /// Creates a new boon and adds it to the list of boons the player can receive
    /// </summary>
    /// <param name="guid">The guid of the mod adding the boon</param>
    /// <param name="name">The name of the boom as it should appear in the rulebook</param>
    /// <param name="rulebookDescription">The description of this boon in the rulebook</param>
    /// <param name="pathToIcon">Path to the icon that appears in the center of the card art and in the rulebook</param>
    /// <param name="pathToCardArt">Path to the art that surrounds the boon</param>
    /// <param name="stackable">Indicates if the player can have multiple instances of this boon</param>
    /// <param name="appearInLeshyTrials">Indicates if the boon should appear in Leshy trials (the boon trials that happen right before battling Leshy in a non-Kaycee's mod run</param>
    /// <param name="appearInRulebook">Indicates if the boon should appear in the rulebook</param>
    /// <typeparam name="T">A subclass of [BoonBehaviour](xref:InscryptionAPI.Boons.BoonBehaviour) that implements the boon's behaviour</typeparam>
    /// <returns>The unique identifier for this boon</returns>
    public static BoonData.Type New<T>(string guid, string name, string rulebookDescription, string pathToIcon, string pathToCardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
        appearInRulebook = true) where T : BoonBehaviour
    {
        return New<T>(guid, name, rulebookDescription, pathToIcon, pathToCardArt, stackable, appearInLeshyTrials, appearInRulebook);
    }

    internal static void SyncBoonList()
    {
        var boons = BaseGameBoons.Concat(NewBoons.Select((x) => x.boon)).ToList();
        AllBoonsCopy = boons;
    }

    static BoonManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(BoonData))
            {
                ScriptableObjectLoader<BoonData>.allData = AllBoonsCopy;
            }
        };
        NewBoons.CollectionChanged += static (_, _) =>
        {
            SyncBoonList();
        };
    }

    [HarmonyPatch(typeof(BoonsHandler), nameof(BoonsHandler.ActivatePreCombatBoons))]
    [HarmonyPostfix]
    private static IEnumerator ActivatePreCombatBoons(IEnumerator result, BoonsHandler __instance)
    {
        BoonBehaviour.DestroyAllInstances();
        if (__instance.BoonsEnabled && RunState.Run != null && RunState.Run.playerDeck != null && RunState.Run.playerDeck.Boons != null && NewBoons != null)
        {
            foreach (BoonData boon in RunState.Run.playerDeck.Boons)
            {
                if (boon != null)
                {
                    FullBoon nb = NewBoons.ToList().Find((x) => x.boon.type == boon.type);

                    if (nb == null)
                        continue;

                    int instances = BoonBehaviour.CountInstancesOfType(nb.boon.type);
                    if (nb != null && nb.boonHandlerType != null && nb.boonHandlerType.IsSubclassOf(typeof(BoonBehaviour)) && (nb.stacks || instances < 1))
                    {
                        GameObject boonhandler = new(nb.boon.name + " Boon Handler");
                        BoonBehaviour behav = boonhandler.AddComponent(nb.boonHandlerType) as BoonBehaviour;
                        if (behav != null)
                        {
                            GlobalTriggerHandler.Instance?.RegisterNonCardReceiver(behav);
                            behav.boon = nb;
                            behav.instanceNumber = instances + 1;
                            BoonBehaviour.Instances.Add(behav);
                            if (behav.RespondsToPreBoonActivation())
                            {
                                yield return behav.OnPreBoonActivation();
                            }
                        }
                    }
                }
            }
        }
        yield return result;
        BoonBehaviour[] bbs = BoonBehaviour.Instances.ToArray();
        foreach (BoonBehaviour bb in bbs)
        {
            if (bb != null && bb.RespondsToPostBoonActivation())
            {
                yield return bb.OnPostBoonActivation();
            }
        }
        yield break;
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
    [HarmonyPostfix]
    private static IEnumerator Postfix(IEnumerator result)
    {
        BoonBehaviour[] bbs = BoonBehaviour.Instances.ToArray();
        foreach (BoonBehaviour bb in bbs)
        {
            if (bb != null && bb.RespondsToPreBattleCleanup())
            {
                yield return bb.OnPreBattleCleanup();
            }
        }
        yield return result;
        bbs = BoonBehaviour.Instances.ToArray();
        foreach (BoonBehaviour bb in bbs)
        {
            if (bb != null && bb.RespondsToPostBattleCleanup())
            {
                yield return bb.OnPostBattleCleanup();
            }
        }
        BoonBehaviour.DestroyAllInstances();
        yield break;
    }

    [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.AddBoon))]
    [HarmonyPostfix]
    private static void AddBoon(BoonData.Type boonType)
    {
        if (TurnManager.Instance != null && !TurnManager.Instance.GameEnded && !TurnManager.Instance.GameEnding && !TurnManager.Instance.IsSetupPhase && TurnManager.Instance.Opponent != null)
        {
            FullBoon nb = NewBoons.ToList().Find((x) => x.boon.type == boonType);
            if (nb != null && nb.boonHandlerType != null && (nb.stacks || BoonBehaviour.CountInstancesOfType(nb.boon.type) < 1))
            {
                int instances = BoonBehaviour.CountInstancesOfType(nb.boon.type);
                GameObject boonhandler = new GameObject(nb.boon.name + " Boon Handler");
                BoonBehaviour behav = boonhandler.AddComponent(nb.boonHandlerType) as BoonBehaviour;
                if (behav != null)
                {
                    GlobalTriggerHandler.Instance?.RegisterNonCardReceiver(behav);
                    behav.boon = nb;
                    behav.instanceNumber = instances + 1;
                    BoonBehaviour.Instances.Add(behav);
                }
            }
        }
    }

    internal static void DestroyWhenStackClear(this GameObject obj)
    {
        if (obj.GetComponent<DestroyingFlag>() == null)
        {
            var flag = obj.AddComponent<DestroyingFlag>();
            flag.StartCoroutine(flag.WaitForStackClearThenDestroy());
        }
    }

    [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.ClearBoons))]
    [HarmonyPostfix]
    private static void ClearBoons()
    {
        BoonBehaviour.DestroyAllInstances();
    }

    [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.Boons), MethodType.Getter)]
    [HarmonyPrefix]
    private static void get_Boons(DeckInfo __instance)
    {
        if (__instance.boons != null && __instance.boonIds != null && __instance.boons.Count != __instance.boonIds.Count)
            __instance.LoadBoons();
    }

    [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.LoadBoons))]
    [HarmonyPostfix]
    private static void LoadBoons(DeckInfo __instance)
    {
        __instance.boons.RemoveAll((x) => x == null);
    }

    [HarmonyPatch(typeof(RuleBookInfo), nameof(RuleBookInfo.ConstructPageData))]
    [HarmonyPostfix]
    private static void ConstructPageData(ref List<RuleBookPageInfo> __result, RuleBookInfo __instance, AbilityMetaCategory metaCategory)
    {
        if (NewBoons.Count > 0 && metaCategory == AbilityMetaCategory.Part1Rulebook)
        {
            foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
            {
                // regular abilities
                if (pageRangeInfo.type == PageRangeType.Boons)
                {
                    int insertPosition = __result.FindLastIndex(rbi => rbi.pagePrefab == pageRangeInfo.rangePrefab) + 1;
                    int curPageNum = (int)Ability.NUM_ABILITIES;
                    List<FullBoon> abilitiesToAdd = NewBoons.Where(x => x != null && x.boon != null && BoonsUtil.GetData(x.boon.type)?.icon != null).ToList();
                    //InscryptionAPIPlugin.Logger.LogInfo($"Adding {abilitiesToAdd.Count} out of {NewAbilities.Count} abilities to rulebook");
                    foreach (FullBoon fboo in abilitiesToAdd)
                    {
                        RuleBookPageInfo info = new();
                        info.pagePrefab = pageRangeInfo.rangePrefab;
                        info.headerText = string.Format(Localization.Translate("APPENDIX XII, SUBSECTION I - MOD BOONS {0}"), curPageNum);
                        __instance.FillBoonPage(info, pageRangeInfo, (int)fboo.boon.type);
                        __result.Insert(insertPosition, info);
                        curPageNum += 1;
                        insertPosition += 1;
                    }
                }
            }
        }
    }
}