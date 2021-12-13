using System;
using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public class AbilityIdentifier
	{
		private static List<AbilityIdentifier> ids = new();
		private string guid;
		private string name;
		public Ability id;

		private AbilityIdentifier(string guid, string name)
		{
			this.guid = guid;
			this.name = name;
			ids.Add(this);
		}

		[Obsolete("Replaced by GetID")]
		public static AbilityIdentifier GetAbilityIdentifier(string guid, string name)
		{
			return GetID(guid, name);
		}

		public static AbilityIdentifier GetID(string guid, string name)
		{
			return ids.Exists(x => x.guid == guid && x.name == name)
				? ids.Find(x => x.guid == guid && x.name == name)
				: new AbilityIdentifier(guid, name);
		}

		public override string ToString()
		{
			return $"{guid}({name})";
		}
	}
}