using BepInEx;
using HarmonyLib;
using UnityEngine;
using DiskCardGame;

namespace InscryptionAPI.Helpers;

[HarmonyPatch]
public static class TextureHelper
{
    public enum SpriteType : int
    {
        CardPortrait = 0,
        PixelPortrait = 1,
        PixelAbilityIcon = 2,
        PixelStatIcon = 3,
        ChallengeIcon = 4
    };

    private static Vector2 DEFAULT_PIVOT = new(0.5f, 0.5f);

    private static Dictionary<Sprite, Sprite> emissionMap = new();

    private static Dictionary<SpriteType, Rect> SPRITE_RECTS = new () 
    {
        { SpriteType.CardPortrait, new Rect(0.0f, 0.0f, 114.0f, 94.0f) },
        { SpriteType.PixelPortrait, new Rect(0.0f, 0.0f, 41.0f, 28.0f) },
        { SpriteType.PixelAbilityIcon, new Rect(0f, 0f, 17f, 17f) },
        { SpriteType.PixelStatIcon, new Rect(0f, 0f, 16f, 8f) },
        { SpriteType.ChallengeIcon, new Rect(0f, 0f, 49f, 49f) }
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

    public static Sprite ConvertTexture(this Texture2D texture, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        texture.filterMode = filterMode;
        Sprite retval = Sprite.Create(texture, SPRITE_RECTS[spriteType], DEFAULT_PIVOT);
        return retval;
    }

    public static Sprite GetImageAsSprite(string pathCardArt, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        return GetImageAsTexture(pathCardArt).ConvertTexture(spriteType, filterMode);
    }

    public static void RegisterEmissionForSprite(this Sprite regularSprite, Sprite emissionSprite)
    {
        emissionSprite.name = regularSprite.name + "_emission";
        emissionMap.Add(regularSprite, emissionSprite);
    }

    public static void RegisterEmissionForSprite(this Sprite regularSprite, Texture2D texture, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        Sprite emissionSprite = texture.ConvertTexture(spriteType, filterMode);
        emissionSprite.name = regularSprite.name + "_emission";
        emissionMap.Add(regularSprite, emissionSprite);
    }

    public static void RegisterEmissionForSprite(this Sprite regularSprite, string pathToArt, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        Sprite emissionSprite = GetImageAsSprite(pathToArt, spriteType, filterMode);
        emissionSprite.name = regularSprite.name + "_emission";
        emissionMap.Add(regularSprite, emissionSprite);
    }

    public static void TryReuseEmission(CardInfo info, Sprite alternatePortrait)
    {
        if (info.portraitTex != null)
            if (emissionMap.ContainsKey(info.portraitTex))
                emissionMap.Add(alternatePortrait, emissionMap[info.portraitTex]);
    }

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.GetEmissivePortrait))]
    [HarmonyPrefix]
    public static bool GetCustomEmission(Sprite mainPortrait, ref Sprite __result)
    {
        if (emissionMap.ContainsKey(mainPortrait))
        {
            __result = emissionMap[mainPortrait];
            return false;
        }
        return true;
    }
}