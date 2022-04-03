using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using System.Reflection;
using System.Collections;
using InscryptionAPI.Card;

namespace InscryptionAPI.Triggers
{
    public static class CustomGlobalTriggerHandler
    {
        #region Triggering
        public static IEnumerator TriggerAll<T>(bool triggerFacedown, Func<T, bool> respond, Func<T, IEnumerator> trigger)
        {
            List<T> all = GetAll<T>(triggerFacedown);
            foreach(T trigg in all)
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
            List<T> all = handler.GetReceivers<T>();
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
            List<T> all = GetOnBoard<T>(triggerFacedown);
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
            List<T> all = GetInHand<T>();
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
            List<T> all = GetAll<T>(collectFromFacedown);
            foreach (T trigg in all)
            {
                if(respond(trigg) && (trigg as TriggerReceiver) != null)
                {
                    ret.Add((trigg as TriggerReceiver, collect(trigg)));
                }
            }
            return ret;
        }

        public static List<(TriggerReceiver, T2)> CollectDataOnBoard<T, T2>(bool collectFromFacedown, Func<T, bool> respond, Func<T, T2> collect)
        {
            List<(TriggerReceiver, T2)> ret = new();
            List<T> all = GetOnBoard<T>(collectFromFacedown);
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
            List<T> all = GetInHand<T>();
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
            List<T> all = self.GetReceivers<T>();
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

        #region Getting
        public static List<T> GetAll<T>(bool getFacedown)
        {
            List<T> ret = new(GetOnBoard<T>(getFacedown));
            ret.AddRange(GetInHand<T>());
            return ret;
        }

        public static List<T> GetOnBoard<T>(bool getFacedown)
        {
            List<T> ret = new();
            if(BoardManager.Instance != null && BoardManager.Instance.CardsOnBoard != null)
            {
                ret.AddRange(GetNonCardReceivers<T>(true));
                List<PlayableCard> list = new(Singleton<BoardManager>.Instance.CardsOnBoard);
                List<T> toAddLater = new();
                foreach (PlayableCard playableCard in list)
                {
                    if (playableCard != null && (!playableCard.FaceDown || getFacedown) && playableCard.TriggerHandler != null)
                    {
                        ret.AddRange(playableCard.TriggerHandler.GetReceivers<T>());
                    }
                    else if(playableCard != null && playableCard.FaceDown && !getFacedown && playableCard.TriggerHandler != null)
                    {
                        toAddLater.AddRange(playableCard.TriggerHandler.GetReceivers<T>(true));
                    }
                }
                ret.AddRange(GetNonCardReceivers<T>(false));
                ret.AddRange(toAddLater); // to mimic how facedown triggers marked with IActivateWhenFacedown activate later than anything else. for consistency
            }
            return ret;
        }

        public static List<T> GetInHand<T>()
        {
            List<T> ret = new();
            if(PlayerHand.Instance != null && PlayerHand.Instance.CardsInHand != null)
            {
                List<PlayableCard> list = new(PlayerHand.Instance.CardsInHand);
                foreach (PlayableCard playableCard in list)
                {
                    if (playableCard != null && playableCard.TriggerHandler != null)
                    {
                        ret.AddRange(playableCard.TriggerHandler.GetReceivers<T>());
                    }
                }
            }
            return ret;
        }

        public static List<T> GetReceivers<T>(this CardTriggerHandler self, bool getFacedownReceivers = false)
        {
            List<T> ret = new();
            if(self != null)
            {
                foreach (TriggerReceiver receiver in self.GetAllReceivers())
                {
                    if (receiver != null && receiver is T && (!getFacedownReceivers || (receiver is IActivateWhenFacedown && (receiver as IActivateWhenFacedown).ShouldCustomTriggerFaceDown<T>())))
                    {
                        ret.Add((T)(object)receiver);
                    }
                }
            }
            return ret;
        }

        public static List<T> GetNonCardReceivers<T>(bool beforeCards)
        {
            List<T> ret = new();
            if(GlobalTriggerHandler.Instance != null && GlobalTriggerHandler.Instance.nonCardReceivers != null)
            {
                foreach (NonCardTriggerReceiver nonCardTriggerReceiver in GlobalTriggerHandler.Instance.nonCardReceivers)
                {
                    if (nonCardTriggerReceiver != null && nonCardTriggerReceiver.TriggerBeforeCards == beforeCards && nonCardTriggerReceiver is T)
                    {
                        ret.Add((T)(object)nonCardTriggerReceiver);
                    }
                }
            }
            return ret;
        }
        #endregion

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
    }
}
