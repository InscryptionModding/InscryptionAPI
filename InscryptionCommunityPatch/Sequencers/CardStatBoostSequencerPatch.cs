/*using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Sequencers;

[HarmonyPatch]
internal class CardStatBoostSequencerPatch
{
    private const string name_Current = "System.Object <>2__current";
    private const string name_State = "System.Int32 <>1__state";

    private const string name_DisplayClass = "DiskCardGame.CardStatBoostSequencer+<>c__DisplayClass12_0 <>8__1";
    private const string name_AttackMod = "System.Boolean attackMod";

    private const string name_ViewInstance = "DiskCardGame.ViewManager get_Instance()";

    private const string name_GetValidCards = "System.Collections.Generic.List`1[DiskCardGame.CardInfo] GetValidCards(Boolean)";
    private const string name_DestroyCard = "Void DestroyCard()";
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(CardStatBoostSequencer), nameof(CardStatBoostSequencer.StatBoostSequence));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }

    private static object Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        // startIndex is where the if statement will be placed,
        // endIndex is where the custom method will be placed
        int startIndex = -1, endIndex = -1;

        object op_Current = null;
        object op_State = null;

        object op_DisplayClass = null;
        object op_AttackMod = null;

        object op_GetValidCards = null;
        object op_GetCount = null;

        CodeInstruction instruction = null;

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Stfld && codes[i].operand.ToString() == name_Current && op_Current == null)
                op_Current = codes[i].operand;

            else if (codes[i].opcode == OpCodes.Stfld && codes[i].operand.ToString() == name_State && op_State == null)
                op_State = codes[i].operand;

            else if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand.ToString() == name_DisplayClass && op_DisplayClass == null)
                op_DisplayClass = codes[i].operand;

            else if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand.ToString() == name_AttackMod && op_AttackMod == null)
                op_AttackMod = codes[i].operand;

            else if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == name_GetValidCards && op_GetValidCards == null)
            {
                op_GetValidCards = codes[i].operand;
                op_GetCount = codes[i + 1].operand;
            }
            else if (codes[i].opcode == OpCodes.Ldc_I4_7)
                startIndex = i - 6;

            else if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == name_ViewInstance && codes[i + 1].opcode == OpCodes.Ldc_I4_1 && codes[i + 3].opcode == OpCodes.Ldc_I4_0)
                endIndex = i + 5;

            // get the CodeInstruction that skips past the bulk of the sequence
            else if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == name_DestroyCard)
                instruction = new(codes[i + 1]);

            if (startIndex != -1 && endIndex != -1)
            {
                // if (sequencer.GetValidCards(forAttackMod: attackMod).Count != 0)
                //      vanilla sequence
                // else
                //      custom message
                //      end sequence

                MethodBase customMethod = AccessTools.Method(typeof(CardStatBoostSequencerPatch),
                    nameof(CardStatBoostSequencerPatch.NewStatBoostSequence),
                    new Type[] { typeof(CardStatBoostSequencer), typeof(bool) });

                // Insert custom method before the if, since it's further down

                // this.<>2__current = customMethod
                codes.Insert(endIndex, new CodeInstruction(OpCodes.Ldarg_0));

                // sequencer
                codes.Insert(endIndex + 1, new CodeInstruction(OpCodes.Ldloc_1));

                // this.<DisplayClass>.attackMod
                codes.Insert(endIndex + 2, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(endIndex + 3, new CodeInstruction(OpCodes.Ldfld, op_DisplayClass));
                codes.Insert(endIndex + 4, new CodeInstruction(OpCodes.Ldfld, op_AttackMod));

                // method, current
                codes.Insert(endIndex + 5, new CodeInstruction(OpCodes.Callvirt, customMethod));
                codes.Insert(endIndex + 6, new CodeInstruction(OpCodes.Stfld, op_Current));

                // this.<>1__state = 66
                codes.Insert(endIndex + 7, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(endIndex + 8, new CodeInstruction(OpCodes.Ldc_I4_S, 66));
                codes.Insert(endIndex + 9, new CodeInstruction(OpCodes.Stfld, op_State));

                // return true
                codes.Insert(endIndex + 10, new CodeInstruction(OpCodes.Ldc_I4_1));
                codes.Insert(endIndex + 11, new CodeInstruction(OpCodes.Ret));

                // if (sequencer.GetValidCards(forAttackMod: attackMod).Count != 0)

                // sequencer
                codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldloc_1));

                // this.<DisplayClass>.attackMod
                codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Ldfld, op_DisplayClass));
                codes.Insert(startIndex + 3, new CodeInstruction(OpCodes.Ldfld, op_AttackMod));

                // GetValidCards().Count
                codes.Insert(startIndex + 4, new CodeInstruction(OpCodes.Callvirt, op_GetValidCards));
                codes.Insert(startIndex + 5, new CodeInstruction(OpCodes.Callvirt, op_GetCount));

                // else
                codes.Insert(startIndex + 6, instruction);
                codes[startIndex + 6].opcode = OpCodes.Brfalse_S;
                break;
            }
        }
        return codes;
    }

    private static IEnumerator NewStatBoostSequence(CardStatBoostSequencer instance, bool attackMod)
    {
        // if there aren't any valid cards, exit the sequence
        if (instance.GetValidCards(forAttackMod: attackMod).Count == 0)
        {
            PatchPlugin.Logger.LogDebug("Player has no cards that can be boosted at the campfire.");
            Singleton<ViewManager>.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.5f);
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("You have no creatures to warm.");
            yield return new WaitForSeconds(0.5f);
            AudioController.Instance.PlaySound3D("campfire_putout", MixerGroup.TableObjectsSFX, instance.selectionSlot.transform.position);
            AudioController.Instance.StopLoop(1);
            instance.campfireLight.gameObject.SetActive(value: false);
            Singleton<ExplorableAreaManager>.Instance.HandLight.gameObject.SetActive(value: false);
            yield return instance.pile.DestroyCards();
            yield return new WaitForSeconds(0.2f);
            instance.figurines.ForEach(delegate (CompositeFigurine x)
            {
                x.gameObject.SetActive(value: false);
            });
            instance.stakeRingParent.SetActive(value: false);
            instance.confirmStone.SetStoneInactive();
            instance.selectionSlot.gameObject.SetActive(value: false);
            CustomCoroutine.WaitThenExecute(0.4f, delegate
            {
                Singleton<ExplorableAreaManager>.Instance.HangingLight.intensity = 0f;
                Singleton<ExplorableAreaManager>.Instance.HangingLight.gameObject.SetActive(value: true);
                Singleton<ExplorableAreaManager>.Instance.HandLight.intensity = 0f;
                Singleton<ExplorableAreaManager>.Instance.HandLight.gameObject.SetActive(value: true);
            });

            ProgressionData.SetMechanicLearned(MechanicsConcept.CardStatBoost);
            if (Singleton<GameFlowManager>.Instance != null)
                Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.Map);
        }
    }
}*/