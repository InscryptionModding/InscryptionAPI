using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.TalkingCards.Helpers;

#nullable enable
namespace InscryptionAPI.TalkingCards.Create;

[HarmonyPatch]
internal static class EmotionManager
{
    // An accurate list of the names.
    public static readonly string[] EmotionNames = Enum.GetNames(typeof(Emotion));

    [HarmonyPatch(typeof(SequentialText), nameof(SequentialText.ConsumeCode))]
    [HarmonyPrefix]
    private static void CorrectEmotionNames(ref string code)
    {
        if (!code.StartsWith("[e")) return;

        if (code.StartsWith("[end")) return;

        string x = DialogueParser.GetStringValue(code, "e");

        if (EmotionNames.Contains(x) || int.TryParse(x, out _)) return;

        code = $@"[e:{x.SentenceCase()}]";
    }
}
