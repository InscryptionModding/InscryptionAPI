using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Adds an animation to the Permadeath sigil
[HarmonyPatch(typeof(CardAnimationController), nameof(CardAnimationController.PlayPermaDeathAnimation))]
internal class Act1PermaDeathPatch
{
    [HarmonyPostfix]
    private static void PlayPermaDeathAnimation(CardAnimationController __instance, bool playSound)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            var component = __instance.PlayableCard.GetComponentInChildren<SkinnedMeshRenderer>();

            if (playSound)
            {
                AudioController.Instance.PlaySound3D("disk_card_overload", MixerGroup.CardPaperSFX, __instance.transform.position, 1f, 0.45f);
                AudioController.Instance.PlaySound3D("card_death", MixerGroup.TableObjectsSFX, __instance.transform.position, 1f, 0f, new(AudioParams.Pitch.Variation.VerySmall), new(0.05f));
            }

            //__instance.PlayableCard.Anim.PlayDeathAnimation(playSound);
            GameObject obj = Singleton<FirstPersonController>.Instance.AnimController.PlayOneShotAnimation("SplitCard");
            obj.transform.parent = null;
            obj.transform.position = __instance.PlayableCard.transform.position;
            obj.transform.eulerAngles = new Vector3(90f, 0f, 0f);

            component.enabled = false;
            __instance.StopAllCoroutines();
        }
        else
        {
            __instance.PlayDeathAnimation(playSound);
        }
    }
}