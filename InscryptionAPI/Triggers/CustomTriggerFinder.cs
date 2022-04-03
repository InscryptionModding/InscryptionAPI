using DiskCardGame;

namespace InscryptionAPI.Triggers;

public static class CustomTriggerFinder
{
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

    public static IEnumerable<T> FindTriggersInHand<T>()
    {
        return PlayerHand.Instance.CardsInHand.SelectMany(FindTriggersOnCard<T>);
    }

    public static IEnumerable<T> FindTriggersInHandExcluding<T>(PlayableCard card)
    {
        return PlayerHand.Instance.CardsInHand.Where(x => x != card).SelectMany(FindTriggersOnCard<T>);
    }

    public static IEnumerable<T> FindTriggersOnCard<T>(PlayableCard card)
    {
        return card.TriggerHandler.GetAllReceivers().OfType<T>();
    }
}
