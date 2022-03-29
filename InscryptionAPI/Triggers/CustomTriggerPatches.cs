using DiskCardGame;
using System.Collections;
using HarmonyLib;

namespace InscryptionAPI.Triggers
{
    [HarmonyPatch]
    static class CustomTriggerPatches
    {
        [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.AddCardToHand))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnAddedToHand(IEnumerator result, PlayableCard card)
        {
            yield return result;
            if (card.TriggerHandler.RespondsToCustomTrigger(CustomTrigger.OnAddedToHand, Array.Empty<object>()))
            {
                yield return card.TriggerHandler.OnCustomTrigger(CustomTrigger.OnAddedToHand, Array.Empty<object>());
            }
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnOtherCardAddedToHand, false, card);
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.DoCombatPhase))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnBellRung(IEnumerator result, bool playerIsAttacker)
        {
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnBellRung, false, playerIsAttacker);
            yield return result;
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSequence))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnSlotAttackSequence(IEnumerator result, CardSlot slot)
        {
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPreSlotAttackSequence, false, slot);
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPostSlotAttackSequence, false, slot);
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnPostSingularSlotAttackSlot(IEnumerator result, CardSlot attackingSlot, CardSlot opposingSlot)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPostSingularSlotAttackSlot, false, attackingSlot, opposingSlot);
            yield break;
        }

        [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnScalesChanged(IEnumerator result, int damage, bool toPlayer)
        {
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPreScalesChanged, false, damage, toPlayer);
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPostScalesChanged, false, damage, toPlayer);
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnUpkeepInHand(IEnumerator result, bool playerUpkeep)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnUpkeepInHand, playerUpkeep);
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnOtherCardResolveInHand(IEnumerator result, PlayableCard card, bool resolveTriggers = true)
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

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
        [HarmonyPostfix]
        private static void UpdateOpposingSlots(ref PlayableCard __instance, ref List<CardSlot> __result)
        {   
            List<AttackModifier> attacks = __instance.TriggerHandler.GetDataFromCustomTrigger<AttackModifier>(CustomTrigger.OnGetOpposingSlots);
            if (attacks == null)
                return;

            bool isAttackingDefaultSlot = !__instance.HasTriStrike() && !__instance.HasAbility(Ability.SplitStrike);
            CardSlot defaultslot = __instance.Slot.opposingSlot;

            List<CardSlot> alteredOpposings = new List<CardSlot>();
            bool removeDefaultAttackSlot = false;

            foreach (AttackModifier modifier in attacks.Where(a => a != null))
            {
                removeDefaultAttackSlot |= modifier.shouldRemoveOriginalAttackSlot;

                if (modifier.slotsToAttack != null)
                    alteredOpposings.AddRange(modifier.slotsToAttack);
            }
            
            if (alteredOpposings.Count > 0) 
                __result.AddRange(alteredOpposings);

            if (isAttackingDefaultSlot && removeDefaultAttackSlot)
                __result.Remove(defaultslot);
        }

        // TODO: Uncomment these when we get the passive fix done
        //[HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
        //[HarmonyPostfix]
        private static void AddPassiveAttackBuffs(ref PlayableCard __instance, ref int __result)
        {
            if (__instance.slot == null)
                return;

            List<int> buffs = CustomGlobalTriggerHandler.GetDataFromAllCardsOnBoard<int>(CustomTrigger.OnBuffOtherCardAttack, false, __instance);
            if (buffs != null)
                __result += buffs.Sum();
        }

        // TODO: Uncomment these when we get the passive fix done
        //[HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveHealthBuffs))]
        //[HarmonyPostfix]
        private static void AddPassiveHealthBuffs(ref PlayableCard __instance, ref int __result)
        {
            if (__instance.slot == null)
                return;

            List<int> buffs = CustomGlobalTriggerHandler.GetDataFromAllCardsOnBoard<int>(CustomTrigger.OnBuffOtherCardHealth, false, __instance);
            if (buffs != null)
                __result += buffs.Sum();
        }
    }
}
