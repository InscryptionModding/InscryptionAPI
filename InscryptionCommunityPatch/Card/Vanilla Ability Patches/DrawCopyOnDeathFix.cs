using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class DrawCopyOnDeathFix
{
    [HarmonyPatch(typeof(DrawCopyOnDeath), nameof(DrawCopyOnDeath.CardToDraw), MethodType.Getter)]
    [HarmonyPostfix]
    private static void UseAlternatePortrait(DrawCopyOnDeath __instance, ref CardInfo __result)
    {
        if (__instance?.Card?.Info != null)
            __result = __instance.Card.Info.Clone() as CardInfo;
    }
}