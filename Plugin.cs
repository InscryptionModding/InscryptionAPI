using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;

namespace CardLoaderPlugin
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "cyantist.inscryption.cardLoader";
        private const string PluginName = "Cardloader";
        private const string PluginVersion = "1.2.1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            AddBears();
            ChangeWolf();

	          Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }

        private void AddBears(){
            List<CardMetaCategory> metaCategories = new List<CardMetaCategory>();
            metaCategories.Add(CardMetaCategory.ChoiceNode);
            metaCategories.Add(CardMetaCategory.Rare);
            List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = new List<CardAppearanceBehaviour.Appearance>();
            appearanceBehaviour.Add(CardAppearanceBehaviour.Appearance.RareCardBackground);
            byte[] imgBytes = System.IO.File.ReadAllBytes("BepInEx/plugins/CardLoader/Artwork/eightfuckingbears.png");
            Texture2D tex = new Texture2D(2,2);
            tex.LoadImage(imgBytes);
            new NewCard("Eight_Bears", metaCategories, CardComplexity.Simple, CardTemple.Nature,"8 fucking bears!",32,48,description:"Kill this abomination please",cost:3,appearanceBehaviour:appearanceBehaviour, tex:tex);
        }

        private void ChangeWolf(){
            new CustomCard("Wolf", baseAttack:10);
        }
    }

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
  			public Texture2D tex;
  			public Texture2D altTex;
  			public Texture titleGraphic;
  			public Texture2D pixelTex;
  			public GameObject animatedPortrait;
  			public List<Texture> decals;

        public CustomCard(string name, List<CardMetaCategory> metaCategories = null, CardComplexity? cardComplexity = null, CardTemple? temple = null, string displayedName = "",
                          int? baseAttack = null, int? baseHealth = null, string description = "", bool? hideAttackAndHealth = null, int? cost = null, int? bonesCost = null,
                          int? energyCost = null, List<GemType> gemsCost = null, SpecialStatIcon? specialStatIcon = null, List<Tribe> tribes = null, List<Trait> traits = null,
                          List<SpecialTriggeredAbility> specialAbilities = null, List<Ability> abilities = null, EvolveParams evolveParams = null, string defaultEvolutionName = "",
                          TailParams tailParams = null, IceCubeParams iceCubeParams = null, bool? flipPortraitForStrafe = null, bool? onePerDeck = null,
                          List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D tex = null, Texture2D altTex = null, Texture titleGraphic = null,
                          Texture2D pixelTex = null, GameObject animatedPortrait = null, List<Texture> decals = null)
        {
            this.name = name;
            this.metaCategories = metaCategories;
            this.cardComplexity = cardComplexity;
            this.temple = temple;
            this.displayedName = displayedName;
            this.baseAttack = baseAttack;
            this.baseHealth = baseHealth;
            this.description = description;
            this.hideAttackAndHealth = hideAttackAndHealth;
            this.cost = cost;
            this.bonesCost = bonesCost;
            this.energyCost = energyCost;
            this.gemsCost = gemsCost;
            this.specialStatIcon = specialStatIcon;
            this.tribes = tribes;
            this.traits = traits;
            this.specialAbilities = specialAbilities;
            this.abilities = abilities;
            this.evolveParams = evolveParams;
            this.defaultEvolutionName = defaultEvolutionName;
            this.tailParams = tailParams;
            this.iceCubeParams = iceCubeParams;
            this.flipPortraitForStrafe = flipPortraitForStrafe;
            this.onePerDeck = onePerDeck;
            this.appearanceBehaviour = appearanceBehaviour;
            this.tex = tex;
            this.altTex = altTex;
            this.titleGraphic = titleGraphic;
            this.pixelTex = pixelTex;
            this.animatedPortrait = animatedPortrait;
            this.decals = decals;
            CustomCard.cards.Add(this);
        }

        public CardInfo AdjustCard(CardInfo card)
        {
            var cardTraverse = Traverse.Create(card);
            if(this.metaCategories is not null) {card.metaCategories = this.metaCategories;}
            if(this.cardComplexity is not null) {card.cardComplexity = (CardComplexity)this.cardComplexity;}
            if(this.temple is not null) {card.temple = (CardTemple)this.temple;}
            if(!String.IsNullOrEmpty(displayedName)){cardTraverse.Field("displayedName").SetValue(displayedName);}
            if(this.baseAttack is not null) {cardTraverse.Field("baseAttack").SetValue(baseAttack);}
            if(this.baseHealth is not null) {cardTraverse.Field("baseHealth").SetValue(baseHealth);}
            if(!String.IsNullOrEmpty(description)){card.description = this.description;}
            if(this.cost is not null) {cardTraverse.Field("cost").SetValue(cost);}
            if(this.bonesCost is not null) {cardTraverse.Field("bonesCost").SetValue(bonesCost);}
            if(this.energyCost is not null) {cardTraverse.Field("energyCost").SetValue(energyCost);}
            if(this.gemsCost is not null) {cardTraverse.Field("gemsCost").SetValue(gemsCost);}
            if(this.specialStatIcon is not null) {cardTraverse.Field("specialStatIcon").SetValue(specialStatIcon);}
            if(this.tribes is not null) {card.tribes = this.tribes;}
            if(this.traits is not null) {card.traits = this.traits;}
            if(this.specialAbilities is not null){cardTraverse.Field("specialAbilities").SetValue(specialAbilities);}
            if(this.abilities is not null) {cardTraverse.Field("abilities").SetValue(abilities);}
            if(this.appearanceBehaviour is not null) {card.appearanceBehaviour = this.appearanceBehaviour;}
            if(this.onePerDeck is not null) {card.onePerDeck = (bool)this.onePerDeck;}
            if(this.hideAttackAndHealth is not null) {card.hideAttackAndHealth = (bool)this.hideAttackAndHealth;}
            if(this.tex is not null) {
              tex.name = "portrait_"+name;
              card.portraitTex = Sprite.Create(tex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
              card.portraitTex.name = "portrait_"+name;
            }
            if(this.altTex is not null) {
              altTex.name = "portrait_"+name;
              card.alternatePortrait = Sprite.Create(altTex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
              card.alternatePortrait.name = "portrait_"+name;
            }
            if(this.titleGraphic is not null) {card.titleGraphic = this.titleGraphic;}
            if(this.pixelTex is not null) {
              pixelTex.name = "portrait_"+name;
              card.pixelPortrait = Sprite.Create(pixelTex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
              card.pixelPortrait.name = "portrait_"+name;
            }
            if(animatedPortrait is not null) {cardTraverse.Field("animatedPortrait").SetValue(animatedPortrait);}
            if(decals is not null) {cardTraverse.Field("decals").SetValue(decals);}
            Plugin.Log.LogInfo($"Adjusted default card {name}!");
            return card;
        }
    }

    public class NewCard
    {
      public static List<CardInfo> cards = new List<CardInfo>();

      public NewCard(CardInfo card)
      {
          Plugin.Log.LogInfo($"Loaded custom card {card.name}!");
          NewCard.cards.Add(card);
      }

      // TODO Implement a handler for custom appearanceBehaviour - in particular custom card backs
      // TODO Change parameter order, and function setter call order to make more sense
      // TODO Rename parameters to be more user friendly
      public NewCard(string name, List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple, string displayedName, int baseAttack, int baseHealth, string description = "",
                                bool hideAttackAndHealth = false, int cost = 0, int bonesCost = 0, int energyCost = 0, List<GemType> gemsCost = null, SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
                                List<Tribe> tribes = null, List<Trait> traits = null, List<SpecialTriggeredAbility> specialAbilities = null, List<Ability> abilities = null, EvolveParams evolveParams = null,
                                string defaultEvolutionName = "", TailParams tailParams = null, IceCubeParams iceCubeParams = null, bool flipPortraitForStrafe = false, bool onePerDeck = false,
                                List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D tex = null, Texture2D altTex = null, Texture titleGraphic = null, Texture2D pixelTex = null,
                                GameObject animatedPortrait = null, List<Texture> decals = null)
      {
          CardInfo card = ScriptableObject.CreateInstance<CardInfo>();
          var cardTraverse = Traverse.Create(card);
          card.metaCategories = metaCategories;
          card.cardComplexity = cardComplexity;
          card.temple = temple;
          cardTraverse.Field("displayedName").SetValue(displayedName);
          cardTraverse.Field("baseAttack").SetValue(baseAttack);
          cardTraverse.Field("baseHealth").SetValue(baseHealth);
          if(description!=""){
            card.description = description;
          }
          cardTraverse.Field("cost").SetValue(cost);
          cardTraverse.Field("bonesCost").SetValue(bonesCost);
          cardTraverse.Field("energyCost").SetValue(energyCost);
          if(gemsCost!=null){
             cardTraverse.Field("gemsCost").SetValue(gemsCost);
          }
          cardTraverse.Field("specialStatIcon").SetValue(specialStatIcon);
          if(tribes!=null){
            card.tribes = tribes;
          }
          if(traits!=null){
            card.traits = traits;
          }
          if(specialAbilities!=null){
            cardTraverse.Field("specialAbilities").SetValue(specialAbilities);
          }
          if(abilities!=null){
            cardTraverse.Field("abilities").SetValue(abilities);
          }
          if(appearanceBehaviour!=null){
            card.appearanceBehaviour = appearanceBehaviour;
          }
          card.onePerDeck = onePerDeck;
          card.hideAttackAndHealth = hideAttackAndHealth;
          if(tex!=null){
            tex.name = "portrait_"+name;
            card.portraitTex = Sprite.Create(tex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
            card.portraitTex.name = "portrait_"+name;
          }
          if(altTex!=null){
            altTex.name = "portrait_"+name;
            card.alternatePortrait = Sprite.Create(altTex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
            card.alternatePortrait.name = "portrait_"+name;
          }
          if(titleGraphic!=null){
            card.titleGraphic = titleGraphic;
          }
          if(pixelTex!=null){
            pixelTex.name = "portrait_"+name;
            card.pixelPortrait = Sprite.Create(pixelTex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
            card.pixelPortrait.name = "portrait_"+name;
          }
          if(animatedPortrait!=null){
            // TODO Provide a function to create animated card textures
            cardTraverse.Field("animatedPortrait").SetValue(animatedPortrait);
          }
          if(decals!=null){
            // TODO Access and provide default decals
            cardTraverse.Field("decals").SetValue(decals);
          }
          card.name = name;
          Plugin.Log.LogInfo($"Loaded custom card {name}!");
          NewCard.cards.Add(card);
      }

    }

    [HarmonyPatch(typeof(LoadingScreenManager), "LoadGameData")]
    public class LoadingScreenManager_LoadGameData
    {
        public static void Prefix()
        {
            var allData = Traverse.Create<ScriptableObjectLoader<CardInfo>>().Field("allData");
            if(allData.GetValue<List<CardInfo>>() == null)
            {
                List<CardInfo> official = ScriptableObjectLoader<CardInfo>.AllData;
                foreach(CustomCard card in CustomCard.cards){
                    int index = official.FindIndex((CardInfo x) => x.name == card.name);
                    if(index == -1)
                    {
                        Plugin.Log.LogInfo($"Could not find card {card.name} to modify");
                    } else
                    {
                        Plugin.Log.LogInfo($"Loaded modified {card.name} into data");
                        official[index] = card.AdjustCard(official[index]);
                    }
                }
                allData.SetValue(official.Concat(NewCard.cards).ToList());
                Plugin.Log.LogInfo($"Loaded custom cards into data");
            }
        }
    }

    [HarmonyPatch(typeof(ProgressionData), "UnlockAll")]
    public class ProgressionData_UnlockAll
    {
        public static void Prefix()
        {
            var allData = Traverse.Create<ScriptableObjectLoader<CardInfo>>().Field("allData");
            if(allData.GetValue<List<CardInfo>>() == null)
            {
                List<CardInfo> official = ScriptableObjectLoader<CardInfo>.AllData;
                foreach(CustomCard card in CustomCard.cards){
                    int index = official.FindIndex((CardInfo x) => x.name == card.name);
                    if(index == -1)
                    {
                        Plugin.Log.LogInfo($"Could not find card {card.name} to modify");
                    } else
                    {
                        Plugin.Log.LogInfo($"Loaded modified {card.name} into data");
                        official[index] = card.AdjustCard(official[index]);
                    }
                }
                allData.SetValue(official.Concat(NewCard.cards).ToList());
                Plugin.Log.LogInfo($"Loaded custom cards into data");
            }
        }
    }
}
