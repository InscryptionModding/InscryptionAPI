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
