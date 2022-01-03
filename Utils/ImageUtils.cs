using System.IO;
using BepInEx;
using UnityEngine;

namespace APIPlugin
{
	public static class ImageUtils
	{
		public static readonly Rect DefaultCardArtRect = new Rect(0.0f, 0.0f, 114.0f, 94.0f);
		public static readonly Rect DefaultCardPixelArtRect = new Rect(0.0f, 0.0f, 41.0f, 28.0f);
		public static readonly Rect DefaultTribeIconRect = new Rect(0.0f, 0.0f, 109f, 149f);
		public static readonly Vector2 DefaultVector2Pivot = new Vector2(0.5f, 0.5f);

		public static string GetFullPathOfFile(string fileToLookFor)
		{
			return Directory.GetFiles(Paths.PluginPath, fileToLookFor, SearchOption.AllDirectories)[0];
		}

		public static byte[] ReadArtworkFileAsBytes(string nameOfCardArt)
		{
			return File.ReadAllBytes(GetFullPathOfFile(nameOfCardArt));
		}

		public static Texture2D LoadImageAndGetTexture(string nameOfCardArt)
		{
			Texture2D texture = new Texture2D(2, 2) { filterMode = FilterMode.Point };
			byte[] imgBytes = ReadArtworkFileAsBytes(nameOfCardArt);
			bool isLoaded = texture.LoadImage(imgBytes);
			return texture;
		}

		public static Sprite CreateSpriteFromPng(string pngFile)
		{
			return CreateSpriteFromPng(pngFile, DefaultVector2Pivot);
		}

		public static Sprite CreateSpriteFromPng(string pngFile, Vector2 pivot)
		{
			if (string.IsNullOrEmpty(pngFile))
			{
				return null;
			}

			if (!pngFile.EndsWith(".png"))
			{
				pngFile = string.Concat(pngFile, ".png");
			}

			Texture2D texture2D = LoadImageAndGetTexture(pngFile);

			return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), pivot);
		}
	}
}