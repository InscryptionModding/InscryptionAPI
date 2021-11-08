using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
#pragma warning disable 169

namespace APIPlugin
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "cyantist.inscryption.api";
        private const string PluginName = "API";
        private const string PluginVersion = "1.8.2.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }
    }
}
