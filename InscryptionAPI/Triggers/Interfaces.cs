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
        /// <summary>
        /// Returns true if this should modify the damage added to the scales.
        /// </summary>
        /// <param name="damage">Number of damage currently dealt.</param>
        /// <param name="numWeights">Number of weights currently added to the scales.</param>
        /// <param name="toPlayer">True if the damage is dealt to the player.</param>
        /// <returns></returns>
        public bool RespondsToPreScalesChangedRef(int damage, int numWeights, bool toPlayer);
        /// <summary>
        /// Returns the new amount of damage that will be added to the scales.
        /// </summary>
        /// <param name="damage">Number of damage currently dealt.</param>
        /// <param name="numWeights">Number of weights currently added to the scales. Change this value to modify that number.</param>
        /// <param name="toPlayer">True if the damage is dealt to the player. Change this value to modify the side the damage is getting added to.</param>
        /// <returns>The new amount of damage that will be added to the scales.</returns>
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
    /// Run whenever a card is asked what its opposing slots are as part of the attack sequence
    /// </summary>
    public interface IGetOpposingSlots
    {
        /// <summary>
        /// Indicates if this card wants to respond to getting the opposing slots
        /// </summary>
        /// <returns>True to provide opposing slots, False to leave the slots as default</returns>
        bool RespondsToGetOpposingSlots();

        /// <summary>
        /// Gets the card slots that the card wants to attack
        /// </summary>
        /// <param name="originalSlots">The set of original card slots that the card would attack (as set by abilities in the base game)</param>
        /// <param name="otherAddedSlots">Slots that have been added by other custom abilities</param>
        /// <returns>The list of card slots you want to attack</returns>
        /// <remarks>If your card is replacing the default attack slot (the opposing slot) see 'RemoveDefaultAttackSlot'
        /// If you are **not** replacing the default attack slot, do **not** include that here. Simple ensure 'RemoveDefaultAttackSlot' returns false.
        /// Only return the default attack slot if you want to attack it an additional time.</remarks>
        List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots);

        /// <summary>
        /// If true, this means that the attack slots provided by GetOpposingSlots should override the default attack slot. 
        /// If false, it means they should be in addition to the default opposing slot.
        /// </summary>
        bool RemoveDefaultAttackSlot();
    }
    
    /// <summary>
     /// Data collection trigger that collects data related to attacked slots.
     /// </summary>
    public interface ISetupAttackSequence
    {
        /// <summary>
        /// If true, this trigger will collect data from CollectModifyAttackSlots.
        /// </summary>
        /// <param name="card">Card whose slots will be modified.</param>
        /// <param name="modType">Type of modification that is currently triggering.</param>
        /// <param name="originalSlots">Original slots for attack.</param>
        /// <param name="currentSlots">Current slots that are targeted.</param>
        /// <param name="attackCount">How many attacks the card does.</param>
        /// <param name="didRemoveDefaultSlot">True if the original (opposing) slot was removed by Bifurcated Strike or any other ability that does it.</param>
        /// <returns>True if this trigger will collect data from CollectModifyAttackSlots.</returns>
        public bool RespondsToModifyAttackSlots(PlayableCard card, OpposingSlotTriggerPriority modType, List<CardSlot> originalSlots, List<CardSlot> currentSlots, int attackCount, bool didRemoveDefaultSlot);
        /// <summary>
        /// Modifies data about targeted slots for attack.
        /// </summary>
        /// <param name="card">Card whose slots will be modified.</param>
        /// <param name="modType">Type of modification that is currently triggering.</param>
        /// <param name="originalSlots">Original slots for attack.</param>
        /// <param name="currentSlots">Current slots that are targeted.</param>
        /// <param name="attackCount">How many attacks the card does.</param>
        /// <param name="didRemoveDefaultSlot">True if the original (opposing) slot was removed by Bifurcated Strike or any other ability that does it.</param>
        /// <returns>Modified data about targeted slots for attack.</returns>
        public List<CardSlot> CollectModifyAttackSlots(PlayableCard card, OpposingSlotTriggerPriority modType, List<CardSlot> originalSlots, List<CardSlot> currentSlots, ref int attackCount, ref bool didRemoveDefaultSlot);
        /// <summary>
        /// Gets the priority for this trigger. Triggers with a higher priority will trigger first.
        /// </summary>
        /// <param name="card">Card whose slots will be modified.</param>
        /// <param name="modType">Type of modification that is currently triggering.</param>
        /// <param name="originalSlots">Original slots for attack.</param>
        /// <param name="currentSlots">Current slots that are targeted. NOTE: THIS VALUE IS *NOT* UP TO DATE, GETTING PRIORITIES HAPPENS *BEFORE* THE MODIFICATION ITSELF.</param>
        /// <param name="attackCount">How many attacks the card does. NOTE: THIS VALUE IS *NOT* UP TO DATE, GETTING PRIORITIES HAPPENS *BEFORE* THE MODIFICATION ITSELF.</param>
        /// <param name="didRemoveDefaultSlot">True if the original (opposing) slot was removed by Bifurcated Strike or any other ability that does it. NOTE: THIS VALUE IS *NOT* UP TO DATE, GETTING PRIORITIES HAPPENS *BEFORE* THE MODIFICATION ITSELF.</param>
        /// <returns>The priority for this trigger.</returns>
        public int GetTriggerPriority(PlayableCard card, OpposingSlotTriggerPriority modType, List<CardSlot> originalSlots, List<CardSlot> currentSlots, int attackCount, bool didRemoveDefaultSlot);
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
        /// <summary>
        /// Returns true if this should modify if an item can be used.
        /// </summary>
        /// <param name="itemname">The name of the item that is about to get used.</param>
        /// <param name="currentValue">True if it can be used currently.</param>
        /// <returns>True if this should modify if an item can be used.</returns>
        public bool RespondsToItemCanBeUsed(string itemname, bool currentValue);
        /// <summary>
        /// Returns true if an item can be used.
        /// </summary>
        /// <param name="itemname">The name of the item that is about to get used.</param>
        /// <param name="currentValue">True if it can be used currently.</param>
        /// <returns>True if the item can be used, false otherwise.</returns>
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
        /// <summary>
        /// Returns true if this should modify the passive attack buffs given to a card.
        /// </summary>
        /// <param name="card">Card the buffs will be given to.</param>
        /// <param name="currentValue">The current buff value.</param>
        /// <returns>True if this should modify the passive attack buffs given to a card.</returns>
        public bool RespondsToCardPassiveAttackBuffs(PlayableCard card, int currentValue);
        /// <summary>
        /// Returns the new attack buff value for a card.
        /// </summary>
        /// <param name="card">Card the buffs will be given to.</param>
        /// <param name="currentValue">The current buff value.</param>
        /// <returns>The new attack buff value for a card.</returns>
        public int CollectCardPassiveAttackBuffs(PlayableCard card, int currentValue);
    }

    /// <summary>
    /// Data collection trigger that modifies the passive health buffs of any card.
    /// </summary>
    public interface IOnCardPassiveHealthBuffs
    {
        /// <summary>
        /// Returns true if this should modify the passive health buffs given to a card.
        /// </summary>
        /// <param name="card">Card the buffs will be given to.</param>
        /// <param name="currentValue">The current buff value.</param>
        /// <returns>True if this should modify the passive health buffs given to a card.</returns>
        public bool RespondsToCardPassiveHealthBuffs(PlayableCard card, int currentValue);
        /// <summary>
        /// Returns the new health buff value for a card.
        /// </summary>
        /// <param name="card">Card the buffs will be given to.</param>
        /// <param name="currentValue">The current buff value.</param>
        /// <returns>The new health buff value for a card.</returns>
        public int CollectCardPassiveHealthBuffs(PlayableCard card, int currentValue);
    }

    /// <summary>
    /// Used when a card wants to provide a passive buff to other cards on the board
    /// </summary>
    public interface IPassiveHealthBuff
    {
        /// <summary>
        /// Used to provide a passive health buff to a target
        /// </summary>
        /// <returns>The amount of health you want to buff the target by</returns>
        /// <remarks>Do not assume that the target is on your side of the board! There may be negative sigils that buff opposing cards.</remarks>
        int GetPassiveHealthBuff(PlayableCard target);
    }

    /// <summary>
    /// Used when a card wants to provide a passive buff to other cards on the board
    /// </summary>
    public interface IPassiveAttackBuff
    {
        /// <summary>
        /// Used to provide a passive attack buff to a target
        /// </summary>
        /// <returns>The amount of attack you want to buff the target by</returns>
        /// <remarks>Do not assume that the target is on your side of the board! There may be negative sigils that buff opposing cards.</remarks>
        int GetPassiveAttackBuff(PlayableCard target);
    }

    /// <summary>
    /// Data collection trigger that modifies the damage taken by any card.
    /// </summary>
    public interface ICardTakenDamageModifier
    {
        /// <summary>
        /// Returns true if this should modify the amount of damage taken by a card.
        /// </summary>
        /// <param name="card">Card that took damage.</param>
        /// <param name="currentValue">The current amount of damage dealt.</param>
        /// <returns>True if this should modify the amount of damage taken by a card.</returns>
        public bool RespondsToCardTakenDamageModifier(PlayableCard card, int currentValue);
        /// <summary>
        /// Returns the new amount of damage that will be taken by a card.
        /// </summary>
        /// <param name="card">Card that took damage.</param>
        /// <param name="currentValue">The current amount of damage dealt.</param>
        /// <returns>The new amount of damage that will be taken by a card.</returns>
        public int CollectCardTakenDamageModifier(PlayableCard card, int currentValue);
    }
}
