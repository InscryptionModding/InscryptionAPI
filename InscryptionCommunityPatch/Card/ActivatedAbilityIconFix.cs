using DiskCardGame;
using HarmonyLib;
using Sirenix.Utilities;

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
        if (SaveManager.SaveFile.IsPart2 || !__instance.HasActivatedAbility())
            return;

        ActivatedAbilityHandler3D abilityHandler3D = __instance.GetComponent<ActivatedAbilityHandler3D>();

        if (abilityHandler3D.SafeIsUnityNull())
        {
            if (PatchPlugin.configFullDebug.Value)
                PatchPlugin.Logger.LogDebug($"[PlayableCard.OnStatsChanged] Adding activated ability handler to card [{__instance.Info.displayedName}]");

            abilityHandler3D = __instance.gameObject.AddComponent<ActivatedAbilityHandler3D>();
        }

        if (!abilityHandler3D.SafeIsUnityNull() && __instance.AbilityIcons.abilityIcons != null)
        {
            if (PatchPlugin.configFullDebug.Value)
                PatchPlugin.Logger.LogDebug($"[PlayableCard.OnStatsChanged] -> Resetting icon list for [{__instance.Info.displayedName}]");

            abilityHandler3D.UpdateInteractableList(__instance.AbilityIcons.abilityIcons);
        }
    }
}