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
							new Action<RuleBookPageInfo, PageRangeInfo, int>(__instance.FillAbilityPage),
							Localization.Translate("APPENDIX XII, SUBSECTION I - MOD ABILITIES {0}")));
					}
				}
			}
		}
	}
}
