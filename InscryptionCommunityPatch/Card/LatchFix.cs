using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace InscryptionCommunityPatch.Card
{
    [HarmonyPatch]
    public static class LatchFix
    {
        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ChooseTarget))]
        [HarmonyPostfix]
        public static IEnumerator ReEnanbleSlots(IEnumerator result, BoardManager __instance, List<CardSlot> allTargets)
        {
            List<CardSlot> validSlots = __instance.currentValidSlots;
            __instance.currentValidSlots = null;
            (__instance.GetComponent<SelectTargetHolder>() ?? __instance.gameObject.AddComponent<SelectTargetHolder>()).isSelectingTarget = true;
            List<CardSlot> toDisable = new();
            foreach(CardSlot cs in allTargets)
            {
                if(!cs.Enabled)
                {
                    cs.SetEnabled(true);
                    toDisable.Add(cs);
                }
            }
            yield return result;
            foreach(CardSlot cs in toDisable)
            {
                cs.SetEnabled(false);
            }
            (__instance.GetComponent<SelectTargetHolder>() ?? __instance.gameObject.AddComponent<SelectTargetHolder>()).isSelectingTarget = false;
            __instance.currentValidSlots = validSlots;
            yield break;
        }

        [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.OnCursorEnter))]
        [HarmonyPostfix]
        public static void NoSacrificeMarker(CardSlot __instance)
        {
            if ((Singleton<BoardManager>.Instance?.GetComponent<SelectTargetHolder>()?.isSelectingTarget).GetValueOrDefault())
            {
                __instance.Card?.Anim?.SetSacrificeHoverMarkerShown(false);
            }
        }

        [HarmonyPatch(typeof(Latch), nameof(Latch.OnPreDeathAnimation))]
        [HarmonyPostfix]
        public static IEnumerator ChangeClawMaterial(IEnumerator result)
        {
            bool appliedAct1ClawVisualChange = !(SaveManager.SaveFile?.IsPart1).GetValueOrDefault();
            GameObject clawParent = null;
            GameObject claw = null;
            Material cannonmat = null;
            if (!appliedAct1ClawVisualChange)
            {
                try
                {
                    cannonmat = new Material(ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CannonTargetIcon").GetComponentInChildren<Renderer>().material);
                }
                catch { }
            }
            if(cannonmat == null)
            {
                appliedAct1ClawVisualChange = true;
            }
            while (result.MoveNext())
            {
                if (!appliedAct1ClawVisualChange)
                {
                    if(clawParent == null)
                    {
                        clawParent = result.GetType().GetField("<gameObject>5__5", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(result) as GameObject;
                    }
                    if (clawParent != null)
                    {
                        if (claw == null && clawParent.transform.childCount > 0 && clawParent.transform.GetChild(0) != null)
                        {
                            claw = clawParent.transform.GetChild(0).gameObject;
                        }
                    }
                    if(claw != null)
                    {
                        Renderer[] renderers = claw.GetComponentsInChildren<Renderer>();
                        foreach(Renderer rend in renderers)
                        {
                            if(rend != null)
                            {
                                rend.material = cannonmat;
                            }
                        }
                        appliedAct1ClawVisualChange = true;
                    }
                }
                yield return result.Current;
            }
            yield break;
        }

        private class SelectTargetHolder : MonoBehaviour
        {
            public bool isSelectingTarget;
        }
    }
}
