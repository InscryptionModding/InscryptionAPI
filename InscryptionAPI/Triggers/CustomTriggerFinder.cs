using DiskCardGame;
using InscryptionAPI.Card;
using System.Collections;

namespace InscryptionAPI.Triggers;

/// <summary>
/// Finds custom trigger recievers that exists on the board/in the hand
/// Always excludes facedowns by default, use <see cref="IActivateWhenFacedown"/> to alter this default
/// </summary>
public static class CustomTriggerFinder
{
    public static IEnumerator CustomTriggerSequence(this TriggerReceiver receiver, IEnumerator triggerCoroutine)
    {
        GlobalTriggerHandler self = GlobalTriggerHandler.Instance;
        if (self != null)
        {
            self.NumTriggersThisBattle += 1;
            self.StackSize += 1;
            receiver.Activating = true;
            yield return triggerCoroutine;
            self.StackSize -= 1;
            receiver.Activating = false;
            if (receiver.DestroyAfterActivation)
            {
                receiver.Destroy();
            }
        }
        yield break;
    }

    #region Triggering
    public static IEnumerator TriggerAll<T>(bool triggerFacedown, Func<T, bool> respond, Func<T, IEnumerator> trigger)
    {
        var all = FindGlobalTriggers<T>(triggerFacedown);
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                yield return CustomTriggerSequence(trigg as TriggerReceiver, trigger(trigg));
            }
        }
        if (!triggerFacedown)
        {
        }
        yield break;
    }

    public static IEnumerator Trigger<T>(this CardTriggerHandler handler, Func<T, bool> respond, Func<T, IEnumerator> trigger)
    {
        var all = FindTriggersOnCard<T>(handler);
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                yield return CustomTriggerSequence(trigg as TriggerReceiver, trigger(trigg));
            }
        }
        yield break;
    }

    public static IEnumerator TriggerOnBoard<T>(bool triggerFacedown, Func<T, bool> respond, Func<T, IEnumerator> trigger)
    {
        var all = FindTriggersOnBoard<T>(triggerFacedown);
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                yield return CustomTriggerSequence(trigg as TriggerReceiver, trigger(trigg));
            }
        }
        yield break;
    }

    public static IEnumerator TriggerInHand<T>(Func<T, bool> respond, Func<T, IEnumerator> trigger)
    {
        var all = FindTriggersInHand<T>();
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                yield return CustomTriggerSequence(trigg as TriggerReceiver, trigger(trigg));
            }
        }
        yield break;
    }
    #endregion

    #region DataCollection
    public static List<(TriggerReceiver, T2)> CollectDataAll<T, T2>(bool collectFromFacedown, Func<T, bool> respond, Func<T, T2> collect)
    {
        List<(TriggerReceiver, T2)> ret = new();
        var all = FindGlobalTriggers<T>(collectFromFacedown);
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                ret.Add((trigg as TriggerReceiver, collect(trigg)));
            }
        }
        return ret;
    }

    public static List<(TriggerReceiver, T2)> CollectDataOnBoard<T, T2>(bool collectFromFacedown, Func<T, bool> respond, Func<T, T2> collect)
    {
        List<(TriggerReceiver, T2)> ret = new();
        var all = FindTriggersOnBoard<T>(collectFromFacedown);
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                ret.Add((trigg as TriggerReceiver, collect(trigg)));
            }
        }
        return ret;
    }

    public static List<(TriggerReceiver, T2)> CollectDataInHand<T, T2>(Func<T, bool> respond, Func<T, T2> collect)
    {
        List<(TriggerReceiver, T2)> ret = new();
        var all = FindTriggersInHand<T>();
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                ret.Add((trigg as TriggerReceiver, collect(trigg)));
            }
        }
        return ret;
    }

    public static List<(TriggerReceiver, T2)> CollectData<T, T2>(this CardTriggerHandler self, Func<T, bool> respond, Func<T, T2> collect)
    {
        List<(TriggerReceiver, T2)> ret = new();
        var all = FindTriggersOnCard<T>(self);
        foreach (T trigg in all)
        {
            if (respond(trigg) && (trigg as TriggerReceiver) != null)
            {
                ret.Add((trigg as TriggerReceiver, collect(trigg)));
            }
        }
        return ret;
    }
    #endregion

    /// <summary>
    /// Finds all trigger recievers, on the board and in the hand
    /// </summary>
    /// <param name="excluding">Card to exclude from the hand search</param>
    /// <typeparam name="T">The trigger type to search for</typeparam>
    /// <returns>All trigger recievers of type T</returns>
    public static IEnumerable<T> FindGlobalTriggers<T>(bool findFacedown, PlayableCard excluding = null)
    {
        var result = Enumerable.Empty<T>();

        if (BoardManager.Instance)
        {
            result = result.Concat(FindTriggersOnBoard<T>(findFacedown));
        }

        if (PlayerHand.Instance)
        {
            result = result.Concat(FindTriggersInHandExcluding<T>(excluding));
        }

        return result;
    }

    /// <summary>
    /// Finds all trigger recievers, on the board and in the hand
    /// </summary>
    /// <param name="excluding">Card to exclude from the hand search</param>
    /// <typeparam name="T">The trigger type to search for</typeparam>
    /// <returns>All trigger recievers of type T</returns>
    public static IEnumerable<T> FindGlobalTriggers<T>(PlayableCard excluding = null)
    {
        return FindGlobalTriggers<T>(true, excluding);
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
    /// <typeparam name="T">The trigger type to search for</typeparam>
    /// <returns>All trigger recievers of type T in the hand</returns>
    public static IEnumerable<T> FindTriggersOnBoard<T>()
    {
        return FindTriggersOnBoard<T>(true);
    }

    /// <summary>
    /// Find all trigger recievers in the hand
    /// </summary>
    /// <typeparam name="T">The trigger type to search for</typeparam>
    /// <returns>All trigger recievers of type T in the hand</returns>
    public static IEnumerable<T> FindTriggersOnBoard<T>(bool findFacedown)
    {
        return GlobalTriggerHandler.Instance.nonCardReceivers.Where(x => x.TriggerBeforeCards).OfType<T>().Concat(BoardManager.Instance.CardsOnBoard.Where(x => !x.FaceDown || findFacedown).SelectMany(FindTriggersOnCard<T>))
            .Concat(GlobalTriggerHandler.Instance.nonCardReceivers.Where(x => !x.TriggerBeforeCards).OfType<T>())
            .Concat(BoardManager.Instance.CardsOnBoard.Where(x => x.FaceDown && !findFacedown).SelectMany(FindTriggersOnCard<T>)
            .Where(x => x is IActivateWhenFacedown && (x as IActivateWhenFacedown).ShouldTriggerCustomWhenFaceDown(typeof(T))));
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
            yield return recv;
        }
    }

    /// <summary>
    /// Finds all trigger recievers on a card
    /// </summary>
    /// <param name="card">The card to search</param>
    /// <typeparam name="T">The type of reciever to search for</typeparam>
    /// <returns>All trigger recievers of type T on the card</returns>
    public static IEnumerable<T> FindTriggersOnCard<T>(CardTriggerHandler card)
    {
        var tType = typeof(T);
        foreach (var recv in card.GetAllReceivers().OfType<T>())
        {
            yield return recv;
        }
    }
}
