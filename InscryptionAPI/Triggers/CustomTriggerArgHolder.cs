using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DiskCardGame;

namespace InscryptionAPI.Triggers
{
    internal static class CustomTriggerArgHolder
    {
        internal static MethodInfo GetMethodInfo(this Type t) => t.GetMethod("Invoke");

        internal static readonly Dictionary<CustomTrigger, MethodInfo> methodSignaturesForTriggers = new()
        {
            //trigger                                       args
            { CustomTrigger.OnAddedToHand,                  typeof(TriggerDelegates.OnAddedToHand).GetMethodInfo() },
            { CustomTrigger.OnOtherCardAddedToHand,         typeof(TriggerDelegates.OnOtherCardAddedToHand).GetMethodInfo() },
            { CustomTrigger.OnCardAssignedToSlotNoResolve,  typeof(TriggerDelegates.OnCardAssignedToSlotNoResolve).GetMethodInfo() },
            { CustomTrigger.OnBellRung,                     typeof(TriggerDelegates.OnBellRung).GetMethodInfo() },
            { CustomTrigger.OnPreSlotAttackSequence,        typeof(TriggerDelegates.OnPreSlotAttackSequence).GetMethodInfo() },
            { CustomTrigger.OnPostSlotAttackSequence,       typeof(TriggerDelegates.OnPostSlotAttackSequence).GetMethodInfo() },
            { CustomTrigger.OnPostSingularSlotAttackSlot,   typeof(TriggerDelegates.OnPostSingularSlotAttackSlot).GetMethodInfo() },
            { CustomTrigger.OnPreScalesChanged,             typeof(TriggerDelegates.OnPreScalesChanged).GetMethodInfo() },
            { CustomTrigger.OnPostScalesChanged,            typeof(TriggerDelegates.OnPostScalesChanged).GetMethodInfo() },
            { CustomTrigger.OnUpkeepInHand,                 typeof(TriggerDelegates.OnUpkeepInHand).GetMethodInfo() },
            { CustomTrigger.OnOtherCardResolveInHand,       typeof(TriggerDelegates.OnOtherCardResolveInHand).GetMethodInfo() },
            { CustomTrigger.OnTurnEndInHand,                typeof(TriggerDelegates.OnTurnEndInHand).GetMethodInfo() },
            { CustomTrigger.OnOtherCardAssignedToSlotInHand,typeof(TriggerDelegates.OnOtherCardAssignedToSlotInHand).GetMethodInfo() },
            { CustomTrigger.OnOtherCardPreDeathInHand,      typeof(TriggerDelegates.OnOtherCardPreDeathInHand).GetMethodInfo() },
            { CustomTrigger.OnOtherCardDealtDamageInHand,   typeof(TriggerDelegates.OnOtherCardDealtDamageInHand).GetMethodInfo() },
            { CustomTrigger.OnOtherCardDieInHand,           typeof(TriggerDelegates.OnOtherCardDieInHand).GetMethodInfo() },
            { CustomTrigger.OnGetOpposingSlots,             typeof(TriggerDelegates.OnGetOpposingSlots).GetMethodInfo() },
            { CustomTrigger.OnBuffOtherCardAttack,          typeof(TriggerDelegates.OnBuffOtherCardAttack).GetMethodInfo() },
            { CustomTrigger.OnBuffOtherCardHealth,          typeof(TriggerDelegates.OnBuffOtherCardHealth).GetMethodInfo() }
        };

        internal static readonly Dictionary<CustomTrigger, MethodInfo> customArgsForTriggers = new();

        internal static bool TryGetArgs(CustomTrigger identification, out List<Type> args)
        {
            if (methodSignaturesForTriggers != null && identification > CustomTrigger.None && methodSignaturesForTriggers.ContainsKey(identification))
            {
                args = new(methodSignaturesForTriggers[identification].GetParameters().Select(pi => pi.ParameterType));
                return true;
            }
            else if (customArgsForTriggers != null && identification > CustomTrigger.None && customArgsForTriggers.ContainsKey(identification))
            {
                args = new(customArgsForTriggers[identification].GetParameters().Select(pi => pi.ParameterType));
                return true;
            }
            args = new();
            return false;
        }

        internal static bool TryGetReturnType(CustomTrigger identification, out Type args)
        {
            if (methodSignaturesForTriggers != null && identification > CustomTrigger.None && methodSignaturesForTriggers.ContainsKey(identification))
            {
                args = methodSignaturesForTriggers[identification].ReturnType;
                return true;
            }
            else if (customArgsForTriggers != null && identification > CustomTrigger.None && customArgsForTriggers.ContainsKey(identification))
            {
                args = customArgsForTriggers[identification].ReturnType;
                return true;
            }
            args = typeof(IEnumerator);
            return false;
        }
    }
}
