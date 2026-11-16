using BepInEx;
using System.Text;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards.Helpers;

/// <summary>
/// Class of helper methods for the creation of custom talking cards.
/// </summary>
public static class AssetHelpers
{
    private static readonly Dictionary<string, Texture2D> TextureCache = new()
    {
        { "_", EmptyAndTransparent() }
    };

    /// <summary>
    /// x-y coordinates of the centre-bottom pivot on the talking card portrait's sprite.
    /// </summary>
    public static readonly Vector2 PIVOT_BOTTOM = new Vector2(0.5f, 0f);
    /// <summary>
    /// x-y coordinates of the centre pivot on the talking card portrait's sprite.
    /// </summary>
    public static readonly Vector2 PIVOT_CENTER = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Locates the file name with the given file path.
    /// </summary>
    /// <param name="file">The file path.</param>
    /// <returns>The name of the file at the given path, or null if one could not be found.</returns>
    public static string? GetFile(string file) => Directory.GetFiles(Paths.PluginPath, file, SearchOption.AllDirectories).FirstOrDefault();

    public static Texture2D? MakeTexture(string? path)
    {
        if (path == null || path.IsWhiteSpace()) return null;

        if (TextureCache.ContainsKey(path))
        {
            //FileLog.Log($"Loading from cache: {path}");
            return TextureCache[path];
        }

        string? file = Path.IsPathRooted(path) ? path : GetFile(path);
        Texture2D? tex = file == null ? null : MakeTexture(File.ReadAllBytes(file));

        if (tex != null) TextureCache.Add(path, tex);
        return tex;
    }

    /// <summary>
    /// Creates a new <see cref="Texture2D"/> using the given array of bytes representing resource data.
    /// </summary>
    /// <param name="data">Resource bytes.</param>
    /// <returns>The created texture.</returns>
    public static Texture2D MakeTexture(byte[] data)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(data);
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    /// <summary>
    /// Makes a sprite from the texture file at the given path.
    /// </summary>
    /// <param name="path">The file path for the texture file.</param>
    /// <returns>The created sprite or null if the path is invalid.</returns>
    public static Sprite? MakeSprite(string? path)
    {
        Texture2D? tex = MakeTexture(path);
        return tex == null ? null : MakeSprite(tex);
    }

    /// <summary>
    /// Makes a sprite using the given array of bytes representing resource data.
    /// </summary>
    /// <param name="data">Resource bytes.</param>
    /// <returns>The created sprite.</returns>
    public static Sprite MakeSprite(byte[] data)
    {
        return MakeSprite(MakeTexture(data));
    }

    /// <summary>
    /// Converts the given texture to a sprite using <see cref="PIVOT_BOTTOM"/> as the pivot.
    /// </summary>
    /// <param name="tex">The texture to convert.</param>
    /// <returns>The created sprite.</returns>
    public static Sprite MakeSprite(Texture2D tex)
    {
        Rect texRect = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = PIVOT_BOTTOM;
        return Sprite.Create(tex, texRect, pivot);
    }

    /// <summary>
    /// Creates two sprites from the two given file paths then returns them as a <see cref="Tuple"/>.
    /// </summary>
    /// <param name="tuple"><see cref="Tuple"/> of file paths.</param>
    /// <returns>A tuple of sprites. If either path is null or invalid, that sprite will also be null.</returns>
    public static (Sprite?, Sprite?) MakeSpriteTuple((string? a, string? b)? tuple)
    {
        Sprite? a = MakeSprite(tuple?.a);
        Sprite? b = MakeSprite(tuple?.b);
        return (a, b);
    }

    /// <summary>
    /// Attempts to parse the given string as an enum value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="str">The value of the enum type.</param>
    /// <returns>The parsed enum value, or null if it couldn't be parsed.</returns>
    public static T? ParseAsEnumValue<T>(string? str) where T : Enum
    {
        if (str == null) return default(T);
        Type type = typeof(T);
        object? x = Enum.Parse(type, str);
        return x != null ? (T?)x : default(T);
    }

    /// <summary>
    /// Creates a <see cref="Color32"/> using a string representing its hexadecimal code.
    /// </summary>
    /// <param name="hex">The hexadecimal code of the colour to create, represented as a string.</param>
    /// <returns>The Color with the given hex code. If the hex code is less than 5 character, returns white.</returns>
    public static Color32 HexToColor(string hex)
    {
        Queue<char> chars = new Queue<char>(hex.Trim());
        if (chars.Count == 0) return default;
        if (chars.Peek() == '#') chars.Dequeue();

        if (chars.Count < 6)
        {
            LogHelpers.LogError($"Invalid hexcode: {hex}");
            return Color.white;
        }

        // I could have used a List<byte>, but this is a tiny bit faster.
        // (And I know exactly how many items I'll need, anyway.)
        byte[] rgb = new byte[3];

        for (int i = 0; i < 3; i++)
        {
            StringBuilder sb = new();
            sb.Append(chars.Dequeue());
            sb.Append(chars.Dequeue());

            byte x = Convert.ToByte(sb.ToString(), 16);
            rgb[i] = x;
        }

        return new(rgb[0], rgb[1], rgb[2], 255);
    }

    internal static Texture2D EmptyAndTransparent()
    {
        Texture2D tex = new Texture2D(114, 94);
        Color transparent = new Color(0, 0, 0, 0);

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, transparent);
            }
        }
        tex.Apply();
        return tex;
    }
}