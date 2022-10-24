using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Card;

// Inserts a null check for opposingSlot.Card into SlotAttackSlot after CardGettingAttacked has been triggered
// Should prevent compatibility issues with anyone trying to patch the method
[HarmonyPatch]
public class SentrySlotAttackSlotFix
{
    private const string getTriggerInstance = "DiskCardGame.GlobalTriggerHandler get_Instance()";
    private const string getCard = "DiskCardGame.PlayableCard get_Card()";
    private const string opposingSlot = "DiskCardGame.CardSlot opposingSlot";

    // Get the method MoveNext, which is where the actual code is located
    private static IEnumerable<MethodBase> TargetMethods()
    {
        Assembly assembly = typeof(CombatPhaseManager).Assembly;
        Type rtype = Type.GetType("DiskCardGame.CombatPhaseManager+<SlotAttackSlot>d__5, " + assembly.FullName);
        yield return AccessTools.Method(rtype, "MoveNext");
    }

    // We want to add a null check after CardGettingAttacked is triggered, so we'll look for triggers
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // required operands for inserting our null check
        object opposingSlotOperand = null;
        object getCardOperand = null;
        MethodInfo inequalityOperand = null;
        object ifLabelOperand = null;

        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        // iterate through the code
        // we want to slowly narrow our search until we find exactly where we want to insert our code
        for (int i = 0; i < codes.Count; i++)
        {
            // if true we've found the start of trigger --> CardGettingAttacked
            if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == getTriggerInstance && codes[i + 1].opcode == OpCodes.Ldc_I4_7)
            {
                for (int j = i + 1; j < codes.Count; j++)
                {
                    // we need to get the operand for opposingSlot so we can insert it later
                    if (opposingSlotOperand == null && codes[j].opcode == OpCodes.Ldfld)
                    {
                        if (codes[j].operand.ToString() == opposingSlot)
                            opposingSlotOperand = codes[j].operand;
                    }
                    // also get the operand for get_card
                    if (getCardOperand == null && codes[j].opcode == OpCodes.Callvirt)
                    {
                        if (codes[j].operand.ToString() == getCard)
                            getCardOperand = codes[j].operand;
                    }
                    // if true, we've found the start of the if statement we'll be nesting in
                    if (codes[j].opcode == OpCodes.Stfld && codes[j - 1].opcode == OpCodes.Ldc_I4_M1)
                    {
                        for (int k = j + 1; k < codes.Count; k++)
                        {
                            // we want to grab the operand for !=
                            if (inequalityOperand == null && codes[k].opcode == OpCodes.Call)
                                inequalityOperand = (MethodInfo)codes[k].operand;

                            // we also want to grab the label so we know where to jump
                            if (ifLabelOperand == null && codes[k].opcode == OpCodes.Brfalse)
                                ifLabelOperand = codes[k].operand;

                            // if true, we've found where we want to insert our junk
                            if (codes[k].opcode == OpCodes.Stfld)
                            {
                                // check that we have all the operands we need
                                if (opposingSlotOperand == null || getCardOperand == null || inequalityOperand == null || ifLabelOperand == null)
                                {
                                    PatchPlugin.Logger.LogError("One of the required operands for code insertion was not retrieved properly. See below for which one(s) are null.");
                                    PatchPlugin.Logger.LogError($"opposingSlotOperand is null : {opposingSlotOperand == null}");
                                    PatchPlugin.Logger.LogError($"getCardOperand is null : {getCardOperand == null}");
                                    PatchPlugin.Logger.LogError($"inequalityOperand is null : {inequalityOperand == null}");
                                    PatchPlugin.Logger.LogError($"ifLabelOperand is null : {ifLabelOperand == null}");
                                }
                                else
                                {
                                    // if (this.opposingSlot.Card != null)
                                    codes.Insert(k + 1, new CodeInstruction(OpCodes.Ldarg_0));
                                    codes.Insert(k + 2, new CodeInstruction(OpCodes.Ldfld, opposingSlotOperand));
                                    codes.Insert(k + 3, new CodeInstruction(OpCodes.Callvirt, getCardOperand));
                                    codes.Insert(k + 4, new CodeInstruction(OpCodes.Ldnull));
                                    codes.Insert(k + 5, new CodeInstruction(OpCodes.Call, inequalityOperand));
                                    codes.Insert(k + 6, new CodeInstruction(OpCodes.Brfalse, ifLabelOperand));
                                }
                                break;
                            }

                        }
                        break;
                    }
                }
                break;
            }
        }

        return codes;
    }
}