using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class Part1HintColourFixes
{
    [HarmonyPrefix, HarmonyPatch(typeof(HintsHandler), nameof(HintsHandler.OnNonplayableCardClicked))]
    private static void FixHintSpeaker(PlayableCard card)
    {
        if (card.EnergyCost > Singleton<ResourcesManager>.Instance.PlayerEnergy)
        {
            DialogueEvent dialogueEvent = DialogueDataUtil.Data.GetEvent(HintsHandler.notEnoughEnergyHint.dialogueId);
            if (SaveManager.SaveFile.IsPart1)
                ModifyDialogueEventLines(dialogueEvent, x => x.text = RemoveColourCodes(x.text, "[c:bB]"));
            else
                ModifyDialogueEventLines(dialogueEvent, x => x.text = ReAddColourCodes(x.text, "[c:bB]"));

            return;
        }
    }
    private static void ModifyDialogueEventLines(DialogueEvent dialogueEvent, Action<DialogueEvent.Line> action = null)
    {
        if (action == null)
            return;

        dialogueEvent.mainLines.lines.ForEach(action);
        dialogueEvent.repeatLines.ForEach(x => x.lines.ForEach(action));
    }
    private static string RemoveColourCodes(string text, string codeToRemove) => text.Replace(codeToRemove, "").Replace("[c:]", "");
    private static string ReAddColourCodes(string text, string startingCode)
    {
        string newText = text;
        if (!text.StartsWith(startingCode))
            newText = startingCode + text;

        if (!text.EndsWith("[c:]"))
            newText += "[c:]";

        return newText;
    }
}