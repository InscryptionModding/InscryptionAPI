using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionAPI.Triggers;

// Inserts a null check for opposingSlot.Card after CardGettingAttacked is triggered
// Tweaks the direct damage logic to subtract from DamageDealtThisPhase if the opposing and attacking are on the same side
[HarmonyPatch]
public static class SlotAttackSlotPatches
{
    private const string name_GlobalTrigger = "DiskCardGame.GlobalTriggerHandler get_Instance()";
    private const string name_GetCard = "DiskCardGame.PlayableCard get_Card()";
    private const string name_OpposingSlot = "DiskCardGame.CardSlot opposingSlot";
    private const string name_CombatPhase = "DiskCardGame.CombatPhaseManager+<>c__DisplayClass5_0 <>8__1";
    private const string name_AttackingSlot = "DiskCardGame.CardSlot attackingSlot";
    private const string name_CanAttackDirectly = "Boolean CanAttackDirectly(DiskCardGame.CardSlot)";

    // make this public so people can alter it themselves
    public static int DamageToDealThisPhase(CardSlot attackingSlot, CardSlot opposingSlot)
    {
        // first thing we check for is self-damage; if the attacking slot is on the same side as the opposing slot, deal self-damage
        if (attackingSlot.IsPlayerSlot == opposingSlot.IsPlayerSlot)
            return -attackingSlot.Card.Attack;

        // this is some new stuff to account for out-of-turn damage
        else if (TurnManager.Instance.IsPlayerTurn)
        {
            // if an opponent is attacking during the player's turn, deal positive/negative damage depending on what slot is being attacked
            if (attackingSlot.IsOpponentSlot())
                return opposingSlot.IsPlayerSlot ? -attackingSlot.Card.Attack : attackingSlot.Card.Attack;
        }
        else if (attackingSlot.IsPlayerSlot)
        {
            // if a player is attacking during the opponent's turn, deal positive/negative damage depending on what slot is being attacked
            return opposingSlot.IsOpponentSlot() ? -attackingSlot.Card.Attack : attackingSlot.Card.Attack;
        }

        return attackingSlot.Card.Attack;
    }

    // We want to add a null check after CardGettingAttacked is triggered, so we'll look for triggers
    [HarmonyTranspiler, HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        // we want to slowly narrow our search until we find exactly where we want to insert our code
        for (int a = 0; a < codes.Count; a++)
        {
            // separated into their own methods so I can save on eye strain and brain fog
            if (DirectSelfDamageCheck(codes, a))
            {
                for (int b = a + 1; b < codes.Count; b++)
                {
                    if (OpposingCardNullCheck(codes, b))
                        break;
                }
                break;
            }
        }

        return codes;
    }

    private static bool OpposingCardNullCheck(List<CodeInstruction> codes, int i)
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
                            return true;
                        }
                    }
                    break;
                }
            }
        }
        return false;
    }

    private static bool DirectSelfDamageCheck(List<CodeInstruction> codes, int index)
    {
        if (codes[index].opcode == OpCodes.Callvirt && codes[index].operand.ToString() == name_CanAttackDirectly)
        {
            int startIndex = index + 2;

            // we want to turn this:
            // ldloc.1
            // ldloc.1
            // call getDamage
            // ldarg.0
            // ldfld displayClass
            // ldfld attackingSlot
            // callvirt getCard
            // callvirt getAttack
            // call setDamage

            // into this:
            // ldloc.1
            // ldloc.1
            // call getDamage
            // ldarg.0
            // ldfld displayClass
            // ldfld attackingSlot
            // ~ldarg.0
            // ~ldfld opposingSlot
            // +callvirt newDamage
            // call setDamage

            object op_OpposingSlot = null;

            // look backwards and retrieve opposingSlot
            for (int i = index - 1; i > 0; i--)
            {
                if (op_OpposingSlot == null && codes[i].operand?.ToString() == name_OpposingSlot)
                {
                    op_OpposingSlot = codes[i].operand;
                    break;
                }
            }

            // get the endIndex
            for (int j = startIndex; j < codes.Count; j++)
            {
                if (codes[j].opcode == OpCodes.Callvirt && codes[j].operand.ToString() == name_GetCard)
                {
                    MethodInfo customMethod = AccessTools.Method(typeof(SlotAttackSlotPatches), nameof(SlotAttackSlotPatches.DamageToDealThisPhase),
                        new Type[] { typeof(CardSlot), typeof(CardSlot) });

                    codes[j++] = new(OpCodes.Ldarg_0);
                    codes[j++] = new(OpCodes.Ldfld, op_OpposingSlot);
                    codes.Insert(j++, new(OpCodes.Callvirt, customMethod));
                    return true;
                }
            }
        }
        return false;
    }
}