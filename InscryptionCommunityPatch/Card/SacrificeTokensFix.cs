using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

public static class SacrificeTokensFix
{
    // hard-coding this to 20 extra tokens for a total of 24 (the vanilla max is 12 but eh-eh)
    // if a modder wants to change this they can set it to something else in their own mod
    public static int amountOfNewTokens = 20;

    [HarmonyPatch(typeof(BoardManager3D), nameof(BoardManager3D.Start))]
    private class BoardManager3DPatch
    {
        // Fixes blood costs greater than 4 breaking the game by adding more blood tokens
        // Currently the game still only displays 4, but it fixes it so idc
        [HarmonyPostfix]
        private static void Postfix(ref BoardManager3D __instance)
        {
            if (__instance.tokens != null)
            {
                for (int i = 0; i < amountOfNewTokens; i++)
                {
                    SacrificeToken token = SacrificeToken.Instantiate(__instance.tokens.tokens[i]);
                    __instance.tokens.tokens.Add(token);
                }
            }
        }
    }
}