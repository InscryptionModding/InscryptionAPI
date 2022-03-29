using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using System.Reflection;
using System.Collections;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;

namespace InscryptionAPI.Triggers
{
    public static class CustomGlobalTriggerHandler
    {
        internal enum TriggerType
        {
            Responder = 0,
            Coroutine = 1,
            DataCollector = 2
        }

        /// <summary>
        /// Registers a new custom trigger that can process a specific form of method
        /// </summary>
        /// <param name="pluginGuid">The GUID of the mod creating the custom trigger</param>
        /// <param name="triggerName">The name of the custom trigger</param>
        /// <param name="method">The definition of the method that this trigger expects to respond to</param>
        public static CustomTrigger RegisterCustomTrigger(string pluginGuid, string triggerName, MethodInfo method)
        {
            CustomTrigger trigger = GuidManager.GetEnumValue<CustomTrigger>(pluginGuid, triggerName);
            CustomTriggerArgHolder.customArgsForTriggers.Add(trigger, method);
            return trigger;
        }

        private static bool IsDecoratedFor(this MethodInfo method, TriggerType triggerType, CustomTrigger trigger)
        {
            if (triggerType == TriggerType.Responder)
                return method.GetCustomAttributes(true).ToList().Exists(x => x != null && x is CustomTriggerResponderAttribute ctra && ctra.trigger == trigger);
            if (triggerType == TriggerType.DataCollector)
                return method.GetCustomAttributes(true).ToList().Exists(x => x != null && x is CustomTriggerDataAttribute ctda && ctda.trigger == trigger);
            
            return method.GetCustomAttributes(true).ToList().Exists(x => x != null && x is CustomTriggerCoroutineAttribute ctca && ctca.trigger == trigger);
        }

        private static bool IsValidTriggerMethod(MethodInfo method, CustomTrigger trigger, TriggerType triggerType)
        {
            if (method != null)
            {
                if (CustomTriggerArgHolder.TryGetReturnType(trigger, out Type returnType)) // Only continue if we know about this thing
                {
                    // It's only a valid trigger in this context if it returns IEnumerator
                    // If you somehow try to fire the whole global trigger handler for a data trigger response (i.e., not a coroutine response)
                    // we just return false.
                    //
                    // In other words - even if the method itself returns IEnumerator, we still return false if the 
                    // MethodInfo for this custom trigger does NOT return IEnumerator
                    if (triggerType == TriggerType.Coroutine && !(returnType == typeof(IEnumerator) || returnType.IsSubclassOf(typeof(IEnumerator)) || returnType.GetInterfaces().Contains(typeof(IEnumerator))))
                        return false;

                    if ((triggerType == TriggerType.Responder && method.ReturnType == typeof(bool)) || (triggerType != TriggerType.Responder && (method.ReturnType == returnType || method.ReturnType.IsSubclassOf(returnType) || method.ReturnType.GetInterfaces().Contains(returnType))))
                    {
                        if (method.IsDecoratedFor(triggerType, trigger))
                        {
                            if (CustomTriggerArgHolder.TryGetArgs(trigger, out var types))
                            {
                                ParameterInfo[] parameters = method.GetParameters();
                                if (parameters.Length == types.Count && types.SequenceEqual(parameters.Where(x => x != null).Select(x => x.ParameterType)))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool ReceiverRespondsToCustomTrigger(CustomTrigger trigger, TriggerReceiver receiver, params object[] otherArgs)
        {
            otherArgs ??= Array.Empty<object>();
            if (receiver != null)
            {
                try
                {
                    MethodInfo respondTo = receiver.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(x => IsValidTriggerMethod(x, trigger, TriggerType.Responder));
                    return respondTo != null && otherArgs != null && respondTo.GetParameters().Length == otherArgs.Length && (bool)respondTo.Invoke(receiver, otherArgs);
                }
                catch { }
            }
            return false;
        }

        private static IEnumerator CustomTriggerSequence(TriggerReceiver receiver, IEnumerator triggerCoroutine)
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

        private static IEnumerator CustomTriggerSequence(CustomTrigger trigger, TriggerReceiver receiver, params object[] otherArgs)
        {
            otherArgs ??= Array.Empty<object>();
            if (GlobalTriggerHandler.Instance != null)
            {
                if (receiver != null)
                {
                    MethodInfo respondTo = receiver.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(x => IsValidTriggerMethod(x, trigger, TriggerType.Coroutine));
                    if (respondTo != null && otherArgs != null && respondTo.GetParameters().Length == otherArgs.Length)
                    {
                        bool success = true;
                        IEnumerator rat = default;
                        try
                        {
                            rat = (IEnumerator)respondTo.Invoke(receiver, otherArgs);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            InscryptionAPIPlugin.Logger.LogError(ex);
                        }
                        if (success)
                        {
                            yield return CustomTriggerSequence(receiver, rat);
                        }
                    }
                }
            }
            yield break;
        }

        private static T GetDataFromReceiver<T>(CustomTrigger trigger, TriggerReceiver receiver, params object[] otherArgs)
        {
            otherArgs ??= Array.Empty<object>();
            T retval = default(T);
            if (GlobalTriggerHandler.Instance != null)
            {
                if (receiver != null)
                {
                    MethodInfo respondTo = receiver.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(x => IsValidTriggerMethod(x, trigger, TriggerType.DataCollector));
                    if (respondTo != null && otherArgs != null && respondTo.GetParameters().Length == otherArgs.Length)
                    {
                        try
                        {
                            retval = (T)respondTo.Invoke(receiver, otherArgs);
                        }
                        catch (Exception ex)
                        {
                            InscryptionAPIPlugin.Logger.LogError(ex);
                        }
                    }
                }
            }
            return retval;
        }

        /// <summary>
        /// Asks a specific card if it responds to the given trigger
        /// </summary>
        /// <param name="cth">The trigger handler for a specific card</param>
        /// <param name="trigger">The triggering event</param>
        /// <param name="otherArgs">Arguments associated with this trigger. These must match the parameters of the MethodInfo associated with this trigger</param>
        /// <returns>A boolean indicating if the card responds to the given custom trigger</returns>
        public static bool RespondsToCustomTrigger(this CardTriggerHandler cth, CustomTrigger trigger, params object[] otherArgs)
        {
            foreach (TriggerReceiver receiver in cth.GetAllReceivers())
            {
                if (ReceiverRespondsToCustomTrigger(trigger, receiver, otherArgs))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Asks a specific card if it responds to the given trigger, then executes the coroutine for that trigger if the answer is yes
        /// </summary>
        /// <param name="cth">The trigger handler for a specific card</param>
        /// <param name="trigger">The triggering event</param>
        /// <param name="otherArgs">Arguments associated with this trigger. These must match the parameters of the MethodInfo associated with this trigger</param>
        /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
        public static IEnumerator OnCustomTrigger(this CardTriggerHandler cth, CustomTrigger trigger, params object[] otherArgs)
        {
            foreach (TriggerReceiver receiver in cth.GetAllReceivers())
            {
                if (ReceiverRespondsToCustomTrigger(trigger, receiver, otherArgs))
                {
                    yield return CustomTriggerSequence(trigger, receiver, otherArgs);
                }
            }
            yield break;
        }

        public static List<T> GetDataFromCustomTrigger<T>(this CardTriggerHandler cth, CustomTrigger trigger, params object[] otherArgs)
        {
            List<T> retval = null;
            foreach (TriggerReceiver receiver in cth.GetAllReceivers())
                if (ReceiverRespondsToCustomTrigger(trigger, receiver, otherArgs))
                    (retval ??= new()).Add(GetDataFromReceiver<T>(trigger, receiver, otherArgs));
            return retval;
        }

        /// <summary>
        /// Asks each card on the board AND in hand if it responds to the given trigger, then executes the coroutine for that trigger if the answer is yes
        /// </summary>
        /// <param name="trigger">The triggering event</param>
        /// <param name="triggerFacedown">Indicates whether or not facedown cards should respond to this trigger. Note that if this is false, triggers that explicitly force themselves to fire when facedown will still fire.</param>
        /// <param name="otherArgs">Arguments associated with this trigger. These must match the parameters of the MethodInfo associated with this trigger</param>
        /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
        public static IEnumerator CustomTriggerAll(CustomTrigger trigger, bool triggerFacedown, params object[] otherArgs)
        {
            yield return CustomTriggerCardsOnBoard(trigger, triggerFacedown, otherArgs);
            yield return CustomTriggerCardsInHand(trigger, otherArgs);
            yield break;
        }

        /// <summary>
        /// Asks each card on the board if it responds to the given trigger, then executes the coroutine for that trigger if the answer is yes
        /// </summary>
        /// <param name="trigger">The triggering event</param>
        /// <param name="triggerFacedown">Indicates whether or not facedown cards should respond to this trigger. Note that if this is false, triggers that explicitly force themselves to fire when facedown will still fire.</param>
        /// <param name="otherArgs">Arguments associated with this trigger. These must match the parameters of the MethodInfo associated with this trigger</param>
        /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
        private static IEnumerator CustomTriggerCardsOnBoard(CustomTrigger trigger, bool triggerFacedown, params object[] otherArgs)
        {
            if (trigger == CustomTrigger.None)
                yield break;

            if (BoardManager.Instance != null && BoardManager.Instance.CardsOnBoard != null)
            {
                yield return CustomTriggerNonCardReceivers(true, trigger, otherArgs);

                List<PlayableCard> list = new(BoardManager.Instance.CardsOnBoard);

                foreach (PlayableCard playableCard in list)
                    if (playableCard != null && (!playableCard.FaceDown || triggerFacedown) && playableCard.TriggerHandler.RespondsToCustomTrigger(trigger, otherArgs))
                        yield return playableCard.TriggerHandler.OnCustomTrigger(trigger, otherArgs);

                yield return CustomTriggerNonCardReceivers(false, trigger, otherArgs);

                if (!triggerFacedown)
                {                  
                    foreach (PlayableCard playableCard in BoardManager.Instance.CardsOnBoard)
                        if (playableCard != null && playableCard.FaceDown)
                            foreach (TriggerReceiver receiver in playableCard.TriggerHandler.GetAllReceivers())
                                if (ReceiverRespondsToCustomTrigger(trigger, receiver, otherArgs) && ((receiver is IActivateWhenFacedown iawf && iawf.ShouldCustomTriggerFaceDown(trigger, otherArgs))))
                                    yield return CustomTriggerSequence(trigger, receiver, otherArgs);
                }
            }
            yield break;
        }

        /// <summary>
        /// Asks each card on the board if it responds to the given trigger, then gets the data from that trigger if the answer is yes
        /// </summary>
        /// <param name="trigger">The triggering event</param>
        /// <param name="triggerFacedown">Indicates whether or not facedown cards should respond to this trigger. Note that if this is false, triggers that explicitly force themselves to fire when facedown will still fire.</param>
        /// <param name="otherArgs">Arguments associated with this trigger. These must match the parameters of the MethodInfo associated with this trigger</param>
        /// <returns>The data for the trigger</returns>
        public static List<T> GetDataFromAllCardsOnBoard<T>(CustomTrigger trigger, bool triggerFacedown, params object[] otherArgs)
        {
            List<T> retval = null;

            if (trigger == CustomTrigger.None)
                return retval;

            if (BoardManager.Instance != null && BoardManager.Instance.CardsOnBoard != null)
            {
                List<PlayableCard> list = new(BoardManager.Instance.CardsOnBoard);
                foreach (PlayableCard playableCard in list)
                    if (playableCard != null && (!playableCard.FaceDown || triggerFacedown) && playableCard.TriggerHandler.RespondsToCustomTrigger(trigger, otherArgs))
                        (retval ??= new()).AddRange(playableCard.TriggerHandler.GetDataFromCustomTrigger<T>(trigger, otherArgs));

                if (!triggerFacedown)
                {
                    foreach (PlayableCard playableCard in BoardManager.Instance.CardsOnBoard)
                        if (playableCard != null && playableCard.FaceDown)
                            foreach (TriggerReceiver receiver in playableCard.TriggerHandler.GetAllReceivers())
                                if (ReceiverRespondsToCustomTrigger(trigger, receiver, otherArgs) && (receiver is IActivateWhenFacedown iawf && iawf.ShouldCustomTriggerFaceDown(trigger, otherArgs)))
                                    (retval ??= new()).AddRange(playableCard.TriggerHandler.GetDataFromCustomTrigger<T>(trigger, otherArgs));        
                }
            }

            return retval;
        }

        /// <summary>
        /// Asks each card in the player's hand if it responds to the given trigger, then executes the coroutine for that trigger if the answer is yes
        /// </summary>
        /// <param name="trigger">The triggering event</param>
        /// <param name="otherArgs">Arguments associated with this trigger. These must match the parameters of the MethodInfo associated with this trigger</param>
        /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
        public static IEnumerator CustomTriggerCardsInHand(CustomTrigger trigger, params object[] otherArgs)
        {
            if (PlayerHand.Instance != null && PlayerHand.Instance.CardsInHand != null)
            {
                List<PlayableCard> list = new(PlayerHand.Instance.CardsInHand);
                foreach (PlayableCard playableCard in list)
                {
                    if (playableCard != null && playableCard.TriggerHandler.RespondsToCustomTrigger(trigger, otherArgs))
                    {
                        yield return playableCard.TriggerHandler.OnCustomTrigger(trigger, otherArgs);
                    }
                }
            }
            yield break;
        }

        /// <summary>
        /// Asks each non-card trigger receiver in the global space if it responds to the given custom trigger, then executes the coroutine for that trigger if the answer is yes
        /// </summary>
        /// <param name="trigger">The triggering event</param>
        /// <param name="otherArgs">Arguments associated with this trigger. These must match the parameters of the MethodInfo associated with this trigger</param>
        /// <returns>An enumeration of Unity actions that create a sequence of events the player sees</returns>
        public static IEnumerator CustomTriggerNonCardReceivers(bool beforeCards, CustomTrigger trigger, params object[] otherArgs)
        {
            foreach (NonCardTriggerReceiver nonCardTriggerReceiver in GlobalTriggerHandler.Instance?.nonCardReceivers ?? new())
            {
                if (nonCardTriggerReceiver != null && nonCardTriggerReceiver.TriggerBeforeCards == beforeCards && ReceiverRespondsToCustomTrigger(trigger, nonCardTriggerReceiver, otherArgs))
                {
                    yield return CustomTriggerSequence(trigger, nonCardTriggerReceiver, otherArgs);
                }
            }
            yield break;
        }
    }
}
