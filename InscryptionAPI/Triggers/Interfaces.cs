using System.Collections;
using DiskCardGame;

namespace InscryptionAPI.Triggers;

/// <summary>
/// Run when the card this trigger is attached to is added to the hand
/// </summary>
public interface IAddedToHand
{
    bool RespondsToAddedToHand();
    IEnumerator OnAddedToHand();
}

/// <summary>
/// Run when a card other than the card this trigger is attached to is added to the hand
/// </summary>
public interface IOtherAddedToHand
{
    bool RespondsToOtherAddedToHand(PlayableCard card);
    IEnumerator OnOtherAddedToHand(PlayableCard card);
}

/// <summary>
/// Run when the combat phase starts
/// </summary>
public interface IBellRung
{
    /// <param name="playerIsAttacker">Whether this is the player's or the AIs combat phase</param>
    bool RespondsToBellRung(bool playerIsAttacker);
    /// <param name="playerIsAttacker">Whether this is the player's or the AIs combat phase</param>
    IEnumerator OnBellRung(bool playerIsAttacker);
}

/// <summary>
/// Run before a slot begins attacking
/// </summary>
public interface IPreSlotAttackSequence
{
    /// <param name="slot">The slot attacking</param>
    bool RespondsToPreSlotAttackSequence(CardSlot slot);
    /// <param name="slot">The slot attacking</param>
    IEnumerator OnPreSlotAttackSequence(CardSlot slot);
}

/// <summary>
/// Run after a card slot attacks
/// </summary>
public interface IPostSlotAttackSequence
{
    /// <param name="slot">The slot that attacked</param>
    bool RespondsToPostSlotAttackSequence(CardSlot slot);
    /// <param name="slot">The slot that attacked</param>
    IEnumerator OnPostSlotAttackSequence(CardSlot slot);
}

/// <summary>
/// Run after a slot is attacked
/// </summary>
public interface IPostSingularSlotAttackSlot
{
    /// <param name="attacker">The attacking slot</param>
    /// <param name="defender">The defending slot</param>
    bool RespondsToPostSingularSlotAttackSlot(CardSlot attacker, CardSlot defender);
    /// <param name="attacker">The attacking slot</param>
    /// <param name="defender">The defending slot</param>
    IEnumerator OnPostSingularSlotAttackSlot(CardSlot attacker, CardSlot defender);
}

/// <summary>
/// Run before the scales are changed
/// </summary>
public interface IPreScalesChanged
{
    /// <param name="damage">The amount of damage being dealt</param>
    /// <param name="toPlayer">Whether the damage is to the player or to the AI</param>
    bool RespondsToPreScalesChanged(int damage, bool toPlayer);
    /// <param name="damage">The amount of damage being dealt</param>
    /// <param name="toPlayer">Whether the damage is to the player or to the AI</param>
    IEnumerator OnPreScalesChanged(int damage, bool toPlayer);
}

/// <summary>
/// Run after the scales are changed
/// </summary>
public interface IPostScalesChanged
{
    /// <param name="damage">The amount of damage being dealt</param>
    /// <param name="toPlayer">Whether the damage is to the player or to the AI</param>
    bool RespondsToPostScalesChanged(int damage, bool toPlayer);
    /// <param name="damage">The amount of damage being dealt</param>
    /// <param name="toPlayer">Whether the damage is to the player or to the AI</param>
    IEnumerator OnPostScalesChanged(int damage, bool toPlayer);
}

/// <summary>
/// Triggered during the upkeep phase while in the hand
/// </summary>
public interface IUpkeepInHand
{
    /// <param name="playerUpkeep">Whether this is during the player's turn or the AI's turn</param>
    bool RespondsToUpkeepInHand(bool playerUpkeep);
    /// <param name="playerUpkeep">Whether this is during the player's turn or the AI's turn</param>
    IEnumerator OnUpkeepInHand(bool playerUpkeep);
}

/// <summary>
/// Run when another card is resolved on the board while in the hand
/// </summary>
public interface IOtherCardResolveInHand
{
    /// <param name="card">Card being resolved</param>
    bool RespondsToOtherCardResolveInHand(PlayableCard card);
    /// <param name="card">Card being resolved</param>
    IEnumerator OnOtherCardResolveInHand(PlayableCard card);
}

/// <summary>
/// Run when the turn is ended while in the hand
/// </summary>
public interface ITurnEndInHand
{
    bool RespondsToTurnEndedInHand();
    IEnumerator OnTurnEndInHand();
}

/// <summary>
/// Run when another card is assigned to a slot while in the hand
/// </summary>
public interface IOtherCardAssignedToSlotInHand
{
    /// <param name="card">Card being assigned to a slot</param>
    /// <param name="fromHand">Whether it is being assigned from the hand</param>
    bool RespondsToOtherCardAssignedToSlotInHand(PlayableCard card, bool fromHand);
    /// <param name="card">Card being assigned to a slot</param>
    /// <param name="fromHand">Whether it is being assigned from the hand</param>
    IEnumerator OnOtherCardAssignedToSlotInHand(PlayableCard card, bool fromHand);
}

/// <summary>
/// Runs before a card on the board dies in the hand
/// </summary>
public interface IOtherCardPreDeathInHand
{
    /// <param name="slot">Slot dying</param>
    /// <param name="wasSacrificed">Whether the card was sacrificed</param>
    /// <param name="killer">The killer, <see langword="null"/> if card was sacrificed</param>
    bool RespondsToOtherCardPreDeathInHand(CardSlot slot, bool wasSacrificed, PlayableCard killer);
    /// <param name="slot">Slot dying</param>
    /// <param name="wasSacrificed">Whether the card was sacrificed</param>
    /// <param name="killer">The killer, <see langword="null"/> if card was sacrificed</param>
    IEnumerator OnOtherCardPreDeathInHand(CardSlot slot, bool wasSacrificed, PlayableCard killer);
}

/// <summary>
/// Runs after a card on the board dies in the hand
/// </summary>
public interface IOtherCardDieInHand
{
    /// <param name="card">The card that died</param>
    /// <param name="slot">The slot before death</param>
    /// <param name="wasSacrificed">Whether the card was sacrificed</param>
    /// <param name="killer">The killer, <see langword="null"/> if card was sacrificed</param>
    bool RespondsToOtherCardDieInHand(PlayableCard card, CardSlot slot, bool wasSacrificed, PlayableCard killer);
    /// <param name="card">The card that died</param>
    /// <param name="slot">The slot before death</param>
    /// <param name="wasSacrificed">Whether the card was sacrificed</param>
    /// <param name="killer">The killer, <see langword="null"/> if card was sacrificed</param>
    IEnumerator OnOtherCardDieInHand(PlayableCard card, CardSlot slot, bool wasSacrificed, PlayableCard killer);
}

/// <summary>
/// Run when another card deals damage while in the hand
/// </summary>
public interface IOtherCardDealtDamageInHand
{
    /// <param name="attacker">The attacker</param>
    /// <param name="attack">The amount of damage being dealt</param>
    /// <param name="defender">The defender</param>
    bool RespondsToOtherCardDealtDamageInHand(PlayableCard attacker, int attack, PlayableCard defender);
    /// <param name="attacker">The attacker</param>
    /// <param name="attack">The amount of damage being dealt</param>
    /// <param name="defender">The defender</param>
    IEnumerator OnOtherCardDealtDamageInHand(PlayableCard attacker, int attack, PlayableCard defender);
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