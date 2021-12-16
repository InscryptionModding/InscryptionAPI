using BepInEx;
using HarmonyLib;

namespace InscryptionAPI;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    public const string ModGUID = "cyanist.inscryption.api";
    public const string ModName = "InscryptionAPI";
    public const string ModVer = "2.0.0";

    public void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(InscryptionAPIPlugin).Assembly, ModGUID);
    }
}
