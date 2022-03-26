global using UnityObject = UnityEngine.Object;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using InscryptionCommunityPatch.Card;
using InscryptionCommunityPatch.ResourceManagers;
using InscryptionCommunityPatch.Tests;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

[assembly: InternalsVisibleTo("Assembly-CSharp")]

namespace InscryptionCommunityPatch;

[BepInPlugin(ModGUID, ModName, ModVer)]
[HarmonyPatch]
public class PatchPlugin : BaseUnityPlugin
{
    public const string ModGUID = "community.inscryption.patch";
    public const string ModName = "InscryptionCommunityPatch";
    public const string ModVer = "1.0.0";

    internal static PatchPlugin Instance;

    internal static ConfigEntry<bool> configEnergy;
    internal static ConfigEntry<bool> configDrone;
    internal static ConfigEntry<bool> configMox;
    internal static ConfigEntry<bool> configDroneMox;

    internal static ConfigEntry<bool> rightAct2Cost;
    internal static ConfigEntry<bool> act2CostRender;

    internal static ConfigEntry<bool> act2StackIconType;

    internal static ConfigEntry<bool> configMergeOnBottom;

    internal static ConfigEntry<bool> configRemovePatches;

    internal static ConfigEntry<bool> configTestState;

    new internal static ManualLogSource Logger;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public void OnEnable()
    {
        Logger = base.Logger;

        HarmonyInstance.PatchAll(typeof(PatchPlugin).Assembly);
        SceneManager.sceneLoaded += this.OnSceneLoaded;

        if (configTestState.Value)
            ExecuteCommunityPatchTests.PrepareForTests();

        CommunityArtPatches.PatchCommunityArt();
    }

    public void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }

    public void Awake()
    {
        Instance = this;
        configEnergy = Config.Bind("Energy","Energy Refresh",true,"Max energy increases and energy refreshes at end of turn");
        configDrone = Config.Bind("Energy","Energy Drone",false,"Drone is visible to display energy (requires Energy Refresh)");
        configMox = Config.Bind("Mox","Mox Refresh",true,"Mox refreshes at end of battle");
        configDroneMox = Config.Bind("Mox","Mox Drone",false,"Drone displays mox (requires Energy Drone and Mox Refresh)");
        act2CostRender = Config.Bind("Card Costs", "GBC Cost render", true, "GBC Cards are able to display custom costs and hybrid costs through the API.");
        rightAct2Cost = Config.Bind("Card Costs","GBC Cost On Right",true,"GBC Cards display their costs on the top-right corner. If false, display on the top-left corner");
        configMergeOnBottom = Config.Bind("Sigil Display", "Merge_On_Botom", false, "Makes it so if enabled, merged sigils will display on the bottom of the card instead of on the artwork. In extreme cases, this can cause some visual bugs.");
		configRemovePatches = Config.Bind("Sigil Display", "Remove_Patches", false, "Makes it so if enabled, merged sigils will not have a patch behind them anymore and will instead be glowing yellow (only works with Merge_On_Bottom).");
        act2StackIconType = Config.Bind("Sigil Display", "Act 2 Sigil icon type", true, "If true, stacking icons are a cream outline with a black center. If false, stacking icons are a black outline with a cream center. Act 2");

        configTestState = Config.Bind("General", "Test Mode", false, "Puts the game into test mode. This will cause (among potentially other things) a new run to spawn a number of cards into your opening deck that will demonstrate card behaviors.");        
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnergyDrone.TryEnableEnergy(scene.name);
    }
}
