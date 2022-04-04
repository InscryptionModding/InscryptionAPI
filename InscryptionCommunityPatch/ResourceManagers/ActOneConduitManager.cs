using DiskCardGame;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace InscryptionCommunityPatch.ResourceManagers;

[HarmonyPatch]
internal static class ActOneConduitManager
{
    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
    [HarmonyILManipulator]
    private static void EnablePart1IL(ILContext il)
    {
        // This modifies the if statement in GetPassiveAttackBuffs so that it doesn't needlessly exlude act 1
        ILCursor ilCursor = new ILCursor(il);
        ILLabel label = null;
        ilCursor.GotoNext(MoveType.After, new Func<Instruction, bool>[]
        {
            ins => ILPatternMatchingExt.MatchCallvirt(ins, AccessTools.PropertyGetter(typeof(SaveFile), nameof(SaveFile.IsPart2))),
            ins => ILPatternMatchingExt.MatchBrtrue(ins, out label)
        });
        ilCursor.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(SaveManager), nameof(SaveManager.SaveFile)));
        ilCursor.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(SaveFile), nameof(SaveFile.IsPart1)));
        ilCursor.Emit(OpCodes.Brtrue, label);
    }
}