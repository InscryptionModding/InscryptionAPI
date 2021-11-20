using System;
using System.Collections.Generic;
using System.Linq;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;

namespace API.Patches
{
	[HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] { typeof(AbilityMetaCategory) })]
	public class RuleBookInfo_ConstructPageData
	{
		public static void Postfix(AbilityMetaCategory metaCategory, RuleBookInfo __instance,
			ref List<RuleBookPageInfo> __result)
		{
			if (NewAbility.abilities.Count > 0)
			{
				foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
				{
					// regular abilities
					if (pageRangeInfo.type == PageRangeType.Abilities)
					{
						List<int> customAbilities = NewAbility.abilities.Select(x => (int)x.ability).ToList();
						int min = customAbilities.AsQueryable().Min();
						int max = customAbilities.AsQueryable().Max();
						PageRangeInfo pageRange = pageRangeInfo;
						Func<int, bool> doAddPageFunc;
						doAddPageFunc = (int index) =>
							customAbilities.Contains(index)
							&& AbilitiesUtil.GetInfo((Ability)index).metaCategories.Contains(metaCategory);
						__result.AddRange(__instance.ConstructPages(pageRange,
							max + 1,
							min,
							doAddPageFunc,
							__instance.FillAbilityPage,
							Localization.Translate("APPENDIX XII, SUBSECTION I - MOD ABILITIES {0}")));
					}
				}
			}

			if (NewSpecialAbility.specialAbilities.Count > 0)
			{
				foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
				{
					// special abilities
					if (pageRangeInfo.type == PageRangeType.StatIcons)
					{
						List<int> customAbilities = NewSpecialAbility.specialAbilities
							.Select(x => (int)x.statIconInfo.iconType).ToList();
						Plugin.Log.LogInfo(
							$"Number of custom abilities found to add to rulebook [{customAbilities.Count}]");
						int min = customAbilities.AsQueryable().Min();
						int max = customAbilities.AsQueryable().Max();
						PageRangeInfo pageRange = pageRangeInfo;
						Func<int, bool> doAddPageFunc;
						doAddPageFunc = (int index) =>
							customAbilities.Contains(index)
							&& StatIconInfo.GetIconInfo((SpecialStatIcon)index).metaCategories.Contains(metaCategory);
						__result.AddRange(__instance.ConstructPages(pageRange,
							max + 1,
							min,
							doAddPageFunc,
							__instance.FillStatIconPage,
							Localization.Translate("APPENDIX XII, SUBSECTION II - VARIABLE STATS {0}")));
					}
				}
			}
		}
	}
}