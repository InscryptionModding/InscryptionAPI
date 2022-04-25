using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal static class Part3StatIcons
{
    [HarmonyPatch(typeof(DiskCardGame.Card), "RenderCard")]
    [HarmonyPrefix]
    private static void UpdateLiveRenderedCard(ref DiskCardGame.Card __instance)
    {
        if (SaveManager.SaveFile.IsPart3)
        {
            CardStatIcons icons = __instance.gameObject.GetComponentInChildren<CardStatIcons>();
            icons.attackIconRenderer.gameObject.transform.localPosition = new(0.39f, 0.033f, 0f);
            icons.attackIconRenderer.gameObject.transform.localScale = new (0.2f, 0.2f, 1f);
            icons.attackIconRenderer.material.color = GameColors.Instance.brightBlue;
            icons.attackIconRenderer.enabled = false;

            icons.healthIconRenderer.gameObject.transform.localPosition = new(-0.39f, 0.033f, 0f);
            icons.healthIconRenderer.gameObject.transform.localScale = new (0.2f, 0.2f, 1f);
            icons.healthIconRenderer.material.color = GameColors.Instance.brightBlue;
            icons.healthIconRenderer.enabled = false;
        }
    }

    [HarmonyPatch(typeof(CardDisplayer3D), "DisplayInfo")]
    [HarmonyPrefix]
    private static void UpdateCardDisplayer(ref CardDisplayer3D __instance, CardRenderInfo renderInfo)
    {
        if (__instance is DiskScreenCardDisplayer)
        {
            CardStatIcons icons = __instance.gameObject.GetComponentInChildren<CardStatIcons>();
            icons.attackIconRenderer.gameObject.transform.localPosition = new(-0.39f, 0.033f, 0f);
            icons.attackIconRenderer.gameObject.transform.localScale = new (0.2f, 0.2f, 1f);
            icons.attackIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
            icons.attackIconRenderer.material.color = GameColors.Instance.brightBlue;
            icons.attackIconRenderer.enabled = true;

            icons.healthIconRenderer.gameObject.transform.localPosition = new(0.39f, 0.033f, 0f);
            icons.healthIconRenderer.gameObject.transform.localScale = new (0.2f, 0.2f, 1f);
            icons.healthIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
            icons.healthIconRenderer.material.color = GameColors.Instance.brightBlue;
            icons.healthIconRenderer.enabled = true;
        }
    }

    [HarmonyPatch(typeof(CardRenderCamera), "LiveRenderCard")]
    [HarmonyPrefix]
    private static void UpdateCamera(ref CardRenderCamera __instance, CardRenderInfo info)
    {
        if (SaveManager.SaveFile.IsPart3)
        {
            CardStatIcons icons = __instance.gameObject.GetComponentInChildren<CardStatIcons>();
            icons.attackIconRenderer.gameObject.transform.localPosition = new(0.39f, 0.033f, 0f);
            icons.attackIconRenderer.gameObject.transform.localScale = new (0.2f, 0.2f, 1f);
            icons.attackIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
            icons.attackIconRenderer.material.color = GameColors.Instance.brightBlue;
            icons.attackIconRenderer.enabled = true;

            icons.healthIconRenderer.gameObject.transform.localPosition = new(-0.39f, 0.033f, 0f);
            icons.healthIconRenderer.gameObject.transform.localScale = new (0.2f, 0.2f, 1f);
            icons.healthIconRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
            icons.healthIconRenderer.material.color = GameColors.Instance.brightBlue;
            icons.healthIconRenderer.enabled = true;
        }
    }
}