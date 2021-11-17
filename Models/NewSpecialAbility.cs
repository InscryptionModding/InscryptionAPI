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
		public Texture tex;
		public SpecialAbilityIdentifier id;

		public NewSpecialAbility(
			StatIconInfo statIconInfo,
			Type abilityBehaviour,
			Texture tex,
			SpecialAbilityIdentifier id
		)
		{
			specialTriggeredAbility = (SpecialTriggeredAbility)26 + specialAbilities.Count;
			this.statIconInfo = statIconInfo;
			this.abilityBehaviour = abilityBehaviour;
			tex.filterMode = FilterMode.Point;
			this.tex = tex;
			this.id = id;
			specialAbilities.Add(this);
			id.id = specialTriggeredAbility;
			this.id = id;

			Plugin.Log.LogInfo($"Loaded custom special ability [{statIconInfo.rulebookName}]!");
		}
	}
}