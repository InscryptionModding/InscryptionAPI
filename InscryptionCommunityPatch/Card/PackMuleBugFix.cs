using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

//This is a bugfix to the packmule special ability, to prevent it from softlocking if the player some how obtains it and tries to play it.
internal static class PackMuleBugFix
{
    [HarmonyPatch(typeof(PackMule), nameof(PackMule.RespondsToResolveOnBoard))]
    private class PackMulePatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }
}