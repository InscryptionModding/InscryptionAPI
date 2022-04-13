using DiskCardGame;
using HarmonyLib;
using Sirenix.Utilities;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class ActivatedAbilityIconFix
{
    public static bool HasActivatedAbility(this PlayableCard card)
    {
        return card.Info.Abilities.Exists(elem => AbilitiesUtil.GetInfo(elem).activated)
            || card.temporaryMods.Exists(elem => elem.abilities.Exists(elem2 => AbilitiesUtil.GetInfo(elem2).activated));

    }

    [HarmonyPostfix, HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.RespondsToResolveOnBoard))]
    private static void RespondsToResolveOnBoard_PostFix(ref bool __result)
    {
        __result &= SaveManager.saveFile.IsPart2;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.OnStatsChanged))]
    private static void FixActivatedAbilitiesOnAnyChange(ref PlayableCard __instance)
    {
        if (!__instance.HasActivatedAbility())
            return;

        ActivatedAbilityHandler3D abilityHandler3D = __instance.GetComponent<ActivatedAbilityHandler3D>();

        // at least 1 group will always be active
        GameObject activeDefaultIconGroup = __instance.AbilityIcons.defaultIconGroups.Find(group => group.activeInHierarchy);

        if (abilityHandler3D.SafeIsUnityNull())
        {
            PatchPlugin.Logger.LogDebug($"[PlayableCard.OnStatsChanged] Adding activated ability handler to card [{__instance.Info.displayedName}]");
            abilityHandler3D = __instance.gameObject.AddComponent<ActivatedAbilityHandler3D>();
        }

        if (abilityHandler3D.currentIconGroup != activeDefaultIconGroup)
        {
            PatchPlugin.Logger.LogDebug($"[PlayableCard.OnStatsChanged] -> Need to reassign activated ability list as icon list has changed for card [{__instance.Info.displayedName}]");
            abilityHandler3D.UpdateInteractableList(activeDefaultIconGroup);
        }
    }
}