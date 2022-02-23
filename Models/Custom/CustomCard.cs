using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public class CustomCard
	{
		public static List<CustomCard> cards = new();

		public static Dictionary<int,List<AbilityIdentifier>> abilityIds = new();
		public static Dictionary<int, List<SpecialAbilityIdentifier>> specialAbilityIds = new();
		public static Dictionary<int,EvolveIdentifier> evolveIds = new();
		public static Dictionary<int,IceCubeIdentifier> iceCubeIds = new();
		public static Dictionary<int,TailIdentifier> tailIds = new();

		public static Dictionary<string, Sprite> emissions = new();

		public string name;
		public List<CardMetaCategory> metaCategories;
		public CardComplexity? cardComplexity;
		public CardTemple? temple;
		public string displayedName;
		public int? baseAttack;
		public int? baseHealth;
		public string description;
		public bool? hideAttackAndHealth;
		public int? cost;
		public int? bonesCost;
		public int? energyCost;
		public List<GemType> gemsCost;
		public SpecialStatIcon? specialStatIcon;
		public List<Tribe> tribes;
		public List<Trait> traits;
		public List<SpecialTriggeredAbility> specialAbilities;
		public List<Ability> abilities;
		public EvolveParams evolveParams;
		public string defaultEvolutionName;
		public TailParams tailParams;
		public IceCubeParams iceCubeParams;
		public bool? flipPortraitForStrafe;
		public bool? onePerDeck;
		public List<CardAppearanceBehaviour.Appearance> appearanceBehaviour;
		[IgnoreMapping] public Texture2D tex;
		[IgnoreMapping] public Texture2D altTex;
		public Texture titleGraphic;
		[IgnoreMapping] public Texture2D pixelTex;
		[IgnoreMapping] public Texture2D emissionTex;
		public GameObject animatedPortrait;
		public List<Texture> decals;

		public CustomCard(
			string name,
			List<AbilityIdentifier> abilityIdParam=null,
			List<SpecialAbilityIdentifier> specialAbilityIdParam=null,
			EvolveIdentifier evolveId=null,
			IceCubeIdentifier iceCubeId=null,
			TailIdentifier tailId=null)
		{
			this.name = name;
			CustomCard.cards.Add(this);

			// Handle AbilityIdentifier
			if (abilityIdParam is not null && abilityIdParam.Count > 0)
			{
				CustomCard.abilityIds[CustomCard.cards.Count - 1] = abilityIdParam;
			}

			if (specialAbilityIdParam is not null && specialAbilityIdParam.Count > 0)
			{
				CustomCard.specialAbilityIds[CustomCard.cards.Count - 1] = specialAbilityIdParam;
			}

			// Handle EvolveIdentifier
			if (evolveId is not null)
			{
				CustomCard.evolveIds[CustomCard.cards.Count - 1] = evolveId;
			}

			// Handle IceCubeIdentifier
			if (iceCubeId is not null)
			{
				CustomCard.iceCubeIds[CustomCard.cards.Count - 1] = iceCubeId;
			}

			// Handle TailIdentifier
			if (tailId is not null)
			{
				CustomCard.tailIds[CustomCard.cards.Count - 1] = tailId;
			}
		}

		public CardInfo AdjustCard(CardInfo card)
		{
			TypeMapper<CustomCard, CardInfo>.Convert(this, card);
			Plugin.Log.LogDebug($"Finished TypeMapping for card [{card.name}]!");
			
			Plugin.Log.LogDebug($"Checking default tex is not null...");
			if (this.tex is not null)
			{
				Plugin.Log.LogDebug($"Default tex is not null, setting fields...");
				tex.name = "portrait_" + name;
				tex.filterMode = FilterMode.Point;
				card.portraitTex = Sprite.Create(tex, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
				card.portraitTex.name = "portrait_" + name;
				if (this.emissionTex is not null)
				{
					Plugin.Log.LogDebug($"Emission tex is not null, setting fields...");
					emissionTex.name = tex.name + "_emission";
					emissionTex.filterMode = FilterMode.Point;
					Sprite emissionSprite = Sprite.Create(emissionTex, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
					emissionSprite.name = tex.name + "_emission";
					emissions.Add(tex.name, emissionSprite);
				}
			}

			Plugin.Log.LogDebug($"Checking AltTex is not null...");
			if (this.altTex is not null)
			{
				Plugin.Log.LogDebug($"--> AltTex is not null, setting fields...");
				altTex.name = "portrait_" + name;
				altTex.filterMode = FilterMode.Point;
				card.alternatePortrait = Sprite.Create(altTex, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
				card.alternatePortrait.name = "portrait_" + name;
			}

			Plugin.Log.LogDebug($"Checking pixelTex is not null...");
			if (this.pixelTex is not null)
			{
				Plugin.Log.LogDebug($"PixelTex is not null, setting fields...");
				pixelTex.name = "portrait_" + name;
				pixelTex.filterMode = FilterMode.Point;
				card.pixelPortrait =
					Sprite.Create(pixelTex, CardUtils.DefaultCardPixelArtRect, CardUtils.DefaultVector2);
				card.pixelPortrait.name = "portrait_" + name;
			}

			Plugin.Log.LogDebug($"Checking decals is not null...");
			if (this.decals is not null)
			{
				foreach (var decal in this.decals)
				{
					decal.filterMode = FilterMode.Point;
				}

				card.decals = this.decals;
			}

			Plugin.Log.LogInfo($"Adjusted default card {name}!");
			return card;
		}
	}
}
