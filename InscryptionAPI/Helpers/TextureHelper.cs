using BepInEx;
using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Helpers;

/// <summary>
/// This class contains a number of helper methods for managing textures.
/// </summary>
[HarmonyPatch]
public static class TextureHelper
{
    /// <summary>
    /// Thie is used to indicate what type of sprite you wish to create so that the appropriate size and pivot point can be determined.
    /// </summary>
    public enum SpriteType : int
    {
        /// <summary>
        /// A card's portrait art in Act 1 or Act 3
        /// </summary>
        CardPortrait = 0,

        /// <summary>
        /// A card's portrait art in Act 2
        /// </summary>
        PixelPortrait = 1,

        /// <summary>
        /// An ability icon (sigil) in Act 2 
        /// </summary>
        PixelAbilityIcon = 2,

        /// <summary>
        /// A special stat icon in Act 2
        /// </summary>
        PixelStatIcon = 3,

        /// <summary>
        /// A challenge skull displayed on the challenge UI during the setup of a Kaycee's Mod run
        /// </summary>
        ChallengeIcon = 4,

        /// <summary>
        /// The texture that displays the card's cost in Act 1
        /// </summary>
        CostDecal = 5,

        /// <summary>
        /// The large decal used to display multiple/hybrid card costs in Act 1
        /// </summary>
        OversizedCostDecal = 6,

        /// <summary>
        /// The decal used to display card costs in Act 2, on the top-left of the card
        /// </summary>
        Act2CostDecalLeft = 7,

        /// <summary>
        /// The decal used to display card costs in Act 2, on the top-right of the card
        /// </summary>
        Act2CostDecalRight = 8,

        /// <summary>
        /// The starter deck icon displayed on the challenge UI during the setup on a Kaycee's Mod run
        /// </summary>
        StarterDeckIcon = 9,

        /// <summary>
        /// The starter deck icon displayed on the challenge UI during the setup on a Kaycee's Mod run
        /// </summary>
        PixelDecal = 10
    };

    private static Vector2 DEFAULT_PIVOT = new(0.5f, 0.5f);

    private static Dictionary<Sprite, Sprite> emissionMap = new();

    private static Dictionary<SpriteType, Rect> SPRITE_RECTS = new()
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
        { SpriteType.PixelDecal, new Rect(0f, 0f, 42f, 56f) }
    };

    private static Dictionary<SpriteType, Vector2> SPRITE_PIVOTS = new()
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
        { SpriteType.PixelDecal, DEFAULT_PIVOT }
    };

    /// <summary>
    /// Reads the contents of an image file on disk and returns it as a byte array.
    /// </summary>
    /// <param name="pathCardArt">The path to the card on disk. This can be relative to the BepInEx plugin path, or can be an absolute (rooted) path.</param>
    /// <returns>The contents of the file in pathCardArt as a byte array.</returns>
    public static byte[] ReadArtworkFileAsBytes(string pathCardArt)
    {
        if (!Path.IsPathRooted(pathCardArt))
        {
            var files = Directory.GetFiles(Paths.PluginPath, pathCardArt, SearchOption.AllDirectories);
            if (files.Length < 1) throw new FileNotFoundException($"Could not find relative artwork file!\nFile name: {pathCardArt}", pathCardArt);
            pathCardArt = files[0];
        }

        if (!File.Exists(pathCardArt)) throw new FileNotFoundException($"Absolute path to artwork file does not exist!\nFile name: {pathCardArt}", pathCardArt);

        return File.ReadAllBytes(pathCardArt);
    }

    /// <summary>
    /// Converts an artwork file on disk to a Unity Texture2D object.
    /// </summary>
    /// <param name="pathCardArt">The path to the card on disk. This can be relative to the BepInEx plugin path, or can be an absolute (rooted) path.</param>
    /// <param name="filterMode">Sets the filter mode of the art. Leave this alone unless you know why you're changing it.</param>
    /// <returns>The image file on disk as a Texture2D object.</returns>
    public static Texture2D GetImageAsTexture(string pathCardArt, FilterMode filterMode = FilterMode.Point)
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        byte[] imgBytes = ReadArtworkFileAsBytes(pathCardArt);
        bool isLoaded = texture.LoadImage(imgBytes);
        texture.filterMode = filterMode;
        return texture;
    }

    /// <summary>
    /// Converts an artwork file stored as a resource in an assembly file to a Unity Texture2D object.
    /// </summary>
    /// <param name="pathCardArt">The name of the artwork file in the assembly.</param>
    /// <param name="target">The assembly to pull the artwork file from.</param>
    /// <param name="filterMode">Sets the filter mode of the art. Leave this alone unless you know why you're changing it.</param>
    /// <returns>The image file from the assembly as a Texture2D object.</returns>
    public static Texture2D GetImageAsTexture(string pathCardArt, Assembly target, FilterMode filterMode = FilterMode.Point)
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        byte[] imgBytes = GetResourceBytes(pathCardArt, target);
        bool isLoaded = texture.LoadImage(imgBytes);
        texture.filterMode = filterMode;
        return texture;
    }

    /// <summary>
    /// Converts a Unity Texture2D object to a Sprite that conforms to the expectations for the given sprite type.
    /// </summary>
    /// <param name="texture">The Texture2D object to convert.</param>
    /// <param name="spriteType">The type of sprite to create.</param>
    /// <param name="filterMode">Sets the filter mode of the art. Leave this alone unless you know why you're changing it.</param>
    /// <returns>A sprite containing the given texture.</returns>
    public static Sprite ConvertTexture(this Texture2D texture, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        texture.filterMode = filterMode;
        Sprite retval = Sprite.Create(texture, SPRITE_RECTS[spriteType], SPRITE_PIVOTS[spriteType]);
        return retval;
    }

    /// <summary>
    /// Converts a Unity Texture2D object to a Sprite with the same dimensions as the texture.
    /// </summary>
    /// <param name="texture">The Texture2D object to convert.</param>
    /// <param name="pivot">The pivot of the sprite. If null/default, the pivot will be the middle of the texture.</param>
    /// <returns>A sprite containing the given texture.</returns>
    public static Sprite ConvertTexture(this Texture2D texture, Vector2? pivot = null)
    {
        pivot ??= new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), pivot.Value);
    }

    /// <summary>
    /// Converts an image on disk to a Sprite that conforms to the expectations for the given sprite type.
    /// </summary>
    /// <param name="pathCardArt">The path to the card on disk. This can be relative to the BepInEx plugin path, or can be an absolute (rooted) path.</param>
    /// <param name="spriteType">The type of sprite to create</param>
    /// <param name="filterMode">Sets the filter mode of the art. Leave this alone unless you know why you're changing it.</param>
    /// <returns>A sprite containing the image file on disk</returns>
    public static Sprite GetImageAsSprite(string pathCardArt, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        return GetImageAsTexture(pathCardArt).ConvertTexture(spriteType, filterMode);
    }
    /// <summary>
    /// Converts an artwork file stored as a resource in an assembly file to a Sprite that conforms to the expectations for the given sprite type.
    /// </summary>
    /// <param name="pathCardArt">The path to the card on disk. This can be relative to the BepInEx plugin path, or can be an absolute (rooted) path.</param>
    /// <param name="target">The assembly to pull the artwork file from.</param>
    /// <param name="spriteType">The type of sprite to create.</param>
    /// <param name="filterMode">Sets the filter mode of the art. Leave this alone unless you know why you're changing it.</param>
    /// <returns>A sprite containing the image file from the assembly.</returns>
    public static Sprite GetImageAsSprite(string pathCardArt, Assembly target, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        return GetImageAsTexture(pathCardArt, target).ConvertTexture(spriteType, filterMode);
    }

    /// <summary>
    /// Sets the emissive sprite for a given sprite. This is used when an Act 1 card receives an ability from a card merge.
    /// </summary>
    /// <param name="regularSprite">The normal sprite.</param>
    /// <param name="emissionSprite">The emissive sprite.</param>
    public static void RegisterEmissionForSprite(this Sprite regularSprite, Sprite emissionSprite)
    {
        emissionSprite.name = regularSprite.name + "_emission";
        emissionMap[regularSprite] = emissionSprite;
    }

    /// <summary>
    /// Sets the emissive sprite for a given sprite. This is used when an Act 1 card receives an ability from a card merge.
    /// </summary>
    /// <param name="regularSprite">The normal sprite.</param>
    /// <param name="emissiveTexture">The emissive texture to register.</param>
    /// <param name="spriteType">The type of sprite to create.</param>
    /// <param name="filterMode">Sets the filter mode of the art. Leave this alone unless you know why you're changing it.</param>
    public static void RegisterEmissionForSprite(this Sprite regularSprite, Texture2D emissiveTexture, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        Sprite emissionSprite = emissiveTexture.ConvertTexture(spriteType, filterMode);
        regularSprite.RegisterEmissionForSprite(emissionSprite);
    }

    /// <summary>
    /// Sets the emissive sprite for a given sprite. This is used when an Act 1 card receives an ability from a card merge.
    /// </summary>
    /// <param name="regularSprite">The normal sprite.</param>
    /// <param name="pathToArt">The path to the card art.</param>
    /// <param name="spriteType">The type of sprite to create.</param>
    /// <param name="filterMode">Sets the filter mode of the art. Leave this alone unless you know why you're changing it.</param>
    public static void RegisterEmissionForSprite(this Sprite regularSprite, string pathToArt, SpriteType spriteType, FilterMode filterMode = FilterMode.Point)
    {
        Sprite emissionSprite = GetImageAsSprite(pathToArt, spriteType, filterMode);
        regularSprite.RegisterEmissionForSprite(emissionSprite);
    }

    /// <summary>
    /// Sets the emissive portrait for the card's alternate portrait using the same emission as the default portrait.
    /// </summary>
    /// <param name="info">The card to set the emission for.</param>
    /// <param name="alternatePortrait">The alternate portrait for the card.</param>
    public static void TryReuseEmission(CardInfo info, Sprite alternatePortrait)
    {
        if (info.portraitTex != null)
            if (emissionMap.ContainsKey(info.portraitTex) && !emissionMap.ContainsKey(alternatePortrait))
                emissionMap[alternatePortrait] = emissionMap[info.portraitTex];
    }

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.GetEmissivePortrait))]
    [HarmonyPrefix]
    private static bool GetCustomEmission(Sprite mainPortrait, ref Sprite __result)
    {
        if (emissionMap.ContainsKey(mainPortrait))
        {
            __result = emissionMap[mainPortrait];
            return false;
        }
        return true;
    }

    /// <summary>
    /// Reads the contents of an image file in an assembly and returns it as a byte array.
    /// </summary>
    /// <param name="pathCardArt">The name of the art file stored as a resource in the assembly.</param>
    /// <param name="target">The assembly to pull the art from.</param>
    /// <returns>The contents of the file in pathCardArt as a byte array.</returns>
    public static byte[] GetResourceBytes(string filename, Assembly target)
    {
        string lowerKey = $".{filename.ToLowerInvariant()}";
        string resourceName = target.GetManifestResourceNames().FirstOrDefault(r => r.ToLowerInvariant().EndsWith(lowerKey));

        if (string.IsNullOrEmpty(resourceName))
            throw new KeyNotFoundException($"Could not find resource matching {filename} in assembly {target}.");

        using (Stream resourceStream = target.GetManifestResourceStream(resourceName))
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                resourceStream.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }

    /// <summary>
    /// Combines multiple textures into one, using a tiled approach.
    /// </summary>
    /// <remarks>
    /// This helper has a very specific purpose. The pixels in <paramref>baseTexture</paramref> will be iteratively replaced with the pixels
    /// in the <paramref>pieces</paramref> array. The X position for the i-th texture will be 
    /// <paramref>xOffset</paramref> + <paramref>xStep</paramref> * i. The Y position for the i-th texture will be
    /// <paramref>yOffset</paramref> + <paramref>yStep</paramref> * (<paramref>pieces</paramref>.Count - i - 1).
    /// 
    /// **Note**: <paramref>baseTexture</paramref> will be modified in-place!
    /// </remarks>
    /// <param name="pieces">The individual textures to combine into the base texture.</param>
    /// <param name="baseTexture">The background texture for the combined texture.</param>
    /// <param name="xStep">Used to set the position for individual textures.</param>
    /// <param name="yStep">Used to set the position for individual textures.</param>
    /// <param name="xOffset">Used to set the position for individual textures.</param>
    /// <param name="yOffset">Used to set the position for individual textures.</param>
    /// <returns>The modified texture (the same Texture references as <paramref>baseTexture</paramref>)</returns>
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
