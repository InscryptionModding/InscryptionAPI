using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class Act2SubmergeFix
{
    [HarmonyPatch(typeof(Submerge), nameof(Submerge.OnUpkeep))]
    [HarmonyPostfix]
    private static IEnumerator AlwaysResolveOnBoard(IEnumerator enumerator)
    {
        yield return enumerator;
        if (SaveManager.SaveFile.IsPart2)
            yield return new WaitForSeconds(0.25f);
    }
}