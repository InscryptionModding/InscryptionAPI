using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class ShieldManager
{
    /// <summary>
    /// Used to determine whether or not to update the card display.
    /// Only used for shield rendering due to its unique logic, but this the general pattern whenever adding a hidden ability.
    /// </summary>
    private static bool RenderCard = false;

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
                if (component.Ability.GetHideSingleStacks())
                {
                    RenderCard = true;
                    target.Status.hiddenAbilities.Add(component.Ability);
                }
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
                if (component2.Ability.GetHideSingleStacks())
                {
                    RenderCard = true;
                    target.Status.hiddenAbilities.Add(component2.Ability);
                }

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

    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.ResetShield), new Type[] { } )]
    private static void ResetModShields(PlayableCard __instance)
    {
        foreach (var com in __instance.GetComponents<DamageShieldBehaviour>())
            com.ResetShields(false);

        foreach (var com in __instance.GetComponents<ActivatedDamageShieldBehaviour>())
            com.ResetShields(false);

        __instance.SwitchToDefaultPortrait();
        // base ResetShield runs after this
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.UpdateFaceUpOnBoardEffects))]
    private static IEnumerable<CodeInstruction> BetterHideShieldLogic(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        int start = codes.IndexOf(codes.Find(x => x.operand?.ToString() == "DiskCardGame.PlayableCardStatus get_Status()"));
        int end = codes.IndexOf(codes.Find(x => x.operand?.ToString() == "Void RenderCard()")) + 1;

        MethodBase method = AccessTools.Method(typeof(ShieldManager), nameof(ShieldManager.RenderHidden),
            new Type[] { typeof(PlayableCard) });

        codes.RemoveRange(start, end - start);
        codes.Insert(start, new(OpCodes.Call, method));
        
        return codes;
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.GetDistinctShownAbilities))]
    private static IEnumerable<CodeInstruction> RemoveSingleHiddenStacks(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        int start = -1, end = -1;

        MethodBase method = AccessTools.Method(typeof(ShieldManager), nameof(ShieldManager.HiddensOnlyRemoveStacks),
            new Type[] { typeof(List<Ability>), typeof(List<Ability>) });

        object hidden = null;
        for (int i = codes.Count - 1; i >= 0; i--)
        {
            if (end == -1 && codes[i].opcode == OpCodes.Pop)
            {
                end = i + 1;
                continue;
            }
            if (start == -1 && codes[i].opcode == OpCodes.Ldftn)
            {
                start = i;
                continue;
            }
            if (hidden == null && codes[i].opcode == OpCodes.Ldfld)
            {
                hidden = codes[i].operand;
                break;
            }
        }
        codes.RemoveRange(start, end - start);
        codes.Insert(start++, new(OpCodes.Ldfld, hidden));
        codes.Insert(start++, new(OpCodes.Callvirt, method));
        codes.Insert(start++, new(OpCodes.Stloc_1));

        return codes;
    }

    private static List<Ability> HiddensOnlyRemoveStacks(List<Ability> abilities, List<Ability> hiddenAbilities)
    {
        // remove all abilities that hide the entire stack
        abilities.RemoveAll(x => !x.GetHideSingleStacks() && hiddenAbilities.Contains(x));
        foreach (var ab in hiddenAbilities.Where(x => x.GetHideSingleStacks()))
            abilities.Remove(ab);

        return abilities;
    }
    private static void RenderHidden(PlayableCard card)
    {
        foreach (var com in card.GetComponents<DamageShieldBehaviour>())
        {
            if (com.Ability.GetHideSingleStacks())
                continue;

            if (com.HasShields() && card.Status.hiddenAbilities.Contains(com.Ability))
            {
                RenderCard = true;
                card.Status.hiddenAbilities.Remove(com.Ability);
                break;
            }
            if (!com.HasShields() && !card.Status.hiddenAbilities.Contains(com.Ability))
            {
                RenderCard = true;
                card.Status.hiddenAbilities.Add(com.Ability);
                break;
            }
        }

        foreach (var com in card.GetComponents<ActivatedDamageShieldBehaviour>())
        {
            if (com.Ability.GetHideSingleStacks())
                continue;

            if (com.HasShields() && card.Status.hiddenAbilities.Contains(com.Ability))
            {
                RenderCard = true;
                card.Status.hiddenAbilities.Remove(com.Ability);
                break;
            }
            if (!com.HasShields() && !card.Status.hiddenAbilities.Contains(com.Ability))
            {
                RenderCard = true;
                card.Status.hiddenAbilities.Add(com.Ability);
                break;
            }
        }

        if (RenderCard)
        {
            card.RenderCard();
            RenderCard = false;
        }
    }
}