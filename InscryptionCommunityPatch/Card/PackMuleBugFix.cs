using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card
{
    //This is a bugfix to the packmule special ability, to prevent it from softlocking if the player some how obtains it and tries to play it.
    public class PackMuleBugFix
    {
        [HarmonyPatch(typeof(PackMule), nameof(PackMule.RespondsToResolveOnBoard))]
        public class PackMulePatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result)
            {
                __result = true;
            }
        }
    }
}
