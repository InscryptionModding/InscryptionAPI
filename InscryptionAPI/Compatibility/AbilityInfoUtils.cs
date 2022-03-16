using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
    [Obsolete("Use AbilityManager and extension methods instead", true)]
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