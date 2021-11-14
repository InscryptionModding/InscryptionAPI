using System.Collections.Generic;
using CardLoaderPlugin.lib;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public static class NewCard
	{
		public static List<CardInfo> cards = new List<CardInfo>();
		public static Dictionary<int,List<AbilityIdentifier>> abilityIds = new Dictionary<int,List<AbilityIdentifier>>();
		public static Dictionary<int,EvolveIdentifier> evolveIds = new Dictionary<int,EvolveIdentifier>();
		public static Dictionary<int,IceCubeIdentifier> iceCubeIds = new Dictionary<int,IceCubeIdentifier>();
		public static Dictionary<int,TailIdentifier> tailIds = new Dictionary<int,TailIdentifier>();

		public static CardInfo CreateCard(string name, string displayedName, int baseAttack, int baseHealth,
			List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple,
			string description = null, bool hideAttackAndHealth = false, 
			int bloodCost = 0, int bonesCost = 0, int energyCost = 0,
			List<GemType> gemsCost = null, SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
			List<Tribe> tribes = null, List<Trait> traits = null, List<SpecialTriggeredAbility> specialAbilities = null,
			List<Ability> abilities = null, List<AbilityIdentifier> abilityIds = null, EvolveParams evolveParams = null,
			string defaultEvolutionName = null, TailParams tailParams = null, IceCubeParams iceCubeParams = null,
			bool flipPortraitForStrafe = false, bool onePerDeck = false,
			List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D defaultTex = null,
			Texture2D altTex = null, Texture titleGraphic = null, Texture2D pixelTex = null,
			GameObject animatedPortrait = null, List<Texture> decals = null, EvolveIdentifier evolveId = null,
			IceCubeIdentifier iceCubeId = null, TailIdentifier tailId = null)
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
			DetermineAndSetCardArt(name, card, defaultTex, altTex, pixelTex);

			if (animatedPortrait is not null)
			{
				// TODO Provide a function to create animated card textures
				card.animatedPortrait = animatedPortrait;
			}

			if (decals is not null)
			{
				// TODO Access and provide default decals
				card.decals = decals;
			}

			if (titleGraphic is not null)
			{
				card.titleGraphic = titleGraphic;
			}
			
			return card;
		}
		
		public static void AddToPool(CardInfo card, List<AbilityIdentifier> abilityIds = null, EvolveIdentifier evolveId = null,
			IceCubeIdentifier iceCubeId = null, TailIdentifier tailId = null)
		{
			NewCard.cards.Add(card);
			handleIdentifiers(card, abilityIds, evolveId, iceCubeId, tailId);
			Plugin.Log.LogInfo($"Loaded custom card {card.name}!");
		}

		// TODO Implement a handler for custom appearanceBehaviour - in particular custom card backs
		// TODO Change parameter order, and function setter call order to make more sense
		// TODO Rename parameters to be more user friendly
		public static void AddToPool(string name, string displayedName, int baseAttack, int baseHealth,
			List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple,
			string description = null, bool hideAttackAndHealth = false, 
			int bloodCost = 0, int bonesCost = 0, int energyCost = 0,
			List<GemType> gemsCost = null, SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
			List<Tribe> tribes = null, List<Trait> traits = null, List<SpecialTriggeredAbility> specialAbilities = null,
			List<Ability> abilities = null, List<AbilityIdentifier> abilityIds = null, EvolveParams evolveParams = null,
			string defaultEvolutionName = null, TailParams tailParams = null, IceCubeParams iceCubeParams = null,
			bool flipPortraitForStrafe = false, bool onePerDeck = false,
			List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D defaultTex = null,
			Texture2D altTex = null, Texture titleGraphic = null, Texture2D pixelTex = null,
			GameObject animatedPortrait = null, List<Texture> decals = null, EvolveIdentifier evolveId = null,
			IceCubeIdentifier iceCubeId = null, TailIdentifier tailId = null)
		{
			var createdCard = CreateCard(
				name, displayedName, baseAttack, baseHealth, metaCategories, cardComplexity, temple, description, 
				hideAttackAndHealth, bloodCost, bonesCost, energyCost, gemsCost, specialStatIcon, tribes, traits, 
				specialAbilities, abilities, abilityIds, evolveParams, defaultEvolutionName, tailParams, iceCubeParams,
				flipPortraitForStrafe, onePerDeck, appearanceBehaviour, 
				defaultTex, altTex, titleGraphic, pixelTex, animatedPortrait, decals, 
				evolveId, iceCubeId, tailId
			);

			NewCard.AddToPool(createdCard);
		}

		private static void handleIdentifiers(CardInfo card, List<AbilityIdentifier> abilityIds, EvolveIdentifier evolveId,
		IceCubeIdentifier iceCubeId, TailIdentifier tailId)
		{
			// Handle AbilityIdentifier
			List<AbilityIdentifier> toRemove = new List<AbilityIdentifier>();
			if (abilityIds is not null)
			{
				foreach (AbilityIdentifier id in abilityIds)
				{
					if (id.id != 0)
					{
						card.abilities.Add(id.id);
					}
				}
				foreach (AbilityIdentifier id in toRemove)
				{
					abilityIds.Remove(id);
				}
			}
			if (abilityIds is not null && abilityIds.Count > 0)
			{
				NewCard.abilityIds[NewCard.cards.Count - 1] = abilityIds;
			}

			// Handle EvolveIdentifier
			if (evolveId is not null)
			{
				NewCard.evolveIds[NewCard.cards.Count - 1] = evolveId;
			}

			// Handle IceCubeIdentifier
			if (iceCubeId is not null)
			{
				NewCard.iceCubeIds[NewCard.cards.Count - 1] = iceCubeId;
			}

			// Handle TailIdentifier
			if (tailId is not null)
			{
				NewCard.tailIds[NewCard.cards.Count - 1] = tailId;
			}
		}

		private static void DetermineAndSetCardArt(
			string name, CardInfo card,
			Texture2D defaultTex, Texture2D altTex, Texture2D pixelTex)
		{
			var newName = "portrait_" + name;
			if (defaultTex is not null)
			{
				defaultTex.name = newName;
				defaultTex.filterMode = FilterMode.Point;

				card.portraitTex = Sprite.Create(defaultTex, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
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

				card.pixelPortrait = Sprite.Create(pixelTex, CardUtils.DefaultCardPixelArtRect, CardUtils.DefaultVector2);
				card.pixelPortrait.name = newName;
			}
		}
	}
}
