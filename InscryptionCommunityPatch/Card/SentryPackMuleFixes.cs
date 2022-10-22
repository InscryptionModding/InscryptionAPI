using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using Pixelplacement;
using Pixelplacement.TweenSystem;

namespace InscryptionCommunityPatch.Card;

// Fixes PackMule breaking the game when killed before instantiating its pack object
[HarmonyPatch]
public class SentryPackMuleFixes
{
    // Prevents the logic for opening the pack from running if the pack object is null
    [HarmonyPatch(typeof(PackMule), nameof(PackMule.SpawnAndOpenPack))]
    [HarmonyPrefix]
    public static bool FixPackMuleBreakingRealityOnDeath(Transform pack)
    {
        if (pack == null)
            return false;

        return true;
    }

    [HarmonyPatch(typeof(PackMule), nameof(PackMule.OnDie))]
    [HarmonyPostfix]
    public static IEnumerator FixPackNotExisting(IEnumerator enumerator, PackMule __instance)
    {
        if (__instance.pack == null)
        {
            PatchPlugin.Logger.LogDebug($"{__instance.PlayableCard.name} died before fully resolving on board, instantiating pack.");

            // Instantiates the pack object if it's null
            // Code copied from PackMule's OnResolveOnBoard logic
            yield return new WaitForSeconds(0.4f);
            GameObject gameObject = UnityEngine.Object.Instantiate(ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CardPack"));
            __instance.pack = gameObject.transform;
            Vector3 position = __instance.PlayableCard.Slot.transform.position;
            Vector3 position2 = position + Vector3.forward * 8f;
            __instance.pack.position = position2;
            Tween.Position(__instance.pack, position, 0.25f, 0f, Tween.EaseOut);
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => !Tween.activeTweens.Exists((TweenBase t) => t.targetInstanceID == __instance.PlayableCard.transform.GetInstanceID()));
            __instance.pack.SetParent(__instance.PlayableCard.transform);

            PatchPlugin.Logger.LogDebug($"Pack has been instantiated. Reality has been saved.");
        }
        yield return enumerator;
    }

    // Prevents the logic for OnResolve from running if the card is dead
    // (prevents a minor visual glitch)
    [HarmonyPatch(typeof(PackMule), nameof(PackMule.OnResolveOnBoard))]
    [HarmonyPrefix]
    public static bool FixPackMuleVisualGlitch(PackMule __instance)
    {
        if (__instance.PlayableCard.Dead)
            return false;

        return true;
    }
}