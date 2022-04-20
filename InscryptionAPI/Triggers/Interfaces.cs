using DiskCardGame;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Triggers
{
    /// <summary>
    /// Trigger that is triggered when the card is drawn, but after it has been added to the list of cards in hand.
    /// </summary>
    public interface IOnAddedToHand
    {
        public bool RespondsToAddedToHand();
        public IEnumerator OnAddedToHand();
    }

    /// <summary>
    /// Trigger that is triggered when any card is drawn, but after it has been added to the list of cards in hand.
    /// </summary>
    public interface IOnOtherCardAddedToHand
    {
        public bool RespondsToOtherCardAddedToHand(PlayableCard card);
        public IEnumerator OnOtherCardAddedToHand(PlayableCard card);
    }

    /// <summary>
    /// Trigger that is triggered after any card is assigned to a slot, but only if it was assigned to a slot before.
    /// </summary>
    public interface IOnCardAssignedToSlotNoResolve
    {
        public bool RespondsToCardAssignedToSlotNoResolve(PlayableCard card);
        public IEnumerator OnCardAssignedToSlotNoResolve(PlayableCard card);
    }

    /// <summary>
    /// Trigger that is triggered after any card is assigned to a slot. The difference between this and normal OnOtherCardAssignedToSlot is that this trigger also provides information about the new and old slot.
    /// </summary>
    public interface IOnCardAssignedToSlotContext
    {
        public bool RespondsToCardAssignedToSlotContext(PlayableCard card, CardSlot oldSlot, CardSlot newSlot);
        public IEnumerator OnCardAssignedToSlotContext(PlayableCard card, CardSlot oldSlot, CardSlot newSlot);
    }

    /// <summary>
    /// Trigger that is triggered after the combat phase starts.
    /// </summary>
    public interface IOnBellRung
    {
        public bool RespondsToBellRung(bool playerCombatPhase);
        public IEnumerator OnBellRung(bool playerCombatPhase);
    }

    /// <summary>
    /// Trigger that is triggered before a slot does its attacks.
    /// </summary>
    public interface IOnPreSlotAttackSequence
    {
        public bool RespondsToPreSlotAttackSequence(CardSlot attackingSlot);
        public IEnumerator OnPreSlotAttackSequence(CardSlot attackingSlot);
    }

    /// <summary>
    /// Trigger that is triggered after a slot does its attacks.
    /// </summary>
    public interface IOnPostSlotAttackSequence
    {
        public bool RespondsToPostSlotAttackSequence(CardSlot attackingSlot);
        public IEnumerator OnPostSlotAttackSequence(CardSlot attackingSlot);
    }

    /// <summary>
    /// Trigger that is triggered after a slot does an individual attack.
    /// </summary>
    public interface IOnPostSingularSlotAttackSlot
    {
        public bool RespondsToPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot);
        public IEnumerator OnPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot targetSlot);
    }

    /// <summary>
    /// Data collection trigger that is triggered before the scales are changed, can be used to change the amount of damage added and to which side it's added.
    /// </summary>
    public interface IOnPreScalesChangedRef
    {
        public bool RespondsToPreScalesChangedRef(int damage, int numWeights, bool toPlayer);
        public int CollectPreScalesChangedRef(int damage, ref int numWeights, ref bool toPlayer);
    }

    /// <summary>
    /// Trigger that is triggered before the scales are changed, after IOnPreScalesChangedRef.
    /// </summary>
    public interface IOnPreScalesChanged
    {
        public bool RespondsToPreScalesChanged(int damage, bool toPlayer);
        public IEnumerator OnPreScalesChanged(int damage, bool toPlayer);
    }

    /// <summary>
    /// Trigger that is triggered after the scales are changed. Also includes information about the original damage and side that the damage is added at, before those values potentially get changed by IOnPreScalesChangedRef.
    /// </summary>
    public interface IOnPostScalesChanged
    {
        public bool RespondsToPostScalesChanged(int damage, bool toPlayer, int originalDamage, bool originalToPlayer);
        public IEnumerator OnPostScalesChanged(int damage, bool toPlayer, int originalDamage, bool originalToPlayer);
    }

    /// <summary>
    /// Trigger that is triggered each turn, but unlike normal OnUpkeep this one only works in hand.
    /// </summary>
    public interface IOnUpkeepInHand
    {
        public bool RespondsToUpkeepInHand(bool playerUpkeep);
        public IEnumerator OnUpkeepInHand(bool playerUpkeep);
    }

    /// <summary>
    /// Trigger that is triggered when any card gets played, but unlike normal OnOtherCardResolveOnBoard this one only works in hand.
    /// </summary>
    public interface IOnOtherCardResolveInHand
    {
        public bool RespondsToOtherCardResolveInHand(PlayableCard resolvingCard);
        public IEnumerator OnOtherCardResolveInHand(PlayableCard resolvingCard);
    }

    /// <summary>
    /// Trigger that is triggered when the turn ends, but unlike normal OnTurnEnd this one only works in hand.
    /// </summary>
    public interface IOnTurnEndInHand
    {
        public bool RespondsToTurnEndInHand(bool playerTurn);
        public IEnumerator OnTurnEndInHand(bool playerTurn);
    }

    /// <summary>
    /// Trigger that is triggered when any card is assigned to a slot, but unlike normal OnOtherCardAssignedToSlotInHand this one only works in hand.
    /// </summary>
    public interface IOnOtherCardAssignedToSlotInHand
    {
        public bool RespondsToOtherCardAssignedToSlotInHand(PlayableCard card);
        public IEnumerator OnOtherCardAssignedToSlotInHand(PlayableCard card);
    }

    /// <summary>
    /// Trigger that is triggered before any card dies, but unlike normal OnOtherCardPreDeath this one only works in hand.
    /// </summary>
    public interface IOnOtherCardPreDeathInHand
    {
        public bool RespondsToOtherCardPreDeathInHand(CardSlot deathSlot, bool fromCombat, PlayableCard killer);
        public IEnumerator OnOtherCardPreDeathInHand(CardSlot deathSlot, bool fromCombat, PlayableCard killer);
    }

    /// <summary>
    /// Trigger that is triggered when any card deals damage to another card, but unlike normal OnOtherCardDealtDamage this one only works in hand.
    /// </summary>
    public interface IOnOtherCardDealtDamageInHand
    {
        public bool RespondsToOtherCardDealtDamageInHand(PlayableCard attacker, int amount, PlayableCard target);
        public IEnumerator OnOtherCardDealtDamageInHand(PlayableCard attacker, int amount, PlayableCard target);
    }

    /// <summary>
    /// Trigger that is triggered after any card dies, but unlike normal OnOtherCardDie this one only works in hand.
    /// </summary>
    public interface IOnOtherCardDieInHand
    {
        public bool RespondsToOtherCardDieInHand(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer);
        public IEnumerator OnOtherCardDieInHand(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer);
    }

    /// <summary>
    /// Trigger that is triggered before any item is used.
    /// </summary>
    public interface IOnPreItemUsed
    {
        public bool RespondsToPreItemUsed(string itemname, bool isHammer);
        public IEnumerator OnPreItemUsed(string itemname, bool isHammer);
    }

    /// <summary>
    /// Trigger that is triggered after any item is used.
    /// </summary>
    public interface IOnPostItemUsed
    {
        public bool RespondsToPostItemUsed(string itemname, bool success, bool isHammer);
        public IEnumerator OnPostItemUsed(string itemname, bool success, bool isHammer);
    }

    /// <summary>
    /// Data collection trigger that collects data related to attacked slots.
    /// </summary>
    public interface IGetOpposingSlots
    {
        /// <summary>
        /// If true, this trigger will collect data from CollectModifyAttackSlots.
        /// </summary>
        /// <param name="currentSlots">Current slots that are targeted.</param>
        /// <param name="didRemoveDefaultSlot">True if the original (opposing) slot was removed by Bifurcated Strike or any other ability that does it.</param>
        /// <returns>True if this trigger will collect data from CollectModifyAttackSlots.</returns>
        public bool RespondsToModifyAttackSlots(PlayableCard card, OpposingSlotTriggerPriority modType, List<CardSlot> originalSlots, List<CardSlot> currentSlots, bool didRemoveDefaultSlot);
        /// <summary>
        /// Modifies data about targeted slots for attack.
        /// </summary>
        /// <param name="currentSlots">Current slots that are targeted.</param>
        /// <param name="didRemoveDefaultSlot">True if the original (opposing) slot was removed by Bifurcated Strike or any other ability that does it.</param>
        /// <returns>Modified data about targeted slots for attack.</returns>
        public List<CardSlot> CollectModifyAttackSlots(PlayableCard card, OpposingSlotTriggerPriority modType, List<CardSlot> originalSlots, List<CardSlot> currentSlots, ref bool didRemoveDefaultSlot);
        /// <summary>
        /// If true, this trigger will collect data from CollectGetAttackSlotCount.
        /// </summary>
        /// <returns>True if this trigger will collect data from CollectGetAttackSlotCount.</returns>
        public bool RespondsToGetAttackSlotCount(PlayableCard card);
        /// <summary>
        /// Modifies data about the amount of slots that will be selected for the Sniper ability (and any other ability that may require that).
        /// </summary>
        /// <returns>Modified data about the amount of slots that will be selected.</returns>
        public int CollectGetAttackSlotCount(PlayableCard card);
    }

    /// <summary>
    /// Flag that tells IGetOpposingSlots when to trigger.
    /// </summary>
    public enum OpposingSlotTriggerPriority
    {
        /// <summary>
        /// Normal mode - use this for opposing slot modifications that either don't do anything to the original opposing slots or remove it.
        /// </summary>
        Normal,
        /// <summary>
        /// Bring back opposing slot - use this for opposing slot modifications that bring back the original opposing slot if it was removed like Trifurcated Strike.
        /// </summary>
        BringsBackOpposingSlot,
        /// <summary>
        /// Post slot addition modification - use this for opposing slot modifications that don't interact with any other modification and instead just statically modify the finished result like Double Strike.
        /// </summary>
        PostAdditionModification,
        /// <summary>
        /// Replaces default opposing slot - use this for opposing slot modifications that replace the original opposing slot like Omni Strike.
        /// </summary>
        ReplacesDefaultOpposingSlot
    }

    /// <summary>
    /// Data collection trigger that collects data about the usability of an item.
    /// </summary>
    public interface IItemCanBeUsed
    {
        public bool RespondsToItemCanBeUsed(string itemname, bool currentValue);
        public bool CollectItemCanBeUsed(string itemname, bool currentValue);
    }

    /// <summary>
    /// Trigger that is triggered when an item is prevented from use using IItemCanBeUsed.
    /// </summary>
    public interface IOnItemPreventedFromUse
    {
        public bool RespondsToItemPreventedFromUse(string itemname);
        public IEnumerator OnItemPreventedFromUse(string itemname);
    }

    /// <summary>
    /// Data collection trigger that modifies the passive attack buffs of any card.
    /// </summary>
    public interface IOnCardPassiveAttackBuffs
    {
        public bool RespondsToCardPassiveAttackBuffs(PlayableCard card, int currentValue);
        public int CollectCardPassiveAttackBuffs(PlayableCard card, int currentValue);
    }

    /// <summary>
    /// Data collection trigger that modifies the passive health buffs of any card.
    /// </summary>
    public interface IOnCardPassiveHealthBuffs
    {
        public bool RespondsToCardPassiveHealthBuffs(PlayableCard card, int currentValue);
        public int CollectCardPassiveHealthBuffs(PlayableCard card, int currentValue);
    }

    /// <summary>
    /// Data collection trigger that modifies the damage taken by any card.
    /// </summary>
    public interface ICardTakenDamageModifier
    {
        public bool RespondsToCardTakenDamageModifier(PlayableCard card, int currentValue);
        public int CollectCardTakenDamageModifier(PlayableCard card, int currentValue);
    }
}
