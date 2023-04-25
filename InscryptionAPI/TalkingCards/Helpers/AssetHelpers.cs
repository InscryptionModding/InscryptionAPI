using BepInEx;
using System.Text;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards.Helpers;

public static class AssetHelpers
{
    private static readonly Dictionary<string, Texture2D> TextureCache = new()
    {
        { "_", EmptyAndTransparent() }
    };

    public static readonly Vector2 PIVOT_BOTTOM = new Vector2(0.5f, 0f);
    public static readonly Vector2 PIVOT_CENTER = new Vector2(0.5f, 0.5f);

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

    public static Texture2D MakeTexture(byte[] data)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(data);
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    public static Sprite? MakeSprite(string? path)
    {
        Texture2D? tex = MakeTexture(path);
        return tex == null ? null : MakeSprite(tex);
    }

    public static Sprite MakeSprite(byte[] data)
    {
        return MakeSprite(MakeTexture(data));
    }

    public static Sprite MakeSprite(Texture2D tex)
    {
        Rect texRect = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = PIVOT_BOTTOM;
        return Sprite.Create(tex, texRect, pivot);
    }

    public static (Sprite?, Sprite?) MakeSpriteTuple((string? a, string? b)? tuple)
    {
        Sprite? a = MakeSprite(tuple?.a);
        Sprite? b = MakeSprite(tuple?.b);
        return (a, b);
    }

    public static T? ParseAsEnumValue<T>(string? str) where T : Enum
    {
        if (str == null) return default(T);
        Type type = typeof(T);
        object? x = Enum.Parse(type, str);
        return x != null ? (T?)x : default(T);
    }

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