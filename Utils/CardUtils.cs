using System;
using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public static class CardUtils
	{
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
	}
}