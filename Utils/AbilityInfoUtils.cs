using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public class AbilityInfoUtils
	{
		public static AbilityInfo CreateInfoWithDefaultSettings(string rulebookName, string rulebookDescription)
		{
			AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
			info.powerLevel = 0;
			info.rulebookName = rulebookName;
			info.rulebookDescription = rulebookDescription;
			info.metaCategories = new List<AbilityMetaCategory>()
			{
				AbilityMetaCategory.Part1Modular, AbilityMetaCategory.Part1Rulebook
			};

			return info;
		}
	}
}