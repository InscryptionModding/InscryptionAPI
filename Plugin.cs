using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections;

#pragma warning disable 169

namespace APIPlugin
{
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public partial class Plugin : BaseUnityPlugin
  {
    private const string PluginGuid = "cyantist.inscryption.api";
    private const string PluginName = "API";
    private const string PluginVersion = "1.13.4";

    internal static ManualLogSource Log;
    internal static ConfigEntry<bool> configEnergy;
    internal static ConfigEntry<bool> configDrone;
    internal static ConfigEntry<bool> configMox;
    internal static ConfigEntry<bool> configDroneMox;

    private void Awake()
    {
      Logger.LogInfo($"Loaded {PluginName}!");
      Plugin.Log = base.Logger;

      configEnergy = Config.Bind("Energy","Energy Refresh",true,"Max energy increases and energy refreshes at end of turn");
      configDrone = Config.Bind("Energy","Energy Drone",false,"Drone is visible to display energy (requires Energy Refresh)");
      configMox = Config.Bind("Mox","Mox Refresh",false,"Mox refreshes at end of battle");
      configDroneMox = Config.Bind("Mox","Mox Drone",false,"Drone displays mox (requires Energy Drone and Mox Refresh)");

      Harmony harmony = new Harmony(PluginGuid);
      try
      {
        harmony.PatchAll();
      }
      catch
      {
        Log.LogError("Failed to apply patches for API. Are you using Kaycee's Mod?");
        StartCoroutine(KayceeError());
      }
    }

    private IEnumerator KayceeError()
    {
      AsyncOperation asyncOp = SceneLoader.StartAsyncLoad("CorruptedSaveMessage");
      yield return new WaitUntil(() => asyncOp.isDone);
      GameObject[] objects = SceneManager.GetSceneByName("CorruptedSaveMessage").GetRootGameObjects();
      TextMeshProUGUI text = objects.ToList().Find(name => name.name == "Screen").GetComponentInChildren<TextMeshProUGUI>();
      text.text = "Oh no! It looks like you're using the <color=#FB3F4F>normal API</color>\nfor Kaycee's Mod.\nYou need to use Kaycee's API if you want to use\nmods for Kaycee's Mod, or switch back to the base\ngame through Steam. Kaycee's API can be\nfound in the <color=#FB3F4F>Inscryption Modding Discord\n#mod-showcase</color> channel.\nKeep in mind that some mods may require a\ndifferent version to work, or are entirely\nincompatible with Kaycee's Mod.\n\n(Press Escape to quit)";
    }

    private void Start()
    {
      Log.LogDebug($"APIPlugin Start() begin");
      SetAbilityIdentifiers();
      SetSpecialAbilityIdentifiers();
      SetEvolveIdentifiers();
      SetIceCubeIdentifiers();
      SetTailIdentifiers();
      Log.LogDebug($"APIPlugin Start() end");
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
