using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public class NewSpecialAbility
	{
		public static List<NewSpecialAbility> specialAbilities = new();
		public SpecialTriggeredAbility specialTriggeredAbility;
		public StatIconInfo statIconInfo;
		public Type abilityBehaviour;
		public SpecialAbilityIdentifier id;

		public NewSpecialAbility(
			Type abilityBehaviour,
			SpecialAbilityIdentifier id,
			StatIconInfo statIconInfo = null
		)
		{
			specialTriggeredAbility = (SpecialTriggeredAbility)26 + specialAbilities.Count;
			var logNameOrIdNumber = specialTriggeredAbility.ToString();
			if (statIconInfo)
			{
				this.statIconInfo = statIconInfo;
				HandleStatIconInfo(statIconInfo);
				logNameOrIdNumber = this.statIconInfo.rulebookName;
			}
			this.abilityBehaviour = abilityBehaviour;
			this.id = id;
			id.id = specialTriggeredAbility;

			HandleStatIconInfo(statIconInfo);

			specialAbilities.Add(this);
			Plugin.Log.LogInfo($"Loaded custom special ability [{logNameOrIdNumber}]!");
		}

		// is only called if StatIconInfo is not null
		private static void HandleStatIconInfo(StatIconInfo statIconInfo)
		{
			statIconInfo.iconType = (SpecialStatIcon)8 + specialAbilities.Count;
			
			if (statIconInfo.iconGraphic is not null)
			{
				// the reason for this is just one less step for the end user to setup
				statIconInfo.iconGraphic.filterMode = FilterMode.Point;
			}

			// a lazy initializer
			if (statIconInfo.metaCategories.Count == 0)
			{
				statIconInfo.metaCategories = new List<AbilityMetaCategory>
				{
					AbilityMetaCategory.Part1Modular, AbilityMetaCategory.Part1Rulebook
				};
			}
		}
	}
}