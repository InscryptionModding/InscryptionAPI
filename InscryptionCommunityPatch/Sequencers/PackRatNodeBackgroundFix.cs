using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Sequencers;

[HarmonyPatch]
internal class PackRatNodeBackgroundFix
{
    private const string name_RatCard = "DiskCardGame.SelectableCard ratCard";
    private const string name_Reward = "DiskCardGame.CardInfo fullConsumablesReward";
    private const string name_PixelPlacement = "Pixelplacement.TweenSystem.TweenBase Position(UnityEngine.Transform, UnityEngine.Vector3, Single, Single, UnityEngine.AnimationCurve, LoopType, System.Action, System.Action, Boolean)";
    private const string name_GetComponent = "DiskCardGame.SelectableCard GetComponent[SelectableCard]()";
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(GainConsumablesSequencer), nameof(GainConsumablesSequencer.FullConsumablesSequence));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        object op_RatCard = null;
        object op_Reward = null;

        // we want to slowly narrow our search until we find exactly where we want to insert our code
        for (int i = 0; i < codes.Count; i++)
        {
            // get the ratCard operand
            if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand.ToString() == name_RatCard)
                op_RatCard = codes[i].operand;

            if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand.ToString() == name_Reward)
                op_Reward = codes[i].operand;

            // look for the original code for `component`
            if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == name_GetComponent)
            {
                int startIndex = -1, endIndex = -1;
                for (int j = i; j > 0; j--)
                {
                    // find the startIndex
                    if (codes[j].opcode == OpCodes.Ldarg_0)
                    {
                        startIndex = j;
                        break;
                    }
                }

                // find the endIndex
                for (int k = i; k < codes.Count; k++)
                {
                    if (codes[k].opcode == OpCodes.Stloc_2)
                    {
                        for (int l = k; l > 0; l--)
                        {
                            if (codes[l].opcode == OpCodes.Callvirt)
                            {
                                endIndex = l + 1;
                                break;
                            }
                        }
                        break;
                    }
                }

                MethodInfo customMethod = AccessTools.Method(typeof(PackRatNodeBackgroundFix), nameof(PackRatNodeBackgroundFix.InstantiateSelectableCard), new Type[] { typeof(SelectableCard), typeof(CardInfo) });

                // remove all the old code
                codes.RemoveRange(startIndex, endIndex - startIndex);

                // gainConsumablesSequence.ratCard
                codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldloc_1));
                codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Ldfld, op_RatCard));

                // gainConsumablesSequence.fullConsumablesReward
                codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Ldloc_1));
                codes.Insert(startIndex + 3, new CodeInstruction(OpCodes.Ldfld, op_Reward));

                // InstantiateSelectableCard
                codes.Insert(startIndex + 4, new CodeInstruction(OpCodes.Call, customMethod));
                break;
            }
        }

        return codes;
    }

    public static void InstantiateSelectableCard(SelectableCard card, CardInfo info)
    {
        SelectableCard component = SelectableCard.Instantiate(card);
        component.SetInfo(info);
        component.SetInteractionEnabled(false);
        SelectableCard.Destroy(component);
    }
}