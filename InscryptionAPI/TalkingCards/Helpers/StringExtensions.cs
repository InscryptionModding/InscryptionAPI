#nullable enable
namespace InscryptionAPI.TalkingCards.Helpers;
public static class StringExtensions
{
    public static string SentenceCase(this string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;
        if (key.Length <= 1) return key.ToUpper();

        return char.ToUpper(key[0]) + key.Substring(1).ToLower();
    }

    /* I know about string.IsNullOrWhiteSpace().
     * I'm defining my own becaue of nullability. */
    public static bool IsWhiteSpace(this string str)
        => str.Length == 0 || str.All(char.IsWhiteSpace);
}
