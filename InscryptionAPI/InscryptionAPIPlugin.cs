global using UnityObject = UnityEngine.Object;

using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Regions;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using InscryptionAPI.Items;
using InscryptionAPI.Totems;
using BepInEx.Bootstrap;
using Mono.Cecil;
using System.Reflection;
using System.IO;

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
    public const string ModVer = "2.6.0";

    internal static ConfigEntry<TotemManager.TotemTopState> configCustomTotemTopTypes;
    internal static ConfigEntry<ConsumableItemManager.ConsumableState> configCustomItemTypes;

    static InscryptionAPIPlugin()
    {
        AppDomain.CurrentDomain.AssemblyResolve += static (_, e) => {
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
                if (refAsm.Name.Equals("API"))
                    outdatedPlugins += $" - {pluginAsm.GetName().Name}\n";
        }
        Logger.LogWarning("The following plugins have been flagged as using an outdated version of the API:\n"
            + outdatedPlugins
            + "\nAn attempt has been made to ensure they still work, but it isn't perfect so please update/disable them if problems arise.");

    }

    private void Awake()
    {
        configCustomTotemTopTypes = Config.Bind("Totems","Top Types",TotemManager.TotemTopState.CustomTribes,"If Vanilla, Don't change totem tops, If CustomTribes, all custom tribes added will use custom totem tops. If AllTribes then all totem tops will use a custom top.");
        configCustomItemTypes = Config.Bind("Items","Types",ConsumableItemManager.ConsumableState.Custom,"If Vanilla, only vanilla items used, If Custom, all custom items added will use custom models. If All then all tops will use a custom model.");
    }

    private void Start()
    {
        CheckForOutdatedPlugins();
        CardManager.ActivateEvents();
        CardManager.ResolveMissingModPrefixes();
        ResyncAll();
        CardManager.AuditCardList();
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
