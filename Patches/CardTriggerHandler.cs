using System;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace API.Patches
{
	[HarmonyPatch(typeof(CardTriggerHandler), "AddAbility", new Type[] { typeof(Ability) })]
	public class CardTriggerHandler_AddAbility
	{
		public static bool Prefix(Ability ability, CardTriggerHandler __instance)
		{
			if ((int)ability < 99)
			{
				return true;
			}
			
			Predicate<Tuple<Ability, AbilityBehaviour>> checkAbilityExists = tuple =>
				tuple.Item1 == ability || AbilityCanStackAndIsNotPassive(ability);

			// return true if the ability is equal to pair item1 OR if ability cannot stack and is passive 
			if (!__instance.triggeredAbilities.Exists(checkAbilityExists))
			{
				NewAbility newAbility = NewAbility.abilities.Find((NewAbility x) => x.ability == ability);
				Type type = newAbility.abilityBehaviour;
				Component baseC = (Component)__instance;
				AbilityBehaviour item = baseC.gameObject.GetComponent(type) as AbilityBehaviour;
				if (item == null)
				{
					item = baseC.gameObject.AddComponent(type) as AbilityBehaviour;
				}

				__instance.triggeredAbilities.Add(new Tuple<Ability, AbilityBehaviour>(ability, item));
			}

			return false;
		}

		public static bool AbilityCanStackAndIsNotPassive(Ability ability)
		{
			return AbilitiesUtil.GetInfo(ability).canStack && !AbilitiesUtil.GetInfo(ability).passive;
		}
	}
}