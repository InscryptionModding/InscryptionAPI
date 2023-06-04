using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Card;

// Fixes non-Giant cards with AllStrike always attacking slot 0 instead of the opposing slot
[HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
public class AllStrikeAbilityFix
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        int startIndex = -1, endIndex = -1;

        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            // this is where we'll be inserting our code
            if (codes[i].opcode == OpCodes.Endfinally)
            {
                for (int j = i + 1; j < codes.Count; j++)
                {
                    // start at the first ldc.i4.0
                    if (codes[j].opcode == OpCodes.Ldc_I4_0)
                        startIndex = j;

                    // end at the first break
                    if (codes[j].opcode == OpCodes.Br)
                    {
                        endIndex = j;
                        break;
                    }
                }
                break;
            }
        }

        if (startIndex > -1 && endIndex > -1)
        {
            // get our custom method
            MethodInfo customMethod = AccessTools.Method(typeof(AllStrikeAbilityFix), nameof(AllStrikeAbilityFix.AllStrikeDirectAttackFix), new Type[] { typeof(List<CardSlot>), typeof(List<CardSlot>), typeof(PlayableCard) });

            // remove the previous code then insert our own
            codes.RemoveRange(startIndex, endIndex - startIndex);
            codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldarg_0));
            codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Call, customMethod));
        }

        return codes;
    }
    public static void AllStrikeDirectAttackFix(List<CardSlot> list, List<CardSlot> list2, PlayableCard __instance)
    {
        // for whatever reason the transpiler breaks the if-else statement so we need to put this check here
        if (!list2.Exists((CardSlot x) => x.Card != null && !__instance.CanAttackDirectly(x)))
        {
            // if this card isn't a giant add opposingSlot, otherwise add slot 0
            if (!__instance.Info.HasTrait(Trait.Giant))
            {
                list.Add(__instance.Slot.opposingSlot);
            }
            else
            {
                list.Add(list2[0]);
            }
        }
    }
}