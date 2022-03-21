using BepInEx;
using HarmonyLib;
using UnityEngine;
using DiskCardGame;
using System.Reflection;

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
        ChallengeIcon = 4,
        CostDecal = 5,
        OversizedCostDecal = 6,
        Act2CostDecalLeft = 7,
        Act2CostDecalRight = 8,
        StarterDeckIcon = 9
    };

    private static Vector2 DEFAULT_PIVOT = new(0.5f, 0.5f);

    private static Dictionary<Sprite, Sprite> emissionMap = new();

    private static Dictionary<SpriteType, Rect> SPRITE_RECTS = new () 
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
        { SpriteType.StarterDeckIcon, new Rect(0f, 0f, 35f, 44f) }
    };

    private static Dictionary<SpriteType, Vector2> SPRITE_PIVOTS = new () 
    {
        { SpriteType.CardPortrait, new Vector2(0.5f, 0.5f) },
        { SpriteType.PixelPortrait, new Vector2(0.5f, 0.5f) },
        { SpriteType.PixelAbilityIcon, new Vector2(0.5f, 0.5f) },
        { SpriteType.PixelStatIcon, new Vector2(0.5f, 0.5f) },
        { SpriteType.ChallengeIcon, new Vector2(0.5f, 0.5f) },
        { SpriteType.CostDecal, new Vector2(0.5f, 0.5f) },
        { SpriteType.OversizedCostDecal, new Vector2(0.5f, (28f * 4f - 14f) / (28f * 4f)) },
        { SpriteType.Act2CostDecalLeft, new Vector2(0.88f, 0.8f) },
        { SpriteType.Act2CostDecalRight, new Vector2(0.55f, 0.8f) },
        { SpriteType.StarterDeckIcon, new Vector2(0.5f, 0.5f) }
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
        texture.filterMode = filterMode;
        Sprite retval = Sprite.Create(texture, SPRITE_RECTS[spriteType], SPRITE_PIVOTS[spriteType]);
        return retval;
    }

    public static Sprite ConvertTexture(this Texture2D texture, Vector2? pivot = null)
    {
        pivot ??= new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), pivot.Value);
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
    
    public static Texture2D GetTextureFromResource(string resourceName)
    {
        string file = resourceName;
        file = file.Replace("/", ".");
        file = file.Replace("\\", ".");
        byte[] bytes = ExtractEmbeddedResource(file, Assembly.GetCallingAssembly());
        if (bytes == null)
        {
            Debug.LogWarning("No bytes found in \"" + file + "\"");
            return null;
        }
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.LoadImage(bytes);
        texture.filterMode = FilterMode.Point;
        string name = file.Substring(0, file.LastIndexOf('.'));
        if (name.LastIndexOf('.') >= 0)
        {
            name = name.Substring(name.LastIndexOf('.') + 1);
        }
        texture.name = name;
        return texture;
    }

    public static byte[] ExtractEmbeddedResource(string filePath, Assembly overrideAssembly = null)
    {
        filePath = filePath.Replace("/", ".");
        filePath = filePath.Replace("\\", ".");
        var baseAssembly = overrideAssembly ?? Assembly.GetCallingAssembly();
        using Stream resFilestream = baseAssembly.GetManifestResourceStream(filePath);
        if (resFilestream == null)
        {
            return null;
        }
        byte[] ba = new byte[resFilestream.Length];
        resFilestream.Read(ba, 0, ba.Length);
        return ba;
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