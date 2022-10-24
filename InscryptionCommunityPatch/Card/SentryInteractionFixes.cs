using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using Pixelplacement;
using Pixelplacement.TweenSystem;

namespace InscryptionCommunityPatch.Card;

// Fixes a few issues relating to Sentry interacting with certain Act 1 abilities / mechanics
[HarmonyPatch]
public class SentryInteractionFixes
{
    // Fixes a soft crash that occurs when the enemy totem tries to perform its check OnAssignedToSlot
    [HarmonyPatch(typeof(CardGainAbility), nameof(CardGainAbility.RespondsToOtherCardAssignedToSlot))]
    [HarmonyPrefix]
    public static bool CardGainAbilityNullCheckPatch(ref PlayableCard otherCard)
    {
        if (!otherCard.Dead && otherCard.Slot.IsPlayerSlot && Singleton<CardGainAbility>.Instance.RespondsToOtherCardDrawn(otherCard) && !otherCard.HasAbility(Singleton<TotemTriggerReceiver>.Instance.Data.bottom.effectParams.ability))
            return !otherCard.Info.Mods.Exists((CardModificationInfo x) => x.fromEvolve);

        return false;
    }

    // Override Sentry.FireAtOpposingSlot with the fixed version
    // Fixes Sentry: not triggering OnCardGettingAttacked freezing mid-animation
    [HarmonyPatch(typeof(Sentry), nameof(Sentry.FireAtOpposingSlot))]
    [HarmonyPrefix]
    public static bool FireAtOpposingSlotPatch(Sentry __instance, PlayableCard otherCard, ref IEnumerator __result)
    {
        __result = NewFireAtOpposingSlot(__instance, otherCard);
        return false;
    }

    // COMMENTED OUT UNTIL A BETTER FIX CAN BE MADE
    // Override CombatPhaseManager.SlotAttackSlot with the fixed version
    // Fixes a bug caused by the opposing card dying mid-attack and becoming null
    // [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
    // [HarmonyPrefix]
    // public static bool SlotAttackSlotPatch(CombatPhaseManager __instance, CardSlot attackingSlot, CardSlot opposingSlot, float waitAfter, ref IEnumerator __result)
    // {
    //     __result = NewSlotAttackSlot(__instance, attackingSlot, opposingSlot, waitAfter);
    //     return false;
    // }

    // Fixes Sentry not triggering OnCardGettingAttacked and freezing
    private static IEnumerator NewFireAtOpposingSlot(Sentry instance, PlayableCard otherCard)
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
                    PatchPlugin.Logger.LogDebug($"{instance.Card} is frozen, unpausing animation.");
                    // indicates that we need to restart the attack animation at the end of the sequence
                    midCombat = true;

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

    // Fixes a bug caused by the opposing card dying mid-attack and becoming null
    private static IEnumerator NewSlotAttackSlot(CombatPhaseManager instance, CardSlot attackingSlot, CardSlot opposingSlot, float waitAfter)
    {
        yield return Singleton<GlobalTriggerHandler>.Instance.TriggerCardsOnBoard(Trigger.SlotTargetedForAttack, false, opposingSlot, attackingSlot.Card);
        yield return new WaitForSeconds(0.025f);
        if (!(attackingSlot.Card != null))
            yield break;

        if (attackingSlot.Card.Anim.DoingAttackAnimation)
        {
            yield return new WaitUntil(() => !attackingSlot.Card.Anim.DoingAttackAnimation);
            yield return new WaitForSeconds(0.25f);
        }
        if (opposingSlot.Card != null && attackingSlot.Card.AttackIsBlocked(opposingSlot))
        {
            ProgressionData.SetAbilityLearned(Ability.PreventAttack);
            yield return instance.ShowCardBlocked(attackingSlot.Card);
        }
        else if (attackingSlot.Card.CanAttackDirectly(opposingSlot))
        {
            instance.DamageDealtThisPhase += attackingSlot.Card.Attack;
            yield return instance.VisualizeCardAttackingDirectly(attackingSlot, opposingSlot, attackingSlot.Card.Attack);
            if (attackingSlot.Card.TriggerHandler.RespondsToTrigger(Trigger.DealDamageDirectly, attackingSlot.Card.Attack))
                yield return attackingSlot.Card.TriggerHandler.OnTrigger(Trigger.DealDamageDirectly, attackingSlot.Card.Attack);

        }
        else
        {
            float heightOffset = ((opposingSlot.Card == null) ? 0f : opposingSlot.Card.SlotHeightOffset);
            if (heightOffset > 0f)
                Tween.Position(attackingSlot.Card.transform, attackingSlot.Card.transform.position + Vector3.up * heightOffset, 0.05f, 0f, Tween.EaseInOut);

            // Start attack animation
            attackingSlot.Card.Anim.PlayAttackAnimation(attackingSlot.Card.IsFlyingAttackingReach(), opposingSlot, null);
            yield return new WaitForSeconds(0.07f);
            attackingSlot.Card.Anim.SetAnimationPaused(paused: true);

            // Trigger CardGettingAttacked
            PlayableCard attackingCard = attackingSlot.Card;
            yield return Singleton<GlobalTriggerHandler>.Instance.TriggerCardsOnBoard(Trigger.CardGettingAttacked, false, opposingSlot.Card);

            // If attacking card and its slot aren't null
            if (attackingCard != null && attackingCard.Slot != null)
            {
                CardSlot attackingSlot2 = attackingCard.Slot;

                // If the opposing card is still alive, attack it
                if (opposingSlot.Card != null)
                {
                    if (attackingSlot2.Card.IsFlyingAttackingReach())
                    {
                        opposingSlot.Card.Anim.PlayJumpAnimation();
                        yield return new WaitForSeconds(0.3f);
                        attackingSlot2.Card.Anim.PlayAttackInAirAnimation();
                    }
                    attackingSlot2.Card.Anim.SetAnimationPaused(paused: false);
                    yield return new WaitForSeconds(0.05f);
                    int overkillDamage = attackingSlot2.Card.Attack - opposingSlot.Card.Health;
                    yield return opposingSlot.Card.TakeDamage(attackingSlot2.Card.Attack, attackingSlot2.Card);
                    yield return instance.DealOverkillDamage(overkillDamage, attackingSlot2, opposingSlot);
                }

                if (attackingSlot2.Card != null && heightOffset > 0f)
                    yield return Singleton<BoardManager>.Instance.AssignCardToSlot(attackingSlot2.Card, attackingSlot2.Card.Slot, 0.1f, null, resolveTriggers: false);
            }
        }
        yield return new WaitForSeconds(waitAfter);
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
