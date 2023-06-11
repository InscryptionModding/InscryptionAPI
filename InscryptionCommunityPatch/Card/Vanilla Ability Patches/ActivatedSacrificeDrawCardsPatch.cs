using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class ActivatedSacrificeDrawCardsPatch
{
    [HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.CanActivate))]
    [HarmonyPostfix]
    private static void FixGemsDraw(ActivatedAbilityBehaviour __instance, ref bool __result)
    {
        if (!__result)
            return;

        if (__instance.Ability == Ability.ActivatedSacrificeDrawCards &&
            !ResourcesManagerHelpers.OwnerHasGems(!__instance.Card.OpponentCard, GemType.Blue))
        {
            __result = false;
        }
    }
}