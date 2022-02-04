global using UnityObject = UnityEngine.Object;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp")]

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

    public static event Action<Type> ScriptableObjectLoaderLoad;
    internal static void InvokeSOLEvent(Type type)
    {
        ScriptableObjectLoaderLoad?.Invoke(type);
    }

    public void OnEnable()
    {
        Logger = base.Logger;

        HarmonyInstance.PatchAll(typeof(InscryptionAPIPlugin).Assembly);
    }

    public void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }

    public void Start()
    {
        CardManager.SyncCardList();
    }
}
