using DiskCardGame;
using GBC;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InscryptionCommunityPatch.Card;
[HarmonyPatch]
internal class DisableTalkingCardRenderingAct2 {
    [HarmonyPrefix, HarmonyPatch(typeof(TalkingCard), nameof(TalkingCard.Start))]
    private static bool DisableRenderIfPixelCard(TalkingCard __instance) {
        if (CardRenderCamera.Instance == null) {
            return false;
        }
        return true;
    }
}
