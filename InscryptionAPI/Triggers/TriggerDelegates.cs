using System.Collections;
using DiskCardGame;

namespace InscryptionAPI.Triggers;

/// <summary>
/// This contains the delegates/method signatures for all custom trigger handlers
/// </summary>
public static class TriggerDelegates
{
    /// <summary>
    /// Fires whenever the card is added to the players hand.
    /// </summary>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    /// <remarks> This differs from the existing 'OnDrawn' event because that event fires after the card is drawn but before it actually is in the player's hand.</remarks>
    public delegate IEnumerator OnAddedToHand();
    
    /// <summary>
    /// Fires whenever another card is added to the players hand.
    /// </summary>
    /// <param "otherCard">The card that was added to the player's hand</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnOtherCardAddedToHand(PlayableCard otherCard);
    
    /// <summary>
    /// Fires whenever a card is moved from one slot to another
    /// </summary>
    /// <param name="card">The card that was moved</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnCardAssignedToSlotNoResolve(PlayableCard card);

    /// <summary>
    /// Fires whenever the bell is rung to advance to combat
    /// </summary>
    /// <param name="playerIsAttacker">Indicates if the player is the attacker</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnBellRung(bool playerIsAttacker);

    /// <summary>
    /// Fires before a slot begins its attack sequence
    /// </summary>
    /// <param name="slot">The attacking slot</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnPreSlotAttackSequence(CardSlot slot);

    /// <summary>
    /// Fires after a slot ends its entire attack sequence
    /// </summary>
    /// <param name="slot"></param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnPostSlotAttackSequence(CardSlot slot);

    /// <summary>
    /// Fires after a slot has finished attacking a specific slot
    /// </summary>
    /// <param name="attackingSlot">The attacking slot</param>
    /// <param name="defendingSlot">The defending slot</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnPostSingularSlotAttackSlot(CardSlot attackingSlot, CardSlot defendingSlot);

    /// <summary>
    /// Fires right before damage is added to the scales
    /// </summary>
    /// <param name="damage">The amount of damage that will be added</param>
    /// <param name="toPlayer">Indicates if the player will be receiving the damage</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnPreScalesChanged(int damage, bool toPlayer);
    
    /// <summary>
    /// Fires right after daamge is added to the scales
    /// </summary>
    /// <param name="damage">The amont of damage that was added</param>
    /// <param name="toPlayer">Indicates if the player received the daamge</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnPostScalesChanged(int damage, bool toPlayer);

    /// <summary>
    /// Fires on upkeep, but only for cards in hand.
    /// </summary>
    /// <param name="playerUpkeep">Indicates if this is the player's upkeep</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    /// <remarks>Use this instead of OnUpkeep if you want the card to respond in the player's hand</remarks>
    public delegate IEnumerator OnUpkeepInHand(bool playerUpkeep);

    /// <summary>
    /// Fires when another card resolves on board, but only trigger receivers in the player's hand will respond
    /// </summary>
    /// <param name="card">The card that resolved on board</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    /// /// <remarks>Use this instead of OnOtherCardResolve if you want the card to respond in the player's hand</remarks>
    public delegate IEnumerator OnOtherCardResolveInHand(PlayableCard card);

    /// <summary>
    /// Fires at the end of the player's turn, but only trigger receivers in the player's hand will respond
    /// </summary>
    /// <param name="playerTurn">Indicates if it was the player's turn</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    /// /// <remarks>Use this instead of OnTurnEnd if you want the card to respond in the player's hand</remarks>
    public delegate IEnumerator OnTurnEndInHand(bool playerTurn);

    /// <summary>
    /// Fires whenever a card was assigned to a slot, but only trigger receivers in the player's hand will respond
    /// </summary>
    /// <param name="card">The card that was assigned a slot</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnOtherCardAssignedToSlotInHand(PlayableCard card);

    /// <summary>
    /// Fires right before another card dies, but only trigger receivers in the player's hand will respond
    /// </summary>
    /// <param name="slotBeforeDeath">The slot the card is in before it dies</param>
    /// <param name="wasKilled">Indicates if the card will be killed (true means killed, false means sacrificed)</param>
    /// <param name="killer">The card that killed it (null if the card was sacrificed)</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnOtherCardPreDeathInHand(CardSlot slotBeforeDeath, bool wasKilled, PlayableCard killer);

    /// <summary>
    /// Fires when another card takes damage, but only trigger receivers in the player's hand will respond
    /// </summary>
    /// <param name="attacker">The attacking card</param>
    /// <param name="damage">The damage dealt</param>
    /// <param name="defender">The defending card</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnOtherCardDealtDamageInHand(PlayableCard attacker, int damage, PlayableCard defender);

    /// <summary>
    /// Fires when another card dies, but only trigger receivers in the player's hand will respond
    /// </summary>
    /// <param name="dyingCard">The card that died</param>
    /// <param name="slotBeforeDeath">The slot the card was in before it does</param>
    /// <param name="wasKilled">Indicates if the card was killed (true means killed, false means sacrificed)</param>
    /// <param name="killer">The card that killed it (null if the card was sacrificed)</param>
    /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
    public delegate IEnumerator OnOtherCardDieInHand(PlayableCard dyingCard, CardSlot slotBeforeDeath, bool wasKilled, PlayableCard killer);

    /// <summary>
    /// Fires whenever the card is preparing to attack and the game needs to know which slots it is going to attack
    /// </summary>
    /// <returns>An instance of [AttackModifier](xref:InscryptionAPI.Triggers.AttackModifier) that indicates how the card should attack.</returns>
    public delegate AttackModifier OnGetOpposingSlots();

    /// <summary>
    /// Fires whenever a card is calculating its passive attack buffs.
    /// </summary>
    /// <param name="otherCard">The card to buff</param>
    /// <returns>A value to to be added to the attack value of **otherCard**</returns>
    /// <remarks>Whenever a card calculates its passive attack buffs, it looks at all 
    /// other cards and sees if they respond to this event. If so, it fires this trigger to 
    /// see how much it should add.</remarks>
    public delegate int OnBuffOtherCardAttack(PlayableCard otherCard);

    /// <summary>
    /// Fires whenever the card is calculating its passive health buffs.
    /// </summary>
    /// <returns>A value to to be added to the card's health value</returns>
    /// <remarks>Whenever a card calculates its passive health buffs, it looks at all 
    /// other cards and sees if they respond to this event. If so, it fires this trigger to 
    /// see how much it should add.</remarks>
    public delegate int OnBuffOtherCardHealth(PlayableCard otherCard);
}