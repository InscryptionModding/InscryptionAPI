using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public class CustomCard
	{
		public static List<CustomCard> cards = new List<CustomCard>();
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
		[IgnoreMapping]
		public Texture2D tex;
		[IgnoreMapping]
		public Texture2D altTex;
		public Texture titleGraphic;
		[IgnoreMapping]
		public Texture2D pixelTex;
		public GameObject animatedPortrait;
		public List<Texture> decals;

		public CustomCard(string name)
		{
			this.name = name;
			CustomCard.cards.Add(this);
		}

		public CardInfo AdjustCard(CardInfo card)
		{
			TypeMapper<CustomCard, CardInfo>.Convert(this, card);

			if (this.tex is not null)
			{
				tex.name = "portrait_" + name;
				tex.filterMode = FilterMode.Point;
				card.portraitTex = Sprite.Create(tex, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
				card.portraitTex.name = "portrait_" + name;
			}
			if (this.altTex is not null)
			{
				altTex.name = "portrait_" + name;
				altTex.filterMode = FilterMode.Point;
				card.alternatePortrait = Sprite.Create(altTex, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
				card.alternatePortrait.name = "portrait_" + name;
			}
			if (this.pixelTex is not null)
			{
				pixelTex.name = "portrait_" + name;
				pixelTex.filterMode = FilterMode.Point;
				card.pixelPortrait = Sprite.Create(pixelTex, new Rect(0.0f, 0.0f, 41.0f, 28.0f), new Vector2(0.5f, 0.5f));
				card.pixelPortrait.name = "portrait_" + name;
			}
			Plugin.Log.LogInfo($"Adjusted default card {name}!");
			return card;
		}
	}
}