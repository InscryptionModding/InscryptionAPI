using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Sequencers;

/// <summary>
/// Patches in Our Cost Card Backs into the <see cref="CardSingleChoicesSequencer.GetCardbackTexture"/> function for Energy and Mox costs.
/// </summary>
[HarmonyPatch]
internal static class ChoiceNodePatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.GetCardbackTexture))]
    private static void CardSingleChoicesSequencer_GetCardbackTexture(ref Texture __result, CardChoice choice)
    {
        switch (choice.resourceType)
        {
            case ResourceType.Energy:
                __result = TextureHelper.GetImageAsTexture("energyCost.png", typeof(ChoiceNodePatch).Assembly);
                break;
            case ResourceType.Gems:
                __result = TextureHelper.GetImageAsTexture(MoxTextureName(choice.resourceAmount), typeof(ChoiceNodePatch).Assembly);
                break;
        }
    }
    
    /// <summary>
    /// Handles getting the correct Mox Texture Name for usage in the <see cref="CardSingleChoicesSequencer_GetCardbackTexture"/> patch.
    /// </summary>
    /// <param name="index">1 -> <c>Green</c>, 2 -> <c>Orange</c>, 3 -> <c>Blue</c>, otherwise -> <c>Empty</c></param>
    /// <returns>A string texture name in the form of <c>moxCost{Color}.png</c></returns>
    private static string MoxTextureName(int index) => "moxCost" + (index switch { 1 => "Green", 2 => "Orange", 3 => "Blue", _ => "" }) + ".png";
}