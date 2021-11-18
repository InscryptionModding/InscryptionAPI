using System;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace API.Patches
{
	[HarmonyPatch(typeof(CardTriggerHandler), "AddAbility", typeof(Ability))]
	public class CardTriggerHandler_AddAbility_Ability
	{
		public static bool Prefix(Ability ability, CardTriggerHandler __instance)
		{
			if ((int)ability < 99)
			{
				return true;
			}

			Predicate<Tuple<Ability, AbilityBehaviour>> checkAbilityExists = tuple =>
				tuple.Item1 == ability || AbilityCanStackAndIsNotPassive(ability);
			Plugin.Log.LogDebug($"Attempting to add regular ability in card trigger handler [{ability}]");
			// return true if the ability is equal to the ability in the pair OR if ability cannot stack and is passive
			if (!__instance.triggeredAbilities.Exists(checkAbilityExists))
			{
				Plugin.Log.LogDebug($"-> Ability [{ability}] does not exist, adding...");
				NewAbility newAbility = NewAbility.abilities.Find(x => x.ability == ability);
				Plugin.Log.LogDebug($"-> New Ability is [{newAbility.ability}]");
				Type type = newAbility.abilityBehaviour;
				Component baseC = __instance;
				AbilityBehaviour item = baseC.gameObject.GetComponent(type) as AbilityBehaviour;
				if (item == null)
				{
					item = baseC.gameObject.AddComponent(type) as AbilityBehaviour;
					Plugin.Log.LogDebug($"--> Item is [{item}] | Ability toString [{ability}]");
				}

				__instance.triggeredAbilities.Add(new Tuple<Ability, AbilityBehaviour>(ability, item));
			}

			return false;
		}

		private static bool AbilityCanStackAndIsNotPassive(Ability ability)
		{
			return AbilitiesUtil.GetInfo(ability).canStack && !AbilitiesUtil.GetInfo(ability).passive;
		}
	}
	
	[HarmonyPatch(typeof(CardTriggerHandler), "AddAbility", typeof(SpecialTriggeredAbility))]
	public class CardTriggerHandler_AddAbility_SpecialTriggeredAbility
	{
		public static bool Prefix(SpecialTriggeredAbility ability, CardTriggerHandler __instance)
		{
			Plugin.Log.LogDebug($"Attempting to add spec ability to card trigger handler [{ability}]");
			if ((int)ability < 25)
			{
				return true;
			}
			if (!__instance.specialAbilities.Exists(ab => ab.Item1 == ability))
			{
				Plugin.Log.LogDebug($"-> Special ability [{ability}] does not exist, adding...");
				NewSpecialAbility newAbility = NewSpecialAbility.specialAbilities
					.Find(x => x.specialTriggeredAbility == ability);
				Plugin.Log.LogDebug($"-> New Ability is [{newAbility.specialTriggeredAbility}]");
				Type type = newAbility.abilityBehaviour;
				Component baseC = __instance;
				SpecialCardBehaviour item = baseC.gameObject.GetComponent(type) as SpecialCardBehaviour;
				if (item == null)
				{
					item = baseC.gameObject.AddComponent(type) as SpecialCardBehaviour;
					Plugin.Log.LogDebug($"--> Item is [{item}] | Ability toString [{ability}]");
				}

				__instance.specialAbilities.Add(new Tuple<SpecialTriggeredAbility, SpecialCardBehaviour>(ability, item));
			}

			return false;
		}
	}
}
