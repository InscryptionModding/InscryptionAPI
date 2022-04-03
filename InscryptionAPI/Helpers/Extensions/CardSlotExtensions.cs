using System.Collections;
using DiskCardGame;

namespace InscryptionAPI.Helpers.Extensions;

public static class CardSlotExtensions
{
    /// <summary>
    /// Check if the slot is an opponent's slot.
    /// </summary>
    /// <param name="cardSlot">The slot to check.</param>
    /// <returns>true if the slot is not a player slot.</returns>
    public static bool IsOpponentSlot(this CardSlot cardSlot)
    {
        return !cardSlot.IsPlayerSlot;
    }
    
    /// <summary>
    /// Check if the card in the slot has a specific card name.
    /// This is primarily useful if you don't have a Trait, Tribe, or other distinct attribute to check the card against.
    /// </summary>
    /// <param name="cardSlot">The slot to check.</param>
    /// <param name="cardName">The name of the card check.</param>
    /// <returns>true if the slot has a card and that card has the specific cardName.</returns>
    public static bool CardInSlotIs(this CardSlot cardSlot, string cardName)
    {
        return cardSlot.Card && cardSlot.Card.Info.name == cardName;
    }

    /// <summary>
    /// Create a card in a specific slot from the CardSlot object. A much more robust way that's the same as `yield return BoardManager.Instance.CreateCardInSlot()`.
    /// </summary>
    /// <param name="slotToSpawnIn">The slot to spawn in.</param>
    /// <param name="cardInfo">The CardInfo object to spawn in said slot.</param>
    /// <param name="transitionLength">Time to transition the card to the slot. The longer the time, the longer it will take to be placed at the slot.</param>
    /// <param name="resolveTriggers">Whether or not to activate other card triggers.</param>
    /// <returns>The enumeration of the card being placed in the slot.</returns>
    public static IEnumerator CreateCardInSlot(
        this CardSlot slotToSpawnIn,
        CardInfo cardInfo,
        float transitionLength = 0.1f,
        bool resolveTriggers = true
    )
    {
        yield return BoardManager.Instance.CreateCardInSlot(cardInfo, slotToSpawnIn, transitionLength, resolveTriggers);
    }
    
    /// <summary>
    /// Get the adjacent slots of the slot that is being accessed.
    /// </summary>
    /// <param name="cardSlot">The slot that is being accessed.</param>
    /// <param name="removeNulls">If true, remove slots that are null.</param>
    /// <returns>
    /// The list of card slots that is to the left and to the right.
    /// Results can be null unless removeNulls parameter is set to true.
    /// </returns>
    public static List<CardSlot> GetAdjacentSlots(this CardSlot cardSlot, bool removeNulls = false)
    {
        return BoardManager.Instance.GetAdjacentSlots(cardSlot).Where(slot => !removeNulls || slot).ToList();
    }
    
    /// <summary>
    /// Get the adjacent slots of the slot that is being accessed.
    /// </summary>
    /// <param name="cardSlot">The slot that is being accessed.</param>
    /// <param name="adjacentOnLeft">Whether or not to retrieve the slot on the left.</param>
    /// <returns>The list of card slots that is to the left and to the right. Results can be null.</returns>
    public static CardSlot GetAdjacent(this CardSlot cardSlot, bool adjacentOnLeft)
    {
        return BoardManager.Instance.GetAdjacent(cardSlot, adjacentOnLeft);
    }
    
    /// <summary>
    /// Retrieve all the PlayableCard objects from the collection of slots provided.
    /// </summary>
    /// <param name="slots">Collection of CardSlots</param>
    /// <param name="filterOnPredicate">Predicate to filter each slot's playable card against, if one exists.</param>
    /// <returns>The list of cards from each slot</returns>
    public static List<PlayableCard> GetCards(this IEnumerable<CardSlot> slots, Predicate<PlayableCard> filterOnPredicate = null)
    {
        return slots
            .Where(slot => slot.Card && (filterOnPredicate == null || filterOnPredicate.Invoke(slot.Card)))
            .Select(slot => slot.Card)
            .ToList();
    }
    
    /// <summary>
    /// Retrieve all the CardSlot objects that are not occupied by a PlayableCard object.
    /// </summary>
    /// <param name="slots">Collection of CardSlots</param>
    /// <param name="filterOnPredicate">Predicate to filter each slot against</param>
    /// <returns>The list of slots not occupied by a playable card.</returns>
    public static List<CardSlot> OpenSlots(this IEnumerable<CardSlot> slots, Predicate<CardSlot> filterOnPredicate = null)
    {
        return slots
            .Where(slot => !slot.Card && (filterOnPredicate == null || filterOnPredicate.Invoke(slot)))
            .ToList();
    }
}