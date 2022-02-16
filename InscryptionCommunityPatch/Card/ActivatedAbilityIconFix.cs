using HarmonyLib;
using DiskCardGame;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class ActivatedAbilityIconFix
{
    public static bool HasActivatedAbility(this PlayableCard card)
    {
        if (!card.Info.Abilities.Exists(elem => AbilitiesUtil.GetInfo(elem).activated))
        {
            return card.temporaryMods.Exists(elem => elem.abilities.Exists(elem2 => AbilitiesUtil.GetInfo(elem2).activated));
        }

        return true;
    }

    [HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.RespondsToResolveOnBoard))]
    [HarmonyPostfix]
    public static void RespondsToResolveOnBoard_PostFix(ref bool __result)
    {
        __result &= SaveManager.saveFile.IsPart2;
    }

    [HarmonyPatch(typeof(DiskCardGame.Card), nameof(DiskCardGame.Card.UpdateInteractableIcons))]
    [HarmonyPostfix]
    public static void UpdateInteractableIcons_PostFix(ref DiskCardGame.Card __instance)
    {
        if (SaveManager.saveFile.IsPart2 || __instance is not PlayableCard card || !card.HasActivatedAbility())
            return;

        var abilityHandler = __instance.gameObject.GetComponent<ActivatedAbilityHandler3D>();

        if (abilityHandler is null)
        {
            abilityHandler = __instance.gameObject.AddComponent<ActivatedAbilityHandler3D>();
            abilityHandler.SetCard(__instance as PlayableCard);
        }

        var abilityIcons = __instance.gameObject.GetComponentsInChildren<AbilityIconInteractable>().Where(elem => AbilitiesUtil.GetInfo(elem.Ability).activated).ToList();

        var activatedAbilityComponents = __instance.gameObject.GetComponentsInChildren<ActivatedAbilityIconInteractable>(true).ToList();

        if (abilityIcons.Count() == activatedAbilityComponents.Count())
            return;

        abilityIcons.RemoveAll(elem => activatedAbilityComponents.Exists(elem2 => elem.Ability == elem2.Ability));

        foreach (var icon in abilityIcons)
        {
            var go = icon.gameObject;
            go.layer = 0;

            var interactable = go.AddComponent<ActivatedAbilityIconInteractable>();
            interactable.AssigneAbility(icon.Ability);

            abilityHandler.AddInteractable(interactable);
        }
    }
}