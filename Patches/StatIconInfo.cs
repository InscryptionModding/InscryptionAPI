using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;

namespace API.Patches
{
	[HarmonyPatch(typeof(StatIconInfo), "AllIconInfo", MethodType.Getter)]
	public class StatIconInfoPatch
	{
		[HarmonyPostfix]
		static void Postfix(ref List<StatIconInfo> __result)
		{
			foreach (var ability in NewSpecialAbility.specialAbilities)
			{
				// this is never being logged?
				APIPlugin.Plugin.Log.LogInfo($"Adding {ability.statIconInfo.name} in StatIconInfo");
				if (!__result.Exists(x => x == ability.statIconInfo)) {
					__result.Add(ability.statIconInfo);
				}
			}

		}
	}
}