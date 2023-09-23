using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class ShieldManager
{
    public static CardModificationInfo NegateShieldMod(Ability ab)
    {
        CardModificationInfo mod = new()
        {
            negateAbilities = new() { ab },
            nonCopyable = true
        };
        mod.SetExtendedProperty("APINegateShield", true);
        return mod;
    }
    public static CardModificationInfo ResetShieldMod(Ability ab)
    {
        CardModificationInfo mod = new(ab)
        {
            nonCopyable = true
        };
        mod.SetExtendedProperty("APIResetShield", true);
        return mod;
    }

    /// <summary>
    /// The method used for when a shielded card is damaged. Includes extra parameters for modders looking to modify this further.
    /// This method is only called when damage > 0 and the target has a shield.
    /// </summary>
    public static void BreakShield(PlayableCard target, int damage, PlayableCard attacker)
    {
        target.Anim.StrongNegationEffect();

        var components = target.GetComponents<DamageShieldBehaviour>();
        foreach (var component in components)
        {
            // only reduce numShields for components with positive count
            if (component.HasShields())
            {
                component.numShields--;

                // if we've exhausted this shield component's count, negate it
                // so it updates the display
                if (component.NumShields == 0)
                    target.AddTemporaryMod(NegateShieldMod(component.Ability));

                break;
            }
        }
        var components2 = target.GetComponents<ActivatedDamageShieldBehaviour>();
        foreach (var component2 in components2)
        {
            // only reduce numShields for components with positive count
            if (component2.HasShields())
            {
                component2.numShields--;

                // if we've exhausted this shield component's count, negate it
                // so it updates the display
                if (component2.NumShields == 0)
                    target.AddTemporaryMod(NegateShieldMod(component2.Ability));

                break;
            }
        }

        // if we removed the last shield
        if (target.GetTotalShields() == 0)
        {
            target.Status.lostShield = true;

            if (target.Info.name == "MudTurtle")
                target.SwitchToAlternatePortrait();
            else if (target.Info.HasBrokenShieldPortrait())
                target.SwitchToPortrait(target.Info.BrokenShieldPortrait());
        }
        target.UpdateFaceUpOnBoardEffects();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasShield))]
    private static void ReplaceHasShieldBool(PlayableCard __instance, ref bool __result)
    {
        __result = NewHasShield(__instance);
    }
    public static bool NewHasShield(PlayableCard instance)
    {
        if (instance.GetTotalShields() > 0)
            return !instance.Status.lostShield;

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.ResetShield))]
    private static void ResetModShields(PlayableCard __instance)
    {
        foreach (var com in __instance.GetComponents<DamageShieldBehaviour>())
            com.ResetShields();

        foreach (var com in __instance.GetComponents<ActivatedDamageShieldBehaviour>())
            com.ResetShields();

        // when you negate an ability the component is removed,
        // so we need to re-add the components when resetting the shields
        CardModificationInfo[] mods = __instance.TemporaryMods.FindAll(x => x.GetExtendedPropertyAsBool("APINegateShield") ?? false).ToArray();
        List<CardModificationInfo> resetMods = new();
        foreach (CardModificationInfo mod in mods)
        {
            Ability ab = mod.negateAbilities[0];
            resetMods.Add(ResetShieldMod(ab));
        }

        // remove negating mods before adding new activating mods
        __instance.RemoveTemporaryMods(mods);
        __instance.AddTemporaryMods(resetMods.ToArray());
    }
}