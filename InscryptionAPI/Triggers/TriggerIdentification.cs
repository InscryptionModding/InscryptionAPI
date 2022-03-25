using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Triggers
{
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
}
