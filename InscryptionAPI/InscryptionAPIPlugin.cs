global using UnityObject = UnityEngine.Object;

using BepInEx;
using HarmonyLib;

namespace InscryptionAPI;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    public const string ModGUID = "cyanist.inscryption.api";
    public const string ModName = "InscryptionAPI";
    public const string ModVer = "2.0.0";

    private readonly Harmony HarmonyInstance = new(ModGUID);
    
    public void OnEnable()
    {
        HarmonyInstance.PatchAll(typeof(InscryptionAPIPlugin).Assembly);
    }

    public void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }
}
