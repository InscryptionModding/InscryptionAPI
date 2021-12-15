using API.Utils;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;

namespace API.Patches
{
	// TODO: Fix both to work with ascension

	[HarmonyPatch(typeof(RunState), "Initialize")]
	public class RunState_Initialize
	{
		public static void Postfix(ref RunState __instance)
		{
			__instance.regionTier = RegionUtils.GetRandomRegionFromTier(0);
		}
	}

	[HarmonyPatch(typeof(RunState), "NextRegion")]
	public class RunState_NextRegion
	{
		public static bool Prefix(ref RunState __instance)
		{
			List<RegionData> original = RegionProgression.Instance.regions;
			int tier = RegionUtils.TrueTier();
			__instance.regionTier = RegionUtils.GetRandomRegionFromTier(tier);
			__instance.map = MapGenerator.GenerateMap(RunState.CurrentMapRegion, 3, 13);
			__instance.currentNodeId = __instance.map.RootNode.id;
			return false;
		}
	}
}
