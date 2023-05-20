using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class GemsDrawFix
{
    [HarmonyPatch(typeof(GemsDraw), nameof(GemsDraw.OnOtherCardResolve))]
    [HarmonyPrefix]
    private static bool FixGemsDraw(GemsDraw __instance, ref IEnumerator __result)
    {
        __result = BetterGemsDraw(__instance);
        return false;
    }

    private static IEnumerator BetterGemsDraw(GemsDraw __instance)
    {
        yield return __instance.PreSuccessfulTriggerSequence();
        int numGems = Singleton<BoardManager>.Instance.PlayerSlotsCopy.FindAll(
            x => x.Card != null && x.Card.Info.HasTrait(Trait.Gem)).Count;

        if (numGems == 0)
        {
            yield return new WaitForSeconds(0.1f);
            __instance.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.45f);
            yield break;
        }

        Singleton<ViewManager>.Instance.SwitchToView(SaveManager.SaveFile.IsPart2 ? View.WizardBattleSlots : View.Hand);
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < numGems; i++)
        {
            if (SaveManager.SaveFile.IsPart2)
                yield return Singleton<CardDrawPiles>.Instance.DrawCardFromDeck();
            else
            {
                Singleton<CardDrawPiles3D>.Instance.Pile.Draw();
                yield return Singleton<CardDrawPiles3D>.Instance.DrawCardFromDeck();
            }
        }
        yield return __instance.LearnAbility(0.5f);
    }
}