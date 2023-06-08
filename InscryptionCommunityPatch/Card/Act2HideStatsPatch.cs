using DiskCardGame;
using GBC;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class Act2HideStatsPatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(PixelCardDisplayer), nameof(PixelCardDisplayer.DisplayInfo))]
    private static void HideAttackAndHealth(PixelCardDisplayer __instance, CardRenderInfo renderInfo)
    {
        __instance.SetAttackHidden(__instance.info.hideAttackAndHealth || renderInfo.hiddenAttack);
        __instance.SetHealthHidden(__instance.info.hideAttackAndHealth || renderInfo.hiddenHealth);
    }
}