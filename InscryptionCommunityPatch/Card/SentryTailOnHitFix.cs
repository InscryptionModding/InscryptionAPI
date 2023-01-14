using DiskCardGame;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes a number of issues relating to Sentry interacting with Act 1-exclusive abilities
// A lot of these issues are interwoven with core parts of the game (because of course they are, why would it be easy)
// Currently fixes bugs relating to:
// PackMule, TailOnHit
[HarmonyPatch]
public class SentryTailOnHitFix
{
    // Override TailOnHit.OnCardGettingAttacked with fixed version
    [HarmonyPatch(typeof(TailOnHit), nameof(TailOnHit.OnCardGettingAttacked))]
    [HarmonyPrefix]
    private static bool TailOnHitGettingAttackedPatch(TailOnHit __instance, ref IEnumerator __result)
    {
        __result = NewOnCardGettingAttacked(__instance);
        return false;
    }

    // Fixes visual and gameplay bugs relating to TailOnHit's sequence
    // Firstly prevents TailOnHit from activating multiple times in a single turn (no back-to-back shimmying between Sentries)
    // Also negates a couple visual bugs relating to the cards (both base and tail) reappearing as vengeful spirits to do a quick shake before popping away again
    public static IEnumerator NewOnCardGettingAttacked(TailOnHit instance)
    {
        CardSlot slot = instance.Card.Slot;
        CardSlot toLeft = Singleton<BoardManager>.Instance.GetAdjacent(instance.Card.Slot, adjacentOnLeft: true);
        CardSlot toRight = Singleton<BoardManager>.Instance.GetAdjacent(instance.Card.Slot, adjacentOnLeft: false);
        bool num = toLeft != null && toLeft.Card == null;
        bool toRightValid = toRight != null && toRight.Card == null;
        if (num || toRightValid)
        {
            yield return instance.PreSuccessfulTriggerSequence();
            yield return new WaitForSeconds(0.2f);

            // Set lostTail to True before moving
            // This prevents TailOnHit from activating twice, which creates problems
            instance.SetTailLost(lost: true);

            if (toRightValid)
                yield return Singleton<BoardManager>.Instance.AssignCardToSlot(instance.Card, toRight);
            else
                yield return Singleton<BoardManager>.Instance.AssignCardToSlot(instance.Card, toLeft);

            // No point in doing all this if the card's dead (visual glitch primarily, no major softlocks)
            if (!instance.Card.Dead)
            {
                instance.Card.Anim.StrongNegationEffect();
                instance.Card.Status.hiddenAbilities.Add(instance.Ability);
                instance.Card.RenderCard();
            }

            yield return new WaitForSeconds(0.2f);
            CardInfo info = ((instance.Card.Info.tailParams == null) ? TailParams.GetDefaultTail(instance.Card.Info) : (instance.Card.Info.tailParams.tail.Clone() as CardInfo));
            PlayableCard tail = CardSpawner.SpawnPlayableCardWithCopiedMods(info, instance.Card, Ability.TailOnHit);
            tail.transform.position = slot.transform.position + Vector3.back * 2f + Vector3.up * 2f;
            tail.transform.rotation = Quaternion.Euler(110f, 90f, 90f);
            yield return Singleton<BoardManager>.Instance.ResolveCardOnBoard(tail, slot);
            Singleton<ViewManager>.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.2f);

            // a simple isDead check to prevent the tail's ghost from being summoned
            if (!tail.Dead)
                tail.Anim.StrongNegationEffect();

            yield return instance.StartCoroutine(instance.LearnAbility(0.5f));
            yield return new WaitForSeconds(0.2f);
        }
    }
}