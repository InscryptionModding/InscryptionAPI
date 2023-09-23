using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using Sirenix.Serialization.Utilities;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionAPI.Triggers;

// Patches to TakeDamage that add some more triggers and multi-shield support
[HarmonyPatch]
public static class TakeDamagePatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
    private static bool AddModifyDamageTrigger(PlayableCard __instance, ref int damage, PlayableCard attacker)
    {
        int originalDamage = damage;

        var modifyTakeDamage = CustomTriggerFinder.FindGlobalTriggers<IModifyDamageTaken>(true).ToList();
        modifyTakeDamage.Sort((a, b) => a.TriggerPriority(__instance, originalDamage, attacker) - b.TriggerPriority(__instance, originalDamage, attacker));
        foreach (var modify in modifyTakeDamage)
        {
            if (modify.RespondsToModifyDamageTaken(__instance, damage, attacker, originalDamage))
              damage = modify.OnModifyDamageTaken(__instance, damage, attacker, originalDamage);  
        }

        // no negative damage
        if (damage < 0)
            damage = 0;

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
    private static IEnumerator AddTakeDamageTriggers(IEnumerator enumerator, PlayableCard __instance, int damage, PlayableCard attacker)
    {
        var preTake = CustomTriggerFinder.FindTriggersOnCard<IPreTakeDamage>(__instance);
        foreach (var pre in preTake)
        {
            if (pre.RespondsToPreTakeDamage(attacker, damage))
                yield return pre.OnPreTakeDamage(attacker, damage);
        }

        yield return enumerator;
        if (__instance?.HasShield() == false && attacker != null)
        {
            yield return CustomTriggerFinder.TriggerInHand<IOnOtherCardDealtDamageInHand>(
            x => x.RespondsToOtherCardDealtDamageInHand(attacker, attacker.Attack, __instance),
                x => x.OnOtherCardDealtDamageInHand(attacker, attacker.Attack, __instance));
        }
    }

    private const string name_Damage = "System.Int32 damage";
    private const string name_TriggerHandler = "DiskCardGame.CardTriggerHandler get_TriggerHandler()";
    private const string name_CardAttacker = "DiskCardGame.PlayableCard attacker";

    [HarmonyTranspiler, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> TakeDamageTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        object hasShieldLabel = null;
        object damage = null;
        object damageLabel = null;
        object attacker = null;

        int shieldStart = -1, shieldEnd = -1;
        for (int i = 0; i < codes.Count; i++)
        {
            // grab the required operands, in order of appearance in the code
            if (shieldStart == -1 && codes[i].operand?.ToString() == "Boolean HasShield()")
            {
                shieldStart = i + 2;
                hasShieldLabel = codes[i + 1].operand;
                continue;
            }
            if (shieldEnd == -1 && codes[i].opcode == OpCodes.Br)
            {
                shieldEnd = i;
                continue;
            }
            if (damage == null && codes[i].operand?.ToString() == name_Damage)
            {
                damage = codes[i].operand;
                continue;
            }
            if (damageLabel == null && codes[i].operand?.ToString() == name_TriggerHandler && codes[i + 1].opcode == OpCodes.Ldc_I4_8)
            {
                // if (RespondsToTrigger(8) && damage > 0)
                //  ....

                i += 10;
                damageLabel = codes[i++].operand;
                codes.Insert(i++, new(OpCodes.Ldarg_0));
                codes.Insert(i++, new(OpCodes.Ldfld, damage));
                codes.Insert(i++, new(OpCodes.Ldc_I4_0));
                codes.Insert(i++, new(OpCodes.Cgt));
                codes.Insert(i++, new(OpCodes.Brfalse, damageLabel));
                continue;
            }
            if (codes[i].operand?.ToString() == name_CardAttacker)
            {
                attacker = codes[i].operand;
                if (shieldEnd != -1)
                {
                    // if (HasShield && damage > 0)
                    //   BreakShield();

                    MethodBase breakShield = AccessTools.Method(typeof(TakeDamagePatches), nameof(TakeDamagePatches.BreakShield),
                        new Type[] { typeof(PlayableCard), typeof(int), typeof(PlayableCard) });

                    codes.RemoveRange(shieldStart, shieldEnd - shieldStart);

                    // && damage > 0
                    codes.Insert(shieldStart++, new(OpCodes.Ldarg_0));
                    codes.Insert(shieldStart++, new(OpCodes.Ldfld, damage));
                    codes.Insert(shieldStart++, new(OpCodes.Ldc_I4_0));
                    codes.Insert(shieldStart++, new(OpCodes.Cgt));
                    codes.Insert(shieldStart++, new(OpCodes.Brfalse, hasShieldLabel));
                    // BreakShield();
                    //break;
                    codes.Insert(shieldStart++, new(OpCodes.Ldloc_1));
                    codes.Insert(shieldStart++, new(OpCodes.Ldarg_0));
                    codes.Insert(shieldStart++, new(OpCodes.Ldfld, damage));
                    codes.Insert(shieldStart++, new(OpCodes.Ldarg_0));
                    codes.Insert(shieldStart++, new(OpCodes.Ldfld, attacker));
                    codes.Insert(shieldStart++, new(OpCodes.Callvirt, breakShield));
                }
                break;
            }
        }
        //codes.LogCodeInscryptions();
        return codes;
    }

    public static void BreakShield(PlayableCard target, int damage, PlayableCard attacker)
    {
        // this void assumes that damage > 0

        var components = target.GetComponents<DamageShieldBehaviour>();
        foreach (var component in components)
        {
            // only reduce numShields for components with positive count
            if (component.HasShields())
            {
                component.numShields--;

                target.Anim.StrongNegationEffect();

                // if we've exhausted this shield component's count, negate it
                // so it updates the display
                if (component.NumShields == 0)
                {
                    target.AddTemporaryMod(new()
                    {
                        negateAbilities = new() { component.Ability },
                        nonCopyable = true
                    });
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
                break;
            }
        }
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
        {
            com.ResetShields();
            CardModificationInfo mod = __instance.TemporaryMods.Find(x => x.negateAbilities != null && x.negateAbilities.Contains(com.Ability));
            __instance.TemporaryMods.Remove(mod);
        }
    }
}