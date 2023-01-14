using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Inserts a null check for opposingSlot.Card after CardGettingAttacked is triggered
// Tweaks the direct damage logic to subtract from DamageDealtThisPhase if the opposing and attacking are on the same side
[HarmonyPatch]
public class SelfAttackDamagePatch
{
    private const string name_GetDamageDealt = "Int32 get_DamageDealtThisPhase()";
    private const string name_PlayerAttacker = "System.Boolean playerIsAttacker";
    private const string name_SpecialSequencer = "DiskCardGame.SpecialBattleSequencer specialSequencer";
    private const string name_SquirrelAttacker = "System.Boolean <attackedWithSquirrel>5__4";
    private const string name_CombatCurrent = "System.Object <>2__current";

    // Get the method MoveNext, which is where the actual code for SlotAttackSlot is located
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(CombatPhaseManager), nameof(CombatPhaseManager.DoCombatPhase));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }

    // We want to add support for negative values of DamageDealtThisPhase so modders can add self damage behaviours more easily
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        // we want to slowly narrow our search until we find exactly where we want to insert our code
        for (int i = 0; i < codes.Count; i++)
        {
            if (SelfDamageSupport(codes, i))
                break;
        }

        return codes;
    }

    private static bool SelfDamageSupport(List<CodeInstruction> codes, int i)
    {
        if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == name_GetDamageDealt && codes[i + 1].opcode == OpCodes.Ldc_I4_0)
        {
            int startIndex = i - 1, endIndex = -1;

            object op_PlayerAttacker = null;
            object op_SpecialSequencer = null;
            object op_SquirrelAttacker = null;

            // get everything we need
            for (int j = i - 1; j < codes.Count; j++)
            {
                if (codes[j].opcode == OpCodes.Ldfld && codes[j].operand.ToString() == name_PlayerAttacker && op_PlayerAttacker == null)
                {
                    op_PlayerAttacker = codes[j].operand;
                    continue;
                }

                if (codes[j].opcode == OpCodes.Ldfld && codes[j].operand.ToString() == name_SpecialSequencer && op_SpecialSequencer == null)
                {
                    op_SpecialSequencer = codes[j].operand;
                    continue;
                }

                if (codes[j].opcode == OpCodes.Ldfld && codes[j].operand.ToString() == name_SquirrelAttacker && op_SquirrelAttacker == null)
                {
                    op_SquirrelAttacker = codes[j].operand;
                    continue;
                }

                if (codes[j].opcode == OpCodes.Ldc_R4 && codes[j].operand.ToString() == "0.15")
                {
                    for (int k = j + 1; k < codes.Count; k--)
                    {
                        // preserve the state changes or the ienum won't work
                        if (codes[k].opcode == OpCodes.Stfld && codes[k].operand.ToString() == name_CombatCurrent)
                        {
                            endIndex = k;
                            break;
                        }
                    }
                    break;
                }
            }

            if (endIndex != -1)
            {
                MethodBase customMethod = AccessTools.Method(typeof(SelfAttackDamagePatch), nameof(SelfAttackDamagePatch.NewAddDamageToScales),
                    new Type[] { typeof(CombatPhaseManager), typeof(bool), typeof(SpecialBattleSequencer), typeof(bool) });

                // move labels and then mark code as Nop
                for (int j = startIndex + 1; j < endIndex; j++)
                {
                    codes[j].MoveLabelsTo(codes[startIndex]);
                    codes[j].opcode = OpCodes.Nop;
                }
                // remove codes marked with Nop
                codes.RemoveAll((x) => x.opcode == OpCodes.Nop);

                // change startIndex.opcode to Ldarg_0 for the stfld that will immediately follow the callvirt, then add a new ldloc_2
                codes[startIndex].opcode = OpCodes.Ldarg_0;
                codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Ldloc_2));
                // this.playerIsAttacker
                codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(startIndex + 3, new CodeInstruction(OpCodes.Ldfld, op_PlayerAttacker));
                // this.specialSequencer
                codes.Insert(startIndex + 4, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(startIndex + 5, new CodeInstruction(OpCodes.Ldfld, op_SpecialSequencer));
                // this.attackedWithSquirrel
                codes.Insert(startIndex + 6, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(startIndex + 7, new CodeInstruction(OpCodes.Ldfld, op_SquirrelAttacker));

                codes.Insert(startIndex + 8, new CodeInstruction(OpCodes.Callvirt, customMethod));
            }
            return true;
        }
        return false;
    }

    public static IEnumerator NewAddDamageToScales(CombatPhaseManager __instance, bool playerIsAttacker, SpecialBattleSequencer specialSequencer, bool attackedWithSquirrel)
    {
        // we want to implement logic for if damage is less than 0 (damage dealt to self)
        if (__instance.DamageDealtThisPhase != 0)
        {
            bool targetOfDamage = playerIsAttacker;
            if (__instance.DamageDealtThisPhase < 0)
            {
                PatchPlugin.Logger.LogDebug("DamageDealtThisPhase is negative, dealing damage to self.");
                targetOfDamage = !playerIsAttacker;
            }

            yield return new WaitForSeconds(0.4f);
            yield return __instance.VisualizeDamageMovingToScales(targetOfDamage);
            int excessDamage2 = 0;
            // calculate excessDamage2 only if we've actually dealt damage to the opponent
            if (playerIsAttacker && __instance.DamageDealtThisPhase > 0)
            {
                excessDamage2 = Singleton<LifeManager>.Instance.Balance + __instance.DamageDealtThisPhase - 5;
                if (attackedWithSquirrel && excessDamage2 >= 0)
                {
                    AchievementManager.Unlock(Achievement.PART1_SPECIAL1);
                }
                excessDamage2 = Mathf.Max(0, excessDamage2);
            }
            // for the purpose of calculations, damage cannot be negative
            int damage = Math.Abs(__instance.DamageDealtThisPhase) - excessDamage2;
            AscensionStatsData.TryIncreaseStat(AscensionStat.Type.MostDamageDealt, __instance.DamageDealtThisPhase);
            if (__instance.DamageDealtThisPhase >= 666)
            {
                AchievementManager.Unlock(Achievement.PART2_SPECIAL2);
            }
            if (!(specialSequencer != null) || !specialSequencer.PreventDamageAddedToScales)
            {
                yield return Singleton<LifeManager>.Instance.ShowDamageSequence(damage, damage, !targetOfDamage, 0f);
            }
            if (specialSequencer != null)
            {
                yield return specialSequencer.DamageAddedToScale(damage + excessDamage2, targetOfDamage);
            }
            if ((!(specialSequencer != null) || !specialSequencer.PreventDamageAddedToScales) && excessDamage2 > 0 && Singleton<TurnManager>.Instance.Opponent.NumLives == 1 && Singleton<TurnManager>.Instance.Opponent.GiveCurrencyOnDefeat)
            {
                yield return Singleton<TurnManager>.Instance.Opponent.TryRevokeSurrender();
                RunState.Run.currency += excessDamage2;
                yield return __instance.VisualizeExcessLethalDamage(excessDamage2, specialSequencer);
            }
        }
    }
}