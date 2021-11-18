using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public class SpecialAbilityIdentifier
	{
		private static List<SpecialAbilityIdentifier> ids = new();
		private string guid;
		private string name;
		public SpecialTriggeredAbility id;

		private SpecialAbilityIdentifier(string guid, string name)
		{
			this.guid = guid;
			this.name = name;
			ids.Add(this);
		}

		public static SpecialAbilityIdentifier GetID(string guid, string name)
		{
			return ids.Exists(x => x.guid == guid && x.name == name)
				? ids.Find(x => x.guid == guid && x.name == name)
				: new SpecialAbilityIdentifier(guid, name);
		}

		public override string ToString()
		{
			return $"{guid}({name})";
		}
	}
}