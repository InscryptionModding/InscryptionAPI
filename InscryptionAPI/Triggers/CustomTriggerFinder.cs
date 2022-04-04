using DiskCardGame;
using InscryptionAPI.Card;

namespace InscryptionAPI.Triggers;

/// <summary>
/// Finds custom trigger recievers that exists on the board/in the hand
/// Always excludes facedowns by default, use <see cref="IActivateWhenFacedown"/> to alter this default
/// </summary>
public static class CustomTriggerFinder
{
    /// <summary>
    /// Finds all trigger recievers, on the board and in the hand
    /// </summary>
    /// <param name="excluding">Card to exclude from the hand search</param>
    /// <typeparam name="T">The trigger type to search for</typeparam>
    /// <returns>All trigger recievers of type T</returns>
    public static IEnumerable<T> FindGlobalTriggers<T>(PlayableCard excluding = null)
    {
        var result = Enumerable.Empty<T>();

        if (BoardManager.Instance)
        {
            IEnumerable<PlayableCard> cards = BoardManager.Instance.CardsOnBoard;
            
            result = result.Concat(cards.SelectMany(FindTriggersOnCard<T>));
        }

        if (PlayerHand.Instance)
        {
            result = result.Concat(FindTriggersInHandExcluding<T>(excluding));
        }

        return result;
    }

    /// <summary>
    /// Find all trigger recievers in the hand
    /// </summary>
    /// <typeparam name="T">The trigger type to search for</typeparam>
    /// <returns>All trigger recievers of type T in the hand</returns>
    public static IEnumerable<T> FindTriggersInHand<T>()
    {
        return PlayerHand.Instance.CardsInHand.SelectMany(FindTriggersOnCard<T>);
    }
    /// <summary>
    /// Find all trigger recievers in the hand
    /// </summary>
    /// <param name="card">Card to exclude from the search</param>
    /// <typeparam name="T">The trigger type to search for</typeparam>
    /// <returns>All trigger recievers of type T in the hand</returns>
    public static IEnumerable<T> FindTriggersInHandExcluding<T>(PlayableCard card)
    {
        return PlayerHand.Instance.CardsInHand.Where(x => x != card).SelectMany(FindTriggersOnCard<T>);
    }

    /// <summary>
    /// Finds all trigger recievers on a card
    /// </summary>
    /// <param name="card">The card to search</param>
    /// <typeparam name="T">The type of reciever to search for</typeparam>
    /// <returns>All trigger recievers of type T on the card</returns>
    public static IEnumerable<T> FindTriggersOnCard<T>(PlayableCard card)
    {
        var tType = typeof(T);
        foreach (var recv in card.TriggerHandler.GetAllReceivers().OfType<T>())
        {
            if (!card.FaceDown || (recv is IActivateWhenFacedown fd && fd.ShouldTriggerCustomWhenFaceDown(tType)))
                yield return recv;
        }
    }
}
