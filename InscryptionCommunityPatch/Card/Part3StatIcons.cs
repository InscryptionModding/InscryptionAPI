using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal static class Part3StatIcons
{
    // This activates the renderers for the special stat icons on the disk cards (where appropriate)
    // scales the icons appropriately
    // set the material color
    // sets the game objects to the correct layer (where appropriate)
    // and in some cases swaps the x position of the health and attack icons because they were wrong for some reason

    private static readonly Vector3 ICON_SCALE = new Vector3(0.2f, 0.2f, 1f);
    private static readonly Vector3 LEFT_ICON = new Vector3(-0.39f, 0.033f, 0f);
    private static readonly Vector3 RIGHT_ICON = new Vector3(0.39f, 0.033f, 0f);
    private static readonly Color ICON_COLOR = GameColors.Instance.brightBlue;

    [HarmonyPatch(typeof(DiskCardGame.Card), "RenderCard")]
    [HarmonyPrefix]
    private static void UpdateLiveRenderedCard(ref DiskCardGame.Card __instance)
    {
        if (SaveManager.SaveFile != null && SaveManager.SaveFile.IsPart3)
        {
            CardStatIcons icons = __instance.gameObject.GetComponentInChildren<CardStatIcons>();

            if (icons == null)
                return;

            if (icons.attackIconRenderer != null)
            {
                icons.attackIconRenderer.gameObject.transform.localPosition = RIGHT_ICON; // Flipped for some reason??
                icons.attackIconRenderer.gameObject.transform.localScale = ICON_SCALE;
                icons.attackIconRenderer.material.color = ICON_COLOR;
                icons.attackIconRenderer.enabled = false;
            }

            if (icons.healthIconRenderer != null)
            {
                icons.healthIconRenderer.gameObject.transform.localPosition = LEFT_ICON;  // Flipped for some reason??
                icons.healthIconRenderer.gameObject.transform.localScale = ICON_SCALE;
                icons.healthIconRenderer.material.color = ICON_COLOR;
                icons.healthIconRenderer.enabled = false;
            }
        }
    }

    [HarmonyPatch(typeof(CardDisplayer3D), "DisplayInfo")]
    [HarmonyPrefix]
    private static void UpdateCardDisplayer(ref CardDisplayer3D __instance, CardRenderInfo renderInfo)
    {
        if (__instance is DiskScreenCardDisplayer)
        {
            CardStatIcons icons = __instance.gameObject.GetComponentInChildren<CardStatIcons>();

            if (icons == null)
                return;

            if (icons.attackIconRenderer != null)
            {
                icons.attackIconRenderer.gameObject.transform.localPosition = LEFT_ICON;
                icons.attackIconRenderer.gameObject.transform.localScale = ICON_SCALE;
                icons.attackIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
                icons.attackIconRenderer.material.color = ICON_COLOR;
                icons.attackIconRenderer.enabled = true;
            }

            if (icons.healthIconRenderer != null)
            {
                icons.healthIconRenderer.gameObject.transform.localPosition = RIGHT_ICON;
                icons.healthIconRenderer.gameObject.transform.localScale = ICON_SCALE;
                icons.healthIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
                icons.healthIconRenderer.material.color = ICON_COLOR;
                icons.healthIconRenderer.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(CardRenderCamera), "LiveRenderCard")]
    [HarmonyPrefix]
    private static void UpdateCamera(ref CardRenderCamera __instance, CardRenderInfo info)
    {
        if (SaveManager.SaveFile.IsPart3)
        {
            CardStatIcons icons = __instance.gameObject.GetComponentInChildren<CardStatIcons>();

            if (icons == null)
                return;

            if (icons.attackIconRenderer != null)
            {
                icons.attackIconRenderer.gameObject.transform.localPosition = RIGHT_ICON; // Flipped for some reason??
                icons.attackIconRenderer.gameObject.transform.localScale = ICON_SCALE;
                icons.attackIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
                icons.attackIconRenderer.material.color = ICON_COLOR;
                icons.attackIconRenderer.enabled = true;
            }

            if (icons.healthIconRenderer != null)
            {
                icons.healthIconRenderer.gameObject.transform.localPosition = LEFT_ICON; // Flipped for some reason??
                icons.healthIconRenderer.gameObject.transform.localScale = ICON_SCALE;
                icons.healthIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
                icons.healthIconRenderer.material.color = ICON_COLOR;
                icons.healthIconRenderer.enabled = true;
            }
        }
    }

    private static bool OpponentHasGem(GemType gem)
    {
        if (BoardManager.Instance == null || BoardManager.Instance.OpponentSlotsCopy == null)
            return false;

        Ability singleton = gem == GemType.Green ? Ability.GainGemGreen :
                            gem == GemType.Orange ? Ability.GainGemOrange :
                            Ability.GainGemBlue;

        return BoardManager.Instance.OpponentSlotsCopy.Any(slot => 
            slot.Card != null && (
                slot.Card.HasAbility(singleton) ||
                slot.Card.HasAbility(Ability.GainGemTriple)
            )
        );
    }

    [HarmonyPatch(typeof(DiskRenderStatsLayer), nameof(DiskRenderStatsLayer.ManagedUpdate))]
    [HarmonyPrefix]
    private static bool RenderOpposingGemification(DiskRenderStatsLayer __instance)
    {
        if (__instance.gemSquares[0].activeInHierarchy && GameFlowManager.IsCardBattle)
        {
            for (int i = 0; i < __instance.gemRenderers.Count; i++)
            {
                __instance.gemRenderers[i].GetPropertyBlock(__instance.gemsPropertyBlock);

                GemType target = (GemType)i;

                bool show = (__instance.PlayableCard.OpponentCard && OpponentHasGem(target)) ||
                            (!__instance.PlayableCard.OpponentCard && ResourcesManager.Instance.HasGem(target));
                
                __instance.gemsPropertyBlock.SetColor("_EmissionColor", show ? Color.white : Color.black);
                __instance.gemRenderers[i].SetPropertyBlock(__instance.gemsPropertyBlock);
            }
        }
        return false;
    }
}