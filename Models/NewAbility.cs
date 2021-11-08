using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public class NewAbility
	{
		public static List<NewAbility> abilities = new List<NewAbility>();
		public Ability ability;
		public AbilityInfo info;
		public Type abilityBehaviour;
		public Texture tex;

		public NewAbility(AbilityInfo info, Type abilityBehaviour, Texture tex)
		{
			this.ability = (Ability) 100 + NewAbility.abilities.Count;
			info.ability = this.ability;
			this.info = info;
			this.abilityBehaviour = abilityBehaviour;
			this.tex = tex;
			NewAbility.abilities.Add(this);
			Plugin.Log.LogInfo($"Loaded custom ability {info.rulebookName}!");
		}
	}
}