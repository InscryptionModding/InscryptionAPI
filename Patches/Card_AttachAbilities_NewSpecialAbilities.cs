using System.Linq;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace API.Patches
{
	[HarmonyPatch(typeof(Card), nameof(Card.AttachAbilities), typeof(CardInfo))]
	public class Card_AttachAbilities_NewSpecialAbilities
	{
		[HarmonyPrefix]
		public static bool Prefix(CardInfo info, Card __instance)
		{
			Plugin.Log.LogDebug(
				$"Called Card.AttachAbilities with [{info.name}] has [{info.specialAbilities.Count}] special abilities");
			
			// if the card's special triggered ability exists in the NewSpecialAbility list,
			//	then we loop to assign to the game object.
			// if the ability does not exist, return true running the original code
			if (NewSpecialAbility.specialAbilities.Exists(ability =>
				info.specialAbilities.Contains(ability.specialTriggeredAbility)))
			{
				foreach (var type in info.specialAbilities
					.Select(specialTriggeredAbility => NewSpecialAbility.specialAbilities
					.Find(x => x.specialTriggeredAbility == specialTriggeredAbility))
					.Select(newAbility => newAbility.abilityBehaviour))
				{
					Plugin.Log.LogDebug($"-> Special Card Behaviour Type is [{type}]");
					Component baseC = __instance;
					// This assigns it to the gameObject. We do not need to call CardTriggerHandler.AddReceiverToGameObject
					SpecialCardBehaviour item = baseC.gameObject.GetComponent(type) as SpecialCardBehaviour 
					                            ?? baseC.gameObject.AddComponent(type) as SpecialCardBehaviour;
				}

				return false;
			}

			return true;
		}
	}
}