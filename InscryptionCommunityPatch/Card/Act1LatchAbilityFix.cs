using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch(typeof(Latch), nameof(Latch.OnPreDeathAnimation))]
public class Act1LatchAbilityFix
{
    private static GameObject _clawPrefab;
    private static GameObject ClawPrefab
    {
        get
        {
            if (_clawPrefab == null)
                _clawPrefab = ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/LatchClaw");

            return _clawPrefab;
        }
    }

    private static void AimWeaponAnim(GameObject TweenObj, Vector3 target) => Tween.LookAt(TweenObj.transform, target, Vector3.up, 0.075f, 0.0f, Tween.EaseInOut);

    [HarmonyPrefix]
    public static void Prefix1(out Latch __state, ref Latch __instance) => __state = __instance;

    [HarmonyPostfix]
    public static IEnumerator Postfix(IEnumerator enumerator, Latch __state, bool wasSacrifice)
    {
        if (SceneLoader.ActiveSceneName != "Part1_Cabin")
        {
            yield return enumerator;
            yield break;
        }

        PatchPlugin.Logger.LogInfo("Started death");
        PatchPlugin.Logger.LogInfo(__state.name);

        List<CardSlot> validTargets = BoardManager.Instance.AllSlotsCopy;
        validTargets.RemoveAll(slot => slot.Card == null || slot.Card.Dead || __state.CardHasLatchMod(slot.Card) || slot.Card == __state.Card);

        PatchPlugin.Logger.LogInfo("Count of Valid Targets : " + validTargets.Count);

        if (validTargets.Count > 0)
        {
            ViewManager.Instance.SwitchToView(View.Board);
            __state.Card.Anim.PlayHitAnimation();

            yield return new WaitForSeconds(0.1f);

            CardAnimationController anim = __state.Card.Anim;

            GameObject gameObject = new GameObject
            {
                name = "LatchParent",
                transform =
                {
                    position = anim.transform.position
                }
            };
            gameObject.gameObject.transform.parent = anim.transform;

            Transform latchParent = gameObject.transform;
            GameObject claw = UnityEngine.Object.Instantiate<GameObject>(ClawPrefab, latchParent);

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

                yield return BoardManager.Instance.ChooseTarget(
                    allSlotsCopy,
                    validTargets,
                    // target selected callback
                    s => selectedSlot = s,
                    // invalid target callback
                    __state.OnInvalidTarget,
                    // slot cursor enter callback
                    s =>
                    {
                        if (s.Card == null)
                            return;

                        AimWeaponAnim(latchParent.gameObject, s.transform.position);
                    },
                    null, // cancel condition
                    CursorType.Target
                );
            }

            claw.SetActive(true);

            CustomCoroutine.FlickerSequence(() => claw.SetActive(true), () => claw.SetActive(false), true, false, 0.05f, 2);

            if (selectedSlot != null && selectedSlot.Card != null)
            {
                CardModificationInfo mod = new CardModificationInfo(__state.LatchAbility)
                {
                    fromCardMerge = true,
                    fromLatch = true
                };
                PatchPlugin.Logger.LogInfo(selectedSlot.Card.name);

                if (selectedSlot.Card.Info.name == "!DEATHCARD_BASE")
                {
                    selectedSlot.Card.AddTemporaryMod(mod);
                }
                else
                {
                    CardInfo info = selectedSlot.Card.Info.Clone() as CardInfo;
                    info.Mods.Add(mod);
                    selectedSlot.Card.SetInfo(info);
                }
                selectedSlot.Card.Anim.PlayTransformAnimation();
                __state.OnSuccessfullyLatched(selectedSlot.Card);

                yield return new WaitForSeconds(0.75f);
                yield return __state.LearnAbility();
            }
        }
    }
}
