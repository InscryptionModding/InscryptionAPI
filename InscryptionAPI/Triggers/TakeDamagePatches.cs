using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionAPI.Triggers;

// Patches to DoCombatPhase that add negative damage support and modifying attack slots support
[HarmonyPatch]
public static class TakeDamagePatches
{
    private const string name_State = "System.Int32 <>1__state";
    private const string name_Damage = "System.Int32 damage";
    private const string name_CardAttacker = "DiskCardGame.PlayableCard attacker";
    private const string name_CombatCurrent = "System.Object <>2__current";

    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
    private static bool ModifyDamageTrigger(PlayableCard __instance, ref int damage, PlayableCard attacker)
    {
        int originalDamage = damage;

        var modifyTakeDamage = CustomTriggerFinder.FindGlobalTriggers<IModifyDamageTaken>(true).ToList();
        modifyTakeDamage.Sort((a, b) => a.TriggerPriority(__instance, originalDamage, attacker) - b.TriggerPriority(__instance, originalDamage, attacker));
        foreach (var modify in modifyTakeDamage)
        {
            if (modify.RespondsToModifyDamageTaken(__instance, damage, attacker, originalDamage))
              damage = modify.OnModifyDamageTaken(__instance, damage, attacker, originalDamage);  
        }

        return true;
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> TakeDamageTriggers(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        object current = null;
        object state = null;
        // Ldloc.1
        object attacker = null;
        object damage = null;

        int startIndex = -1, endIndex = -1;
        for (int i = 0; i < codes.Count; i++)
        {
            // grab the required operands, in order of appearance in the code
            if (state == null && codes[i].operand?.ToString() == name_State)
            {
                state = codes[i].operand;
                continue;
            }
            if (startIndex == -1 && codes[i].operand?.ToString() == "Boolean HasShield()")
            {
                startIndex = i + 2;
                continue;
            }
            if (endIndex == -1 && codes[i].opcode == OpCodes.Br)
            {
                endIndex = i;
                continue;
            }
            if (damage == null && codes[i].operand?.ToString() == name_Damage)
            {
                damage = codes[i].operand;
                continue;
            }
            if (attacker == null && codes[i].operand?.ToString() == name_CardAttacker)
            {
                attacker = codes[i].operand;
                continue;
            }
            if (current == null && codes[i].operand?.ToString() == name_CombatCurrent)
            {
                current = codes[i].operand;
                if (startIndex != -1)
                {
                    MethodBase customMethod = AccessTools.Method(typeof(TakeDamagePatches), nameof(TakeDamagePatches.BreakShield),
                        new Type[] { typeof(PlayableCard) });
                    // if (HasShield)
                    //   BreakShield();
                    codes.RemoveRange(startIndex, endIndex - startIndex);
                    codes.Insert(startIndex, new(OpCodes.Ldloc_1));
                    codes.Insert(startIndex + 1, new(OpCodes.Callvirt, customMethod));
                }
                break;
            }
        }

        for (int j = codes.Count - 1; j >= 0; j--)
        {
            if (codes[j].opcode == OpCodes.Ldc_I4_0)
            {
                // this.current = TriggerOtherDamageInHand
                // this.state = 5
                // return true
                // this.state = -1

                MethodBase customMethod = AccessTools.Method(typeof(TakeDamagePatches), nameof(TakeDamagePatches.TriggerOtherDamageInHand),
                    new Type[] { typeof(PlayableCard), typeof(PlayableCard) });

                codes.Insert(j, new(OpCodes.Ldarg_0));
                codes.Insert(j + 1, new(OpCodes.Ldarg_0));
                codes.Insert(j + 2, new(OpCodes.Ldloc_1));
                codes.Insert(j + 3, new(OpCodes.Ldfld, attacker));
                codes.Insert(j + 4, new(OpCodes.Callvirt, customMethod));
                codes.Insert(j + 5, new(OpCodes.Stfld, current));

                // this.state = 5
                codes.Insert(j + 6, new(OpCodes.Ldarg_0));
                codes.Insert(j + 7, new(OpCodes.Ldc_I4_5));
                codes.Insert(j + 8, new(OpCodes.Stfld, state));
                // return true
                codes.Insert(j + 9, new(OpCodes.Ldc_I4_1));
                codes.Insert(j + 10, new(OpCodes.Ret));
                // this.state = -1
                codes.Insert(j + 11, new(OpCodes.Ldarg_0));
                codes.Insert(j + 12, new(OpCodes.Ldc_I4_M1));
                codes.Insert(j + 13, new(OpCodes.Stfld, state));
                break;
            }
        }

        codes.LogCodeInscryptions();
        return codes;
    }
    private static IEnumerator TriggerOtherDamageInHand(PlayableCard target, PlayableCard attacker)
    {
        yield return CustomTriggerFinder.TriggerInHand<IOnOtherCardDealtDamageInHand>(
            x => x.RespondsToOtherCardDealtDamageInHand(attacker, attacker.Attack, target),
            x => x.OnOtherCardDealtDamageInHand(attacker, attacker.Attack, target));
    }
    public static void BreakShield(PlayableCard target)
    {
        // is DeathShield even stackable? well whatever
        int shieldStacks = target.GetAbilityStacks(Ability.DeathShield) - 1;

        target.Anim.StrongNegationEffect();
        if (shieldStacks > 0)
        {
            // negate 1 stack of death shield
            target.Info.Mods.Add(new()
            {
                negateAbilities = new() { Ability.DeathShield },
                nonCopyable = true
            });
        }
        else
        {
            target.Status.lostShield = true;

            if (target.Info.name == "MudTurtle")
                target.SwitchToAlternatePortrait();
            else if (target.Info.HasBrokenShieldPortrait())
                target.SwitchToPortrait(target.Info.BrokenShieldPortrait());
        }
        target.UpdateFaceUpOnBoardEffects();
    }
}