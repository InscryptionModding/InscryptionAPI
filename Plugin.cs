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
        private const string PluginVersion = "1.1.0.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            AddBears();

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
          NewCards.AddCard("Eight_Bears", metaCategories, CardComplexity.Simple, CardTemple.Nature,"8 fucking bears!",32,48,description:"Kill this abomination please",cost:3,appearanceBehaviour:appearanceBehaviour, tex:tex);
        }
    }

    public class NewCards
    {
      public static List<CardInfo> cards = new List<CardInfo>();

      // TODO Implement a handler for custom appearanceBehaviour - in particular custom card backs
      // TODO Change parameter order, and function setter call order to make more sense
      // TODO Rename parameters to be more user friendly
      public static void AddCard(string name, List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple, string displayedName, int baseAttack, int baseHealth, string description = "",
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
            tex.name = "portrait_"+name;;
            card.portraitTex = Sprite.Create(tex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
            card.portraitTex.name = "portrait_"+name;
          }
          if(altTex!=null){
            altTex.name = "portrait_"+name;;
            card.alternatePortrait = Sprite.Create(altTex, new Rect(0.0f,0.0f,114.0f,94.0f), new Vector2(0.5f,0.5f));
            card.alternatePortrait.name = "portrait_"+name;
          }
          if(titleGraphic!=null){
            card.titleGraphic = titleGraphic;
          }
          if(pixelTex!=null){
            pixelTex.name = "portrait_"+name;;
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
          NewCards.cards.Add(card);
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
                allData.SetValue(ScriptableObjectLoader<CardInfo>.AllData.Concat(NewCards.cards).ToList());
                Plugin.Log.LogInfo($"Loaded custom cards into data");
            }
        }
    }
}
