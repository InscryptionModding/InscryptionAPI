global using UnityObject = UnityEngine.Object;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Encounters;
using InscryptionAPI.Items;
using InscryptionAPI.Pelts;
using InscryptionAPI.PixelCard;
using InscryptionAPI.Regions;
using InscryptionAPI.RuleBook;
using InscryptionAPI.Slots;
using InscryptionAPI.Totems;
using System.Reflection;
using System.Runtime.CompilerServices;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;

[assembly: InternalsVisibleTo("Assembly-CSharp")]
[assembly: InternalsVisibleTo("Assembly-CSharp.APIPatcher.mm")]
[assembly: InternalsVisibleTo("APIPatcher")]

namespace InscryptionAPI;

/// <summary>
/// The Entry Point to the Inscryption API.
/// </summary>
[BepInPlugin(ModGUID, ModName, ModVer)]
[HarmonyPatch]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    /// <summary>
    /// The GUID for the Inscryption API, if you depend on this API you'll want to load after it in <c>Awake()</c> or, anywhere before <c>Start()</c>.
    /// </summary>
    public const string ModGUID = "cyantist.inscryption.api";
    /// <summary>
    /// This is the Inscryption API's Internal Name, prefer identifying this API with the GUID.
    /// </summary>
    internal const string ModName = "InscryptionAPI";
    /// <summary>
    /// This is the Version of the Inscryption API you are using.
    /// </summary>
    internal const string ModVer = "2.24.2";

    /// <summary>
    /// The Inscryption API's Location as a Directory.
    /// </summary>
    public static string Directory = "";

    /// <summary>
    /// The API's Assembly for Internal Reference.
    /// </summary>
    internal static Assembly APIAssembly { get; private set; } = null;

    /// <summary>
    /// The Configuration for Overriding the Arrows within Kaycee's Mod Menu's.
    /// </summary>
    internal static ConfigEntry<bool> configOverrideArrows;
    /// <summary>
    /// The Configuration for determining whether the Order of Costs in a Cost Choice Node should be Randomized.
    /// </summary>
    internal static ConfigEntry<bool> configRandomCostChoiceOrder;
    /// <summary>
    /// The Configuration for determining whether the Boss Scenery of Act 1/KCM should be hidden.
    /// </summary>
    internal static ConfigEntry<bool> configHideAct1BossScenery;
    /// <summary>
    /// This Configuration is used for determining what Totem Top Model should be used in game.
    /// </summary>
    internal static ConfigEntry<TotemManager.TotemTopState> configCustomTotemTopModelSelection;
    /// <summary>
    /// This Configuration is used for determining what Item Model should be used in game.
    /// </summary>
    internal static ConfigEntry<ConsumableItemManager.ConsumableState> configCustomItemModelSelection;
    /// <summary>
    /// This Configuration is used for determining whether we should Log Debugging for the TraderCostTier Bug. 
    /// </summary>
    internal static ConfigEntry<bool> configEnableTraderCostTierBugDebugLogging;

    /// <summary>
    /// Prevents API 1.0.0 Versions from Loaing.
    /// </summary>
    static InscryptionAPIPlugin()
    {
        AppDomain.CurrentDomain.AssemblyResolve += static (_, e) =>
        {
            if (e.Name.StartsWith("API, Version=1") || e.Name.StartsWith("Cardloader, Version=1"))
            {
                return APIAssembly ??= typeof(InscryptionAPIPlugin).Assembly;
            }
            return null;
        };
    }

    /// <summary>
    /// Our API's Internal Logger.
    /// </summary>
    new internal static ManualLogSource Logger;
    /// <summary>
    /// Our API's Internal Harmony Instance.
    /// </summary>
    private readonly Harmony HarmonyInstance = new(ModGUID);

    /// <summary>
    /// An event of <see cref="System.Action{T}"/> resembling the Scriptable Object Loader Load call.
    /// </summary>
    public static event Action<Type> ScriptableObjectLoaderLoad;
    /// <summary>
    /// A simple internal function that invokes our previously typified Action for Loading the ScriptableObjectLoader.
    /// </summary>
    /// <param name="type">The Type in which to pass to the ScriptableObjectLoader.</param>
    internal static void InvokeSOLEvent(Type type)
    {
        ScriptableObjectLoaderLoad?.Invoke(type);
    }

    /// <summary>
    /// The following runs when the API is enabled by Unity.
    /// </summary>
    private void OnEnable()
    {
        Logger = base.Logger;
        Directory = Path.GetDirectoryName(Info.Location);
        if (TextureHelper.BaseGameCustomAppearancesPublic.IsNullOrEmpty())
            TextureHelper.InstantiateBaseGameCustomAppearances();

        HarmonyInstance.PatchAll(InscryptionAPIPlugin.APIAssembly);
    }

    /// <summary>
    /// The following runs when the API is diabled by Unity.
    /// </summary>
    private void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }

    /// <summary>
    /// The following is used to Resync the API's many lists, e.g. <see cref="CardManager.SyncCardList"/>, <see cref="CardCostManager.SyncCustomCostList"/>, <see cref="CardModificationInfoManager.SyncCardMods"/>, <see cref="AbilityManager.SyncAbilityList()"/>, <see cref="SlotModificationManager.SyncSlotModificationList"/>, <see cref="EncounterManager.SyncEncounterList"/>, <see cref="RegionManager.SyncRegionList"/>, <see cref="RuleBookManager.SyncRuleBookList"/>
    /// </summary>
    internal static void ResyncAll()
    {
        CardManager.SyncCardList();
        CardCostManager.SyncCustomCostList();
        CardModificationInfoManager.SyncCardMods();
        AbilityManager.SyncAbilityList();
        SlotModificationManager.SyncSlotModificationList();
        EncounterManager.SyncEncounterList();
        RegionManager.SyncRegionList();
        RuleBookManager.SyncRuleBookList();
    }

    /// <summary>
    /// A function we use to check when a plugin is using an Outdated version of the API, if they do we log them with a Warning about it.
    /// </summary>
    internal static void CheckForOutdatedPlugins()
    {
        string outdatedPlugins = "";
        foreach (Assembly pluginAsm in Chainloader.PluginInfos.Values.Select(p => p.Instance.GetType().Assembly).Distinct())
        {
            foreach (var refAsm in pluginAsm.GetReferencedAssemblies())
            {
                if (refAsm.Name.Equals("API"))
                {
                    outdatedPlugins += $" - {pluginAsm.GetName().Name}\n";
                    continue;
                }
            }
        }
        if (outdatedPlugins != "")
            Logger.LogWarning("The following mods use an outdated version of the API:\n"
                + outdatedPlugins + "\nThese mods may not work correctly. If problems arise, please update or disable them!");
    }

    /// <summary>
    /// This is our API's Initializer, we primarily use it to set the internal APIAssembly and create the Configurations to the Configuration File.
    /// </summary>
    private void Awake()
    {
        APIAssembly ??= typeof(InscryptionAPIPlugin).Assembly;
        configCustomTotemTopModelSelection = Config.Bind("Totems", "Top Types", TotemManager.TotemTopState.CustomTribes, "If Vanilla, don't change totem tops; if CustomTribes, added custom tribes will use custom totem tops; if AllTribes then all totem tops will use a custom top.");
        configCustomItemModelSelection = Config.Bind("Items", "Types", ConsumableItemManager.ConsumableState.Custom, "If Vanilla, only vanilla items will be used; if Custom, added custom items will use custom models; if All then all items will use a custom model.");
        configOverrideArrows = Config.Bind("Menus", "Override Arrows", false, "When true, forces the challenge screen arrows to appear at the top of the screen instead of the sides.");
        configRandomCostChoiceOrder = Config.Bind("Miscellaneous", "Randomise Cost Choice Order", false, "When true, randomises the order card cost choices are presented in Act 1.");
        configHideAct1BossScenery = Config.Bind("Optimization", "Hide Act 1 Scenery", false, "When true bosses will not spawn their scenery. (eg: Prospector's trees) This can improve performance on low-end machines.");
        configEnableTraderCostTierBugDebugLogging = Config.Bind("Zebugging", "Enable Logging for Info related to the GenerateTradeCardsWithCostTier method", false, "May be of use for debugging issues related to 'GenerateTradeCardsWithCostTier', by how much no clue. Set this to True to enable the Logging.");
    }

    /// <summary>
    /// This is the API's second phase of Initialization, ran after all <c>Awake()</c> calls complete. If you need something both after <c>Awake()</c> and before <c>Start()</c> refer to JSONLoader3's Phase System.
    /// </summary>
    private void Start()
    {
        CheckForOutdatedPlugins();
        DeathCardManager.AddCustomDeathCards();
        CardManager.ActivateEvents();
        CardManager.ResolveMissingModPrefixes();
        ResyncAll();
        CardManager.AuditCardList();
        PixelCardManager.Initialise();
        PeltManager.CreateDialogueEvents();
        Logger.LogDebug($"Inserted {DialogueManager.CustomDialogue.Count} dialogue event(s)!");
        DebugLoggingForGenerateTradeCardsWithCostTier();
    }

    /// <summary>
    /// A Function in which Logs some Debug related to 'GenerateTradeCardsWithCostTier'. There is more related Logging, this is just the Start of it.
    /// </summary>
    private void DebugLoggingForGenerateTradeCardsWithCostTier()
    {
        foreach (CardInfo info in CardLoader.LearnedCards)
        {
            if (configEnableTraderCostTierBugDebugLogging.Value)
            {
                if (info == null)
                {
                    Logger.LogError("CardLoader.LearnedCards contains a null CardInfo.");
                    continue;
                }

                Logger.LogDebug($"===== CardInfo: {info.displayedName ?? info.name ?? "null"} =====");

                Logger.LogDebug($"name: {info.name ?? "null"}");
                Logger.LogDebug($"displayedName: {info.displayedName ?? "null"}");
                Logger.LogDebug($"displayedNameLocId: {info.displayedNameLocId}");
                Logger.LogDebug($"description: {info.description ?? "null"}");

                Logger.LogDebug($"baseAttack: {info.baseAttack}");
                Logger.LogDebug($"baseHealth: {info.baseHealth}");

                Logger.LogDebug($"cost: {info.cost}");
                Logger.LogDebug($"bonesCost: {info.bonesCost}");
                Logger.LogDebug($"energyCost: {info.energyCost}");
                Logger.LogDebug($"gemsCost: {string.Join(", ", info.gemsCost?.Select(x => GetGuidAndKey(x)) ?? Enumerable.Empty<string>())}");

                Logger.LogDebug($"cardComplexity: {info.cardComplexity}");
                Logger.LogDebug($"temple: {info.temple}");
                Logger.LogDebug($"onePerDeck: {info.onePerDeck}");
                Logger.LogDebug($"hideAttackAndHealth: {info.hideAttackAndHealth}");

                Logger.LogDebug($"metaCategories: {string.Join(", ", info.metaCategories?.Select(x => GetGuidAndKey(x)) ?? Enumerable.Empty<string>())}");
                Logger.LogDebug($"tribes: {string.Join(", ", info.tribes?.Select(x => GetGuidAndKey(x)) ?? Enumerable.Empty<string>())}");
                Logger.LogDebug($"traits: {string.Join(", ", info.traits?.Select(x => GetGuidAndKey(x)) ?? Enumerable.Empty<string>())}");

                Logger.LogDebug($"abilities: {string.Join(", ", info.abilities?.Select(x => GetGuidAndKey(x)) ?? Enumerable.Empty<string>())}");
                Logger.LogDebug($"ascensionAbilities: {string.Join(", ", info.ascensionAbilities?.Select(x => GetGuidAndKey(x)) ?? Enumerable.Empty<string>())}");
                Logger.LogDebug($"specialAbilities: {string.Join(", ", info.specialAbilities?.Select(x => GetGuidAndKey(x)) ?? Enumerable.Empty<string>())}");

                LogAbilityInfos(info, "abilities", info.abilities);
                LogAbilityInfos(info, "ascensionAbilities", info.ascensionAbilities);

                Logger.LogDebug($"evolveParams: {GetEvolveParams(info.evolveParams)}");
                Logger.LogDebug($"defaultEvolutionName: {info.defaultEvolutionName ?? "null"}");

                Logger.LogDebug($"tailParams: {GetTailParams(info.tailParams)}");
                Logger.LogDebug($"iceCubeParams: {GetIceCubeParams(info.iceCubeParams)}");

                Logger.LogDebug($"flipPortraitForStrafe: {info.flipPortraitForStrafe}");

                Logger.LogDebug($"specialStatIcon: {info.specialStatIcon}");
                Logger.LogDebug($"boon: {info.boon}");

                Logger.LogDebug($"mods: {info.mods?.Count ?? 0}");
                Logger.LogDebug($"temporaryDecals: {info.temporaryDecals?.Count ?? 0}");
                Logger.LogDebug($"get_decals: {info.get_decals?.Count ?? 0}");

                Logger.LogDebug($"Calculated Attack: {info.Attack}");
                Logger.LogDebug($"Calculated Health: {info.Health}");
                Logger.LogDebug($"Calculated BloodCost: {info.BloodCost}");
                Logger.LogDebug($"Calculated BonesCost: {info.BonesCost}");
                Logger.LogDebug($"Calculated EnergyCost: {info.EnergyCost}");
                Logger.LogDebug($"Calculated Gemified: {info.Gemified}");
                Logger.LogDebug($"Calculated CostTier: {info.CostTier}");
                Logger.LogDebug($"Calculated Sacrificable: {info.Sacrificable}");
                Logger.LogDebug($"Calculated PowerLevel: {info.PowerLevel}");
                Logger.LogDebug($"Calculated NumAbilities: {info.NumAbilities}");

                Logger.LogDebug($"===== End CardInfo: {info.displayedName ?? info.name ?? "null"} =====");
            }
        }
        LogTradeCardAbilityInfoIssues();
    }
    
    /// <summary>
    /// Gets a GUID and Key associated with a Type of Enum from the passed in Enum Value.
    /// </summary>
    /// <param name="value">The Enum we want to get the String Name of.</param>
    /// <typeparam name="T">The Enum Type.</typeparam>
    /// <returns>A String representing the Enum either via the GUID system or the Enum System.</returns>
    private string GetGuidAndKey<T>(T value) where T : System.Enum
    {
        if (GuidManager.TryGetGuidAndKeyEnumValue(value, out string guid, out string key))
            return $"{guid}_{key}";

        return value.ToString();
    }

    /// <summary>
    /// Gets the String Name and DisplayName from a CardInfo.
    /// </summary>
    /// <param name="card">The Card we want to get the DisplayName of.</param>
    /// <returns>The Card Name and DisplayName of the passed in Card as a String.</returns>
    private string GetCardName(CardInfo card)
    {
        if (card == null)
            return "null";

        return $"{card.name} ({card.DisplayedNameEnglish})";
    }

    /// <summary>
    /// Gets the String version of the <see cref="EvolveParams"/>.
    /// </summary>
    /// <param name="evolveParams">The <see cref="EvolveParams"/> we want to turn into a String.</param>
    /// <returns>The <see cref="EvolveParams"/> in the form of a String.</returns>
    private string GetEvolveParams(EvolveParams evolveParams)
    {
        if (evolveParams == null)
            return "null";

        return $"turnsToEvolve={evolveParams.turnsToEvolve}, evolution={GetCardName(evolveParams.evolution)}";
    }

    /// <summary>
    /// Gets the String version of the <see cref="TailParams"/>.
    /// </summary>
    /// <param name="tailParams">The <see cref="TailParams"/> we want to turn into a String.</param>
    /// <returns>The <see cref="TailParams"/> in the form of a String.</returns>
    private string GetTailParams(TailParams tailParams)
    {
        if (tailParams == null)
            return "null";

        return $"tail={GetCardName(tailParams.tail)}, tailLostPortrait={(tailParams.tailLostPortrait != null ? tailParams.tailLostPortrait.name : "null")}";
    }

    /// <summary>
    /// Gets the String version of the <see cref="IceCubeParams"/>.
    /// </summary>
    /// <param name="iceCubeParams">The <see cref="IceCubeParams"/> we want to turn into a String.</param>
    /// <returns>The <see cref="IceCubeParams"/> in the form of a String.</returns>
    private string GetIceCubeParams(IceCubeParams iceCubeParams)
    {
        if (iceCubeParams == null)
            return "null";

        return $"creatureWithin={GetCardName(iceCubeParams.creatureWithin)}";
    }
    
    /// <summary>
    /// A function that allows us to Log Ability Infos.
    /// </summary>
    /// <param name="card">The Card in which bears the AbilityInfos.</param>
    /// <param name="source">The Source of the AbilityInfos.</param>
    /// <param name="abilities">The List of Abilities in which we want to Log.</param>
    private void LogAbilityInfos(CardInfo card, string source, IEnumerable<Ability> abilities)
    {
        if (abilities == null)
        {
            Logger.LogDebug($"{source} AbilityInfo validation: null");
            return;
        }

        foreach (Ability ability in abilities)
        {
            AbilityInfo abilityInfo = AbilitiesUtil.GetInfo(ability);
            string abilityId = GetGuidAndKey(ability);

            if (abilityInfo == null)
            {
                Logger.LogError($"MISSING AbilityInfo | " + $"card={GetCardName(card)} | " + $"source={source} | " + $"ability={abilityId} | " + $"rawValue={(int)ability}");
                continue;
            }

            Logger.LogDebug($"AbilityInfo | " + $"card={GetCardName(card)} | " + $"source={source} | " + $"ability={abilityId} | " + $"rawValue={(int)ability} | " + $"opponentUsable={abilityInfo.opponentUsable} | " + $"passive={abilityInfo.passive} | " + $"activated={abilityInfo.activated} | " + $"powerLevel={abilityInfo.powerLevel} | " + $"canStack={abilityInfo.canStack} | " + $"conduit={abilityInfo.conduit} | " + $"conduitCell={abilityInfo.conduitCell} | " + $"keywordAbility={abilityInfo.keywordAbility} | " + $"rulebookName={abilityInfo.rulebookName ?? "null"}");
        }
    }

    /// <summary>
    /// A function we can run at Startup to try and Validate any Trade Issues that may arise from 'GenerateTradeCardsWithCostTier'. Logging is enabled via 'configEnableTraderCostTierBugDebugLogging'.
    /// </summary>
    private void LogTradeCardAbilityInfoIssues()
    {
        List<CardInfo> learnedCards = CardLoader.LearnedCards;

        if (configEnableTraderCostTierBugDebugLogging.Value)
        {
            Logger.LogDebug($"Learned card count: {learnedCards.Count}");
        }

        foreach (CardInfo card in learnedCards)
        {
            if (card.temple != CardTemple.Nature)
                continue;

            int costTier;

            try
            {
                costTier = card.CostTier;
            }
            catch (Exception ex)
            {
                if (configEnableTraderCostTierBugDebugLogging.Value)
                {
                    Logger.LogError($"Could not calculate CostTier | " + $"card={GetCardName(card)} | " + $"exception={ex}");
                }
                continue;
            }

            if (configEnableTraderCostTierBugDebugLogging.Value)
            {
                Logger.LogDebug($"Trade candidate | " + $"card={GetCardName(card)} | " + $"tier={costTier} | " + $"bloodCost={card.BloodCost} | " + $"bonesCost={card.BonesCost} | " + $"energyCost={card.EnergyCost} | " + $"abilitiesNull={card.abilities == null}");
            }
            
            if (card.abilities == null)
            {
                if (configEnableTraderCostTierBugDebugLogging.Value)
                {
                    Logger.LogError($"NULL abilities list | " + $"card={GetCardName(card)} | " + $"tier={costTier}");
                }
                continue;
            }

            if (card.abilities.Count == 0)
            {
                if (configEnableTraderCostTierBugDebugLogging.Value)
                {
                    Logger.LogDebug($"Trade candidate has no abilities | " + $"card={GetCardName(card)} | " + $"tier={costTier}");
                }
                continue;
            }

            foreach (Ability ability in card.abilities)
            {
                if (configEnableTraderCostTierBugDebugLogging.Value)
                {
                    string abilityId = GetGuidAndKey(ability);
                    AbilityInfo abilityInfo;

                    try
                    {
                        abilityInfo = AbilitiesUtil.GetInfo(ability);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"AbilitiesUtil.GetInfo threw | " + $"card={GetCardName(card)} | " + $"tier={costTier} | " + $"ability={abilityId} | " + $"rawValue={(int)ability} | " +
                            $"exception={ex}");
                        continue;
                    }

                    if (abilityInfo == null)
                    {
                        Logger.LogError($"TRADE CRASH CANDIDATE: Missing AbilityInfo | " + $"card={GetCardName(card)} | " + $"tier={costTier} | " + $"ability={abilityId} | " +
                            $"rawValue={(int)ability} | " + $"reason=AbilitiesUtil.GetInfo returned null; " + $"vanilla code will dereference .opponentUsable");
                        continue;
                    }

                    Logger.LogDebug($"Trade ability OK | " + $"card={GetCardName(card)} | " + $"tier={costTier} | " + $"ability={abilityId} | " + $"rawValue={(int)ability} | " +
                        $"opponentUsable={abilityInfo.opponentUsable}");
                }
            }
        }
    }

    /// <summary>
    /// This patches the Kaycee's Mod Menu to resync all Cards and Abilities when entering the run itself before Kaycee's mod transitions you into the run itself..
    /// </summary>
    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    private static void SyncCardsAndAbilitiesWhenTransitioningToAscensionGame()
    {
        ResyncAll();
    }

    /// <summary>
    /// This patches the Main Menu to resync all Cards and Abilities when entering any part of the base game, before the Menu handles the transition.
    /// </summary>
    [HarmonyPatch(typeof(MenuController), nameof(MenuController.TransitionToGame))]
    [HarmonyPrefix]
    private static void SyncCardsAndAbilitiesWhenTransitioningToGame()
    {
        ResyncAll();
    }
}
