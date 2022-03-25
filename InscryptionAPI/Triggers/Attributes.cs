using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Triggers
{
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
}
