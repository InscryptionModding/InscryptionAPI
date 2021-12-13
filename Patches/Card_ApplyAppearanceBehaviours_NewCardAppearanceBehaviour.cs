using System;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using APIPlugin;

namespace API.Patches
{
	[HarmonyPatch(typeof (Card), "ApplyAppearanceBehaviours", new System.Type[] {typeof (List<CardAppearanceBehaviour.Appearance>)})]
	public class Card_ApplyAppearanceBehaviours
	{
		public static bool Prefix(List<CardAppearanceBehaviour.Appearance> appearances, Card __instance)
		{
			foreach (CardAppearanceBehaviour.Appearance appearance in appearances)
			{
				if (NewCardAppearanceBehaviour.behaviours.TryGetValue(appearance, out NewCardAppearanceBehaviour behaviour))
				{
					Type type = behaviour.Behaviour;
					if (!__instance.gameObject.GetComponent(type))
					{
						(__instance.gameObject.AddComponent(type) as CardAppearanceBehaviour).ApplyAppearance();
					}
				}
				else
				{
					Type type = CustomType.GetType("DiskCardGame", appearance.ToString());
					if (!__instance.gameObject.GetComponent(type))
					{
						(__instance.gameObject.AddComponent(type) as CardAppearanceBehaviour).ApplyAppearance();
					}
				}
			}

			return false;
		}
	}
}