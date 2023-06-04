using DiskCardGame;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes a few issues relating to Sentry interacting with certain Act 1 abilities / mechanics
[HarmonyPatch]
public class SentryInteractionFixes
{
    // Fixes a soft crash that occurs when the enemy totem tries to perform its check OnAssignedToSlot
    [HarmonyPatch(typeof(CardGainAbility), nameof(CardGainAbility.RespondsToOtherCardAssignedToSlot))]
    [HarmonyPrefix]
    private static bool CardGainAbilityNullCheckPatch(ref PlayableCard otherCard)
    {
        if (!otherCard.Dead && otherCard.Slot.IsPlayerSlot && Singleton<CardGainAbility>.Instance.RespondsToOtherCardDrawn(otherCard) && !otherCard.HasAbility(Singleton<TotemTriggerReceiver>.Instance.Data.bottom.effectParams.ability))
            return !otherCard.Info.Mods.Exists((CardModificationInfo x) => x.fromEvolve);

        return false;
    }

    // Override Sentry.FireAtOpposingSlot with the fixed version
    // Fixes Sentry: not triggering OnCardGettingAttacked freezing mid-animation
    [HarmonyPatch(typeof(Sentry), nameof(Sentry.FireAtOpposingSlot))]
    [HarmonyPrefix]
    private static bool FireAtOpposingSlotPatch(Sentry __instance, PlayableCard otherCard, ref IEnumerator __result)
    {
        __result = NewFireAtOpposingSlot(__instance, otherCard);
        return false;
    }

    // Override CombatPhaseManager.SlotAttackSlot with the fixed version
    // Fixes a bug caused by the opposing card dying mid-attack and becoming null

    // Fixes Sentry not triggering OnCardGettingAttacked and freezing
    public static IEnumerator NewFireAtOpposingSlot(Sentry instance, PlayableCard otherCard)
    {
        // Copy otherCard so we can change it later
        PlayableCard opposingCard = otherCard;

        // || instance.Card.Anim.Anim.speed == 0f
        // The above line of code prevents Sentry from activating multiple times during normal combat

        if (!(opposingCard != instance.lastShotCard) && Singleton<TurnManager>.Instance.TurnNumber == instance.lastShotTurn)
            yield break;

        bool midCombat = false;
        instance.lastShotCard = opposingCard;
        instance.lastShotTurn = Singleton<TurnManager>.Instance.TurnNumber;

        Singleton<ViewManager>.Instance.SwitchToView(View.Board, immediate: false, lockAfter: true);
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < instance.NumShots; i++)
        {
            if (opposingCard != null && !opposingCard.Dead)
            {
                yield return instance.PreSuccessfulTriggerSequence();

                // Check if the animation is paused then unpause it
                if (instance.Card.Anim.Anim.speed == 0f)
                {
                    if (PatchPlugin.configFullDebug.Value)
                        PatchPlugin.Logger.LogDebug($"{instance.Card} is frozen, unpausing animation.");

                    midCombat = true; // indicates that we need to restart the attack animation at the end of the sequence

                    ShowPart3Turret(instance.Card, opposingCard);

                    // Unpause the animation then wait for it to stop
                    instance.Card.Anim.Anim.speed = 1f;
                    yield return new WaitUntil(() => !instance.Card.Anim.DoingAttackAnimation);
                }
                else
                {
                    // vanilla sentry code
                    instance.Card.Anim.LightNegationEffect();
                    yield return new WaitForSeconds(0.5f);

                    ShowPart3Turret(instance.Card, opposingCard);

                    // Expand the attack animation to include a part for triggering CardGettingAttacked

                    instance.Card.Anim.PlayAttackAnimation(instance.Card.IsFlyingAttackingReach(), opposingCard.Slot, null);
                    yield return new WaitForSeconds(0.07f);
                    instance.Card.Anim.SetAnimationPaused(paused: true);

                    PlayableCard attackingCard = instance.Card;
                    yield return Singleton<GlobalTriggerHandler>.Instance.TriggerCardsOnBoard(Trigger.CardGettingAttacked, false, opposingCard);

                    opposingCard = UpdateOpposingCard(instance.Card, opposingCard);

                    if (attackingCard != null && attackingCard.Slot != null)
                    {
                        CardSlot attackingSlot = attackingCard.Slot;

                        if (opposingCard != null)
                        {
                            if (attackingSlot.Card.IsFlyingAttackingReach())
                            {
                                opposingCard.Anim.PlayJumpAnimation();
                                yield return new WaitForSeconds(0.3f);
                                attackingSlot.Card.Anim.PlayAttackInAirAnimation();
                            }
                            attackingSlot.Card.Anim.SetAnimationPaused(paused: false);
                            yield return new WaitForSeconds(0.05f);
                        }
                    }
                }

                if (opposingCard != null)
                    yield return opposingCard.TakeDamage(1, instance.Card);
            }
        }
        yield return instance.LearnAbility(0.5f);
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;

        // If otherCard isn't dead, restart the attack animation for when regular combat resumes
        if (midCombat && !otherCard.Dead)
        {
            instance.Card.Anim.PlayAttackAnimation(instance.Card.IsFlyingAttackingReach(), otherCard.Slot, null);
            yield return new WaitForSeconds(0.07f);
            instance.Card.Anim.SetAnimationPaused(paused: true);
        }
    }

    private static void ShowPart3Turret(PlayableCard card, PlayableCard otherCard)
    {
        // putting this code here because the main code's overwhelming enough to look at as is
        if (card.Anim is DiskCardAnimationController)
        {
            (card.Anim as DiskCardAnimationController).SetWeaponMesh(DiskCardWeapon.Turret);
            (card.Anim as DiskCardAnimationController).AimWeaponAnim(otherCard.Slot.transform.position);
            (card.Anim as DiskCardAnimationController).ShowWeaponAnim();
        }
    }
    private static PlayableCard UpdateOpposingCard(PlayableCard source, PlayableCard target)
    {
        // putting this code here because the main code's overwhelming enough to look at as is
        CardSlot opposingSlot = source.Slot.opposingSlot;
        if (opposingSlot.Card != null)
        {
            if (opposingSlot.Card != target)
                return opposingSlot.Card;

            return target;
        }
        return null;
    }
}