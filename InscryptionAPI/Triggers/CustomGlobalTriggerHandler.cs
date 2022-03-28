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
        public static void RegisterCustomTrigger(string pluginGuid, string triggerName, params Type[] args)
        {
            CustomTriggerArgHolder.customArgsForTriggers.Add((pluginGuid, triggerName), args.ToList());
        }

        private static bool IsValidTriggerMethod(MethodInfo method, TriggerIdentification identification, bool isResponder)
        {
            if (method != null)
            {
                if ((isResponder && method.ReturnType == typeof(bool)) || (!isResponder && (method.ReturnType == typeof(IEnumerator) || method.ReturnType.IsSubclassOf(typeof(IEnumerator)) ||
                    method.ReturnType.GetInterfaces().Contains(typeof(IEnumerator)))))
                {
                    bool ctca = method.GetCustomAttributes(true).ToList().Exists(x => x != null && x is CustomTriggerCoroutineAttribute && (x as CustomTriggerCoroutineAttribute).identification.Matches(identification));
                    bool ctra = method.GetCustomAttributes(true).ToList().Exists(x => x != null && x is CustomTriggerResponderAttribute && (x as CustomTriggerResponderAttribute).identification.Matches(identification));
                    if ((isResponder && ctra) || (!isResponder && ctca))
                    {
                        if (CustomTriggerArgHolder.TryGetArgs(identification, out var types))
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
            return false;
        }

        private static bool ReceiverRespondsToCustomTrigger(TriggerIdentification identification, TriggerReceiver receiver, params object[] otherArgs)
        {
            otherArgs ??= Array.Empty<object>();
            if (receiver != null)
            {
                try
                {
                    MethodInfo respondTo = receiver.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(x => IsValidTriggerMethod(x, identification, true));
                    return respondTo != null && otherArgs != null && respondTo.GetParameters().Length == otherArgs.Length && (bool)respondTo.Invoke(receiver, otherArgs);
                }
                catch { }
            }
            return false;
        }

        public static bool ReceiverRespondsToCustomTrigger(CustomTrigger trigger, TriggerReceiver receiver, params object[] otherArgs)
        {
            return ReceiverRespondsToCustomTrigger(new TriggerIdentification(trigger), receiver, otherArgs);
        }

        public static bool ReceiverRespondsToCustomTrigger(string pluginGuid, string triggerName, TriggerReceiver receiver, params object[] otherArgs)
        {
            return ReceiverRespondsToCustomTrigger(new TriggerIdentification(pluginGuid, triggerName), receiver, otherArgs);
        }

        public static IEnumerator CustomTriggerSequence(TriggerReceiver receiver, IEnumerator triggerCoroutine)
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

        private static IEnumerator CustomTriggerSequence(TriggerIdentification identification, TriggerReceiver receiver, params object[] otherArgs)
        {
            otherArgs ??= Array.Empty<object>();
            if (GlobalTriggerHandler.Instance != null)
            {
                if (receiver != null)
                {
                    MethodInfo respondTo = receiver.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList().Find(x => IsValidTriggerMethod(x, identification, false));
                    if (respondTo != null && otherArgs != null && respondTo.GetParameters().Length == otherArgs.Length)
                    {
                        bool success = true;
                        IEnumerator rat = default;
                        try
                        {
                            rat = (IEnumerator)respondTo.Invoke(receiver, otherArgs);
                            success = true;
                        }
                        catch { }
                        if (success)
                        {
                            yield return CustomTriggerSequence(receiver, rat);
                        }
                    }
                }
            }
            yield break;
        }

        public static IEnumerator CustomTriggerSequence(CustomTrigger trigger, TriggerReceiver receiver, params object[] otherArgs)
        {
            yield return CustomTriggerSequence(new TriggerIdentification(trigger), receiver, otherArgs);
            yield break;
        }

        public static IEnumerator CustomTriggerSequence(string pluginGuid, string triggerName, TriggerReceiver receiver, params object[] otherArgs)
        {
            yield return CustomTriggerSequence(new TriggerIdentification(pluginGuid, triggerName), receiver, otherArgs);
            yield break;
        }

        private static bool RespondsToCustomTrigger(this CardTriggerHandler cth, TriggerIdentification identification, params object[] otherArgs)
        {
            foreach (TriggerReceiver receiver in cth.GetAllReceivers())
            {
                if (ReceiverRespondsToCustomTrigger(identification, receiver, otherArgs))
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerator OnCustomTrigger(this CardTriggerHandler cth, TriggerIdentification identification, params object[] otherArgs)
        {
            foreach (TriggerReceiver receiver in cth.GetAllReceivers())
            {
                if (ReceiverRespondsToCustomTrigger(identification, receiver, otherArgs))
                {
                    yield return CustomTriggerSequence(identification, receiver, otherArgs);
                }
            }
            yield break;
        }

        public static bool RespondsToCustomTrigger(this CardTriggerHandler cth, CustomTrigger trigger, params object[] otherArgs)
        {
            return RespondsToCustomTrigger(cth, new TriggerIdentification(trigger), otherArgs);
        }

        public static IEnumerator OnCustomTrigger(this CardTriggerHandler cth, CustomTrigger trigger, params object[] otherArgs)
        {
            yield return OnCustomTrigger(cth, new TriggerIdentification(trigger), otherArgs);
            yield break;
        }

        public static bool RespondsToCustomTrigger(this CardTriggerHandler cth, string pluginGuid, string triggerName, params object[] otherArgs)
        {
            return RespondsToCustomTrigger(cth, new TriggerIdentification(pluginGuid, triggerName), otherArgs);
        }

        public static IEnumerator OnCustomTrigger(this CardTriggerHandler cth, string pluginGuid, string triggerName, params object[] otherArgs)
        {
            yield return OnCustomTrigger(cth, new TriggerIdentification(pluginGuid, triggerName), otherArgs);
            yield break;
        }

        public static IEnumerator CustomTriggerAll(CustomTrigger trigger, bool triggerFacedown, params object[] otherArgs)
        {
            yield return CustomTriggerCardsOnBoard(trigger, triggerFacedown, otherArgs);
            yield return CustomTriggerCardsInHand(trigger, otherArgs);
            yield break;
        }

        public static IEnumerator CustomTriggerAll(string pluginGuid, string triggerName, bool triggerFacedown, params object[] otherArgs)
        {
            yield return CustomTriggerCardsOnBoard(pluginGuid, triggerName, triggerFacedown, otherArgs);
            yield return CustomTriggerCardsInHand(pluginGuid, triggerName, otherArgs);
            yield break;
        }

        public static IEnumerator CustomTriggerCardsOnBoard(CustomTrigger trigger, bool triggerFacedown, params object[] otherArgs)
        {
            yield return CustomTriggerCardsOnBoard(new TriggerIdentification(trigger), triggerFacedown, otherArgs);
            yield break;
        }

        public static IEnumerator CustomTriggerCardsOnBoard(string pluginGuid, string triggerName, bool triggerFacedown, params object[] otherArgs)
        {
            yield return CustomTriggerCardsOnBoard(new TriggerIdentification(pluginGuid, triggerName), triggerFacedown, otherArgs);
            yield break;
        }

        private static IEnumerator CustomTriggerCardsOnBoard(TriggerIdentification identification, bool triggerFacedown, params object[] otherArgs)
        {
            if (Singleton<BoardManager>.Instance != null && Singleton<BoardManager>.Instance.CardsOnBoard != null)
            {
                yield return CustomTriggerNonCardReceivers(true, identification, otherArgs);
                List<PlayableCard> list = new(Singleton<BoardManager>.Instance.CardsOnBoard);
                foreach (PlayableCard playableCard in list)
                {
                    if (playableCard != null && (!playableCard.FaceDown || triggerFacedown) && playableCard.TriggerHandler.RespondsToCustomTrigger(identification, otherArgs))
                    {
                        yield return playableCard.TriggerHandler.OnCustomTrigger(identification, otherArgs);
                    }
                }
                yield return CustomTriggerNonCardReceivers(false, identification, otherArgs);
                if (!triggerFacedown)
                {
                    bool ActivatesWhenFaceDown(IActivateWhenFacedown awf)
                    {
                        if (identification.trigger > CustomTrigger.None)
                        {
                            return awf.ShouldCustomTriggerFaceDown(identification.trigger, otherArgs);
                        }
                        if(!string.IsNullOrEmpty(identification.triggerName) && !string.IsNullOrEmpty(identification.pluginGuid))
                        {
                            return awf.ShouldCustomTriggerFaceDown(identification.pluginGuid, identification.triggerName, otherArgs);
                        }
                        return false;
                    }
                    bool RespondsToTrigger(CardTriggerHandler r, TriggerIdentification identification, params object[] otherArgs)
                    {
                        foreach (TriggerReceiver receiver in r.GetAllReceivers())
                        {
                            if (ReceiverRespondsToCustomTrigger(identification, receiver, otherArgs) && ((receiver is IActivateWhenFacedown && ActivatesWhenFaceDown(receiver as IActivateWhenFacedown)) || 
                                (receiver is ExtendedAbilityBehaviour && (receiver as ExtendedAbilityBehaviour).TriggerWhenFacedown)))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    IEnumerator OnTrigger(CardTriggerHandler r, TriggerIdentification identification, params object[] otherArgs)
                    {
                        foreach (TriggerReceiver receiver in r.GetAllReceivers())
                        {
                            if (ReceiverRespondsToCustomTrigger(identification, receiver, otherArgs) && ((receiver is IActivateWhenFacedown && ActivatesWhenFaceDown(receiver as IActivateWhenFacedown)) ||
                                (receiver is ExtendedAbilityBehaviour && (receiver as ExtendedAbilityBehaviour).TriggerWhenFacedown)))
                            {
                                yield return CustomTriggerSequence(identification, receiver, otherArgs);
                            }
                        }
                        yield break;
                    }
                    List<PlayableCard> list2 = new(Singleton<BoardManager>.Instance.CardsOnBoard);
                    foreach (PlayableCard playableCard in list2)
                    {
                        if (playableCard != null && playableCard.FaceDown && RespondsToTrigger(playableCard.TriggerHandler, identification, otherArgs))
                        {
                            yield return OnTrigger(playableCard.TriggerHandler, identification, otherArgs);
                        }
                    }
                }
            }
            yield break;
        }

        public static IEnumerator CustomTriggerCardsInHand(CustomTrigger trigger, params object[] otherArgs)
        {
            yield return CustomTriggerCardsInHand(new TriggerIdentification(trigger), otherArgs);
            yield break;
        }

        public static IEnumerator CustomTriggerCardsInHand(string pluginGuid, string triggerName, params object[] otherArgs)
        {
            yield return CustomTriggerCardsInHand(new TriggerIdentification(pluginGuid, triggerName), otherArgs);
            yield break;
        }

        private static IEnumerator CustomTriggerCardsInHand(TriggerIdentification identification, params object[] otherArgs)
        {
            if (Singleton<PlayerHand>.Instance != null && Singleton<PlayerHand>.Instance.CardsInHand != null)
            {
                List<PlayableCard> list = new(Singleton<PlayerHand>.Instance.CardsInHand);
                foreach (PlayableCard playableCard in list)
                {
                    if (playableCard != null && playableCard.TriggerHandler.RespondsToCustomTrigger(identification, otherArgs))
                    {
                        yield return playableCard.TriggerHandler.OnCustomTrigger(identification, otherArgs);
                    }
                }
            }
            yield break;
        }

        private static IEnumerator CustomTriggerNonCardReceivers(bool beforeCards, TriggerIdentification identification, params object[] otherArgs)
        {
            foreach (NonCardTriggerReceiver nonCardTriggerReceiver in GlobalTriggerHandler.Instance?.nonCardReceivers ?? new())
            {
                if (nonCardTriggerReceiver != null && nonCardTriggerReceiver.TriggerBeforeCards == beforeCards && ReceiverRespondsToCustomTrigger(identification, nonCardTriggerReceiver, otherArgs))
                {
                    yield return CustomTriggerSequence(identification, nonCardTriggerReceiver, otherArgs);
                }
            }
            yield break;
        }
    }
}
