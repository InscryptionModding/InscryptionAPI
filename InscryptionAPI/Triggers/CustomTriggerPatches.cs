using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Triggers
{
    [HarmonyPatch]
    internal static class CustomTriggerPatches
    {
        [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.AddCardToHand))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnAddedToHand(IEnumerator result, PlayableCard card)
        {
            yield return result;
            foreach (var onCard in CustomTriggerFinder.FindTriggersOnCard<IAddedToHand>(card))
            {
                yield return onCard.OnAddedToHand();
            }
            foreach (var otherCard in CustomTriggerFinder.FindGlobalTriggers<IOtherAddedToHand>(TriggerSearchCategory.ALL, card))
            {
                yield return otherCard.OnOtherAddedToHand(card);
            }
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.DoCombatPhase))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnBellRung(IEnumerator result, bool playerIsAttacker)
        {
            foreach (var i in CustomTriggerFinder.FindGlobalTriggers<IBellRung>())
            {
                yield return i.OnBellRung(playerIsAttacker);
            }
            yield return result;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSequence))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnSlotAttackSequence(IEnumerator result, CardSlot slot)
        {
            foreach (var pre in CustomTriggerFinder.FindGlobalTriggers<IPreSlotAttackSequence>())
            {
                yield return pre.OnPreSlotAttackSequence(slot);
            }
            yield return result;
            foreach (var post in CustomTriggerFinder.FindGlobalTriggers<IPostSlotAttackSequence>())
            {
                yield return post.OnPostSlotAttackSequence(slot);
            }
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnPostSingularSlotAttackSlot(IEnumerator result, CardSlot attackingSlot, CardSlot opposingSlot)
        {
            yield return result;
            foreach (var i in CustomTriggerFinder.FindGlobalTriggers<IPostSingularSlotAttackSlot>())
            {
                yield return i.OnPostSingularSlotAttackSlot(attackingSlot, opposingSlot);
            }
        }

        [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnScalesChanged(IEnumerator result, int damage, bool toPlayer)
        {
            foreach (var pre in CustomTriggerFinder.FindGlobalTriggers<IPreScalesChanged>())
            {
                yield return pre.OnPreScalesChanged(damage, toPlayer);
            }
            yield return result;
            foreach (var post in CustomTriggerFinder.FindGlobalTriggers<IPostScalesChanged>())
            {
                yield return post.OnPostScalesChanged(damage, toPlayer);
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnUpkeepInHand(IEnumerator result, bool playerUpkeep)
        {
            yield return result;
            foreach (var i in CustomTriggerFinder.FindTriggersInHand<IUpkeepInHand>())
            {
                yield return i.OnUpkeepInHand(playerUpkeep);
            }
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnOtherCardResolveInHand(IEnumerator result, PlayableCard card, bool resolveTriggers)
        {
            yield return result;
            if (resolveTriggers)
            {
                
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardResolveInHand, card);
            }
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlayerTurn))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnTurnEndInHandPlayer(IEnumerator result)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnTurnEndInHand, true);
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.OpponentTurn))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnTurnEndInHandOpponent(IEnumerator result, TurnManager __instance)
        {
            bool turnSkipped = __instance.Opponent.SkipNextTurn;
            yield return result;
            if (!turnSkipped)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnTurnEndInHand, false);
            }
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnOtherCardAssignedToSlotInHand(IEnumerator result, PlayableCard card, bool resolveTriggers)
        {
            CardSlot slot2 = card.Slot;
            yield return result;
            if (resolveTriggers && slot2 != card.Slot)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardAssignedToSlotInHand, card);
            }
            if (resolveTriggers && slot2 != card.Slot && slot2 != null)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnCardAssignedToSlotNoResolve, card);
            }
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Die))]
        [HarmonyPostfix]
        private static IEnumerator TriggerDeathTriggers(IEnumerator result, PlayableCard __instance, bool wasSacrifice, PlayableCard killer = null)
        {
            CardSlot slotBeforeDeath = __instance.Slot;
            while (result.MoveNext())
            {
                yield return result.Current;
                if (result.Current.GetType() == triggerType)
                {
                    Trigger t = Trigger.None;
                    try
                    {
                        t = (Trigger)result.Current.GetType().GetField("trigger").GetValue(result.Current);
                    }
                    catch { }
                    if (t == Trigger.OtherCardPreDeath)
                    {
                        yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardPreDeathInHand, slotBeforeDeath, !wasSacrifice, killer);
                    }
                    else if (t == Trigger.OtherCardDie)
                    {
                        yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardDieInHand, __instance, slotBeforeDeath, !wasSacrifice, killer);
                    }
                }
            }
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnTurnEndInHandPlayer(IEnumerator result, PlayableCard __instance, PlayableCard attacker)
        {
            bool hasshield = __instance.HasShield();
            yield return result;
            if (!hasshield && attacker != null)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardDealtDamageInHand, attacker, attacker.Attack, __instance);
            }
            yield break;
        }

        static Type triggerType = AccessTools.TypeByName("DiskCardGame.GlobalTriggerHandler+<TriggerCardsOnBoard>d__16");
    }
}
