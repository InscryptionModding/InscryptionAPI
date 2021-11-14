using System;
using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public class AbilityIdentifier
	{
    private static List<AbilityIdentifier> ids = new List<AbilityIdentifier>();
		private string guid;
    private string name;
    public Ability id;

		private AbilityIdentifier(string guid, string name)
		{
			this.guid = guid;
			this.name = name;
      ids.Add(this);
		}

    public static AbilityIdentifier GetAbilityIdentifier(string guid, string name)
    {
      if (ids.Exists((AbilityIdentifier x) => x.guid == guid && x.name == name))
      {
        return ids.Find((AbilityIdentifier x) => x.guid == guid && x.name == name);
      }
      else
      {
        return new AbilityIdentifier(guid, name);
      }
    }

		public override String ToString()
		{
			return $"{this.guid}({this.name})";
		}
	}
}
