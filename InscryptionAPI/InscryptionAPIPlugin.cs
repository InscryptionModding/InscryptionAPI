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
    public static ConfigEntry<bool> configOverrideArrows;
    /// <summary>
    /// The Configuration for determining whether the Order of Costs in a Cost Choice Node should be Randomized.
    /// </summary>
    public static ConfigEntry<bool> configRandomCostChoiceOrder;
    /// <summary>
    /// The Configuration for determining whether the Boss Scenery of Act 1/KCM should be hidden.
    /// </summary>
    public static ConfigEntry<bool> configHideAct1BossScenery;
    /// <summary>
    /// This Configuration is used for determining what Totem Top Model should be used in game.
    /// </summary>
    internal static ConfigEntry<TotemManager.TotemTopState> configCustomTotemTopModelSelection;
    /// <summary>
    /// This Configuration is used for determining what Item Model should be used in game.
    /// </summary>
    internal static ConfigEntry<ConsumableItemManager.ConsumableState> configCustomItemModelSelection;

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
