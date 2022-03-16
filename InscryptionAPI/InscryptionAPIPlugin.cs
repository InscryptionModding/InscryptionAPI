global using UnityObject = UnityEngine.Object;

using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Regions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp")]

namespace InscryptionAPI;

[BepInPlugin(ModGUID, ModName, ModVer)]
[HarmonyPatch]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    public const string ModGUID = "cyantist.inscryption.api";
    public const string ModName = "InscryptionAPI";
    public const string ModVer = "2.0.2";

    private static bool _hasShownOldApiWarning = false;

    static InscryptionAPIPlugin()
    {
        AppDomain.CurrentDomain.AssemblyResolve += static (_, e) =>
        {
            if (e.Name.StartsWith("API, Version=1"))
            {
                if (!_hasShownOldApiWarning)
                {
                    Logger.LogWarning(
                        "Some plugins installed require an outdated version of the API.\n"
                        + "An attempt has been made that these still work, but it isn't perfect, so please search for those to disable if you experience any problems."
                    );
                    _hasShownOldApiWarning = true;
                }
                return typeof(InscryptionAPIPlugin).Assembly;
            }
            return null;
        };
    }

    new internal static ManualLogSource Logger;

    private readonly Harmony _harmonyInstance = new(ModGUID);

    public static event Action<Type> ScriptableObjectLoaderLoad;
    internal static void InvokeSOLEvent(Type type)
    {
        ScriptableObjectLoaderLoad?.Invoke(type);
    }

    public void OnEnable()
    {
        Logger = base.Logger;

        _harmonyInstance.PatchAll(typeof(InscryptionAPIPlugin).Assembly);
    }

    public void OnDisable()
    {
        _harmonyInstance.UnpatchSelf();
    }

    internal static void ResyncAll()
    {
        CardManager.SyncCardList();
        AbilityManager.SyncAbilityList();
        EncounterManager.SyncEncounterList();
        RegionManager.SyncRegionList();
    }

    public void Start()
    {
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