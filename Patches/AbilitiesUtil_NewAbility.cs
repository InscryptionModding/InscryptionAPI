using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace API.Patches
{
	[HarmonyPatch(typeof(AbilitiesUtil), "LoadAbilityIcon",
		typeof(string), typeof(bool), typeof(bool))]
	public class AbilitiesUtil_LoadAbilityIcon
	{
		public static bool Prefix(string abilityName, CardTriggerHandler __instance, ref Texture __result)
		{
			int ability = 0;
			if (!int.TryParse(abilityName, out ability))
			{
				return true;
			}

			NewAbility newAbility = NewAbility.abilities.Find(x => x.ability == (Ability)ability);
			__result = newAbility.tex;
			return false;
		}
	}

	[HarmonyPatch(typeof(AbilitiesUtil), "GetAbilities",
		typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(AbilityMetaCategory))]
	public class AbilitiesUtil_GetAbilities
	{
		public static void Postfix(
			bool learned,
			bool opponentUsable,
			int minPower,
			int maxPower,
			AbilityMetaCategory categoryCriteria,
			ref List<Ability> __result
		)
		{
			foreach (NewAbility newAbility in NewAbility.abilities)
			{
				AbilityInfo info = newAbility.info;
				bool flag = !opponentUsable || info.opponentUsable;
				bool flag2 = info.powerLevel >= minPower && info.powerLevel <= maxPower;
				bool flag3 = info.metaCategories.Contains(categoryCriteria);
				bool flag4 = true;
				if (learned)
				{
					flag4 = ProgressionData.LearnedAbility(info.ability);
				}

				if (flag && flag2 && flag3 && flag4)
				{
					__result.Add(newAbility.ability);
				}
			}
		}
	}
}
