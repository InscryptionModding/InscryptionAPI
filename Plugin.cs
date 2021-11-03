using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;

namespace APIPlugin
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "cyantist.inscryption.api";
        private const string PluginName = "API";
        private const string PluginVersion = "1.7.0.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
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
        [IgnoreMapping]
        public Texture2D tex;
        [IgnoreMapping]
        public Texture2D altTex;
        public Texture titleGraphic;
        [IgnoreMapping]
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
                card.pixelPortrait = Sprite.Create(pixelTex, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
                card.pixelPortrait.name = "portrait_" + name;
            }
            Plugin.Log.LogInfo($"Adjusted default card {name}!");
            return card;
        }
    }

    public class NewCard
    {
        public static List<CardInfo> cards = new List<CardInfo>();

        public NewCard(CardInfo card)
        {
            NewCard.cards.Add(card);
            Plugin.Log.LogInfo($"Loaded custom card {card.name}!");
        }

        // TODO Implement a handler for custom appearanceBehaviour - in particular custom card backs
        // TODO Change parameter order, and function setter call order to make more sense
        // TODO Rename parameters to be more user friendly
        public NewCard(string name, List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple, string displayedName, int baseAttack, int baseHealth,
            string description = "",
            bool hideAttackAndHealth = false, int cost = 0, int bonesCost = 0, int energyCost = 0, List<GemType> gemsCost = null, SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
            List<Tribe> tribes = null, List<Trait> traits = null, List<SpecialTriggeredAbility> specialAbilities = null, List<Ability> abilities = null, EvolveParams evolveParams = null,
            string defaultEvolutionName = "", TailParams tailParams = null, IceCubeParams iceCubeParams = null, bool flipPortraitForStrafe = false, bool onePerDeck = false,
            List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D tex = null, Texture2D altTex = null, Texture titleGraphic = null, Texture2D pixelTex = null,
            GameObject animatedPortrait = null, List<Texture> decals = null)
        {
            CardInfo card = ScriptableObject.CreateInstance<CardInfo>();
            card.metaCategories = metaCategories;
            card.cardComplexity = cardComplexity;
            card.temple = temple;
            card.displayedName = displayedName;
            card.baseAttack = baseAttack;
            card.baseHealth = baseHealth;
            if (description != "")
            {
                card.description = description;
            }
            card.cost = cost;
            card.bonesCost = bonesCost;
            card.energyCost = energyCost;
            if (gemsCost is not null)
            {
                card.gemsCost = gemsCost;
            }
            card.specialStatIcon = specialStatIcon;
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
            if (abilities is not null)
            {
                card.abilities = abilities;
            }
            if (evolveParams is not null)
            {
                card.evolveParams = evolveParams;
            }
            card.defaultEvolutionName = defaultEvolutionName;
            if (tailParams is not null)
            {
                card.tailParams = tailParams;
            }
            if (iceCubeParams is not null)
            {
                card.iceCubeParams = iceCubeParams;
            }
            card.flipPortraitForStrafe = flipPortraitForStrafe;
            card.onePerDeck = onePerDeck;
            card.hideAttackAndHealth = hideAttackAndHealth;
            if (tex is not null)
            {
                tex.name = "portrait_" + name;
                tex.filterMode = FilterMode.Point;
                card.portraitTex = Sprite.Create(tex, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
                card.portraitTex.name = "portrait_" + name;
            }
            if (altTex is not null)
            {
                altTex.name = "portrait_" + name;
                altTex.filterMode = FilterMode.Point;
                card.alternatePortrait = Sprite.Create(altTex, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
                card.alternatePortrait.name = "portrait_" + name;
            }
            if (titleGraphic is not null)
            {
                card.titleGraphic = titleGraphic;
            }
            if (pixelTex is not null)
            {
                pixelTex.name = "portrait_" + name;
                pixelTex.filterMode = FilterMode.Point;
                card.pixelPortrait = Sprite.Create(pixelTex, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
                card.pixelPortrait.name = "portrait_" + name;
            }
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
            card.name = name;
            NewCard.cards.Add(card);
            Plugin.Log.LogInfo($"Loaded custom card {name}!");
        }

    }

    public class CustomRegion
    {
        public static List<CustomRegion> regions = new List<CustomRegion>();
        public string name;
        public int? tier;
        private List<CardInfo> terrainCards;
        private List<ConsumableItemData> consumableItems;
        private List<EncounterBlueprintData> encounters;
        private List<Opponent.Type> bosses;
        private List<CardInfo> likelyCards;
        private List<Tribe> dominantTribes;
        private PredefinedNodes predefinedNodes;
        private EncounterBlueprintData bossPrepEncounter;
        private StoryEventCondition bossPrepCondition;
        private List<ScarceSceneryEntry> scarceScenery;
        private List<FillerSceneryEntry> fillerScenery;
        private PredefinedScenery predefinedScenery;
        private string ambientLoopId;
        private bool? silenceCabinAmbience;
        private Color? boardLightColor;
        private Color? cardsLightColor;
        private bool? dustParticlesDisabled;
        private bool? fogEnabled;
        private VolumetricFogAndMist.VolumetricFogProfile fogProfile;
        private float? fogAlpha;
        private Texture mapAlbedo;
        private Texture mapEmission;
        private Color? mapEmissionColor;
        private List<GameObject> mapParticlesPrefabs;

        public CustomRegion(string name, int? tier = null, List<CardInfo> terrainCards = null, List<ConsumableItemData> consumableItems = null, List<EncounterBlueprintData> encounters = null,
                            List<Opponent.Type> bosses = null, List<CardInfo> likelyCards = null, List<Tribe> dominantTribes = null, PredefinedNodes predefinedNodes = null,
                            EncounterBlueprintData bossPrepEncounter = null, StoryEventCondition bossPrepCondition = null, List<ScarceSceneryEntry> scarceScenery = null,
                            List<FillerSceneryEntry> fillerScenery = null, PredefinedScenery predefinedScenery = null, string ambientLoopId = "", bool? silenceCabinAmbience = null,
                            Color? boardLightColor = null, Color? cardsLightColor = null, bool? dustParticlesDisabled = null, bool? fogEnabled = null, VolumetricFogAndMist.VolumetricFogProfile fogProfile = null,
                            float? fogAlpha = null, Texture mapAlbedo = null, Texture mapEmission = null, Color? mapEmissionColor = null, List<GameObject> mapParticlesPrefabs = null)
        {
            this.name = name;
            this.tier = tier;
            this.terrainCards = terrainCards;
            this.consumableItems = consumableItems;
            this.encounters = encounters;
            this.bosses = bosses;
            this.likelyCards = likelyCards;
            this.dominantTribes = dominantTribes;
            this.predefinedNodes = predefinedNodes;
            this.bossPrepEncounter = bossPrepEncounter;
            this.bossPrepCondition = bossPrepCondition;
            this.scarceScenery = scarceScenery;
            this.fillerScenery = fillerScenery;
            this.predefinedScenery = predefinedScenery;
            this.ambientLoopId = ambientLoopId;
            this.silenceCabinAmbience = silenceCabinAmbience;
            this.boardLightColor = boardLightColor;
            this.cardsLightColor = cardsLightColor;
            this.dustParticlesDisabled = dustParticlesDisabled;
            this.fogEnabled = fogEnabled;
            this.fogProfile = fogProfile;
            this.fogAlpha = fogAlpha;
            this.mapAlbedo = mapAlbedo;
            this.mapEmission = mapEmission;
            this.mapEmissionColor = mapEmissionColor;
            this.mapParticlesPrefabs = mapParticlesPrefabs;
            CustomRegion.regions.Add(this);
        }

        public RegionData AdjustRegion(RegionData region)
        {
            TypeMapper<CustomRegion, RegionData>.Convert(this, region);
            Plugin.Log.LogInfo($"Adjusted default region {name}!");
            return region;
        }

    }

    public class NewRegion
    {
        public static List<NewRegion> regions = new List<NewRegion>();
        public RegionData region;
        public int tier;

        public NewRegion(RegionData region, int tier){
            this.region = region;
            this.tier = tier;
            NewRegion.regions.Add(this);
            Plugin.Log.LogInfo($"Loaded custom region {region.name}!");
        }
    }

    public class NewAbility
    {
        public static List<NewAbility> abilities = new List<NewAbility>();
        public Ability ability;
        public AbilityInfo info;
        public Type abilityBehaviour;
        public Texture tex;

        public NewAbility(AbilityInfo info, Type abilityBehaviour, Texture tex)
        {
            this.ability = (Ability) 100 + NewAbility.abilities.Count;
            info.ability = this.ability;
            this.info = info;
            this.abilityBehaviour = abilityBehaviour;
            this.tex = tex;
            NewAbility.abilities.Add(this);
            Plugin.Log.LogInfo($"Loaded custom ability {info.rulebookName}!");
        }
    }

    [HarmonyPatch(typeof(LoadingScreenManager), "LoadGameData")]
    public class LoadingScreenManager_LoadGameData
    {
        public static void Prefix()
        {
            if (ScriptableObjectLoader<CardInfo>.allData == null)
            {
                List<CardInfo> official = ScriptableObjectLoader<CardInfo>.AllData;
                foreach (CustomCard card in CustomCard.cards)
                {
                    int index = official.FindIndex((CardInfo x) => x.name == card.name);
                    if (index == -1)
                    {
                        Plugin.Log.LogInfo($"Could not find card {card.name} to modify");
                    }
                    else
                    {
                        official[index] = card.AdjustCard(official[index]);
                        Plugin.Log.LogInfo($"Loaded modified {card.name} into data");
                    }
                }
                ScriptableObjectLoader<CardInfo>.allData = official.Concat(NewCard.cards).ToList();
                Plugin.Log.LogInfo($"Loaded custom cards into data");
            }
            if (ScriptableObjectLoader<AbilityInfo>.allData == null)
            {
                List<AbilityInfo> official = ScriptableObjectLoader<AbilityInfo>.AllData;
                foreach (NewAbility newAbility in NewAbility.abilities)
                {
                    official.Add(newAbility.info);
                }
                ScriptableObjectLoader<AbilityInfo>.allData = official;
                Plugin.Log.LogInfo($"Loaded custom abilities into data");
            }
        }
    }

    [HarmonyPatch(typeof(ChapterSelectMenu), "OnChapterConfirmed")]
    public class ChapterSelectMenu_OnChapterConfirmed
    {
        public static void Prefix()
        {
            if (ScriptableObjectLoader<CardInfo>.allData == null)
            {
                List<CardInfo> official = ScriptableObjectLoader<CardInfo>.AllData;
                foreach (CustomCard card in CustomCard.cards)
                {
                    int index = official.FindIndex((CardInfo x) => x.name == card.name);
                    if (index == -1)
                    {
                        Plugin.Log.LogInfo($"Could not find card {card.name} to modify");
                    }
                    else
                    {
                        official[index] = card.AdjustCard(official[index]);
                        Plugin.Log.LogInfo($"Loaded modified {card.name} into data");
                    }
                }
                ScriptableObjectLoader<CardInfo>.allData = official.Concat(NewCard.cards).ToList();
                Plugin.Log.LogInfo($"Loaded custom cards into data");
            }
        }
    }

    [HarmonyPatch(typeof(RegionProgression), "Instance", MethodType.Getter)]
    public class RegionProgression_get_Instance
    {
        public static void Prefix(ref RegionProgression ___instance)
        {
            if(___instance == null)
            {
                RegionProgression official = ResourceBank.Get<RegionProgression>("Data/Map/RegionProgression");
                foreach(CustomRegion region in CustomRegion.regions){
                    int tier = 0;
                    bool found = false;
                    foreach(List<RegionData> regions in official.regions){
                        int index = regions.FindIndex((RegionData x) => x.name == region.name);
                        if (index != -1)
                        {
                            if (region.tier == null || (int)region.tier == tier)
                            {
                                official.regions[tier][index] = region.AdjustRegion(regions[index]);
                            }
                            else
                            {
                                RegionData officialRegion = regions[index];
                                official.regions[tier].Remove(officialRegion);
                                while ((int)region.tier >= official.regions.Count)
                                {
                                    official.regions.Add(new List<RegionData>());
                                }
                                official.regions[(int)region.tier].Add(region.AdjustRegion(officialRegion));
                            }
                            found = true;
                            Plugin.Log.LogInfo($"Loaded modified {region.name} into data");
                        }
                        tier++;
                    }
                    if (!found)
                    {
                        Plugin.Log.LogInfo($"Could not find region {region.name} to modify");
                    }
                }

                foreach(NewRegion region in NewRegion.regions){
                    while (region.tier >= official.regions.Count)
                    {
                        official.regions.Add(new List<RegionData>());
                    }
                    official.regions[region.tier].Add(region.region);
                }
                ___instance = official;
                Plugin.Log.LogInfo($"Loaded custom regions into data");
            }
        }
    }

    [HarmonyPatch(typeof(CardTriggerHandler), "AddAbility", new Type[] {typeof(Ability)})]
    public class CardTriggerHandler_AddAbility
    {
        public static bool Prefix(Ability ability, CardTriggerHandler __instance)
        {
            if ((int)ability < 99)
            {
                return true;
            }
            if ((!__instance.triggeredAbilities.Exists((Tuple<Ability, AbilityBehaviour> x) => x.Item1 == ability) || AbilitiesUtil.GetInfo(ability).canStack) && !AbilitiesUtil.GetInfo(ability).passive)
            {
                NewAbility newAbility = NewAbility.abilities.Find((NewAbility x) => x.ability == ability);
                Type type = newAbility.abilityBehaviour;
                Component baseC = (Component)__instance;
                AbilityBehaviour item = baseC.gameObject.GetComponent(type) as AbilityBehaviour;
          			if (item == null)
          			{
                    item = baseC.gameObject.AddComponent(type) as AbilityBehaviour;
                }
                __instance.triggeredAbilities.Add(new Tuple<Ability, AbilityBehaviour>(ability, item));
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(AbilitiesUtil), "LoadAbilityIcon", new Type[] {typeof(string), typeof(bool), typeof(bool)})]
    public class AbilitiesUtil_LoadAbilityIcon
    {
        public static bool Prefix(string abilityName, CardTriggerHandler __instance, ref Texture __result)
        {
            int ability = 0;
            if (!int.TryParse(abilityName, out ability))
            {
                return true;
            }
            NewAbility newAbility = NewAbility.abilities.Find((NewAbility x) => x.ability == (Ability)ability);
            __result = newAbility.tex;
            return false;
        }
    }

    [HarmonyPatch(typeof(AbilitiesUtil), "GetAbilities", new Type[] {typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(AbilityMetaCategory)})]
    public class AbilitiesUtil_GetAbilities
    {
        public static void Postfix(bool learned, bool opponentUsable, int minPower, int maxPower, AbilityMetaCategory categoryCriteria, ref List<Ability> __result)
        {
            foreach (NewAbility newAbility in NewAbility.abilities)
            {
                AbilityInfo info = newAbility.info;
                bool flag = !opponentUsable || info.opponentUsable;
                bool flag2 = info.powerLevel >= minPower && info.powerLevel <= maxPower;
                bool flag3 = info.metaCategories.Contains(categoryCriteria);
                bool flag4 = true;
                if (learned)
                {
                  flag4 = ProgressionData.LearnedAbility(info.ability);
                }
                if (flag && flag2 && flag3 && flag4)
                {
                    __result.Add(newAbility.ability);
                }
            }
        }
    }

    [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] {typeof(AbilityMetaCategory)})]
    public class RuleBookInfo_ConstructPageData
    {
        public static void Postfix(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
        {
            foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges) {
                if (pageRangeInfo.type == PageRangeType.Abilities)
                {
                    List<int> customAbilities = NewAbility.abilities.Select(x => (int)x.ability).ToList();
                    int min = customAbilities.AsQueryable().Min();
                    int max = customAbilities.AsQueryable().Max();
                    PageRangeInfo pageRange = pageRangeInfo;
                    Func<int, bool> doAddPageFunc;
                    doAddPageFunc = (int index) => customAbilities.Contains(index) && AbilitiesUtil.GetInfo((Ability)index).metaCategories.Contains(metaCategory);
                    __result.AddRange(__instance.ConstructPages(pageRange, max+1, min, doAddPageFunc, new Action<RuleBookPageInfo, PageRangeInfo, int>(__instance.FillAbilityPage), Localization.Translate("APPENDIX XII, SUBSECTION I - MOD ABILITIES {0}")));
                }
            }
        }
    }
}
