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