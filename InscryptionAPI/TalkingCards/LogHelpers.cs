namespace InscryptionAPI.TalkingCards;
internal static class LogHelpers
{
    internal static void LogError(string message)
        => InscryptionAPIPlugin.Logger.LogError($"TalkingCards: {message}");

    internal static void LogInfo(string message)
        => InscryptionAPIPlugin.Logger.LogInfo($"TalkingCards: {message}");
}
