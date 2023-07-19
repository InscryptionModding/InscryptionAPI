global using UnityObject = UnityEngine.Object;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Encounters;
using InscryptionAPI.Items;
using InscryptionAPI.PixelCard;
using InscryptionAPI.Regions;
using InscryptionAPI.Totems;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp")]
[assembly: InternalsVisibleTo("Assembly-CSharp.APIPatcher.mm")]
[assembly: InternalsVisibleTo("APIPatcher")]

namespace InscryptionAPI;

[BepInPlugin(ModGUID, ModName, ModVer)]
[HarmonyPatch]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    public const string ModGUID = "cyantist.inscryption.api";
    public const string ModName = "InscryptionAPI";
    public const string ModVer = "2.14.3";

    internal static ConfigEntry<bool> configOverrideArrows;
    internal static ConfigEntry<TotemManager.TotemTopState> configCustomTotemTopTypes;
    internal static ConfigEntry<ConsumableItemManager.ConsumableState> configCustomItemTypes;

    static InscryptionAPIPlugin()
    {
        AppDomain.CurrentDomain.AssemblyResolve += static (_, e) =>
        {
            if (e.Name.StartsWith("API, Version=1"))
            {
                return typeof(InscryptionAPIPlugin).Assembly;
            }
            return null;
        };
    }

    new internal static ManualLogSource Logger;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static event Action<Type> ScriptableObjectLoaderLoad;
    internal static void InvokeSOLEvent(Type type)
    {
        ScriptableObjectLoaderLoad?.Invoke(type);
    }

    private void OnEnable()
    {
        Logger = base.Logger;

        HarmonyInstance.PatchAll(typeof(InscryptionAPIPlugin).Assembly);
    }

    private void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }

    internal static void ResyncAll()
    {
        CardManager.SyncCardList();
        CardModificationInfoManager.SyncCardMods();
        AbilityManager.SyncAbilityList();
        EncounterManager.SyncEncounterList();
        RegionManager.SyncRegionList();
    }

    internal static void CheckForOutdatedPlugins()
    {
        string outdatedPlugins = "";
        foreach (var pluginAsm in Chainloader.PluginInfos.Values.Select(p => p.Instance.GetType().Assembly).Distinct())
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

    private void Awake()
    {
        configCustomTotemTopTypes = Config.Bind("Totems", "Top Types", TotemManager.TotemTopState.CustomTribes, "If Vanilla, don't change totem tops; if CustomTribes, added custom tribes will use custom totem tops; if AllTribes then all totem tops will use a custom top.");
        configCustomItemTypes = Config.Bind("Items", "Types", ConsumableItemManager.ConsumableState.Custom, "If Vanilla, only vanilla items will be used; if Custom, added custom items will use custom models; if All then all items will use a custom model.");
        configOverrideArrows = Config.Bind("Menus", "Override Arrows", false, "When true, forces the challenge screen arrows to appear at the top of the screen instead of the sides.");
    }

    private void Start()
    {
        CheckForOutdatedPlugins();
        DeathCardManager.AddCustomDeathCards();
        CardManager.ActivateEvents();
        CardManager.ResolveMissingModPrefixes();
        ResyncAll();
        CardManager.AuditCardList();
        PixelCardManager.Initialise();
        Logger.LogInfo($"Inserted {DialogueManager.CustomDialogue.Count} dialogue event(s)!");
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    private static void SyncCardsAndAbilitiesWhenTransitioningToAscensionGame()
    {
        ResyncAll();
    }

    [HarmonyPatch(typeof(MenuController), nameof(MenuController.TransitionToGame))]
    [HarmonyPrefix]
    private static void SyncCardsAndAbilitiesWhenTransitioningToGame()
    {
        ResyncAll();
    }
}
