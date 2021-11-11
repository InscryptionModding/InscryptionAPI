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

		public static void Add(CardInfo card)
		{
			NewCard.cards.Add(card);
			Plugin.Log.LogInfo($"Loaded custom card {card.name}!");
		}

		// TODO Implement a handler for custom appearanceBehaviour - in particular custom card backs
		// TODO Change parameter order, and function setter call order to make more sense
		// TODO Rename parameters to be more user friendly
		public static void Add(string name, List<CardMetaCategory> metaCategories, CardComplexity cardComplexity,
			CardTemple temple, string displayedName, int baseAttack, int baseHealth,
			string description = null,
			bool hideAttackAndHealth = false, int cost = 0, int bonesCost = 0, int energyCost = 0,
			List<GemType> gemsCost = null, SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
			List<Tribe> tribes = null, List<Trait> traits = null, List<SpecialTriggeredAbility> specialAbilities = null,
			List<Ability> abilities = null, List<AbilityIdentifier> abilityIds = null, EvolveParams evolveParams = null,
			string defaultEvolutionName = null, TailParams tailParams = null, IceCubeParams iceCubeParams = null,
			bool flipPortraitForStrafe = false, bool onePerDeck = false,
			List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D tex = null,
			Texture2D altTex = null, Texture titleGraphic = null, Texture2D pixelTex = null,
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
			card.cost = cost;
			card.bonesCost = bonesCost;
			card.energyCost = energyCost;

			if (gemsCost is not null)
			{
				card.gemsCost = gemsCost;
			}

			// textures
			DetermineAndSetCardArt(name, card, tex, altTex, pixelTex);

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

			NewCard.cards.Add(card);

			foreach (AbilityIdentifier id in abilityIds)
			{
				if (id.id != 0)
				{
					card.abilities.Add(id.id);
					abilityIds.Remove(id);
				}
			}
			if (abilityIds is not null)
			{
				NewCard.abilityIds[NewCard.cards.Count - 1] = abilityIds;
			}
			Plugin.Log.LogInfo($"Loaded custom card {name}!");
		}

		private static void DetermineAndSetCardArt(
			string name, CardInfo card,
			Texture2D tex, Texture2D altTex, Texture2D pixelTex)
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

				card.pixelPortrait = Sprite.Create(pixelTex, CardUtils.DefaultCardPixelArtRect, CardUtils.DefaultVector2);
				card.pixelPortrait.name = newName;
			}
		}
	}
}
