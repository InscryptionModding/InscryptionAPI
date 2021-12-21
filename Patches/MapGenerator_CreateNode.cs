using API.Utils;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace API.Patches
{
	// TODO: Fix both to work with ascension

	[HarmonyPatch(typeof(MapGenerator), "CreateNode", new Type[] {typeof(int), typeof(int), typeof(List<NodeData>), typeof(List<NodeData>), typeof(int)})]
	public class MapGenerator_CreateNode
	{
		public static void Postfix(ref NodeData __result, int y)
		{
			if (__result is CardBattleNodeData)
            {
				((CardBattleNodeData) __result).difficulty = RegionUtils.TrueTier() * 6 + (y + 1) / 3 - 1;
			}
		}
	}
}
