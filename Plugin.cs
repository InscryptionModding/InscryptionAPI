using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using UnityEngine.SceneManagement;

#pragma warning disable 169

namespace APIPlugin
{
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public partial class Plugin : BaseUnityPlugin
  {
    private const string PluginGuid = "cyantist.inscryption.api";
    private const string PluginName = "API";
    private const string PluginVersion = "1.11.0.0";

    internal static ManualLogSource Log;
    internal static ConfigEntry<bool> configEnergy;
    internal static ConfigEntry<bool> configDrone;
    internal static ConfigEntry<bool> configMox;
    internal static ConfigEntry<bool> configDroneMox;

    private void Awake()
    {
      Logger.LogInfo($"Loaded {PluginName}!");
      Plugin.Log = base.Logger;

      configEnergy = Config.Bind("Energy","Energy Refresh",true,"Max energy increaces and energy refreshes at end of turn");
      configDrone = Config.Bind("Energy","Energy Drone",false,"Drone is visible to display energy (requires Energy Refresh)");
      configMox = Config.Bind("Mox","Mox Refresh",false,"Mox refreshes at end of battle");
      configDroneMox = Config.Bind("Mox","Mox Drone",false,"Drone displays mox (requires Energy Drone and Mox Refresh)");

      Harmony harmony = new Harmony(PluginGuid);
      harmony.PatchAll();
    }

    private void Start()
    {
      SetAbilityIdentifiers();
      SetSpecialAbilityIdentifiers();
      SetEvolveIdentifiers();
      SetIceCubeIdentifiers();
      SetTailIdentifiers();
      ScriptableObjectLoader<CardInfo>.allData = null;
    }

    private void OnEnable()
    {
      SceneManager.sceneLoaded += this.OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      EnableEnergy(scene.name);
    }
  }
}
