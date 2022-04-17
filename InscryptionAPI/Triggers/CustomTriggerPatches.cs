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
            yield return card.TriggerHandler.Trigger<IOnAddedToHand>(x => x.RespondsToAddedToHand(), x => x.OnAddedToHand());
            yield return CustomGlobalTriggerHandler.TriggerAll<IOnOtherCardAddedToHand>(false, x => x.RespondsToOtherCardAddedToHand(card), x => x.OnOtherCardAddedToHand(card));
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.DoCombatPhase))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnBellRung(IEnumerator result, bool playerIsAttacker)
        {
            yield return CustomGlobalTriggerHandler.TriggerAll<IOnBellRung>(false, x => x.RespondsToBellRung(playerIsAttacker), x => x.OnBellRung(playerIsAttacker));
            yield return result;
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSequence))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnSlotAttackSequence(IEnumerator result, CardSlot slot)
        {
            yield return CustomGlobalTriggerHandler.TriggerAll<IOnPreSlotAttackSequence>(false, x => x.RespondsToPreSlotAttackSequence(slot), x => x.OnPreSlotAttackSequence(slot));
            yield return result;
            yield return CustomGlobalTriggerHandler.TriggerAll<IOnPostSlotAttackSequence>(false, x => x.RespondsToPostSlotAttackSequence(slot), x => x.OnPostSlotAttackSequence(slot));
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnPostSingularSlotAttackSlot(IEnumerator result, CardSlot attackingSlot, CardSlot opposingSlot)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.TriggerAll<IOnPostSingularSlotAttackSlot>(false, x => x.RespondsToPostSingularSlotAttackSlot(attackingSlot, opposingSlot), x =>
                x.OnPostSingularSlotAttackSlot(attackingSlot, opposingSlot));
            yield break;
        }

        private static Type scaleChangedCoroutine;
        private static FieldInfo scaleChangedDamage;
        private static FieldInfo scaleChangedToPlayer;
        private static FieldInfo scaleChangedNumWeights;

        [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnScalesChanged(IEnumerator result, int damage, int numWeights, bool toPlayer)
        {
            int initialDamage = damage;
            bool initialToPlayer = toPlayer;
            CustomGlobalTriggerHandler.CollectDataAll<IOnPreScalesChangedRef, int>(false, x => x.RespondsToPreScalesChangedRef(damage, numWeights, toPlayer), x =>
            {
                damage = x.CollectPreScalesChangedRef(damage, ref numWeights, ref toPlayer);
                if (damage < 0)
                {
                    damage = -damage;
                    toPlayer = !toPlayer;
                }
                return damage;
            });
            yield return CustomGlobalTriggerHandler.TriggerAll<IOnPreScalesChanged>(false, x => x.RespondsToPreScalesChanged(damage, toPlayer), x => x.OnPreScalesChanged(damage, toPlayer));
            if (damage != 0)
            {
                (scaleChangedDamage ??= (scaleChangedCoroutine ??= result?.GetType())?.GetField("damage"))?.SetValue(result, damage);
                (scaleChangedToPlayer ??= (scaleChangedCoroutine ??= result?.GetType())?.GetField("toPlayer"))?.SetValue(result, toPlayer);
                (scaleChangedNumWeights ??= (scaleChangedCoroutine ??= result?.GetType())?.GetField("numWeights"))?.SetValue(result, numWeights);
                yield return result;
            }
            yield return CustomGlobalTriggerHandler.TriggerAll<IOnPostScalesChanged>(false, x => x.RespondsToPostScalesChanged(damage, toPlayer, initialDamage, initialToPlayer), x =>
                x.OnPostScalesChanged(damage, toPlayer, initialDamage, initialToPlayer));
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnUpkeepInHand(IEnumerator result, bool playerUpkeep)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.TriggerInHand<IOnUpkeepInHand>(x => x.RespondsToUpkeepInHand(playerUpkeep), x => x.OnUpkeepInHand(playerUpkeep));
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnOtherCardResolveInHand(IEnumerator result, PlayableCard card, bool resolveTriggers = true)
        {
            yield return result;
            if (resolveTriggers)
            {
                yield return CustomGlobalTriggerHandler.TriggerInHand<IOnOtherCardResolveInHand>(x => x.RespondsToOtherCardResolveInHand(card), x => x.OnOtherCardResolveInHand(card));
            }
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlayerTurn))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnTurnEndInHandPlayer(IEnumerator result)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.TriggerInHand<IOnTurnEndInHand>(x => x.RespondsToTurnEndInHand(true), x => x.OnTurnEndInHand(true));
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
                yield return CustomGlobalTriggerHandler.TriggerInHand<IOnTurnEndInHand>(x => x.RespondsToTurnEndInHand(false), x => x.OnTurnEndInHand(false));
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
                yield return CustomGlobalTriggerHandler.TriggerAll<IOnCardAssignedToSlotContext>(false, x => x.RespondsToCardAssignedToSlotContext(card, slot2, card.Slot), x =>
                    x.OnCardAssignedToSlotContext(card, slot2, card.Slot));
            }
            if (resolveTriggers && slot2 != card.Slot)
            {
                yield return CustomGlobalTriggerHandler.TriggerInHand<IOnOtherCardAssignedToSlotInHand>(x => x.RespondsToOtherCardAssignedToSlotInHand(card), x => x.OnOtherCardAssignedToSlotInHand(card));
            }
            if (resolveTriggers && slot2 != card.Slot && slot2 != null)
            {
                yield return CustomGlobalTriggerHandler.TriggerAll<IOnCardAssignedToSlotNoResolve>(false, x => x.RespondsToCardAssignedToSlotNoResolve(card), x => x.OnCardAssignedToSlotNoResolve(card));
            }
            yield break;
        }

        private static FieldInfo triggerField;

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
                        t = (Trigger)(triggerField ??= triggerType.GetField("trigger")).GetValue(result.Current);
                    }
                    catch { }
                    if (t == Trigger.OtherCardPreDeath)
                    {
                        yield return CustomGlobalTriggerHandler.TriggerInHand<IOnOtherCardPreDeathInHand>(x => x.RespondsToOtherCardPreDeathInHand(slotBeforeDeath, !wasSacrifice, killer), x =>
                            x.OnOtherCardPreDeathInHand(slotBeforeDeath, !wasSacrifice, killer));
                    }
                    else if (t == Trigger.OtherCardDie)
                    {
                        yield return CustomGlobalTriggerHandler.TriggerInHand<IOnOtherCardDieInHand>(x => x.RespondsToOtherCardDieInHand(__instance, slotBeforeDeath, !wasSacrifice, killer), x =>
                            x.OnOtherCardDieInHand(__instance, slotBeforeDeath, !wasSacrifice, killer));
                    }
                }
            }
            yield break;
        }

        private static Type takeDamageCoroutine;
        private static FieldInfo takeDamageDamage;

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
        [HarmonyPostfix]
        private static IEnumerator TriggerOnTurnEndInHandPlayer(IEnumerator result, PlayableCard __instance, int damage, PlayableCard attacker)
        {
            CustomGlobalTriggerHandler.CollectDataAll<ICardTakenDamageModifier, int>(true, x => x.RespondsToCardTakenDamageModifier(__instance, damage), x => damage = x.CollectCardTakenDamageModifier(__instance, damage));
            if (damage != 0)
            {
                (takeDamageDamage ??= (takeDamageCoroutine ??= result?.GetType())?.GetField("damage"))?.SetValue(result, damage);
                bool hasshield = __instance.HasShield();
                yield return result;
                if (!hasshield && attacker != null)
                {
                    yield return CustomGlobalTriggerHandler.TriggerInHand<IOnOtherCardDealtDamageInHand>(x => x.RespondsToOtherCardDealtDamageInHand(attacker, attacker.Attack, __instance),
                        x => x.OnOtherCardDealtDamageInHand(attacker, attacker.Attack, __instance));
                }
            }
            yield break;
        }

        [HarmonyPatch(typeof(ConsumableItemSlot), nameof(ConsumableItemSlot.ConsumeItem))]
        [HarmonyPostfix]
        private static IEnumerator TriggerItemUse(IEnumerator result, ConsumableItemSlot __instance)
        {
            bool itemCanBeUsed = true;
            string consumableName = __instance?.Consumable?.Data?.name;
            if (!string.IsNullOrEmpty(consumableName))
            {
                CustomGlobalTriggerHandler.CollectDataAll<IItemCanBeUsed, bool>(false, x => x.RespondsToItemCanBeUsed(consumableName, itemCanBeUsed), x => itemCanBeUsed = x.CollectItemCanBeUsed(consumableName, itemCanBeUsed));
            }
            if (itemCanBeUsed)
            {
                bool successInActivation = false;
                Type activationtype = __instance.Consumable.ActivateSequence().GetType();
                if (!string.IsNullOrEmpty(consumableName))
                {
                    yield return CustomGlobalTriggerHandler.TriggerAll<IOnPreItemUsed>(false, x => x.RespondsToPreItemUsed(consumableName, __instance is HammerItemSlot), x =>
                        x.OnPreItemUsed(consumableName, __instance is HammerItemSlot));
                }
                while (result.MoveNext())
                {
                    yield return result.Current;
                    if (result.Current.GetType() == activationtype)
                    {
                        if (!string.IsNullOrEmpty(consumableName) && __instance.Consumable != null)
                        {
                            successInActivation = !__instance.Consumable.ActivationCancelled;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(consumableName))
                {
                    yield return CustomGlobalTriggerHandler.TriggerAll<IOnPostItemUsed>(false, x => x.RespondsToPostItemUsed(consumableName, successInActivation, __instance is HammerItemSlot), x =>
                        x.OnPostItemUsed(consumableName, successInActivation, __instance is HammerItemSlot));
                }
            }
            else
            {
                __instance?.Consumable?.PlayShakeAnimation();
                if (!string.IsNullOrEmpty(consumableName))
                {
                    yield return CustomGlobalTriggerHandler.TriggerAll<IOnItemPreventedFromUse>(false, x => x.RespondsToItemPreventedFromUse(consumableName), x => x.OnItemPreventedFromUse(consumableName));
                }
            }
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
        [HarmonyPostfix]
        private static void PassiveAttackBuffs(PlayableCard __instance, ref int __result)
        {
            int dummyResult = __result;
            CustomGlobalTriggerHandler.CollectDataAll<IOnCardPassiveAttackBuffs, int>(true, x => x.RespondsToCardPassiveAttackBuffs(__instance, dummyResult), x => dummyResult =
                x.CollectCardPassiveAttackBuffs(__instance, dummyResult));
            __result = dummyResult;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveHealthBuffs))]
        [HarmonyPostfix]
        private static void PassiveHealthBuffs(PlayableCard __instance, ref int __result)
        {
            int dummyResult = __result;
            CustomGlobalTriggerHandler.CollectDataAll<IOnCardPassiveHealthBuffs, int>(true, x => x.RespondsToCardPassiveHealthBuffs(__instance, dummyResult), x => dummyResult =
                x.CollectCardPassiveHealthBuffs(__instance, dummyResult));
            __result = dummyResult;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
        [HarmonyPostfix]
        private static void OpposingSlots(PlayableCard __instance, ref List<CardSlot> __result)
        {
            List<CardSlot> original = new(__result);
            bool didRemoveOriginalSlot = __instance.HasAbility(Ability.SplitStrike) && !__instance.HasTriStrike();
            List<IAttackModification> all = CustomGlobalTriggerHandler.GetAll<IAttackModification>(true);
            all.Sort((x, x2) => -x.BringsOriginalSlotBack(__instance).CompareTo(x2.BringsOriginalSlotBack(__instance)));
            foreach (IAttackModification trigg in all)
            {
                if (trigg.RespondsToModifyAttackSlots(__instance, __result, didRemoveOriginalSlot))
                {
                    __result = trigg.CollectModifyAttackSlots(__instance, original, __result, ref didRemoveOriginalSlot);
                }
            }
            __result.Sort((CardSlot a, CardSlot b) => a.Index - b.Index);
        }

        static readonly Type triggerType = AccessTools.TypeByName("DiskCardGame.GlobalTriggerHandler+<TriggerCardsOnBoard>d__16");
    }
}
