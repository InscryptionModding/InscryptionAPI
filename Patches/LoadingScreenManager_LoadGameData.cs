using System.Collections.Generic;
using System.Linq;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;

namespace API.Patches
{
	[HarmonyPatch(typeof(LoadingScreenManager), "LoadGameData")]
	public class LoadingScreenManager_LoadGameData
	{
		public static void Prefix()
		{
			if (ScriptableObjectLoader<CardInfo>.allData == null)
			{
				List<CardInfo> official = ScriptableObjectLoader<CardInfo>.AllData;
				foreach (CustomCard card in CustomCard.cards)
				{
					int index = official.FindIndex((CardInfo x) => x.name == card.name);
					if (index == -1)
					{
						Plugin.Log.LogInfo($"Could not find card {card.name} to modify");
					}
					else
					{
						official[index] = card.AdjustCard(official[index]);
						Plugin.Log.LogInfo($"Loaded modified {card.name} into data");
					}
				}

				ScriptableObjectLoader<CardInfo>.allData = official.Concat(NewCard.cards).ToList();
				Plugin.Log.LogInfo($"Loaded {NewCard.cards.Count} custom cards into data");
			}

			if (ScriptableObjectLoader<AbilityInfo>.allData == null)
			{
				List<AbilityInfo> official = ScriptableObjectLoader<AbilityInfo>.AllData;
				foreach (NewAbility newAbility in NewAbility.abilities)
				{
					official.Add(newAbility.info);
				}

				ScriptableObjectLoader<AbilityInfo>.allData = official;
				Plugin.Log.LogInfo($"Loaded {NewAbility.abilities.Count} custom abilities into data");
			}
		}
	}
}