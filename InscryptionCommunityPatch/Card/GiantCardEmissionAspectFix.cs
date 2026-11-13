using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class GiantCardEmissionAspectFix {
    /// <summary>
    /// Changes the emission render camera's aspect ratio for Giant cards so emission texture don't appear stretched.
    /// Aspect number taken from the base texture renderer's aspect ratio.
    /// </summary>
    [HarmonyPostfix, HarmonyPatch(typeof(CardRenderCamera), nameof(CardRenderCamera.TryCreateCameraForLiveRender))]
    private static void FixGiantEmissionCameraAspectRatio(CardRenderCamera __instance, RenderStatsLayer layer) {
        if (!SaveManager.SaveFile.IsPart1 || !__instance.liveRenderCameras.ContainsKey(layer) || layer is not RenderLiveStatsLayer live || !live.Giant) {
            return;
        }
        Transform emissionRenderCam = __instance.liveRenderCameras[layer].transform.Find("EmissionRenderCamera");
        emissionRenderCam.GetComponent<Camera>().aspect = 1.434f;
        emissionRenderCam.GetComponent<SetCameraAspect>().defaultAspect = 1.434f;
    }
}