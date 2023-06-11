using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class SteelTrapFix
{
    [HarmonyPatch(typeof(SteelTrap), nameof(SteelTrap.OnTakeDamage))]
    [HarmonyPostfix]
    private static IEnumerator UseAlternatePortrait(IEnumerator enumerator, SteelTrap __instance)
    {
        // vanilla code for base Trap cards
        if (__instance.Card.Info.IsBaseGameCard() && __instance.Card.name.Contains("Trap"))
        {
            yield return enumerator;
            yield break;
        }

        yield return new WaitForSeconds(0.65f);
        AudioController.Instance.PlaySound3D("sacrifice_default", MixerGroup.TableObjectsSFX, __instance.Card.transform.position);
        yield return new WaitForSeconds(0.1f);
        __instance.Card.Anim.LightNegationEffect();

        if (__instance.Card.HasAlternatePortrait())
            __instance.Card.SwitchToAlternatePortrait();

        AudioController.Instance.PlaySound3D("dial_metal", MixerGroup.TableObjectsSFX, __instance.Card.transform.position);
        yield return new WaitForSeconds(0.75f);
    }
}