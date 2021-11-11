using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using DiskCardGame;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
#pragma warning disable 169

namespace APIPlugin
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "cyantist.inscryption.api";
        private const string PluginName = "API";
        private const string PluginVersion = "1.9.1.0";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> configEnergy;
        internal static ConfigEntry<bool> configDrone;
        internal static ConfigEntry<bool> configMox;
        internal static ConfigEntry<bool> configDroneMox;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            configEnergy = Config.Bind("Energy",
                                         "Energy Refresh",
                                         false,
                                         "Max energy increaces and energy refreshes at end of turn");
            configDrone = Config.Bind("Energy",
                                         "Energy Drone",
                                         false,
                                         "Drone is visible to display energy (requires Energy Refresh)");
            configMox = Config.Bind("Mox",
                                    "Mox Refresh",
                                    false,
                                    "Mox refreshes at end of battle");
            configDroneMox = Config.Bind("Mox",
                                         "Mox Drone",
                                         false,
                                         "Drone displays mox (requires Energy Drone and Mox Refresh)");
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }

        private void Start()
        {
          foreach(var item in NewCard.abilityIds)
          {
            foreach (AbilityIdentifier id in item.Value)
            {
              if (id.id != 0)
              {
                NewCard.cards[item.Key].abilities.Add(id.id);
                item.Value.Remove(id);
              }
              else
              {
                Plugin.Log.LogWarning($"Ability {id} not found for card {NewCard.cards[item.Key]}");
              }
            }
          }
          foreach(var item in CustomCard.abilityIds)
          {
            foreach (AbilityIdentifier id in item.Value)
            {
              if (id.id != 0)
              {
                CustomCard.cards[item.Key].abilities.Add(id.id);
                item.Value.Remove(id);
              }
              else
              {
                Plugin.Log.LogWarning($"Ability {id} not found for card {CustomCard.cards[item.Key]}");
              }
            }
          }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += this.OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
          if (scene.name == "Part1_Cabin")
          {
            UnityEngine.Object.Instantiate(Resources.Load<ResourceDrone>("prefabs/cardbattle/ResourceModules"));
            if(Plugin.configDrone.Value)
            {
              StartCoroutine(AwakeDrone());
            }
          }
        }

        private IEnumerator AwakeDrone()
        {
          yield return new WaitForSeconds(1);
          Singleton<ResourceDrone>.Instance.Awake();
          yield return new WaitForSeconds(1);
          Singleton<ResourceDrone>.Instance.AttachGemsModule();
        }
      }
}
