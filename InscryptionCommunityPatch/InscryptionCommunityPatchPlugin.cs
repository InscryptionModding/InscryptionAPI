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

/// <summary>
/// The Entry Point for the Community Patch.
/// </summary>
[BepInPlugin(ModGUID, ModName, ModVer)]
[BepInDependency("cyantist.inscryption.api")]
public class PatchPlugin : BaseUnityPlugin
{
    /// <summary>
    /// The GUID for the Inscryption Community Patch, if you depend on this set of Patches and their associated code, you'll want to load after it in <c>Awake()</c>.
    /// </summary>
    public const string ModGUID = "community.inscryption.patch";
    /// <summary>
    /// This is the Inscryption Community Patch's Internal Name, prefer identifying the Community Patch with the GUID.
    /// </summary>
    public const string ModName = "InscryptionCommunityPatch";
    /// <summary>
    /// This is the Version of the Inscryption Community Patch you are using.
    /// </summary>
    public const string ModVer = "2.24.2";

    /// <summary>
    /// This is the Plugin Instance associated with the Inscryption Community Patch.
    /// </summary>
    internal static PatchPlugin Instance;

    /// <summary>
    /// Configure whether Energy should refresh or not. Refresh meaning the per-match removal of the Energy you have acquired.
    /// </summary>
    internal static ConfigEntry<bool> configEnergyPerMatchCleanUp;
    /// <summary>
    /// Configure whether the Energy Drone should be enabled or not.
    /// </summary>
    internal static ConfigEntry<bool> configEnergyDrone;
    /// <summary>
    /// Configure whether Mox should refresh or not. Refresh meaning the per-match removal of the Mox you have acquired.
    /// </summary>
    internal static ConfigEntry<bool> configMoxPerMatchCleanup;
    /// <summary>
    /// Configure whether the Mox Drone should be enabled or not.
    /// </summary>
    internal static ConfigEntry<bool> configMoxDrone;
    /// <summary>
    /// Configure whether the Act 3 Drone or Scale Drone should be used within Act 1.
    /// </summary>
    internal static ConfigEntry<bool> configUtilizeDefaultDrone;
    /// <summary>
    /// Configure whether the Bones Display is active in Act 3 or not.
    /// </summary>
    internal static ConfigEntry<bool> configAct3Bones;
    /// <summary>
    /// Configure whether the Custom Squirrel Tribe Icon should be shown on cards.
    /// </summary>
    internal static ConfigEntry<bool> configCustomSquirrelTribeIconToShowOnCards;
    /// <summary>
    /// Configure whether to use API's custom Act 2 Cost Rendering system or not for Custom and Hybrid Costs.
    /// </summary>
    internal static ConfigEntry<bool> configUseAPIAct2CostRender;
    /// <summary>
    /// Configure whether the Act 2 Cost should be in the Top Left or Top Right corner of the card.
    /// </summary>
    internal static ConfigEntry<bool> configDisplayAct2CostInTheTopRightCorner;
    /// <summary>
    /// Configure whether Cost Icons should use the Vanilla Style icons or the Custom Ones.
    /// </summary>
    internal static ConfigEntry<bool> configDisplayVanillaStyleAct2CostIcons;
    /// <summary>
    /// Configure the rendering of the Act 2 Stack Icon Number Renders, e.g. Black Outline or Cream Outline.
    /// </summary>
    internal static ConfigEntry<bool> configAct2StackIconNumberStyle;
    /// <summary>
    /// Configure whether sigils should be merged on the bottom of a card or not.
    /// </summary>
    internal static ConfigEntry<bool> configMergeSigilsOnBottom;
    /// <summary>
    /// Configure whether the actual patch the sigil is fused with is shown or not. If not the sigil icon shows up orange. Requires <see cref="configMergeSigilsOnBottom"/> to be true.
    /// </summary>
    internal static ConfigEntry<bool> configRemoveSigilPatchesFromCards;
    /// <summary>
    /// Configure whether the Stackable Sigils should be displayed as seperate icons or not.
    /// </summary>
    internal static ConfigEntry<bool> configDisplayStackableSigilsSeparately;
    /// <summary>
    /// Configure whether the Price Tags should be smaller on the cards.
    /// </summary>
    internal static ConfigEntry<bool> configDisplaySmallerPriceTags;
    /// <summary>
    /// Configure whether the Price Tags should be displayed on the Right of a card.
    /// </summary>
    internal static ConfigEntry<bool> configDisplayPriceTagsOnTheRight;
    /// <summary>
    /// Configure whether card selection rows in the Act 2 Tutor Screen should be centered or not.
    /// </summary>
    internal static ConfigEntry<bool> configCenterAct2TutorSelectionRows;
    /// <summary>
    /// Configure whether Emissions should show over or under Cost Icons.
    /// </summary>
    internal static ConfigEntry<bool> configDisplayEmissionsUnderCostIcons;
    /// <summary>
    /// Configure whether Leshy's Eye Color should reset after the 8 Bears Fight.
    /// </summary>
    internal static ConfigEntry<bool> configResetEyesToNormalAfter8BearsFights;
    /// <summary>
    /// Configure whether Undead Cat's Emission should be forced Red or not.
    /// </summary>
    internal static ConfigEntry<bool> configEnforceUndeadCatToHaveARedEmission;
    /// <summary>
    /// Configure whether the Community Patch should display complete Debug Logs or not.
    /// </summary>
    internal static ConfigEntry<bool> configDisplayCompleteDebugLogsInConsole;
    /// <summary>
    /// Configure whether the Community Patch should be in Testing Mode or not.
    /// </summary>
    internal static ConfigEntry<bool> configCommunityPatchTestingMode;
    
    /// <summary>
    /// Our Community Patch's Internal Logger.
    /// </summary>
    new internal static ManualLogSource Logger;
    /// <summary>
    /// Our Community Patch's Internal Harmony Instance.
    /// </summary>
    private readonly Harmony HarmonyInstance = new(ModGUID);

    /// <summary>
    /// The following runs when the Community Patch is enabled by Unity.
    /// </summary>
    private void OnEnable()
    {
        Logger = base.Logger;

        HarmonyInstance.PatchAll(typeof(PatchPlugin).Assembly);
        SceneManager.sceneLoaded += this.OnSceneLoaded;

        if (configCommunityPatchTestingMode.Value)
        {
            ExecuteCommunityPatchTests.PrepareForTests();
            TestCost.Init();
            DebugCards.CreateTestCards();
        }

        CommunityArtPatches.PatchCommunityArt();
    }

    /// <summary>
    /// The following runs when the Community Patch is diabled by Unity.
    /// </summary>
    private void OnDisable()
    {
        HarmonyInstance.UnpatchSelf();
    }

    /// <summary>
    /// This is our Community Patch's Initializer, we primarily use it to set the internal Instance of the Community Patcher and create the Configurations to the Configuration File.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        configEnergyPerMatchCleanUp = Config.Bind("Energy", "Energy Refresh", true, "Max energy increases and energy refreshes at end of turn");
        configEnergyDrone = Config.Bind("Energy", "Energy Drone", false, "Drone is visible to display energy (requires Energy Refresh)");
        configUtilizeDefaultDrone = Config.Bind("Energy", "Default Drone", false, "Drone uses the vanilla model instead of being attached to the scales (requires Energy Drone)");
        configMoxPerMatchCleanup = Config.Bind("Mox", "Mox Refresh", true, "Mox refreshes at end of battle");
        configMoxDrone = Config.Bind("Mox", "Mox Drone", false, "Drone displays mox (requires Energy Drone and Mox Refresh)");
        configAct3Bones = Config.Bind("Bones", "Act 3 Bones", false, "Force bones displayer to be active in Act 3");
        configCustomSquirrelTribeIconToShowOnCards = Config.Bind("Tribes", "Show Squirrel Tribe", false, "Shows the Squirrel tribe icon on cards");
        configUseAPIAct2CostRender = Config.Bind("Card Costs", "GBC Cost render", true, "GBC Cards are able to display custom costs and hybrid costs through the API.");
        configDisplayAct2CostInTheTopRightCorner = Config.Bind("Card Costs", "GBC Cost On Right", true, "GBC Cards display their costs on the top-right corner. If false, display on the top-left corner");
        configDisplayVanillaStyleAct2CostIcons = Config.Bind("Card Costs", "GBC Vanilla Render", false, "GBC cards use vanilla sprites when rendering multiple and custom costs.");
        configAct2StackIconNumberStyle = Config.Bind("Sigil Display", "GBC Stack Icon Number Rendering", true, "If true, stacking icons are a cream outline with a black center. If false, stacking icons are a black outline with a cream center. Act 2");
        configMergeSigilsOnBottom = Config.Bind("Sigil Display", "Merge_On_Bottom", false, "Makes it so if enabled, merged sigils will display on the bottom of the card instead of on the artwork. In extreme cases, this can cause some visual bugs.");
        configRemoveSigilPatchesFromCards = Config.Bind("Sigil Display", "Remove_Patches", false, "Makes it so if enabled, merged sigils will not have a patch behind them anymore and will instead be glowing yellow (only works with Merge_On_Bottom).");
        configDisplayStackableSigilsSeparately = Config.Bind("Sigil Display", "Vanilla Stacking", false, "If enabled, cards with only two visible sigils will display each separately even if they can stack.");
        configDisplaySmallerPriceTags = Config.Bind("Act 1", "Smaller Pricetags", false, "If enabled, the price tags placed on cards while buying from the Trapper will be scaled down.");
        configDisplayPriceTagsOnTheRight = Config.Bind("Act 1", "Move Pricetags", false, "If enabled, the price tags placed on cards while buying from the Trapper will be moved to the right.");
        configCenterAct2TutorSelectionRows = Config.Bind("Act 2", "Centred Hoarder UI", true, "If true, centers displayed cards in each row during the Hoarder selection sequence.");
        configResetEyesToNormalAfter8BearsFights = Config.Bind("Act 1", "Reset Red Eyes", false, "Resets Leshy's eyes to normal if they were turned red due to a boss fight's grizzly bear sequence.");
        configDisplayEmissionsUnderCostIcons = Config.Bind("Act 1", "Render Costs Above Emission", true, "Applies a mask to card emissions that prevents them from covering play costs when rendering in-game.");
        configEnforceUndeadCatToHaveARedEmission = Config.Bind("General", "Undead Cat Emission", false, "If true, Undead Cat will have a forced red emission.");
        configDisplayCompleteDebugLogsInConsole = Config.Bind("General", "Full Debug", true, "If true, displays all debug logs in the console.");
        configCommunityPatchTestingMode = Config.Bind("General", "Test Mode", false, "Puts the game into test mode. This will cause (among potentially other things) a new run to spawn a number of cards into your opening deck that will demonstrate card behaviors.");
    }

    /// <summary>
    /// A simple function that runs when a Scene has Loaded in Unity.
    /// </summary>
    /// <param name="scene">The Scene in which we are trying to Load things during.</param>
    /// <param name="mode">The Mode in which to Load the Scene with.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnergyDrone.TryEnableEnergy(scene.name);
    }
}
