using DiskCardGame;

namespace InscryptionAPI.Helpers.Extensions;

public static class BoardManagerExtensions
{
    /// <summary>
    /// Retrieve all player cards on the board.
    /// </summary>
    /// <param name="manager">Manager instance to access</param>
    /// <param name="filterOnPredicate">The predicate to filter on each card</param>
    /// <returns>The list of player cards on the board</returns>
    public static List<PlayableCard> GetPlayerCards(
        this BoardManager manager,
        Predicate<PlayableCard> filterOnPredicate = null
    )
    {
        return manager.PlayerSlotsCopy.SelectCards(filterOnPredicate).ToList();
    }

    /// <summary>
    /// Retrieve all player slots that are not occupied by a card.
    /// </summary>
    /// <param name="manager">Manager instance to access</param>
    /// <param name="filterOnPredicate">The predicate to filter on each slot</param>
    /// <returns>The list of player slots with no cards</returns>
    public static List<CardSlot> GetPlayerOpenSlots(
        this BoardManager manager,
        Predicate<CardSlot> filterOnPredicate = null
    )
    {
        return manager.PlayerSlotsCopy.SelectOpenSlots(filterOnPredicate).ToList();
    }

    /// <summary>
    /// Retrieve all opponent cards on the board.
    /// </summary>
    /// <param name="manager">Manager instance to access</param>
    /// <param name="filterOnPredicate">The predicate to filter on each card</param>
    /// <returns>The list of opponent cards on the board</returns>
    public static List<PlayableCard> GetOpponentCards(
        this BoardManager manager,
        Predicate<PlayableCard> filterOnPredicate = null
    )
    {
        return manager.OpponentSlotsCopy.SelectCards(filterOnPredicate).ToList();
    }

    /// <summary>
    /// Retrieve all opponent slots that are not occupied by a card.
    /// </summary>
    /// <param name="manager">Manager instance to access</param>
    /// <param name="filterOnPredicate">The predicate to filter on each slot</param>
    /// <returns>The list of opponent slots with no cards</returns>
    public static List<CardSlot> GetOpponentOpenSlots(
        this BoardManager manager,
        Predicate<CardSlot> filterOnPredicate = null
    )
    {
        return manager.OpponentSlotsCopy.SelectOpenSlots(filterOnPredicate).ToList();
    }
}