using DiskCardGame;

namespace InscryptionAPI.Helpers.Extensions;

public static class BoardManagerExtensions
{
    /// <summary>
    /// Retrieve all player cards on the board.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each card.</param>
    /// <returns>The list of player cards on the board.</returns>
    public static List<PlayableCard> GetPlayerCards(
        this BoardManager manager,
        Predicate<PlayableCard> filterOnPredicate = null
    ) => manager.PlayerSlotsCopy.SelectCards(filterOnPredicate).ToList();

    /// <summary>
    /// Retrieve all opponent cards on the board.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each card.</param>
    /// <returns>The list of opponent cards on the board.</returns>
    public static List<PlayableCard> GetOpponentCards(
        this BoardManager manager,
        Predicate<PlayableCard> filterOnPredicate = null
    ) => manager.OpponentSlotsCopy.SelectCards(filterOnPredicate).ToList();

    /// <summary>
    /// Retrieve all player or opponent cards on the board.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="getPlayerCards">Whether to retrieve player cards.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each card.</param>
    /// <returns>The list of relevant cards on the board.</returns>
    public static List<PlayableCard> GetCards(
        this BoardManager manager,
        bool getPlayerCards,
        Predicate<PlayableCard> filterOnPredicate = null
    ) => manager.GetSlotsCopy(getPlayerCards).SelectCards(filterOnPredicate).ToList();

    /// <summary>
    /// Retrieve all player slots that are not occupied by a card.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each slot.</param>
    /// <returns>The list of player slots with no cards.</returns>
    public static List<CardSlot> GetPlayerOpenSlots(
        this BoardManager manager,
        Predicate<CardSlot> filterOnPredicate = null
    ) => manager.PlayerSlotsCopy.SelectOpenSlots(filterOnPredicate).ToList();

    /// <summary>
    /// Retrieve all opponent slots that are not occupied by a card.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each slot.</param>
    /// <returns>The list of opponent slots with no cards.</returns>
    public static List<CardSlot> GetOpponentOpenSlots(
        this BoardManager manager,
        Predicate<CardSlot> filterOnPredicate = null
    ) => manager.OpponentSlotsCopy.SelectOpenSlots(filterOnPredicate).ToList();

    /// <summary>
    /// Retrieve all slots on the player or opponent's side of the board that are not occupied by a card.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="getPlayerSlots">Whether to retrieve player slots.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each slot.</param>
    /// <returns>The list of relevant card slots with no cards.</returns>
    public static List<CardSlot> GetOpenSlots(
        this BoardManager manager,
        bool getPlayerSlots,
        Predicate<CardSlot> filterOnPredicate = null
    ) => manager.GetSlotsCopy(getPlayerSlots).SelectOpenSlots(filterOnPredicate).ToList();

    /// <summary>
    /// Retrieve a copy of the board slots for the player or opponent's side of the board.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="getPlayerSlotsCopy">Whether to retrieve PlayerSlotsCopy or OpponentSlotsCopy.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each slot.</param>
    /// <returns>The list corresponding to the player or opponent's side of the board.</returns>
    public static List<CardSlot> GetSlotsCopy(this BoardManager manager, bool getPlayerSlotsCopy)
        => getPlayerSlotsCopy ? manager.PlayerSlotsCopy : manager.OpponentSlotsCopy;


    /// <summary>
    /// Retrieve all player or opponent card slots on the board.
    /// </summary>
    /// <param name="manager">Manager instance to access.</param>
    /// <param name="getPlayerCards">Whether to retrieve player card slots.</param>
    /// <param name="filterOnPredicate">The predicate to filter on each card slot.</param>
    /// <returns>The list of relevant card slots on the board.</returns>
    public static List<CardSlot> GetCardSlots(
        this BoardManager manager,
        bool getPlayerCards,
        Predicate<CardSlot> filterOnPredicate = null
    ) => manager.GetSlotsCopy(getPlayerCards).FindAll(filterOnPredicate);
}