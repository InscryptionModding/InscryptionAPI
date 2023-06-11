using DiskCardGame;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class Act2HodagFix
{
    [HarmonyPatch(typeof(GainAttackOnKill), nameof(GainAttackOnKill.OnOtherCardDie))]
    [HarmonyPostfix]
    private static IEnumerator MakeHodagWorkInPixels(IEnumerator enumerator, GainAttackOnKill __instance)
    {
        if (!SaveManager.SaveFile.IsPart2 || !__instance.PermanentForRun)
        {
            yield return enumerator;
            yield break;
        }

        // just copy-pasting vanilla code cause i'm lazy, whoops
        yield return __instance.PreSuccessfulTriggerSequence();
        yield return new WaitForSeconds(0.3f);

        CardModificationInfo cardModificationInfo = __instance.Card.Info.Mods.Find((CardModificationInfo x) => x.singletonId == "hodag");
        if (cardModificationInfo == null)
        {
            cardModificationInfo = new()
            {
                singletonId = "hodag"
            };
            SaveManager.SaveFile.gbcData.deck.ModifyCard(__instance.Card.Info, cardModificationInfo);
        }
        cardModificationInfo.attackAdjustment++;

        if (!__instance.Card.Dead)
        {
            __instance.Card.Anim.LightNegationEffect();
            yield return new WaitForSeconds(0.3f);
            yield return __instance.LearnAbility();
        }
    }
}