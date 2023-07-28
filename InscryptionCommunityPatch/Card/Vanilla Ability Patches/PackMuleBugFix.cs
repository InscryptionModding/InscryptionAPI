using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch(typeof(PackMule), nameof(PackMule.RespondsToResolveOnBoard))]
internal class PackMuleBugFix
{
    [HarmonyPostfix]
    private static void AlwaysResolveOnBoard(ref bool __result) => __result = true;
}