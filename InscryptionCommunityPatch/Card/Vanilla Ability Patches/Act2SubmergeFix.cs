using DiskCardGame;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch(typeof(Submerge), nameof(Submerge.OnUpkeep))]
internal class Act2SubmergeFix
{
    [HarmonyPostfix]
    private static IEnumerator AddWaitInAct2(IEnumerator enumerator)
    {
        yield return enumerator;
        if (SaveManager.SaveFile.IsPart2)
            yield return new WaitForSeconds(0.25f);
    }
}