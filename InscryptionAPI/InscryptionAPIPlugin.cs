global using UnityObject = UnityEngine.Object;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace InscryptionAPI;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    public const string ModGUID = "cyantist.inscryption.api";
    public const string ModName = "InscryptionAPI";
    public const string ModVer = "2.0.0";

    private readonly Harmony HarmonyInstance = new(ModGUID);

    internal static ManualLogSource Log;
    
    public void OnEnable()
    {
        Log = this.Logger;
        HarmonyInstance.PatchAll(typeof(InscryptionAPIPlugin).Assembly);
    }

    public void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }
}
