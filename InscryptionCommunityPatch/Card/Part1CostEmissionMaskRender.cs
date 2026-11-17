using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

/// <summary>
///  Modifies how card costs are rendered in Act 1 to handle whether the cost should be displayed above or below the emission.
/// </summary>
[HarmonyPatch]
public class Part1CostEmissionMaskRender {
    /// <summary>
    /// A Dictionary of <see cref="CardDisplayer3D"/> to <see cref="SpriteRenderer"/> representing the Rendering Masks for the Cost.
    /// </summary>
    public static readonly Dictionary<CardDisplayer3D, SpriteRenderer> CostEmissionMaskRenderers = new();

    /// <summary>
    /// Verifies the 3D Cost Emission Mask Render that way we can find out whether we can apply the rendering change or not.
    /// </summary>
    /// <param name="cardDisplayer">The <see cref="CardDisplayer3D"/> for the given Card.</param>
    /// <param name="emissionEnabled">Whether the Emission is enabled or not.</param>
    /// <returns>A <see cref="SpriteRenderer"/> if verified successfully, otherwise <c>default</c> version of the <see cref="SpriteRenderer"/>.</returns>
    public static SpriteRenderer Verify3DCostEmissionMaskRenderer(CardDisplayer3D cardDisplayer, bool emissionEnabled) {
        // add entry for new CardDisplayer3D's
        if (!CostEmissionMaskRenderers.TryGetValue(cardDisplayer, out SpriteRenderer result)) {
            //PatchPlugin.Logger.LogDebug("[Verify3DCostEmissionMaskRenderer] Add cost mask renderer to CardDisplay3D");
            GameObject obj = GameObject.Instantiate(cardDisplayer.costRenderer.gameObject, cardDisplayer.transform);
            obj.name = "CostEmissionMask";
            obj.layer = cardDisplayer.emissivePortraitRenderer.gameObject.layer;
            result = obj.GetComponent<SpriteRenderer>();
            result.color = Color.black;
            result.sortingOrder = 100;
            CostEmissionMaskRenderers.Add(cardDisplayer, result);
        }

        if (result == null) {
            PatchPlugin.Logger.LogWarning("[Verify3DCostEmissionMaskRenderer] Could not find/create SpriteRenderer for CardDisplayer3D instance");
        }
        else {
            // disable if config is false ; otherwise toggle based on appearance of emissive portrait
            result.gameObject.SetActive(PatchPlugin.configDisplayEmissionsUnderCostIcons.Value && emissionEnabled);
        }

        return result;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.Awake))]
    private static void AddCostEmissionMaskOnAwake(CardDisplayer3D __instance) {
        if (SaveManager.SaveFile.IsPart1) {
            Verify3DCostEmissionMaskRenderer(__instance, false);
        }
    }

    [HarmonyPriority(Priority.Last), HarmonyPostfix, HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
    private static void UpdateCostEmissionMask(CardDisplayer3D __instance) {
        if (SaveManager.SaveFile.IsPart1) {
            SpriteRenderer rend = Verify3DCostEmissionMaskRenderer(__instance, __instance.emissivePortraitRenderer.gameObject.activeSelf);
            if (rend != null) {
                //PatchPlugin.Logger.LogDebug("[UpdateCostEmissionMask] Update Cost emission mask");
                rend.sprite = __instance.costRenderer.sprite;
            }
        }
    }
}
