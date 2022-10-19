using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using Pixelplacement;
using Pixelplacement.TweenSystem;

namespace InscryptionCommunityPatch.Card;

// These patches fix various issues relating to the Sentry ability in Act 1
[HarmonyPatch]
public class SentryInteractionPatches
{
    // if pack is null, instantiate it then return the vanilla code
    [HarmonyPatch(typeof(PackMule), nameof(PackMule.OnDie))]
    [HarmonyPostfix]
    public static IEnumerator FixPackNotExisting(IEnumerator enumerator, PackMule __instance)
    {
        if (__instance.pack == null)
        {
            // instantiate pack object
            yield return new WaitForSeconds(0.4f);
            GameObject gameObject = Object.Instantiate(ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CardPack"));
            __instance.pack = gameObject.transform;
            Vector3 position = __instance.PlayableCard.Slot.transform.position;
            Vector3 position2 = position + Vector3.forward * 8f;
            __instance.pack.position = position2;
            Tween.Position(__instance.pack, position, 0.25f, 0f, Tween.EaseOut);
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => !Tween.activeTweens.Exists((TweenBase t) => t.targetInstanceID == __instance.PlayableCard.transform.GetInstanceID()));
            __instance.pack.SetParent(__instance.PlayableCard.transform);
        }
        yield return enumerator;
    }

    // Prevents the game from trying to open the pack when it doesn't exist
    [HarmonyPatch(typeof(PackMule), nameof(PackMule.SpawnAndOpenPack))]
    [HarmonyPostfix]
    public static IEnumerator FixPackMuleBreakingRealityOnDeath(IEnumerator enumerator, Transform pack)
    {
        if (pack == null)
        {
            yield break;
        }
        yield return enumerator;
    }

    // Prevent the game from running the OnResolve logic when this card is dead (fixes a minor visual glitch)
    [HarmonyPatch(typeof(PackMule), nameof(PackMule.OnResolveOnBoard))]
    [HarmonyPostfix]
    public static IEnumerator FixPackMuleVisualGlitch(IEnumerator enumerator, PackMule __instance)
    {
        if (__instance.PlayableCard.Dead)
        {
            yield break;
        }
        yield return enumerator;
    }
}