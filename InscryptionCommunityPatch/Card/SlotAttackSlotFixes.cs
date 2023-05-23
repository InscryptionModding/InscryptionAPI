using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Card;

// Inserts a null check for opposingSlot.Card after CardGettingAttacked is triggered
// Tweaks the direct damage logic to subtract from DamageDealtThisPhase if the opposing and attacking are on the same side
[HarmonyPatch]
internal class SlotAttackSlotPatches
{
    private const string name_GlobalTrigger = "DiskCardGame.GlobalTriggerHandler get_Instance()";
    private const string name_GetCard = "DiskCardGame.PlayableCard get_Card()";
    private const string name_OpposingSlot = "DiskCardGame.CardSlot opposingSlot";
    private const string name_CombatPhase = "DiskCardGame.CombatPhaseManager+<>c__DisplayClass5_0 <>8__1";
    private const string name_AttackingSlot = "DiskCardGame.CardSlot attackingSlot";
    private const string name_CanAttackDirectly = "Boolean CanAttackDirectly(DiskCardGame.CardSlot)";
    private const string name_SetDamageDealt = "Void set_DamageDealtThisPhase(Int32)";

    // Get the method MoveNext, which is where the actual code for SlotAttackSlot is located
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }

    // We want to add a null check after CardGettingAttacked is triggered, so we'll look for triggers
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        // we want to slowly narrow our search until we find exactly where we want to insert our code
        for (int i = 0; i < codes.Count; i++)
        {
            // separated into their own methods so I can save on eye strain and brain fog
            DirectDamageSelfCheck(codes, i);
            OpposingCardNullCheck(codes, i);
        }

        return codes;
    }

    public static void OpposingCardNullCheck(List<CodeInstruction> codes, int i)
    {
        // Looking for where GlobalTriggerHandler is called for CardGettingAttacked (enum 7)
        if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == name_GlobalTrigger && codes[i + 1].opcode == OpCodes.Ldc_I4_7)
        {
            MethodInfo op_Inequality = null;
            object op_OpposingSlot = null;
            object op_GetCard = null;
            object op_BreakLabel = null;

            for (int j = i + 1; j < codes.Count; j++)
            {
                // we need to get the operand for opposingSlot so we can insert it later
                if (codes[j].opcode == OpCodes.Ldfld && codes[j].operand.ToString() == name_OpposingSlot && op_OpposingSlot == null)
                    op_OpposingSlot = codes[j].operand;

                // also get the operand for get_card
                if (codes[j].opcode == OpCodes.Callvirt && codes[j].operand.ToString() == name_GetCard && op_GetCard == null)
                    op_GetCard = codes[j].operand;

                // if true, we've found the start of the if statement we'll be nesting in
                if (codes[j].opcode == OpCodes.Stfld && codes[j - 1].opcode == OpCodes.Ldc_I4_M1)
                {
                    for (int k = j + 1; k < codes.Count; k++)
                    {
                        // we want to grab the operand for !=
                        if (codes[k].opcode == OpCodes.Call && op_Inequality == null)
                            op_Inequality = (MethodInfo)codes[k].operand;

                        // we also want to grab the break label so we know where to jump
                        if (op_BreakLabel == null && codes[k].opcode == OpCodes.Brfalse)
                            op_BreakLabel = codes[k].operand;

                        // if true, we've found where we want to insert our junk
                        if (codes[k].opcode == OpCodes.Stfld)
                        {
                            // if (this.opposingSlot.Card != null)
                            codes.Insert(k + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Insert(k + 2, new CodeInstruction(OpCodes.Ldfld, op_OpposingSlot));
                            codes.Insert(k + 3, new CodeInstruction(OpCodes.Callvirt, op_GetCard));
                            codes.Insert(k + 4, new CodeInstruction(OpCodes.Ldnull));
                            codes.Insert(k + 5, new CodeInstruction(OpCodes.Call, op_Inequality));
                            codes.Insert(k + 6, new CodeInstruction(OpCodes.Brfalse, op_BreakLabel));
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }

    public static void DirectDamageSelfCheck(List<CodeInstruction> codes, int i)
    {
        if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == name_CanAttackDirectly)
        {
            int startIndex = i + 2, endIndex = -1;

            object op_AttackingSlot = null;
            object op_OpposingSlot = null;
            object op_CombatPhase = null;

            // look backwards and retrieve attackingSlot and opposingSlot
            for (int j = i - 1; j > 0; j--)
            {
                if (codes[j].opcode == OpCodes.Ldfld && codes[j].operand.ToString() == name_OpposingSlot && op_OpposingSlot == null)
                    op_OpposingSlot = codes[j].operand;

                if (codes[j].opcode == OpCodes.Ldfld && codes[j].operand.ToString() == name_AttackingSlot && op_AttackingSlot == null)
                    op_AttackingSlot = codes[j].operand;

                if (codes[j].opcode == OpCodes.Ldfld && codes[j].operand.ToString() == name_CombatPhase && op_CombatPhase == null)
                    op_CombatPhase = codes[j].operand;

                if (op_CombatPhase != null && op_AttackingSlot != null && op_OpposingSlot != null)
                    break;
            }

            // get the endIndex
            for (int k = startIndex; k < codes.Count; k++)
            {
                if (codes[k].opcode == OpCodes.Callvirt && codes[k].operand.ToString() == name_SetDamageDealt)
                {
                    endIndex = k + 1;
                    break;
                }
            }

            if (endIndex > -1)
            {
                MethodInfo customMethod = AccessTools.Method(typeof(SlotAttackSlotPatches), nameof(SlotAttackSlotPatches.NewDealDirectDamage),
                    new Type[] { typeof(CombatPhaseManager), typeof(CardSlot), typeof(CardSlot) });

                // remove the previous code then insert our own
                codes.RemoveRange(startIndex, endIndex - startIndex);
                // combatPhaseManager
                codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldloc_1));
                // this.<>.attackingSlot
                codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Ldfld, op_CombatPhase));
                codes.Insert(startIndex + 3, new CodeInstruction(OpCodes.Ldfld, op_AttackingSlot));
                // this.opposingSlot
                codes.Insert(startIndex + 4, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(startIndex + 5, new CodeInstruction(OpCodes.Ldfld, op_OpposingSlot));
                codes.Insert(startIndex + 6, new CodeInstruction(OpCodes.Call, customMethod));
            }
        }
    }

    public static void NewDealDirectDamage(CombatPhaseManager __instance, CardSlot attackingSlot, CardSlot opposingSlot)
    {
        if (attackingSlot.IsPlayerSlot == opposingSlot.IsPlayerSlot)
        {
            __instance.DamageDealtThisPhase -= attackingSlot.Card.Attack;
        }
        else
        {
            __instance.DamageDealtThisPhase += attackingSlot.Card.Attack;
        }
    }
}