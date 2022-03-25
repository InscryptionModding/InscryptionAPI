using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Card
{
    public enum CustomTrigger
    {
        None,
        OnAddedToHand,
        OnOtherCardAddedToHand,
        OnBellRung,
        OnPreSlotAttackSequence,
        OnPostSlotAttackSequence,
        OnPostSingularSlotAttackSlot,
        OnPreScalesChanged,
        OnPostScalesChanged,
        OnUpkeepInHand,
        OnCardAssignedToSlotNoResolve,
        OnOtherCardResolveInHand,
        OnTurnEndInHand,
        OnOtherCardAssignedToSlotInHand,
        OnOtherCardPreDeathInHand,
        OnOtherCardDealtDamageInHand,
        OnOtherCardDieInHand        
    }

    internal class TriggerIdentification
    {
        public TriggerIdentification(CustomTrigger t)
        {
            trigger = t;
        }

        public TriggerIdentification(string guid, string name)
        {
            pluginGuid = guid;
            triggerName = name;
        }

        public bool Matches(TriggerIdentification other)
        {
            return (pluginGuid == other.pluginGuid && triggerName == other.triggerName) || trigger == other.trigger;
        }

        public string pluginGuid;
        public string triggerName;
        public CustomTrigger trigger;
    }

    [HarmonyPatch]
    public static class CustomTriggerPatches
    {
        [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.AddCardToHand))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnAddedToHand(IEnumerator result, PlayableCard card)
        {
            yield return result;
            if (card.TriggerHandler.RespondsToCustomTrigger(CustomTrigger.OnAddedToHand, Array.Empty<object>()))
            {
                yield return card.TriggerHandler.OnCustomTrigger(CustomTrigger.OnAddedToHand, Array.Empty<object>());
            }
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnOtherCardAddedToHand, false, card);
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.DoCombatPhase))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnBellRung(IEnumerator result, bool playerIsAttacker)
        {
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnBellRung, false, playerIsAttacker);
            yield return result;
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSequence))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnSlotAttackSequence(IEnumerator result, CardSlot slot)
        {
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPreSlotAttackSequence, false, slot);
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPostSlotAttackSequence, false, slot);
            yield break;
        }

        [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnPostSingularSlotAttackSlot(IEnumerator result, CardSlot attackingSlot, CardSlot opposingSlot)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPostSingularSlotAttackSlot, false, attackingSlot, opposingSlot);
            yield break;
        }

        [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnScalesChanged(IEnumerator result, int damage, bool toPlayer)
        {
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPreScalesChanged, false, damage, toPlayer);
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnPostScalesChanged, false, damage, toPlayer);
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnUpkeepInHand(IEnumerator result, bool playerUpkeep)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnUpkeepInHand, playerUpkeep);
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnOtherCardResolveInHand(IEnumerator result, PlayableCard card, bool resolveTriggers = true)
        {
            yield return result;
            if (resolveTriggers)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardResolveInHand, card);
            }
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlayerTurn))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnTurnEndInHandPlayer(IEnumerator result)
        {
            yield return result;
            yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnTurnEndInHand, true);
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.OpponentTurn))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnTurnEndInHandOpponent(IEnumerator result, TurnManager __instance)
        {
            bool turnSkipped = __instance.Opponent.SkipNextTurn;
            yield return result;
            if (!turnSkipped)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnTurnEndInHand, false);
            }
            yield break;
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.AssignCardToSlot))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnOtherCardAssignedToSlotInHand(IEnumerator result, PlayableCard card, bool resolveTriggers)
        {
            CardSlot slot2 = card.Slot;
            yield return result;
            if (resolveTriggers && slot2 != card.Slot)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardAssignedToSlotInHand, card);
            }
            if(resolveTriggers && slot2 != card.Slot && slot2 != null)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerAll(CustomTrigger.OnCardAssignedToSlotNoResolve, card);
            }
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Die))]
        [HarmonyPostfix]
        public static IEnumerator TriggerDeathTriggers(IEnumerator result, PlayableCard __instance, bool wasSacrifice, PlayableCard killer = null)
        {
            CardSlot slotBeforeDeath = __instance.Slot;
            while (result.MoveNext())
            {
                yield return result.Current;
                if (result.Current.GetType() == triggerType)
                {
                    Trigger t = Trigger.None;
                    try
                    {
                        t = (Trigger)result.Current.GetType().GetField("trigger").GetValue(result.Current);
                    }
                    catch { }
                    if(t == Trigger.OtherCardPreDeath)
                    {
                        yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardPreDeathInHand, slotBeforeDeath, !wasSacrifice, killer);
                    }
                    else if(t == Trigger.OtherCardDie)
                    {
                        yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardDieInHand, __instance, slotBeforeDeath, !wasSacrifice, killer);
                    }
                }
            }
            yield break;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
        [HarmonyPostfix]
        public static IEnumerator TriggerOnTurnEndInHandPlayer(IEnumerator result, PlayableCard __instance, PlayableCard attacker)
        {
            bool hasshield = __instance.HasShield();
            yield return result;
            if (!hasshield && attacker != null)
            {
                yield return CustomGlobalTriggerHandler.CustomTriggerCardsInHand(CustomTrigger.OnOtherCardDealtDamageInHand, attacker, attacker.Attack, __instance);
            }
            yield break;
        }

        static Type triggerType = AccessTools.TypeByName("DiskCardGame.GlobalTriggerHandler+<TriggerCardsOnBoard>d__16");
    }

    public class CustomTriggerResponderAttribute : Attribute
    {
        public CustomTriggerResponderAttribute(CustomTrigger trigger)
        {
            identification = new(trigger);
        }

        public CustomTriggerResponderAttribute(string pluginGuid, string triggerName)
        {
            identification = new(pluginGuid, triggerName);
        }

        internal TriggerIdentification identification;
    }

    public class CustomTriggerCoroutineAttribute : Attribute
    {
        public CustomTriggerCoroutineAttribute(CustomTrigger trigger)
        {
            identification = new(trigger);
        }

        public CustomTriggerCoroutineAttribute(string pluginGuid, string triggerName)
        {
            identification = new(pluginGuid, triggerName);
        }

        internal TriggerIdentification identification;
    }

    internal static class CustomTriggerArgHolder
    {
        internal static readonly Dictionary<CustomTrigger, List<Type>> argsForTriggers = new()
        {
            //trigger                                       args
            { CustomTrigger.OnAddedToHand,                  new() { } },
            { CustomTrigger.OnOtherCardAddedToHand,         new() { typeof(PlayableCard) } },
            { CustomTrigger.OnCardAssignedToSlotNoResolve,  new() { typeof(PlayableCard) } },
            { CustomTrigger.OnBellRung,                     new() { typeof(bool) } },
            { CustomTrigger.OnPreSlotAttackSequence,        new() { typeof(CardSlot) } },
            { CustomTrigger.OnPostSlotAttackSequence,       new() { typeof(CardSlot) } },
            { CustomTrigger.OnPostSingularSlotAttackSlot,   new() { typeof(CardSlot), typeof(CardSlot) } },
            { CustomTrigger.OnPreScalesChanged,             new() { typeof(int), typeof(bool) } },
            { CustomTrigger.OnPostScalesChanged,            new() { typeof(int), typeof(bool) } },
            { CustomTrigger.OnUpkeepInHand,                 new() { typeof(bool) } },
            { CustomTrigger.OnOtherCardResolveInHand,       new() { typeof(PlayableCard) } },
            { CustomTrigger.OnTurnEndInHand,                new() { typeof(bool) } },
            { CustomTrigger.OnOtherCardAssignedToSlotInHand,new() { typeof(PlayableCard) } },
            { CustomTrigger.OnOtherCardPreDeathInHand,      new() { typeof(CardSlot), typeof(bool), typeof(PlayableCard) } },
            { CustomTrigger.OnOtherCardDealtDamageInHand,   new() { typeof(PlayableCard), typeof(int), typeof(PlayableCard) } },
            { CustomTrigger.OnOtherCardDieInHand,           new() { typeof(PlayableCard), typeof(CardSlot), typeof(bool), typeof(PlayableCard) } },
        };

        internal static readonly Dictionary<(string, string), List<Type>> customArgsForTriggers = new();

        internal static bool TryGetArgs(TriggerIdentification identification, out List<Type> args)
        {
            if(argsForTriggers != null && identification.trigger > CustomTrigger.None && argsForTriggers.ContainsKey(identification.trigger))
            {
                args = new(argsForTriggers[identification.trigger]);
                return true;
            }
            else if(customArgsForTriggers != null && !string.IsNullOrEmpty(identification.pluginGuid) && !string.IsNullOrEmpty(identification.triggerName) && 
                customArgsForTriggers.ContainsKey((identification.pluginGuid, identification.triggerName)))
            {
                args = new(customArgsForTriggers[(identification.pluginGuid, identification.triggerName)]);
                return true;
            }
            args = new();
            return false;
        }
    }

    public static class CustomGlobalTriggerHandler
    {
        public static void RegisterCustomTrigger(string pluginGuid, string triggerName, params Type[] args)
        {
            CustomTriggerArgHolder.customArgsForTriggers.Add((pluginGuid, triggerName), args.ToList());
        }

        private static bool IsValidTriggerMethod(MethodInfo method, TriggerIdentification identification, bool isResponder)
        {
            if(method != null)
            {
                if((isResponder && method.ReturnType == typeof(bool)) || (!isResponder && (method.ReturnType == typeof(IEnumerator) || method.ReturnType.IsSubclassOf(typeof(IEnumerator)) ||
                    method.ReturnType.GetInterfaces().Contains(typeof(IEnumerator)))))
                {
                    bool ctca = method.GetCustomAttributes(true).ToList().Exists(x => x != null && x is CustomTriggerCoroutineAttribute && (x as CustomTriggerCoroutineAttribute).identification.Matches(identification));
                    bool ctra = method.GetCustomAttributes(true).ToList().Exists(x => x != null && x is CustomTriggerResponderAttribute && (x as CustomTriggerResponderAttribute).identification.Matches(identification));
                    if((isResponder && ctra) || (!isResponder && ctca))
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
            if(self != null)
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
            if(Singleton<BoardManager>.Instance != null && Singleton<BoardManager>.Instance.CardsOnBoard != null)
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
                /*if (!triggerFacedown)
                {
                    bool RespondsToTrigger(CardTriggerHandler r, TriggerIdentification identification, params object[] otherArgs)
                    {
                        foreach (TriggerReceiver receiver in r.GetAllReceivers())
                        {
                            if (ReceiverRespondsToCustomTrigger(identification, receiver, otherArgs) && (receiver is ActivateWhenFacedown || 
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
                            if (ReceiverRespondsToCustomTrigger(identification, receiver, otherArgs) && (receiver is ActivateWhenFacedown) ||
                                (receiver is ExtendedAbilityBehaviour && (receiver as ExtendedAbilityBehaviour).TriggerWhenFacedown))
                            {
                                yield return CustomTriggerSequence(identification, receiver, otherArgs);
                            }
                        }
                        yield break;
                    }
                    List<PlayableCard> list = new(Singleton<BoardManager>.Instance.CardsOnBoard);
                    foreach (PlayableCard playableCard in list)
                    {
                        if (playableCard != null && playableCard.FaceDown && RespondsToTrigger(playableCard.TriggerHandler, identification, otherArgs))
                        {
                            yield return OnTrigger(playableCard.TriggerHandler, identification, otherArgs);
                        }
                    }
                }*/ //uncomment this when waterborne fix gets added
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
            if(Singleton<PlayerHand>.Instance != null && Singleton<PlayerHand>.Instance.CardsInHand != null)
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
