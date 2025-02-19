using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System.Collections;
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
    private const string name_GetAttack = "Int32 get_Attack()";
    private const string name_OpposingSlot = "DiskCardGame.CardSlot opposingSlot";
    private const string name_CombatPhase = "DiskCardGame.CombatPhaseManager+<>c__DisplayClass5_0 <>8__1";
    private const string name_AttackingSlot = "DiskCardGame.CardSlot attackingSlot";
    private const string name_CanAttackDirectly = "Boolean CanAttackDirectly(DiskCardGame.CardSlot)";
    private const string name_VisualizeCardAttackingDirectly = "System.Collections.IEnumerator VisualizeCardAttackingDirectly(DiskCardGame.CardSlot, DiskCardGame.CardSlot, Int32)";
    private const string modifiedAttackCustomField = "modifiedAttack";

    private static readonly MethodInfo method_GetCard = AccessTools.PropertyGetter(typeof(CardSlot), nameof(CardSlot.Card));

    public static int DamageToDealThisPhase(CardSlot attackingSlot, CardSlot opposingSlot)
    {
        int originalDamage = attackingSlot.Card.Attack;
        int damage = originalDamage;

        // Trigger IModifyDirectDamage first and treat the new damage as the attacking card's attack
        List<IModifyDirectDamage> modifyDirectDamage = CustomTriggerFinder.FindGlobalTriggers<IModifyDirectDamage>(true).ToList();
        modifyDirectDamage.Sort((a, b) =>
            b.TriggerPriority(opposingSlot, damage, attackingSlot.Card)
            - a.TriggerPriority(opposingSlot, damage, attackingSlot.Card)
        );

        foreach (IModifyDirectDamage modify in modifyDirectDamage)
        {
            if (modify.RespondsToModifyDirectDamage(opposingSlot, damage, attackingSlot.Card, originalDamage))
                damage = modify.OnModifyDirectDamage(opposingSlot, damage, attackingSlot.Card, originalDamage);
        }

        // first thing we check for is self-damage; if the attacking slot is on the same side as the opposing slot, deal self-damage
        if (attackingSlot.IsPlayerSlot == opposingSlot.IsPlayerSlot)
            return -damage;

        // this is some new stuff to account for out-of-turn damage
        else if (TurnManager.Instance.IsPlayerTurn)
        {
            // if an opponent is attacking during the player's turn, deal positive/negative damage depending on what slot is being attacked
            if (attackingSlot.IsOpponentSlot())
                return opposingSlot.IsPlayerSlot ? -damage : damage;
        }
        else if (attackingSlot.IsPlayerSlot)
        {
            // if a player is attacking during the opponent's turn, deal positive/negative damage depending on what slot is being attacked
            return opposingSlot.IsOpponentSlot() ? -damage : damage;
        }

        return damage;
    }

    // Trigger both the vanilla trigger and the new trigger
    public static IEnumerator TriggerOnDirectDamageTriggers(PlayableCard attacker, CardSlot opposingSlot)
    {
        int damage = CustomFields.Get<int>(attacker, modifiedAttackCustomField);

        // trigger the vanilla trigger
        if (attacker.TriggerHandler.RespondsToTrigger(Trigger.DealDamageDirectly, new object[] { damage }))
        {
            yield return attacker.TriggerHandler.OnTrigger(Trigger.DealDamageDirectly, new object[] { damage });
        }

        // trigger the new modded trigger
        yield return CustomTriggerFinder.TriggerAll<IOnCardDealtDamageDirectly>(false, x => x.RespondsToCardDealtDamageDirectly(attacker, opposingSlot, damage), x => x.OnCardDealtDamageDirectly(attacker, opposingSlot, damage));
    }

    public static IEnumerator TriggerCardGettingAttackedTriggers(PlayableCard attacker, PlayableCard opposingCard)
    {
        yield return Singleton<GlobalTriggerHandler>.Instance.TriggerCardsOnBoard(Trigger.CardGettingAttacked, false, opposingCard);

        List<IPostCardGettingAttacked> postAttacked = CustomTriggerFinder.FindGlobalTriggers<IPostCardGettingAttacked>(true).ToList();
        postAttacked.Sort((a, b) => b.PostCardGettingAttackedPriority(opposingCard, attacker) - a.PostCardGettingAttackedPriority(opposingCard, attacker));
        foreach (IPostCardGettingAttacked modify in postAttacked)
        {
            if (modify != null && modify.RespondsToPostCardGettingAttacked(opposingCard, attacker))
                yield return modify.OnPostCardGettingAttacked(opposingCard, attacker);
        }
    }

    // We want to add a null check after CardGettingAttacked is triggered, so we'll look for triggers
    [HarmonyTranspiler, HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSlot), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        object displayClassOperand = codes.First(x => x.opcode == OpCodes.Ldfld && x.operand.ToString() == name_CombatPhase).operand;
        object attackingSlotOperand = codes.First(x => x.opcode == OpCodes.Stfld && x.operand.ToString() == name_AttackingSlot).operand;
        object opposingSlotOperand = codes.First(x => x.opcode == OpCodes.Ldfld && x.operand.ToString() == name_OpposingSlot).operand;
        
        // we want to slowly narrow our search until we find exactly where we want to insert our code
        /*for (int a = 0; a < codes.Count; a++)
        {
            // separated into their own methods so I can save on eye strain and brain fog
            if (ModifyDirectDamageCheck(codes, combatPhaseOperand, attackingSlotOperand, opposingSlotOperand, ref a))
            {
                for (int b = a; b < codes.Count; b++)
                {
                    if (CallTriggerOnDirectDamage(codes, opposingSlotOperand, ref b))
                    {
                        for (int c = b; c < codes.Count; c++)
                        {
                            if (OpposingCardNullCheck(codes, opposingSlotOperand, combatPhaseOperand, attackingSlotOperand, c))
                                break;
                        }
                        break;
                    }
                }
                break;
            }
        }*/

        int a = ModifyDirectDamageCheck(codes, displayClassOperand, attackingSlotOperand, opposingSlotOperand);
        a = CallTriggerOnDirectDamage(codes, a, displayClassOperand, attackingSlotOperand, opposingSlotOperand);
        OpposingCardNullCheck(codes, a, opposingSlotOperand, displayClassOperand, attackingSlotOperand);

        return codes;
    }

    private static void OpposingCardNullCheck(List<CodeInstruction> codes, int start, object opposingSlotOp, object displayClassOp, object attackingSlotOp)
    {
        // Looking for where GlobalTriggerHandler is called for CardGettingAttacked (enum 7)
        int index = 8 + codes.FindIndex(start, x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == "Void SetAnimationPaused(Boolean)");
        object op_BreakLabel = null;
        MethodInfo op_Inequality = null;
        MethodInfo cardGettingAttacked = AccessTools.Method(typeof(SlotAttackSlotPatches), nameof(SlotAttackSlotPatches.TriggerCardGettingAttackedTriggers),
            new Type[] { typeof(PlayableCard), typeof(PlayableCard) });

        codes.RemoveRange(index, codes.FindIndex(index, x => x.opcode == OpCodes.Stfld) - index);

        // yield return TriggerCardGetting...(PlayableCard card, CardSlot slot)
        // this.attackingSlot.Card
        // this.opposingSlot
        codes.Insert(index++, new(OpCodes.Ldarg_0));
        codes.Insert(index++, new(OpCodes.Ldfld, displayClassOp));
        codes.Insert(index++, new(OpCodes.Ldfld, attackingSlotOp));
        codes.Insert(index++, new(OpCodes.Callvirt, method_GetCard));
        codes.Insert(index++, new(OpCodes.Ldarg_0));
        codes.Insert(index++, new(OpCodes.Ldfld, opposingSlotOp));
        codes.Insert(index++, new(OpCodes.Callvirt, method_GetCard));
        codes.Insert(index++, new(OpCodes.Callvirt, cardGettingAttacked));

        op_Inequality = (MethodInfo)codes[codes.FindIndex(index, x => x.opcode == OpCodes.Call && x.operand.ToString() == "Boolean op_Inequality(UnityEngine.Object, UnityEngine.Object)")].operand;
        op_BreakLabel = codes[codes.FindIndex(index, x => x.opcode == OpCodes.Brfalse)].operand;

        index = 1 + codes.FindIndex(2 + codes.FindIndex(index, x => x.opcode == OpCodes.Ldc_I4_M1), x => x.opcode == OpCodes.Stfld);

        codes.Insert(index++, new CodeInstruction(OpCodes.Ldarg_0));
        codes.Insert(index++, new CodeInstruction(OpCodes.Ldfld, opposingSlotOp));
        codes.Insert(index++, new CodeInstruction(OpCodes.Callvirt, method_GetCard));
        codes.Insert(index++, new CodeInstruction(OpCodes.Ldnull));
        codes.Insert(index++, new CodeInstruction(OpCodes.Call, op_Inequality));
        codes.Insert(index++, new CodeInstruction(OpCodes.Brfalse, op_BreakLabel));
    }

    /// <summary>
    /// Replaces the DealDamageDirectly trigger call with a call to TriggerOnDirectDamageTriggers
    /// </summary>
    private static int CallTriggerOnDirectDamage(List<CodeInstruction> codes, int startIndex, object displayClassOperand, object attackingSlotOperand, object opposingSlotOp)
    {
        int index = codes.FindIndex(startIndex, x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == name_GetCard) - 2;
        codes.Insert(index++, new(OpCodes.Ldarg_0));
        index += 3;
        
        MethodInfo info = AccessTools.Method(typeof(SlotAttackSlotPatches), nameof(SlotAttackSlotPatches.TriggerOnDirectDamageTriggers),
            new Type[] { typeof(PlayableCard), typeof(CardSlot) });

        // remove the existing trigger call
        codes.RemoveRange(index, codes.FindIndex(index, x => x.opcode == OpCodes.Ldc_I4_7) - 2 - index);
        //index = startIndex + 1;
        //return index;
        // insert the call to our new trigger
        // this.displayClass.attackingSlot.Card
        // this.opposingSlot
        codes.Insert(index++, new(OpCodes.Ldarg_0));
        codes.Insert(index++, new(OpCodes.Ldfld, opposingSlotOp));
        codes.Insert(index++, new(OpCodes.Callvirt, info));

        return index;
    }

    /// <summary>
    /// Modifies the damage value added to DamageDealtThisPhase to support IModifyDirectDamage and negative damage values (self-damage)
    /// </summary>
    private static int ModifyDirectDamageCheck(List<CodeInstruction> codes, object displayClassOperand, object attackingSlotOperand, object opposingSlotOperand)
    {
        int index = codes.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == name_CanAttackDirectly) + 2;

        MethodInfo setCustomField = AccessTools.Method(typeof(CustomFields), nameof(CustomFields.Set));
        MethodInfo damageToDeal = AccessTools.Method(typeof(SlotAttackSlotPatches), nameof(SlotAttackSlotPatches.DamageToDealThisPhase),
            new Type[] { typeof(CardSlot), typeof(CardSlot) });

        MethodInfo getCustomField = AccessTools.Method(typeof(CustomFields), nameof(CustomFields.Get),
            new Type[] { typeof(object), typeof(string) }, new Type[] { typeof(int) });

        // CustomFields.Set(this.CombatPhase.AttackingSlot.Card, "modifiedAttack", DamageToDealThisPhase(this.CombatPhase.AttackingSlot, this.OpposingSlot));
        // change DamageDealtThisPhase += attackingCard.Attack
        // to DamageDealtThisPhase += (int)modifiedAttack
        codes.Insert(index++, new(OpCodes.Ldarg_0));
        codes.Insert(index++, new(OpCodes.Ldfld, displayClassOperand));
        codes.Insert(index++, new(OpCodes.Ldfld, attackingSlotOperand));
        codes.Insert(index++, new(OpCodes.Callvirt, method_GetCard));
        codes.Insert(index++, new(OpCodes.Ldstr, modifiedAttackCustomField));
        codes.Insert(index++, new(OpCodes.Ldarg_0));
        codes.Insert(index++, new(OpCodes.Ldfld, displayClassOperand));
        codes.Insert(index++, new(OpCodes.Ldfld, attackingSlotOperand));
        codes.Insert(index++, new(OpCodes.Ldarg_0));
        codes.Insert(index++, new(OpCodes.Ldfld, opposingSlotOperand));
        codes.Insert(index++, new(OpCodes.Callvirt, damageToDeal));
        codes.Insert(index++, new(OpCodes.Box, typeof(int)));
        codes.Insert(index++, new(OpCodes.Call, setCustomField));

        // replace the next 2 occurences of get_Attack() with the custom field call
        index = codes.FindIndex(index, x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == name_GetAttack);
        codes[index++] = new(OpCodes.Ldstr, modifiedAttackCustomField);
        codes.Insert(index++, new(OpCodes.Callvirt, getCustomField));

        index = codes.FindIndex(index, x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == name_GetAttack);
        codes[index++] = new(OpCodes.Ldstr, modifiedAttackCustomField);
        codes.Insert(index++, new(OpCodes.Callvirt, getCustomField));

        return index;
    }
}