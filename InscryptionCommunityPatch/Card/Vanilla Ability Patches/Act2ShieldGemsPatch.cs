using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class Act2ShieldGemsPatch
{
    // Replace with custom decal maybe? Reference pixel Gemified border, would require patching various Set/ResetShielded methods
    //    [HarmonyPatch(typeof(ShieldGeneratorItem), nameof(ShieldGeneratorItem.AddShieldsToCards))]
    //    [HarmonyPrefix]
    //    private static bool AlwaysResolveOnBoard(List<PlayableCard> addShields, List<CardSlot> resetShieldsSlots)
    //    {
    //        if (!SaveManager.SaveFile.IsPart2)
    //            return true;

    //        foreach (PlayableCard addShield in addShields)
    //        {
    //            CardModificationInfo mod = new(Ability.DeathShield);
    //            addShield.AddTemporaryMod(mod);
    //        }
    //        foreach (CardSlot resetShieldsSlot in resetShieldsSlots)
    //        {
    //            if (resetShieldsSlot.Card != null)
    //            resetShieldsSlot.Card.ResetShield();
    //        }
    //        return false;
    //    }

    [HarmonyPatch(typeof(ShieldGeneratorItem), nameof(ShieldGeneratorItem.AddShieldsToCards))]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldloc_1)
            {
                MethodInfo customMethod = AccessTools.Method(typeof(Act2ShieldGemsPatch), nameof(Act2ShieldGemsPatch.ShowSigilIcon),
                    new Type[] { typeof(PlayableCard) });

                i++;
                codes.RemoveRange(i, 2);
                codes.Insert(i, new(OpCodes.Call, customMethod));
                break;
            }
        }

        return codes;
    }
    private static bool ShowSigilIcon(PlayableCard card)
    {
        if (SaveManager.SaveFile.IsPart2)
            return true;

        return card.HasAbility(Ability.DeathShield);
    }
}