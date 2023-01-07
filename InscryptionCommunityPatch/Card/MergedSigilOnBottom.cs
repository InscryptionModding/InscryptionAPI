using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public class MergedSigilOnBottom
{
    // This set of patches handles the modification of how ability icons are displayed on the card
    //
    // If the "mergeonbottom" config is set, it means that the emissive card merge ability icon (the big blue one
    // that shows on the card when you merge two cards together) should be displayed at normal size on the bottom
    // instead of being stamped over the artwork.
    //
    // If the "removepatches" config is set, it means that the ability icon should display with the emissive
    // color but without the little patch texture behind it.

    [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.ApplyAbilitiesToIcons))]
    [HarmonyPrefix]
    private static bool DontPlaceCardModIcons_IfShowOnBottom(CardAbilityIcons __instance, List<AbilityIconInteractable> icons, Material iconMat)
    {
        // If we are showing card modifications sigils on the bottom, we don't allow the
        // AppyAbilitiesToIcons method to do anything
        if (iconMat == __instance.emissiveIconMat && PatchPlugin.configMergeOnBottom.Value && PatchPlugin.configRemovePatches.Value)
        {
            foreach (var icon in icons)
                icon.gameObject.SetActive(false);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.PositionModIcons))]
    [HarmonyPrefix]
    private static void MoveMergeIconsToDefaultGroup_IfShowOnBottom(List<Ability> defaultAbilities, List<Ability> mergeAbilities)
    {
        // This is the first time that the default abilities list is used once it has been built
        // What we want to do is take all of the abilities from the merge group and move them to the default group
        // if the "show merged icons on bottom" setting is active.

        if (PatchPlugin.configMergeOnBottom.Value)
        {
            defaultAbilities.AddRange(mergeAbilities);

            if (PatchPlugin.configRemovePatches.Value)
                mergeAbilities.Clear();
        }
    }

    // Token: 0x060000D0 RID: 208 RVA: 0x00004C0C File Offset: 0x00002E0C
    [HarmonyPatch(typeof(AbilityIconInteractable), "LoadIcon")]
    [HarmonyPostfix]
    private static void RepositionAndRetextureMergedIcons_IfShowOnBottom(ref Texture __result, ref CardInfo info, ref AbilityInfo ability, ref AbilityIconInteractable __instance)
    {
        if (!PatchPlugin.configMergeOnBottom.Value)
            return;

        if (info != null)
        {
            if (info.Mods.Count > 0)
            {
                List<CardModificationInfo> MergeSigils = info.Mods.FindAll(x => x.fromCardMerge);

                if (MergeSigils.Count > 0)
                {
                    foreach (CardModificationInfo mod in MergeSigils)
                    {
                        if (mod.abilities.Contains(ability.ability) && (__instance.name == "AbilityIcon" || __instance.name == "DefaultIcons_1Ability"))
                        {
                            if (PatchPlugin.configRemovePatches.Value)
                            {
                                __instance.SetMaterial(Singleton<CardAbilityIcons>.Instance.totemIconMat);
                                if (__instance.GetComponentInParent<MeshRenderer>() != null)
                                {
                                    __instance.GetComponentInParent<MeshRenderer>().enabled = true;
                                }
                                __instance.SetColor(Color.white);
                                return;
                            }
                            int sigils = (info.DefaultAbilities.Count + MergeSigils.Count);
                            if (sigils <= 8)
                            {
                                //Plugin.Log.LogInfo(sigils + " " + ability.ToString() + " " + info.name);

                                Transform[] allChildren = __instance.transform.parent.transform.parent.GetComponentsInChildren<Transform>();
                                //Plugin.Log.LogInfo(__instance.name);
                                if (__instance.name == "DefaultIcons_1Ability")
                                {
                                    allChildren = __instance.transform.parent.GetComponentsInChildren<Transform>();
                                }

                                foreach (Transform child in allChildren)
                                {
                                    if (child.gameObject.activeSelf && child.gameObject.name.StartsWith("CardMergeIcon_"))
                                    {
                                        if (child.gameObject.GetComponent<AbilityIconInteractable>().Ability == __instance.Ability)
                                        {
                                            child.gameObject.transform.localPosition = __instance.transform.localPosition;
                                            child.gameObject.transform.localScale = __instance.transform.localScale;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
            }
        }
    }
}