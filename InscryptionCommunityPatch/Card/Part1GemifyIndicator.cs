using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// Patches in the Gemify Renderer into Act 1 for the ability to use Gemification in Act 1 visually.
/// </summary>
[HarmonyPatch]
public class Part1GemifyIndicator {
    /// <summary>
    /// An <see cref="Renderer"/> array representing the Gemification Renderers. They are ordered as follows:
    /// <list type="table">
    ///     <listheader>Order of the Gemification Renderers:</listheader>
    ///     <item><term>0</term><description>Base Gemification</description></item>
    ///     <item><term>1</term><description>Power Gemification</description></item>
    ///     <item><term>2</term><description>Health Gemification</description></item>
    ///     <item><term>3</term><description>Cost Gemification</description></item>
    /// </list>
    /// </summary>
    public static Renderer[] GemifyRenderers = new Renderer[4];

    [HarmonyPostfix, HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
    private static void HandleGemifyAct1(CardRenderInfo renderInfo, PlayableCard playableCard) {
        if (!SaveManager.SaveFile.IsPart1) {
            return;
        }

        if (GemifyRenderers[0] == null) {
            AddAct1GemifyVisuals(CardRenderCamera.Instance);
        }

        // if the card we're rendering if gemified
        if (renderInfo.baseInfo.Gemified || renderInfo.temporaryMods.Exists(x => x.gemify) || (playableCard != null && playableCard.IsGemified())) {
            bool activateAttack = false;
            bool activateHealth = false;
            bool activateCost = false;
            if (playableCard != null) {
                if (playableCard.OpponentCard) {
                    activateAttack = OpponentGemsManager.Instance.HasGem(GemType.Orange);
                    activateHealth = OpponentGemsManager.Instance.HasGem(GemType.Green);
                    activateCost = OpponentGemsManager.Instance.HasGem(GemType.Blue);
                }
                else {
                    activateAttack = ResourcesManager.Instance.HasGem(GemType.Orange);
                    activateHealth = ResourcesManager.Instance.HasGem(GemType.Green);
                    activateCost = ResourcesManager.Instance.HasGem(GemType.Blue);
                }
            }

            // always render base texture
            GemifyRenderers[0].enabled = true;
            GemifyRenderers[1].enabled = activateAttack;
            GemifyRenderers[2].enabled = activateHealth;
            GemifyRenderers[3].enabled = activateCost;
        }
        else {
            // turn off renderers by default
            GemifyRenderers[0].enabled = false;
            GemifyRenderers[1].enabled = false;
            GemifyRenderers[2].enabled = false;
            GemifyRenderers[3].enabled = false;
        }
    }

    /// <summary>
    /// Handles adding an Act 1 Gemification Visual to a card.
    /// </summary>
    /// <param name="cardRenderCamera">The <see cref="CardRenderCamera"/> associated with the card.</param>
    public static void AddAct1GemifyVisuals(CardRenderCamera cardRenderCamera) {
        CardDisplayer3D dis = cardRenderCamera.cardDisplayer as CardDisplayer3D;

        GameObject obj = new("GemifyTest");
        obj.transform.SetParent(cardRenderCamera.cardDisplayer.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        // in order to get the gemify visualisers to appear correctly, we need to change the rendering order
        // of basically every element in the card displayer

        // new order:
        // portrait: 0 (default)
        // gemify renderers: [1, 4]
        // most everything else: 5
        // card cost: 6
        // decal renderers: [7, 11]
        dis.healthText.gameObject.GetComponent<MeshRenderer>().sortingOrder = 5;
        dis.attackText.gameObject.GetComponent<MeshRenderer>().sortingOrder = 5;
        dis.nameText.gameObject.GetComponent<MeshRenderer>().sortingOrder = 5;
        dis.nameGraphicRenderer.sortingOrder = 5;
        dis.emissivePortraitRenderer.sortingOrder = 5;
        dis.costRenderer.sortingOrder = 6;
        foreach (Renderer r in dis.tribeIconRenderers) {
            r.sortingOrder = 5;
        }

        for (int i = 0; i < dis.decalRenderers.Count; i++) {
            dis.decalRenderers[i].sortingOrder = 7 + i;
            dis.decalRenderers[i].GetComponent<SetSortingLayer>().sortingOrder = 7 + i;
        }

        List<AbilityIconInteractable> acts = dis.AbilityIcons.GetComponentsInChildren<AbilityIconInteractable>(true).ToList();
        foreach (Transform tr in dis.transform) {
            acts.Concat(tr.GetComponentsInChildren<AbilityIconInteractable>(true));
        }
        foreach (AbilityIconInteractable a in acts) {
            Renderer rend = a.GetComponent<Renderer>();
            if (rend.sortingOrder == 0) {
                rend.sortingOrder = 5;

                SetSortingLayer layer = a.GetComponent<SetSortingLayer>();
                if (layer != null) {
                    layer.sortingOrder = 5;
                }
            }
        }

        System.Reflection.Assembly asm = typeof(CommunityArtPatches).Assembly;

        // add new renderers for gemify textures, using the decal renderers as the base
        GameObject obj1 = GameObject.Instantiate(dis.decalRenderers[0].gameObject, obj.transform);
        GemifyRenderers[0] = obj1.GetComponent<Renderer>();
        GemifyRenderers[0].sortingOrder = obj1.GetComponent<SetSortingLayer>().sortingOrder = 1;
        GemifyRenderers[0].enabled = true;
        GemifyRenderers[0].material.mainTexture = TextureHelper.GetImageAsTexture($"act1_gemify_base.png", asm);

        GameObject obj2 = GameObject.Instantiate(dis.decalRenderers[0].gameObject, obj.transform);
        GemifyRenderers[1] = obj2.GetComponent<Renderer>();
        GemifyRenderers[1].sortingOrder = obj2.GetComponent<SetSortingLayer>().sortingOrder = 2;
        GemifyRenderers[1].enabled = true;
        GemifyRenderers[1].material.mainTexture = TextureHelper.GetImageAsTexture($"act1_gemify_attack.png", asm);

        GameObject obj3 = GameObject.Instantiate(dis.decalRenderers[0].gameObject, obj.transform);
        GemifyRenderers[2] = obj3.GetComponent<Renderer>();
        GemifyRenderers[2].sortingOrder = obj3.GetComponent<SetSortingLayer>().sortingOrder = 3;
        GemifyRenderers[2].enabled = true;
        GemifyRenderers[2].material.mainTexture = TextureHelper.GetImageAsTexture($"act1_gemify_health.png", asm);

        GameObject obj4 = GameObject.Instantiate(dis.decalRenderers[0].gameObject, obj.transform);
        GemifyRenderers[3] = obj4.GetComponent<Renderer>();
        GemifyRenderers[3].sortingOrder = obj4.GetComponent<SetSortingLayer>().sortingOrder = 4;
        GemifyRenderers[3].enabled = true;
        GemifyRenderers[3].material.mainTexture = TextureHelper.GetImageAsTexture($"act1_gemify_cost.png", asm);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardRenderCamera), nameof(CardRenderCamera.UpdateTextureWhenReady))]
    private static bool AddGemifyRenderersBeforeUpdateTexture(CardRenderCamera __instance) {
        if (SaveManager.SaveFile.IsPart1 && GemifyRenderers[0] == null) {
            AddAct1GemifyVisuals(__instance);
        }
        return true;
    }
}
