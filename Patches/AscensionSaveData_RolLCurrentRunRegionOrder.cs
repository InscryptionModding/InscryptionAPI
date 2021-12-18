using API.Utils;
using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace API.Patches
{
	[HarmonyPatch(typeof(AscensionSaveData), "RollCurrentRunRegionOrder")]
	public class AscensionSaveData_RolLCurrentRunRegionOrder
	{
		public static void Postfix()
		{
			if (SaveManager.SaveFile.ascensionActive)
			{
				RunState.Run.regionTier = RegionUtils.GetRandomRegionFromTier(RunState.Run.regionOrder[0]);
			}
		}
	}
}
