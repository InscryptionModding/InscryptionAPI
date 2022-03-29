using InscryptionAPI.Guid;

namespace InscryptionAPI.Triggers
{
    /// <summary>
    /// This attribute is used to decorate a method that indicates if a trigger receiver responds to a specific custom trigger
    /// </summary>
    /// <remarks>
    /// Custom triggers behave the same as vanilla triggers: you need one method to acknowledge that you respond to the trigger
    /// and one method that actually handles the response. This attribute is used to decorate the acknowledgement method
    /// 
    /// Your method **must**
    /// 1. Return True if you want to respond to the trigger based on the given parameters, or False if you do not.
    /// 2. Accept the appropriate arguments as outlined in [TriggerDelegates](xref:InscryptionAPI.Triggers.TriggerDelegates)
    /// </remarks>
    public class CustomTriggerResponderAttribute : Attribute
    {

        /// <summary>
        /// Decorates a trigger acknowledgement to an enumerated custom trigger
        /// </summary>
        /// <param name="trigger">The trigger to respond to</param>
        public CustomTriggerResponderAttribute(CustomTrigger trigger)
        {
            this.trigger = trigger;
        }

        /// <summary>
        /// Decorates a trigger acknowledgement to a mod-added custom trigger
        /// </summary>
        /// <param name="pluginGuid">The guid of the plugin that created the custom trigger</param>
        /// <param name="triggerName">The name of the custom trigger</param>
        public CustomTriggerResponderAttribute(string pluginGuid, string triggerName)
        {
            this.trigger = GuidManager.GetEnumValue<CustomTrigger>(pluginGuid, triggerName);
        }

        internal CustomTrigger trigger;
    }

    /// <summary>
    /// This attribute is used to decorate a method that responds to a specific custom trigger and returns an IEnumerator
    /// </summary>
    /// <remarks>
    /// Custom triggers behave the same as vanilla triggers: you need one method to acknowledge that you respond to the trigger
    /// and one method that actually handles the response. This attribute is used to decorate your response to that trigger.
    /// 
    /// Your method **must**
    /// 1. Return IEnumerator
    /// 2. Accept the appropriate arguments as outlined in [TriggerDelegates](xref:InscryptionAPI.Triggers.TriggerDelegates)
    /// </remarks>
    public class CustomTriggerCoroutineAttribute : Attribute
    {
        /// <summary>
        /// Decorates a trigger response to an enumerated custom trigger
        /// </summary>
        /// <param name="trigger">The trigger to respond to</param>
        public CustomTriggerCoroutineAttribute(CustomTrigger trigger)
        {
            this.trigger = trigger;
        }

        /// <summary>
        /// Decorates a trigger response to a mod-added custom trigger
        /// </summary>
        /// <param name="pluginGuid">The guid of the plugin that created the custom trigger</param>
        /// <param name="triggerName">The name of the custom trigger</param>
        public CustomTriggerCoroutineAttribute(string pluginGuid, string triggerName)
        {
            this.trigger = GuidManager.GetEnumValue<CustomTrigger>(pluginGuid, triggerName);
        }

        internal CustomTrigger trigger;
    }

    /// <summary>
    /// This attribute is used to decorate a method that responds to a specific custom trigger and returns data specific to that trigger
    /// </summary>
    /// <remarks>
    /// This is used to decorate a special type of custom triggers that return data instead of returning a sequence of 
    /// unity events. In other words, these triggers don't cause events to play out in front of the player on screen; they
    /// simply provide data that is necessary for other parts of the game to function.
    /// 
    /// A classic example of this is the OnGetOpposingSlots custom trigger, which fires whenever a card needs to calculate
    /// which slots its going to attack. A trigger handler can respond to this custom trigger and provide information about
    /// how the card wants to modify its attack sequence 
    /// 
    /// Your method **must**
    /// 1. Return the appropriate return data type as outlined in [TriggerDelegates](xref:InscryptionAPI.Triggers.TriggerDelegates)
    /// 2. Accept the appropriate arguments as outlined in [TriggerDelegates](xref:InscryptionAPI.Triggers.TriggerDelegates)
    /// </remarks>
    public class CustomTriggerDataAttribute : Attribute
    {
        /// <summary>
        /// Decorates a data response to an enumerated custom trigger
        /// </summary>
        /// <param name="trigger">The trigger to respond to</param>
        public CustomTriggerDataAttribute(CustomTrigger trigger)
        {
            this.trigger = trigger;
        }

        /// <summary>
        /// Decorates a data response to a mod-added custom trigger
        /// </summary>
        /// <param name="pluginGuid">The guid of the plugin that created the custom trigger</param>
        /// <param name="triggerName">The name of the custom trigger</param>
        public CustomTriggerDataAttribute(string pluginGuid, string triggerName)
        {
            this.trigger = GuidManager.GetEnumValue<CustomTrigger>(pluginGuid, triggerName);
        }

        internal CustomTrigger trigger;
    }
}
