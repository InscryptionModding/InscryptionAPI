using APIPlugin;
using DiskCardGame;
using HarmonyLib;

namespace API.Patches
{
	[HarmonyPatch(typeof(StatIconInfo), "LoadAbilityData")]
	public class StatIconInfoPatch
	{
		[HarmonyPostfix]
		static void Postfix()
		{
			foreach (var ability in NewSpecialAbility.specialAbilities)
			{
				Plugin.Log.LogDebug($"Attempting to add {ability.specialTriggeredAbility} in StatIconInfo");
				if (!StatIconInfo.allIconInfo.Exists(x => x == ability.statIconInfo)) {
					Plugin.Log.LogDebug($"-> Adding {ability.specialTriggeredAbility} in StatIconInfo");
					StatIconInfo.allIconInfo.Add(ability.statIconInfo);
				}
			}

		}
	}
}