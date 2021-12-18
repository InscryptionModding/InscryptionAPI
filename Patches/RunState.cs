using API.Utils;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace API.Patches
{
	// TODO: Fix both to work with ascension

	[HarmonyPatch(typeof(RunState), "Initialize")]
	public class RunState_Initialize
	{
		public static void Postfix(ref RunState __instance)
		{
			if (!SaveManager.SaveFile.ascensionActive)
			{
				__instance.regionTier = RegionUtils.GetRandomRegionFromTier(RunState.Run.regionOrder[0]);
			}
		}
	}

	[HarmonyPatch(typeof(RunState), "NextRegion")]
	public class RunState_NextRegion
	{
		public static bool Prefix(ref RunState __instance)
		{
			List<RegionData> original = RegionProgression.Instance.regions;
			int tier = RegionUtils.TrueTier();
			int progressionTier = Array.IndexOf(RunState.Run.regionOrder, tier);
			__instance.regionTier = RegionUtils.GetRandomRegionFromTier((progressionTier == -1 || progressionTier == RunState.Run.regionOrder.Length - 1) ? RunState.Run.regionOrder.Length : RunState.Run.regionOrder[progressionTier + 1]);
			if (__instance.regionTier == RunState.Run.regionOrder.Length)
            {
				if (SaveManager.SaveFile.ascensionActive)
                {
					__instance.regionTier = original.Count - 1;

				}
            }
			__instance.map = MapGenerator.GenerateMap(RunState.CurrentMapRegion, 3, 13);
			__instance.currentNodeId = __instance.map.RootNode.id;
			return false;
		}
	}

	[HarmonyPatch(typeof(RunState), "CurrentMapRegion", MethodType.Getter)]
	public class RunState_get_CurrentMapRegion
	{
		public static bool Prefix(ref RegionData __result)
		{
			if (SaveManager.SaveFile.IsPart3 || SaveManager.SaveFile.IsGrimora)
			{
				__result = ResourceBank.Get<RegionData>("Data/Map/Regions/!TEST_PART3");
			}
			else
            {
				__result = RegionProgression.Instance.regions[RunState.Run.regionTier];
			}
			return false;
		}
	}
}
