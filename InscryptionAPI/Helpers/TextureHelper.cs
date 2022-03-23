using System.Reflection;
using BepInEx;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Helpers;

[HarmonyPatch]
public static class TextureHelper
{
    public enum SpriteType
    {
        CardPortrait = 0,
        PixelPortrait = 1,
        PixelAbilityIcon = 2,
        PixelStatIcon = 3,
        ChallengeIcon = 4,
        CostDecal = 5,
        OversizedCostDecal = 6,
        Act2CostDecalLeft = 7,
        Act2CostDecalRight = 8,
        StarterDeckIcon = 9,
        TribeIcon = 10
    }

    private static Vector2 DEFAULT_PIVOT = new(0.5f, 0.5f);

    private static readonly Dictionary<Sprite, Sprite> EmissionMap = new();

    private static readonly Dictionary<SpriteType, Rect> SpriteRects = new()
    {
        { SpriteType.CardPortrait, new Rect(0.0f, 0.0f, 114.0f, 94.0f) },
        { SpriteType.PixelPortrait, new Rect(0.0f, 0.0f, 41.0f, 28.0f) },
        { SpriteType.PixelAbilityIcon, new Rect(0f, 0f, 17f, 17f) },
        { SpriteType.PixelStatIcon, new Rect(0f, 0f, 16f, 8f) },
        { SpriteType.ChallengeIcon, new Rect(0f, 0f, 49f, 49f) },
        { SpriteType.CostDecal, new Rect(0f, 0f, 64f, 28f) },
        { SpriteType.OversizedCostDecal, new Rect(0f, 0f, 64f, 28f * 4f) },
        { SpriteType.Act2CostDecalLeft, new Rect(0f, 0f, 32f, 32f) },
        { SpriteType.Act2CostDecalRight, new Rect(0f, 0f, 32f, 32f) },
        { SpriteType.StarterDeckIcon, new Rect(0f, 0f, 35f, 44f) },
        { SpriteType.TribeIcon, new Rect(0f, 0f, 109f, 149f) }
    };

    private static readonly Dictionary<SpriteType, Vector2> SpritePivots = new()
    {
        { SpriteType.CardPortrait, DEFAULT_PIVOT },
        { SpriteType.PixelPortrait, DEFAULT_PIVOT },
        { SpriteType.PixelAbilityIcon, DEFAULT_PIVOT },
        { SpriteType.PixelStatIcon, DEFAULT_PIVOT },
        { SpriteType.ChallengeIcon, DEFAULT_PIVOT },
        { SpriteType.CostDecal, DEFAULT_PIVOT },
        { SpriteType.OversizedCostDecal, new Vector2(0.5f, (28f * 4f - 14f) / (28f * 4f)) },
        { SpriteType.Act2CostDecalLeft, new Vector2(0.88f, 0.8f) },
        { SpriteType.Act2CostDecalRight, new Vector2(0.55f, 0.8f) },
        { SpriteType.StarterDeckIcon, DEFAULT_PIVOT },
        { SpriteType.TribeIcon, DEFAULT_PIVOT }
    };

    public static byte[] ReadArtworkFileAsBytes(string pathCardArt)
    {
        return File.ReadAllBytes(
            Directory.GetFiles(Paths.PluginPath, pathCardArt, SearchOption.AllDirectories)[0]
        );
    }

    public static Texture2D GetImageAsTexture(string pathCardArt, FilterMode filterMode = FilterMode.Point)
    {
        Texture2D texture = new Texture2D(2, 2);
        byte[] imgBytes = ReadArtworkFileAsBytes(pathCardArt);
        bool isLoaded = texture.LoadImage(imgBytes);
        texture.filterMode = filterMode;
        return texture;
    }

    public static Texture2D GetImageAsTexture(string pathCardArt, Assembly target, FilterMode filterMode = FilterMode.Point)
    {
        Texture2D texture = new Texture2D(2, 2);
        byte[] imgBytes = GetResourceBytes(pathCardArt, target);
        bool isLoaded = texture.LoadImage(imgBytes);
        texture.filterMode = filterMode;
        return texture;
    }

    public static Sprite ConvertTexture(this Texture2D texture, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        if (!texture)
        {
            InscryptionAPIPlugin.Logger.LogWarning("CovertTexture was called with a null texture object, defaulting to black texture.");
            return Sprite.Create(Rect.zero, Vector2.zero, 100f, Texture2D.blackTexture);
        }
        texture.filterMode = filterMode;
        return Sprite.Create(texture, SpriteRects[spriteType], SpritePivots[spriteType]);
    }

    public static Sprite GetImageAsSprite(string pathCardArt, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        return GetImageAsTexture(pathCardArt).ConvertTexture(spriteType, filterMode);
    }

    public static void RegisterEmissionForSprite(this Sprite regularSprite, Sprite emissionSprite)
    {
        emissionSprite.name = regularSprite.name + "_emission";
        EmissionMap.Add(regularSprite, emissionSprite);
    }

    public static void RegisterEmissionForSprite(this Sprite regularSprite, Texture2D texture, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        Sprite emissionSprite = texture.ConvertTexture(spriteType, filterMode);
        emissionSprite.name = regularSprite.name + "_emission";
        EmissionMap.Add(regularSprite, emissionSprite);
    }

    public static void RegisterEmissionForSprite(this Sprite regularSprite, string pathToArt, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        Sprite emissionSprite = GetImageAsSprite(pathToArt, spriteType, filterMode);
        emissionSprite.name = regularSprite.name + "_emission";
        EmissionMap.Add(regularSprite, emissionSprite);
    }

    public static void TryReuseEmission(CardInfo info, Sprite alternatePortrait)
    {
        if (info.portraitTex != null)
            if (EmissionMap.ContainsKey(info.portraitTex))
                EmissionMap.Add(alternatePortrait, EmissionMap[info.portraitTex]);
    }

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.GetEmissivePortrait))]
    [HarmonyPrefix]
    public static bool GetCustomEmission(Sprite mainPortrait, ref Sprite __result)
    {
        if (EmissionMap.ContainsKey(mainPortrait))
        {
            __result = EmissionMap[mainPortrait];
            return false;
        }
        return true;
    }

    public static byte[] GetResourceBytes(string filename, Assembly target)
    {
        string lowerKey = $".{filename.ToLowerInvariant()}";
        string resourceName = target.GetManifestResourceNames().FirstOrDefault(r => r.ToLowerInvariant().EndsWith(lowerKey));

        if (string.IsNullOrEmpty(resourceName))
            throw new KeyNotFoundException($"Could not find resource matching {filename} in assembly {target}.");

        using Stream resourceStream = target.GetManifestResourceStream(resourceName);
        using MemoryStream memStream = new MemoryStream();
        resourceStream.CopyTo(memStream);
        return memStream.ToArray();
    }

    public static Texture2D CombineTextures(List<Texture2D> pieces, Texture2D baseTexture, int xStep = 0, int yStep = 0, int xOffset = 0, int yOffset = 0)
    {
        if (pieces != null)
        {
            for (int j = 0; j < pieces.Count; j++)
                if (pieces[j] != null)
                    baseTexture.SetPixels(xOffset + xStep * j, yOffset + yStep * (pieces.Count - j - 1), pieces[j].width, pieces[j].height, pieces[j].GetPixels(), 0);

            baseTexture.Apply();
        }

        return baseTexture;
    }
}