using DiskCardGame;

namespace InscryptionAPI.Triggers;

[Flags]
public enum TriggerSearchCategory
{
    NONE = 0,
    BOARD_NO_FACEDOWN = 0b1,
    FACEDOWN = 0b10,
    BOARD_INCLUDING_FACEDOWN = BOARD_NO_FACEDOWN | FACEDOWN,
    HAND = 0b100,
    ALL = BOARD_INCLUDING_FACEDOWN | HAND,
}

public static class CustomTriggerFinder
{
    private static bool IncludesAny(this TriggerSearchCategory search, TriggerSearchCategory category) => (search & category) != TriggerSearchCategory.NONE;

    public static IEnumerable<T> FindGlobalTriggers<T>(TriggerSearchCategory search = TriggerSearchCategory.BOARD_NO_FACEDOWN, PlayableCard excluding = null)
    {
        var result = Enumerable.Empty<T>();

        if (search.IncludesAny(TriggerSearchCategory.BOARD_INCLUDING_FACEDOWN) && BoardManager.Instance)
        {
            IEnumerable<PlayableCard> cards = BoardManager.Instance.CardsOnBoard;
            
            if (!search.IncludesAny(TriggerSearchCategory.FACEDOWN))
                cards = cards.Where(x => !x.FaceDown);
            else if (!search.IncludesAny(TriggerSearchCategory.BOARD_NO_FACEDOWN))
                cards = cards.Where(x => x.FaceDown);
            
            result = result.Concat(cards.SelectMany(FindTriggersOnCard<T>));
        }

        if (search.IncludesAny(TriggerSearchCategory.HAND) && PlayerHand.Instance)
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
