using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class SquirrelOrbitFix
{
    [HarmonyPatch(typeof(SquirrelOrbit), nameof(SquirrelOrbit.OnUpkeep))]
    [HarmonyPrefix]
    private static bool FixSquirrelOrbit(SquirrelOrbit __instance, ref IEnumerator __result)
    {
        __result = BetterSquirrelOrbit(__instance);
        return false;
    }

    private static IEnumerator BetterSquirrelOrbit(SquirrelOrbit instance)
    {
        List<CardSlot> affectedSlots = new();

        if (instance.Card.HasTrait(Trait.Giant))
            affectedSlots = Singleton<BoardManager>.Instance.GetSlotsCopy(instance.Card.OpponentCard);
        else
            affectedSlots = Singleton<BoardManager>.Instance.AllSlotsCopy;

        affectedSlots.RemoveAll(x => !x.Card || !x.Card.IsAffectedByTidalLock());
        if (affectedSlots.Count == 0)
            yield break;

        instance.Card.Anim.LightNegationEffect();
        yield return new WaitForSeconds(0.2f);

        foreach (CardSlot slot in affectedSlots)
        {
            PlayableCard item = slot.Card;
            Singleton<ViewManager>.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.25f);
            yield return item.Die(false);
            yield return new WaitForSeconds(0.1f);

            if (instance.Card.HasTrait(Trait.Giant))
                Singleton<ViewManager>.Instance.SwitchToView(View.OpponentQueue);

            yield return new WaitForSeconds(0.1f);

            if (instance.Card.HasSpecialAbility(SpecialTriggeredAbility.GiantMoon))
            {
                instance.FindMoonPortrait();
                instance.moonPortrait.InstantiateOrbitingObject(item.Info);
            }

            if (instance.HasLearned)
                yield return new WaitForSeconds(0.5f);
            else
            {
                yield return new WaitForSeconds(1f);
                yield return instance.LearnAbility();
            }
        }
    }
}