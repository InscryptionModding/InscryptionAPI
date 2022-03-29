using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;

namespace InscryptionAPI.Triggers
{
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
            if (argsForTriggers != null && identification.trigger > CustomTrigger.None && argsForTriggers.ContainsKey(identification.trigger))
            {
                args = new(argsForTriggers[identification.trigger]);
                return true;
            }
            else if (customArgsForTriggers != null && !string.IsNullOrEmpty(identification.pluginGuid) && !string.IsNullOrEmpty(identification.triggerName) &&
                customArgsForTriggers.ContainsKey((identification.pluginGuid, identification.triggerName)))
            {
                args = new(customArgsForTriggers[(identification.pluginGuid, identification.triggerName)]);
                return true;
            }
            args = new();
            return false;
        }
    }
}
