using System.Collections.Generic;
using CardLoaderPlugin.lib;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public static class NewCard
	{
		public static List<CardInfo> cards = new();

		public static Dictionary<int, List<AbilityIdentifier>> abilityIds = new();

		public static CardInfo CreateCardWithDefaultSettings(
			string name, string displayedName, int baseAttack, int baseHealth)
		{
			return CreateCard(
				name, displayedName, baseAttack, baseHealth,
				CardUtils.getNormalCardMetadata, CardComplexity.Simple, CardTemple.Nature
			);
		}

		public static CardInfo CreateCard(
			string name, string displayedName, int baseAttack, int baseHealth,
			List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple,
			string description = null,
			int bloodCost = 0, int bonesCost = 0, int energyCost = 0,
			List<Trait> traits = null,
			List<Tribe> tribes = null,
			List<GemType> gemsCost = null,
			bool hideAttackAndHealth = false,
			List<Ability> abilities = null, List<SpecialTriggeredAbility> specialAbilities = null,
			SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
			EvolveParams evolveParams = null,
			List<AbilityIdentifier> abilityIds = null,
			string defaultEvolutionName = null, TailParams tailParams = null, IceCubeParams iceCubeParams = null,
			bool flipPortraitForStrafe = false, bool onePerDeck = false,
			List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D defaultTexture = null,
			Texture2D altTexture = null, Texture2D pixelTex = null,
			Texture titleGraphic = null,
			GameObject animatedPortrait = null, List<Texture> decals = null)
		{
			CardInfo card = ScriptableObject.CreateInstance<CardInfo>();

			// names / descriptions
			card.defaultEvolutionName = defaultEvolutionName;
			card.displayedName = displayedName;
			card.name = name;

			if (description != "")
			{
				card.description = description;
			}

			// card info
			card.baseAttack = baseAttack;
			card.baseHealth = baseHealth;
			card.cardComplexity = cardComplexity;
			card.flipPortraitForStrafe = flipPortraitForStrafe;
			card.hideAttackAndHealth = hideAttackAndHealth;
			card.metaCategories = metaCategories;
			card.onePerDeck = onePerDeck;
			card.specialStatIcon = specialStatIcon;
			card.temple = temple;

			if (tribes is not null)
			{
				card.tribes = tribes;
			}

			if (traits is not null)
			{
				card.traits = traits;
			}

			if (specialAbilities is not null)
			{
				card.specialAbilities = specialAbilities;
			}

			if (abilities is not null)
			{
				card.abilities = abilities;
			}

			if (evolveParams is not null)
			{
				card.evolveParams = evolveParams;
			}

			if (tailParams is not null)
			{
				card.tailParams = tailParams;
			}

			if (iceCubeParams is not null)
			{
				card.iceCubeParams = iceCubeParams;
			}

			if (appearanceBehaviour is not null)
			{
				card.appearanceBehaviour = appearanceBehaviour;
			}

			// costs
			card.cost = bloodCost;
			card.bonesCost = bonesCost;
			card.energyCost = energyCost;

			if (gemsCost is not null)
			{
				card.gemsCost = gemsCost;
			}

			// textures
			DetermineAndSetCardArt(
				name, card, defaultTexture, altTexture, pixelTex,
				animatedPortrait, decals, titleGraphic
			);

			if (abilityIds is not null)
			{
				foreach (AbilityIdentifier id in abilityIds)
				{
					if (id.id != 0)
					{
						card.abilities.Add(id.id);
					}
				}
			}

			if (abilityIds is not null)
			{
				NewCard.abilityIds[NewCard.cards.Count - 1] = abilityIds;
			}

			return card;
		}

		public static void AddToPool(CardInfo card)
		{
			NewCard.cards.Add(card);
			Plugin.Log.LogInfo($"Loaded custom card {card.name}!");
		}

		// TODO Implement a handler for custom appearanceBehaviour - in particular custom card backs
		// TODO Change parameter order, and function setter call order to make more sense
		// TODO Rename parameters to be more user friendly
		public static void AddToPool(string name, string displayedName, int baseAttack, int baseHealth,
			List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple,
			string description = null,
			int bloodCost = 0, int bonesCost = 0, int energyCost = 0,
			List<Trait> traits = null,
			List<Tribe> tribes = null,
			List<GemType> gemsCost = null,
			bool hideAttackAndHealth = false,
			List<Ability> abilities = null, List<SpecialTriggeredAbility> specialAbilities = null,
			SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
			EvolveParams evolveParams = null,
			List<AbilityIdentifier> abilityIds = null,
			string defaultEvolutionName = null, TailParams tailParams = null, IceCubeParams iceCubeParams = null,
			bool flipPortraitForStrafe = false, bool onePerDeck = false,
			List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D defaultTexture = null,
			Texture2D altTexture = null, Texture2D pixelTex = null,
			Texture titleGraphic = null,
			GameObject animatedPortrait = null, List<Texture> decals = null)
		{
			var createdCard = CreateCard(
				name, displayedName, baseAttack, baseHealth, metaCategories, cardComplexity, temple,
				description, bloodCost, bonesCost, energyCost, traits, tribes, gemsCost, hideAttackAndHealth,
				abilities, specialAbilities, specialStatIcon, evolveParams, abilityIds, defaultEvolutionName,
				tailParams, iceCubeParams, flipPortraitForStrafe, onePerDeck, appearanceBehaviour,
				defaultTexture, altTexture, pixelTex, titleGraphic, animatedPortrait, decals
			);

			NewCard.AddToPool(createdCard);
		}

		private static void DetermineAndSetCardArt(
			string name, CardInfo card,
			Texture2D tex, Texture2D altTex, Texture2D pixelTex,
			GameObject animatedPortrait, List<Texture> decals, Texture titleGraphic)
		{
			var newName = "portrait_" + name;
			if (tex is not null)
			{
				tex.name = newName;
				tex.filterMode = FilterMode.Point;

				card.portraitTex = Sprite.Create(tex, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
				card.portraitTex.name = newName;
			}

			if (altTex is not null)
			{
				altTex.name = newName;
				altTex.filterMode = FilterMode.Point;

				card.alternatePortrait = Sprite.Create(altTex, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
				card.alternatePortrait.name = newName;
			}

			if (pixelTex is not null)
			{
				pixelTex.name = newName;
				pixelTex.filterMode = FilterMode.Point;

				card.pixelPortrait =
					Sprite.Create(pixelTex, CardUtils.DefaultCardPixelArtRect, CardUtils.DefaultVector2);
				card.pixelPortrait.name = newName;
			}

			// TODO Provide a function to create animated card textures
			card.animatedPortrait ??= animatedPortrait;

			// TODO Access and provide default decals
			card.decals ??= decals;

			card.titleGraphic ??= titleGraphic;
		}
	}
}