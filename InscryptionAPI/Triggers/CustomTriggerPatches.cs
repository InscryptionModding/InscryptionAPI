using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Triggers;

[HarmonyPatch]
internal static class CustomTriggerPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.ManagedUpdate))]
    private static bool StopManagedUpdate() { return false; }
	
    [HarmonyReversePatch, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.ManagedUpdate))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void OriginalManagedUpdate(PlayableCard instance) { throw new NotImplementedException(); }

    public static void UpdateAllCards()
    {
        var cardsInHandAndBoard = BoardManager.Instance.CardsOnBoard.Concat(PlayerHand.Instance.CardsInHand).ToList();
        foreach (PlayableCard c in cardsInHandAndBoard)
        {
            OriginalManagedUpdate(c);
            if (c.OnBoard && c.GetComponent<VariableStatBehaviour>())
            {
                c.GetComponent<VariableStatBehaviour>().UpdateStats();
            }
        }
    }
        
    [HarmonyPostfix, HarmonyPatch(typeof(GlobalTriggerHandler), nameof(GlobalTriggerHandler.TriggerCardsInHand))]
    private static void UpdateCardStatsForCardsInHand()
    {
        UpdateAllCards();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GlobalTriggerHandler), nameof(GlobalTriggerHandler.TriggerCardsOnBoard))]
    public static IEnumerator UpdateCardStatsForCardsOnBoard(IEnumerator enumerator, Trigger trigger, bool triggerFacedown, params object[] otherArgs)
    {
        yield return enumerator;
        UpdateAllCards();
    }
        
    [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.AddCardToHand))]
    [HarmonyPostfix]
    private static IEnumerator TriggerOnAddedToHand(IEnumerator result, PlayableCard card)
    {
        yield return result;
        foreach (var onCard in CustomTriggerFinder.FindTriggersOnCard<IAddedToHand>(card))
        {
            if (onCard.RespondsToAddedToHand())
                yield return onCard.OnAddedToHand();
        }
        foreach (var otherCard in CustomTriggerFinder.FindGlobalTriggers<IOtherAddedToHand>(card))
        {
            if (otherCard.RespondsToOtherAddedToHand(card))
                yield return otherCard.OnOtherAddedToHand(card);
        }
    }

    [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.DoCombatPhase))]
    [HarmonyPostfix]
    private static IEnumerator TriggerOnBellRung(IEnumerator result, bool playerIsAttacker)
    {
        foreach (var i in CustomTriggerFinder.FindGlobalTriggers<IBellRung>())
        {
            if (i.RespondsToBellRung(playerIsAttacker))
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
            if (pre.RespondsToPreSlotAttackSequence(slot))
                yield return pre.OnPreSlotAttackSequence(slot);
        }
        yield return result;
        foreach (var post in CustomTriggerFinder.FindGlobalTriggers<IPostSlotAttackSequence>())
        {
            if (post.RespondsToPostSlotAttackSequence(slot))
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
            if (i.RespondsToPostSingularSlotAttackSlot(attackingSlot, opposingSlot))
                yield return i.OnPostSingularSlotAttackSlot(attackingSlot, opposingSlot);
        }
    }

    [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
    [HarmonyPostfix]
    private static IEnumerator TriggerOnScalesChanged(IEnumerator result, int damage, bool toPlayer)
    {
        foreach (var pre in CustomTriggerFinder.FindGlobalTriggers<IPreScalesChanged>())
        {
            if (pre.RespondsToPreScalesChanged(damage, toPlayer))
                yield return pre.OnPreScalesChanged(damage, toPlayer);
        }
        yield return result;
        foreach (var post in CustomTriggerFinder.FindGlobalTriggers<IPostScalesChanged>())
        {
            if (post.RespondsToPostScalesChanged(damage, toPlayer))
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
            if (i.RespondsToUpkeepInHand(playerUpkeep))
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
            foreach (var i in CustomTriggerFinder.FindTriggersInHandExcluding<IOtherCardResolveInHand>(card))
            {
                if (i.RespondsToOtherCardResolveInHand(card))
                    yield return i.OnOtherCardResolveInHand(card);
            }
        }
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlayerTurn))]
    [HarmonyPostfix]
    private static IEnumerator TriggerOnTurnEndInHandPlayer(IEnumerator result)
    {
        yield return result;
        foreach (var i in CustomTriggerFinder.FindGlobalTriggers<ITurnEndInHand>())
        {
            if (i.RespondsToTurnEndedInHand())
                yield return i.OnTurnEndInHand();
        }
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.OpponentTurn))]
    [HarmonyPostfix]
    private static IEnumerator TriggerOnTurnEndInHandOpponent(IEnumerator result, TurnManager __instance)
    {
        bool turnSkipped = __instance.Opponent.SkipNextTurn;
        yield return result;
        if (!turnSkipped)
        {
            foreach (var i in CustomTriggerFinder.FindGlobalTriggers<ITurnEndInHand>())
            {
                if (i.RespondsToTurnEndedInHand())
                    yield return i.OnTurnEndInHand();
            }
        }
    }

    [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
    [HarmonyPostfix]
    private static IEnumerator TriggerOnOtherCardAssignedToSlotInHand(IEnumerator result, PlayableCard card, bool resolveTriggers)
    {
        CardSlot slot2 = card.Slot;
        yield return result;
        if (resolveTriggers && slot2 != card.Slot)
        {
            bool fromHand = slot2 == null;
            foreach (var i in CustomTriggerFinder.FindTriggersInHand<IOtherCardAssignedToSlotInHand>())
            {
                if (i.RespondsToOtherCardAssignedToSlotInHand(card, fromHand))
                    yield return i.OnOtherCardAssignedToSlotInHand(card, fromHand);
            }
        }
    }

    private static readonly Type TriggerCardsOnBoardEnumerator =
        AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(typeof(GlobalTriggerHandler), nameof(GlobalTriggerHandler.TriggerCardsOnBoard))).DeclaringType;

    private static readonly FieldInfo TriggerField = AccessTools.Field(TriggerCardsOnBoardEnumerator, "trigger");
        
    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Die))]
    [HarmonyPostfix]
    private static IEnumerator TriggerDeathTriggers(IEnumerator result, PlayableCard __instance, bool wasSacrifice, PlayableCard killer = null)
    {
        CardSlot slotBeforeDeath = __instance.Slot;
        while (result.MoveNext())
        {
            yield return result.Current;
            if (result.Current.GetType() == TriggerCardsOnBoardEnumerator)
            {
                Trigger t = (Trigger)TriggerField.GetValue(result.Current);
                if (t == Trigger.OtherCardPreDeath)
                {
                    foreach (var pre in CustomTriggerFinder.FindTriggersInHand<IOtherCardPreDeathInHand>())
                    {
                        if (pre.RespondsToOtherCardPreDeathInHand(slotBeforeDeath, wasSacrifice, killer))
                            yield return pre.OnOtherCardPreDeathInHand(slotBeforeDeath, wasSacrifice, killer);
                    }
                }
                else if (t == Trigger.OtherCardDie)
                {
                    foreach (var i in CustomTriggerFinder.FindTriggersInHand<IOtherCardDieInHand>())
                    {
                        if (i.RespondsToOtherCardDieInHand(__instance, slotBeforeDeath, wasSacrifice, killer))
                            yield return i.OnOtherCardDieInHand(__instance, slotBeforeDeath, wasSacrifice, killer);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
    [HarmonyPostfix]
    private static IEnumerator TriggerOnOtherCardDealtDamageInHand(IEnumerator result, PlayableCard __instance, PlayableCard attacker)
    {
        bool hasshield = __instance.HasShield();
        yield return result;
        if (!hasshield && attacker != null)
        {
            foreach (var i in CustomTriggerFinder.FindTriggersInHand<IOtherCardDealtDamageInHand>())
            {
                if (i.RespondsToOtherCardDealtDamageInHand(attacker, attacker.Attack, __instance))
                    yield return i.OnOtherCardDealtDamageInHand(attacker, attacker.Attack, __instance);
            }
        }
    }
}