using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

public static class SacrificeTokensFix // Allows blood costs over 4 to work properly
{
    // modders can access this to change increase/decrease the amount of tokens added
    public static int amountOfNewTokens = 20;

    [HarmonyPatch(typeof(BoardManager3D), nameof(BoardManager3D.Start))]
    private class BoardManager3DPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref BoardManager3D __instance)
        {
            if (__instance.tokens != null)
            {
                if (amountOfNewTokens > 0)
                {
                    for (int i = 0; i < amountOfNewTokens; i++)
                    {
                        List<SacrificeToken> tokens = __instance.tokens.tokens;
                        SacrificeToken token = UnityObject.Instantiate(tokens[i], tokens[0].transform.parent);
                        token.name = $"SacrificeToken_{5 + i}";
                        token.transform.localPosition += new Vector3(0f, 0.1f, 0f);
                        tokens.Add(token);
                    }
                }
                if (SaveManager.SaveFile.IsPart3)
                {
                    foreach (SacrificeToken token in __instance.tokens.tokens)
                        token.transform.localPosition += new Vector3(1.4f, 0f, 2f);
                }
            }
        }
    }
}