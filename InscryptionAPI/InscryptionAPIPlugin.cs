global using UnityObject = UnityEngine.Object;

using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI;

[BepInPlugin(ModGUID, ModName, ModVer)]
[HarmonyPatch]
public class InscryptionAPIPlugin : BaseUnityPlugin
{
    public const string ModGUID = "cyantist.inscryption.api";
    public const string ModName = "InscryptionAPI";
    public const string ModVer = "2.0.0";

    new internal static ManualLogSource Logger;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    private static readonly (Type, string)[] ObjectLoaderTypes =
    {
        (typeof(AbilityInfo), "Abilities"),
        (typeof(AscensionChallengeInfo), "Ascension/Challenges"),
        (typeof(BoonData), "Boons"),
        (typeof(CommandLineTextSegment), ""),
        (typeof(HoloMapWorldData), "Map/HoloMapWorlds"),
        (typeof(ItemData), "Consumables")
    };
    
    public void OnEnable()
    {
        Logger = base.Logger;
        
        var loaderType = typeof(ScriptableObjectLoader<>);
        foreach (var type in ObjectLoaderTypes)
        {
            Logger.LogMessage($"Loading {type.Item1.FullName}...");
            AccessTools.DeclaredMethod(loaderType.MakeGenericType(type.Item1), "LoadData").Invoke(null, new object[] { type.Item2 });
        }
        
        HarmonyInstance.PatchAll(typeof(InscryptionAPIPlugin).Assembly);
    }

    public void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }
}
