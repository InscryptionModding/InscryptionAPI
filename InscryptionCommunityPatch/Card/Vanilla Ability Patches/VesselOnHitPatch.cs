using DiskCardGame;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class VesselOnHitPatch
{
    [HarmonyPatch(typeof(DrawVesselOnHit), nameof(DrawVesselOnHit.RespondsToTakeDamage))]
    [HarmonyPrefix]
    private static bool NewTrigger(ref bool __result)
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            __result = true;
            return false;
        }
        return true;
    }
    [HarmonyPatch(typeof(DrawVesselOnHit), nameof(DrawVesselOnHit.OnTakeDamage))]
    [HarmonyPostfix]
    private static IEnumerator WorkInAct2(IEnumerator enumerator, DrawVesselOnHit __instance)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            yield return enumerator;
            yield break;
        }

        yield return __instance.PreSuccessfulTriggerSequence();
        __instance.Card.Anim.StrongNegationEffect();
        yield return new WaitForSeconds(0.4f);
        yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName("EmptyVessel"));
        yield return __instance.LearnAbility(0.5f);
    }
}