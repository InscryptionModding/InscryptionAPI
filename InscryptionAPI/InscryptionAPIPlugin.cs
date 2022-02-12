global using UnityObject = UnityEngine.Object;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.ResourceManagers;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

[assembly: InternalsVisibleTo("Assembly-CSharp")]

namespace InscryptionAPI;

[BepInPlugin(ModGUID, ModName, ModVer)]
[HarmonyPatch]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    public const string ModGUID = "cyantist.inscryption.api";
    public const string ModName = "InscryptionAPI";
    public const string ModVer = "2.0.0";

    internal static InscryptionAPIPlugin Instance;

    internal static ConfigEntry<bool> configEnergy;
    internal static ConfigEntry<bool> configDrone;
    internal static ConfigEntry<bool> configMox;
    internal static ConfigEntry<bool> configDroneMox;

    internal static ConfigEntry<bool> rightAct2Cost;

    static InscryptionAPIPlugin()
    {
        AppDomain.CurrentDomain.AssemblyResolve += static (_, e) => {
            Logger.LogInfo($"Assembly resolve attempt for {e.Name}");
            if (e.Name.StartsWith("API, Version=1"))
            {
                Logger.LogInfo($"Resolving old API to new API");
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

    public void OnEnable()
    {
        Logger = base.Logger;

        HarmonyInstance.PatchAll(typeof(InscryptionAPIPlugin).Assembly);
        SceneManager.sceneLoaded += this.OnSceneLoaded;
    }

    public void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }

    public void Start()
    {
        CardManager.SyncCardList();
        AbilityManager.SyncAbilityList();
    }

    public void Awake()
    {
        Instance = this;
        configEnergy = Config.Bind("Energy","Energy Refresh",true,"Max energy increases and energy refreshes at end of turn");
        configDrone = Config.Bind("Energy","Energy Drone",false,"Drone is visible to display energy (requires Energy Refresh)");
        configMox = Config.Bind("Mox","Mox Refresh",false,"Mox refreshes at end of battle");
        configDroneMox = Config.Bind("Mox","Mox Drone",false,"Drone displays mox (requires Energy Drone and Mox Refresh)");
        rightAct2Cost = Config.Bind("Card Costs","GBC Cost On Right",true,"GBC Cards display their costs on the top-right corner. If false, display on the top-left corner");
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ActOneEnergyDrone.TryEnableEnergy(scene.name);
    }
}
