#nullable enable
namespace InscryptionAPI.TalkingCards.Helpers;

/// <summary>
/// Utility class adding new extensions to the <see cref="string"/> object.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts the given string to sentence case (first character upper-case, everything else lower-case.
    /// </summary>
    /// <param name="key">The string to convert.</param>
    /// <returns>The converted string.</returns>
    public static string SentenceCase(this string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;
        if (key.Length <= 1) return key.ToUpper();

        return char.ToUpper(key[0]) + key.Substring(1).ToLower();
    }

    /* I know about string.IsNullOrWhiteSpace().
     * I'm defining my own becaue of nullability. */
    /// <summary>
    /// Variant of <see cref="string.IsNullOrWhiteSpace(string)"/> that accounts for nullability.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if <paramref name="str"/> is empty or made entirely of whitespace character.</returns>
    public static bool IsWhiteSpace(this string str)
        => str.Length == 0 || str.All(char.IsWhiteSpace);
}
