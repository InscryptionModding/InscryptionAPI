using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public class Act1LatchAbilityFix
{
    public static GameObject _clawPrefab;
    private static GameObject ClawPrefab
    {
        get
        {
            if (_clawPrefab == null)
                _clawPrefab = ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/LatchClaw");

            return _clawPrefab;
        }
    }

    [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ChooseTarget))]
    [HarmonyPostfix]
    private static IEnumerator ReEnableSlots(IEnumerator result, BoardManager __instance, List<CardSlot> allTargets)
    {
        List<CardSlot> validSlots = new(__instance.currentValidSlots);
        __instance.currentValidSlots = null;
        (__instance.GetComponent<SelectTargetHolder>() ?? __instance.gameObject.AddComponent<SelectTargetHolder>()).isSelectingTarget = true;
        List<CardSlot> toDisable = new();
        foreach (CardSlot cs in allTargets.Where(cs => !cs.Enabled))
        {
            cs.SetEnabled(true);
            toDisable.Add(cs);
        }
        yield return result;
        foreach (CardSlot cs in toDisable)
        {
            cs.SetEnabled(false);
        }
        (__instance.GetComponent<SelectTargetHolder>() ?? __instance.gameObject.AddComponent<SelectTargetHolder>()).isSelectingTarget = false;
        __instance.currentValidSlots = validSlots;
    }

    [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.OnCursorEnter))]
    [HarmonyPostfix]
    private static void NoSacrificeMarker(CardSlot __instance)
    {
        if (__instance.Card && BoardManager.Instance
            && (BoardManager.Instance.GetComponent<SelectTargetHolder>()?.isSelectingTarget).GetValueOrDefault())
        {
            __instance.Card.Anim.SetSacrificeHoverMarkerShown(false);
        }
    }

    private class SelectTargetHolder : MonoBehaviour
    {
        public bool isSelectingTarget;
    }

    private static void AimWeaponAnim(GameObject tweenObj, Vector3 target) => Tween.LookAt(tweenObj.transform, target, Vector3.up, 0.075f, 0.0f, Tween.EaseInOut);

    [HarmonyPrefix, HarmonyPatch(typeof(Latch), nameof(Latch.OnPreDeathAnimation))]
    private static void PrefixPassStateOnPreDeath(out Latch __state, ref Latch __instance) => __state = __instance;

    [HarmonyPostfix, HarmonyPatch(typeof(Latch), nameof(Latch.OnPreDeathAnimation))]
    private static IEnumerator Postfix(IEnumerator enumerator, Latch __state, bool wasSacrifice)
    {
        // return default logic for Part 3
        if (SaveManager.SaveFile.IsPart3)
        {
            yield return enumerator;
            yield break;
        }

        List<CardSlot> validTargets = BoardManager.Instance.AllSlotsCopy;
        validTargets.RemoveAll(slot => slot.Card == null || slot.Card.Dead || __state.CardHasLatchMod(slot.Card) || slot.Card == __state.Card);

        if (PatchPlugin.configFullDebug.Value)
        {
            PatchPlugin.Logger.LogDebug($"[LatchFix] Started death, latch name: [{__state.name}]");
            PatchPlugin.Logger.LogDebug("[LatchFix] Count of Valid Targets : " + validTargets.Count);
        }

        // break if no valid targets
        if (validTargets.Count == 0)
            yield break;

        ViewManager.Instance.SwitchToView(View.Board);
        __state.Card.Anim.PlayHitAnimation();

        yield return new WaitForSeconds(0.1f);

        CardAnimationController anim = __state.Card.Anim;

        GameObject latchParentGameObject = new GameObject
        {
            name = "LatchParent",
            transform =
                {
                    position = anim.transform.position
                }
        };
        latchParentGameObject.transform.SetParent(anim.transform);

        Transform latchParent = latchParentGameObject.transform;
        GameObject claw = UnityObject.Instantiate(ClawPrefab, latchParent);
        Material cannonMat = null;
        try
        {
            cannonMat = new Material(ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CannonTargetIcon").GetComponentInChildren<Renderer>().material);
        }
        catch { }
        if (cannonMat != null)
        {
            Renderer[] renderers = claw.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers.Where(rend => rend))
            {
                rend.material = cannonMat;
            }
        }

        CardSlot selectedSlot = null;

        if (__state.Card.OpponentCard)
        {
            yield return new WaitForSeconds(0.3f);
            yield return __state.AISelectTarget(validTargets, s => selectedSlot = s);

            if (selectedSlot != null && selectedSlot.Card != null)
            {
                AimWeaponAnim(latchParent.gameObject, selectedSlot.transform.position);
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            List<CardSlot> allSlotsCopy = BoardManager.Instance.AllSlotsCopy;
            allSlotsCopy.Remove(__state.Card.Slot);

            yield return BoardManager.Instance.ChooseTarget(allSlotsCopy, validTargets,
                s => selectedSlot = s, // target selected callback
                __state.OnInvalidTarget, // invalid target callback
                s => // slot cursor enter callback
                {
                    if (s.Card == null)
                        return;

                    AimWeaponAnim(latchParent.gameObject, s.transform.position);
                },
                null, // cancel condition
                CursorType.Target);
        }

        claw.SetActive(true);

        CustomCoroutine.FlickerSequence(
            () => claw.SetActive(true),
            () => claw.SetActive(false),
            true,
            false,
            0.05f,
            2
        );

        if (selectedSlot != null && selectedSlot.Card != null)
        {
            CardModificationInfo mod = new(__state.LatchAbility)
            {
                // these control rendering, so only set to true if said rendering won't butt everything
                fromCardMerge = SaveManager.SaveFile.IsPart1,
                fromLatch = SaveManager.SaveFile.IsPart1 || SaveManager.SaveFile.IsPart3
            };

            if (PatchPlugin.configFullDebug.Value)
                PatchPlugin.Logger.LogDebug($"[LatchFix] Selected card name [{selectedSlot.Card.name}]");

            if (selectedSlot.Card.Info.name == "!DEATHCARD_BASE")
            {
                selectedSlot.Card.AddTemporaryMod(mod);
            }
            else
            {
                CardInfo info = selectedSlot.Card.Info.Clone() as CardInfo;
                info.Mods = new(selectedSlot.Card.Info.Mods) { mod };
                selectedSlot.Card.SetInfo(info);
            }

            selectedSlot.Card.Anim.PlayTransformAnimation();
            __state.OnSuccessfullyLatched(selectedSlot.Card);

            yield return new WaitForSeconds(0.75f);
            yield return __state.LearnAbility();
        }
    }
}