using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;
#pragma warning disable 169

namespace APIPlugin
{
	public class CustomRegion
	{
		public static List<CustomRegion> regions = new List<CustomRegion>();

		public string name;
		public int? tier;
		public List<CardInfo> terrainCards;
		public List<ConsumableItemData> consumableItems;
		public List<EncounterBlueprintData> encounters;
		public List<Opponent.Type> bosses;
		public List<CardInfo> likelyCards;
		public List<Tribe> dominantTribes;
		public PredefinedNodes predefinedNodes;
		public EncounterBlueprintData bossPrepEncounter;
		public StoryEventCondition bossPrepCondition;
		public List<ScarceSceneryEntry> scarceScenery;
		public List<FillerSceneryEntry> fillerScenery;
		public PredefinedScenery predefinedScenery;
		public string ambientLoopId;
		public bool? silenceCabinAmbience;
		public Color? boardLightColor;
		public Color? cardsLightColor;
		public bool? dustParticlesDisabled;
		public bool? fogEnabled;
		public VolumetricFogAndMist.VolumetricFogProfile fogProfile;
		public float? fogAlpha;
		public Texture mapAlbedo;
		public Texture mapEmission;
		public Color? mapEmissionColor;
		public List<GameObject> mapParticlesPrefabs;

		public CustomRegion(string name)
		{
			this.name = name;
			CustomRegion.regions.Add(this);
		}

		public RegionData AdjustRegion(RegionData region)
		{
			TypeMapper<CustomRegion, RegionData>.Convert(this, region);
			Plugin.Log.LogInfo($"Adjusted default region {name}!");
			return region;
		}

	}
}
