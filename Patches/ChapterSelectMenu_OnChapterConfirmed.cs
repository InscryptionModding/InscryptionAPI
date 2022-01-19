﻿using System.Collections.Generic;
using System.Linq;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;

namespace API.Patches
{
	[HarmonyPatch(typeof(ChapterSelectMenu), "OnChapterConfirmed")]
	public class ChapterSelectMenu_OnChapterConfirmed
	{
		public static void Prefix()
		{
			if (!Plugin.CardsLoaded)
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
		}
	}
}
