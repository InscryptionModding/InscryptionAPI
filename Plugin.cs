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
        private const string PluginVersion = "1.0.0.0";

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
                                List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D tex = null, Texture2D altTex = null, Texture titleGraphics = null, Texture2D pixelTex = null,
                                GameObject animatedPortrait = null)
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
            card.animatedPortrait = animatedPortrait;
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

    [HarmonyPatch(typeof(CardLoader), "GetCardByName", new Type[] {typeof(string)})]
    public class CardLoader_GetCardByName
    {
        public static bool Prefix(string name, ref CardInfo __result)
        {
            if (NewCards.cards.Exists((CardInfo x) => x.name == name))
            {
                __result = Traverse.Create<CardLoader>().Method("Clone", new object[] {NewCards.cards.Find((CardInfo x) => x.name == name)}).GetValue<CardInfo>();
                Plugin.Log.LogInfo($"Loaded custom card by name!");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CardLoader), "GetPixelCards")]
    public class CardLoader_GetPixelCards
    {
        public static void Postfix(ref List<CardInfo> __result)
        {
            __result = __result.Concat(NewCards.cards.FindAll((CardInfo x) => x.pixelPortrait != null)).ToList();
            Plugin.Log.LogInfo("Loaded custom pixel cards");
        }
    }

    [HarmonyPatch(typeof(CardLoader), "GetPureRandomCard")]
    public class CardLoader_GetPureRandomCard
    {
        public static bool Prefix(ref CardInfo __result)
        {
            List<CardInfo> allCards = ScriptableObjectLoader<CardInfo>.AllData.Concat(NewCards.cards).ToList();
            __result = Traverse.Create<CardLoader>().Method("Clone", new object[] {allCards[UnityEngine.Random.Range(0, allCards.Count)]}).GetValue<CardInfo>();
            Plugin.Log.LogInfo("Added custom cards to pure random card pool");
            return false;
        }
    }

    [HarmonyPatch(typeof(CardLoader), "GetRandomRareCard")]
    public class CardLoader_GetRandomRareCard
    {
        public static bool Prefix(CardTemple temple, ref CardInfo __result)
        {
            List<CardInfo> list = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == temple);
            __result = Traverse.Create<CardLoader>().Method("Clone", new object[] {list.Concat(NewCards.cards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == temple))}).GetValue<CardInfo>();
            Plugin.Log.LogInfo("Added custom cards to random rare card pool");
            return false;
        }
    }

    [HarmonyPatch(typeof(CardLoader), "GetUnlockedCards", new Type[] {typeof(CardMetaCategory), typeof(CardTemple)})]
    public class CardLoader_GetUnlockedCards
    {
        public static void Postfix(CardMetaCategory category, CardTemple temple, ref List<CardInfo> __result)
        {
            __result = __result.Concat(Traverse.Create<CardLoader>().Method("RemoveDeckSingletonsIfInDeck", new object[] {NewCards.cards.FindAll((CardInfo x) => x.metaCategories.Contains(category) && x.temple == temple && ConceptProgressionTree.Tree.CardUnlocked(x, false))}).GetValue<List<CardInfo>>()).ToList();
            Plugin.Log.LogInfo("Added custom cards to unlocked card pool");
        }
    }

    [HarmonyPatch(typeof(CardLoader), "LearnedCards", MethodType.Getter)]
    public class CardLoader_get_LearnedCards
    {
        public static void Postfix(ref List<CardInfo> __result)
        {
            __result = __result.Concat(NewCards.cards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.ChoiceNode) && ProgressionData.LearnedCard(x))).ToList();
            Plugin.Log.LogInfo("Added custom cards to learned card pool");
        }
    }

    [HarmonyPatch(typeof(DrawRandomCardOnDeath), "CardToDraw", MethodType.Getter)]
    public class DrawRandomCardOnDeath_get_CardToDraw
    {
        public static bool Prefix(ref CardInfo __result, DrawRandomCardOnDeath __instance)
        {
            Traverse.Create(__instance).Field("wasGoodFish").SetValue(false);
            if (!Traverse.Create(__instance).Field("IsAngler").GetValue<bool>())
            {
              List<CardInfo> list = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Part3Random));
              list = list.Concat(NewCards.cards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Part3Random))).ToList();
              __result = list[SeededRandom.Range(0, list.Count, Traverse.Create(new TriggerReceiver()).Method("GetRandomSeed").GetValue<int>())];
              Plugin.Log.LogInfo("Added custom cards to card on death card pool");
              return false;
            }
            return true;
        }
    }
}
