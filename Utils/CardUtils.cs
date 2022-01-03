using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public static class CardUtils
	{
		public static readonly Vector2 DefaultVector2 = new Vector2(0.5f, 0.5f);
		public static readonly Rect DefaultCardArtRect = new Rect(0.0f, 0.0f, 114.0f, 94.0f);
		public static readonly Rect DefaultCardPixelArtRect = new Rect(0.0f, 0.0f, 41.0f, 28.0f);

		public static Predicate<CardInfo> IsNonLivingCard = card
			=> card.traits.Exists(t => t is Trait.Terrain or Trait.Pelt);

		public static List<CardMetaCategory> getNormalCardMetadata = new()
			{ CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer };

		public static List<CardMetaCategory> getRareCardMetadata = new()
			{ CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare };

		public static List<CardMetaCategory> getGBCCardMetadata = new()
			{ CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable };

		public static List<CardMetaCategory> getGBCRareCardMetadata = new()
		{
			CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare
		};

		public static List<CardMetaCategory> getAllActsCardMetadata = new()
		{
			CardMetaCategory.ChoiceNode, CardMetaCategory.Rare, CardMetaCategory.TraderOffer, CardMetaCategory.Rare,
			CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Part3Random
		};


		// card appearance

		public static List<CardAppearanceBehaviour.Appearance> getRareAppearance = new()
			{ CardAppearanceBehaviour.Appearance.RareCardBackground };

		public static List<CardAppearanceBehaviour.Appearance> getTerrainBackgroundAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.TerrainBackground
		};

		public static List<CardAppearanceBehaviour.Appearance> getTerrainLayoutAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.TerrainLayout
		};

		public static List<CardAppearanceBehaviour.Appearance> getTerrainAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout
		};

		public static List<CardAppearanceBehaviour.Appearance> getHologramAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.HologramPortrait
		};

		public static List<CardAppearanceBehaviour.Appearance> getAnimatedAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.AnimatedPortrait
		};

		public static List<CardAppearanceBehaviour.Appearance> getGoldAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.GoldEmission
		};

		public static List<CardAppearanceBehaviour.Appearance> getBloodDecalAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.AlternatingBloodDecal
		};

		public static List<CardAppearanceBehaviour.Appearance> getDynamicAppearance = new()
		{
			CardAppearanceBehaviour.Appearance.DynamicPortrait
		};

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
			Texture2D texture = new Texture2D(2, 2);
			byte[] imgBytes = ReadArtworkFileAsBytes(nameOfCardArt);
			bool isLoaded = texture.LoadImage(imgBytes);
			return texture;
		}

		public static Sprite CreateSpriteFromPng(string pngFile)
		{
			return CreateSpriteFromPng(pngFile, DefaultVector2);
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

			byte[] pngBytes = ReadArtworkFileAsBytes(pngFile);
			Texture2D texture2D = new Texture2D(2, 2)
			{
				filterMode = FilterMode.Point
			};

			texture2D.LoadImage(pngBytes);
			return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), pivot);
		}
	}
}