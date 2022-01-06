using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace API.Patches
{
	[HarmonyPatch(typeof(CardDisplayer3D), "GetEmissivePortrait")]
	public class CardDisplayer3D_GetEmissivePortrait
	{
		static bool Prefix(Sprite mainPortrait, ref Sprite __result)
		{
			Sprite sprite;
			if (RunState.Run.eyeState == EyeballState.Goat)
			{
				if (NewCard.altEmissions.TryGetValue(mainPortrait.name, out sprite))
				{
					__result = sprite;
					return false;
				}

				if (CustomCard.altEmissions.TryGetValue(mainPortrait.name, out sprite))
				{
					__result = sprite;
					return false;
				}
			}
			
			if (NewCard.emissions.TryGetValue(mainPortrait.name, out sprite))
			{
				__result = sprite;
				return false;
			}

			if (CustomCard.emissions.TryGetValue(mainPortrait.name, out sprite))
			{
				__result = sprite;
				return false;
			}

			return true;
		}
	}
}
