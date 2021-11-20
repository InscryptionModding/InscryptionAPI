using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public class NewAbility
	{
		public static List<NewAbility> abilities = new();
		public Ability ability;
		public AbilityInfo info;
		public Type abilityBehaviour;
		public Texture tex;
		public AbilityIdentifier id;

		public NewAbility(AbilityInfo info, Type abilityBehaviour, Texture tex, AbilityIdentifier id = null)
		{
			ability = (Ability) 100 + abilities.Count;
			info.ability = ability;
			this.info = info;
			this.abilityBehaviour = abilityBehaviour;
			tex.filterMode = FilterMode.Point;
			this.tex = tex;
			this.id = id;
			abilities.Add(this);
			if (id != null){
				id.id = ability;
			}
			Plugin.Log.LogInfo($"Loaded custom ability {info.rulebookName}!");
		}
	}
}
