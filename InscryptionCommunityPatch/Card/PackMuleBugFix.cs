using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class PackMuleBugFix
{
    [HarmonyPatch(typeof(PackMule), nameof(PackMule.RespondsToResolveOnBoard))]
    [HarmonyPostfix]
    private static void AlwaysResolveOnBoard(ref bool __result)
    {
        __result = true;
    }
}