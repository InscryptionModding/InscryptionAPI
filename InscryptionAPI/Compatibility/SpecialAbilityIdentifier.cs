using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Guid;

namespace APIPlugin
{
    [Obsolete("Use SpecialTriggeredAbilityManager instead", true)]
	public class SpecialAbilityIdentifier
	{
		internal string guid;
		internal string name;
		public SpecialTriggeredAbility id;
        internal SpecialTriggeredAbility specialTriggerID;
        internal SpecialStatIcon statIconID;
        internal bool ForStatIcon = false;

		private SpecialAbilityIdentifier(string guid, string name)
		{
			this.guid = guid;
			this.name = name;
			specialTriggerID = id = GuidManager.GetEnumValue<SpecialTriggeredAbility>(guid, name);
            statIconID = GuidManager.GetEnumValue<SpecialStatIcon>(guid, name);
		}

		public static SpecialAbilityIdentifier GetID(string guid, string name)
		{
			return new SpecialAbilityIdentifier(guid, name);
		}

		public override string ToString()
		{
			return $"{guid}({name})";
		}
	}
}