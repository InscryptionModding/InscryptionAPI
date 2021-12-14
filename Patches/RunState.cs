using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace API.Patches
{
	[HarmonyPatch(typeof(RunState), "Initialize")]
	public class RunState_Initialize
	{
		static void Postfix(ref RunState __instance)
		{
			// Random regions have been removed
			//__instance.regionIndex = RegionProgression.GetRandomRegionIndexForTier(0);
		}
	}
}
