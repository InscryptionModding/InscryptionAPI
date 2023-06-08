using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Helpers;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Card;


[HarmonyPatch]
internal static class Act2ShapeshifterPatches
{

    [HarmonyPatch(typeof(Shapeshifter), nameof(Shapeshifter.RevealInBattle), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PixelDialogueFix(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        // we want to slowly narrow our search until we find exactly where we want to insert our code
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand.ToString() == "IjiraqRevealed" && codes[i - 1].opcode == OpCodes.Call)
            {
                i--;
                codes.RemoveRange(i, 6);

                MethodBase customMethod = AccessTools.Method(typeof(Act2ShapeshifterPatches), nameof(Act2ShapeshifterPatches.Act2Dialogue));
                codes[i].operand = customMethod;
                break;
            }
        }

        return codes;
    }

    private static IEnumerator Act2Dialogue() => DialogueManager.PlayDialogueEventSafe("IjiraqRevealed", TextDisplayer.MessageAdvanceMode.Input);

    [HarmonyPatch(typeof(Shapeshifter), nameof(Shapeshifter.DisguiseOutOfBattle))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PixelOutsideFix(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        // we want to slowly narrow our search until we find exactly where we want to insert our code
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand.ToString() == "IjiraqRevealed" && codes[i - 1].opcode == OpCodes.Call)
            {
                i--;
                codes.RemoveRange(i, 6);

                MethodBase customMethod = AccessTools.Method(typeof(Act2ShapeshifterPatches), nameof(Act2ShapeshifterPatches.GetPixelCards));
                codes[i].operand = customMethod;
                break;
            }
        }

        return codes;
    }

    private static List<CardInfo> GetPixelCards()
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            return new(SaveData.Data.collection.CardInfos);
        }
        else
        {
            return new(RunState.Run.playerDeck.Cards);
        }
    }
}