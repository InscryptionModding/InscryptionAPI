using System;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Regions;
using UnityEngine;
using VolumetricFogAndMist;

namespace APIPlugin
{
	[Obsolete("Use RegionManager instead", true)]
	public class CustomRegion
	{
		public CustomRegion(string name)
		{
			this.name = name;
            RegionManager.ModifyRegionsList += ModifyRegions;
		}

        public List<RegionData> ModifyRegions(List<RegionData> regions)
        {
            RegionData regionToModify = regions.FirstOrDefault(r => r.name.Equals(this.name));
            if (regionToModify != null)
                AdjustRegion(regionToModify);

            return regions;
        }

		public RegionData AdjustRegion(RegionData region)
		{
			TypeMapper<CustomRegion, RegionData>.Convert(this, region);
			return region;
		}

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
		public VolumetricFogProfile fogProfile;
		public float? fogAlpha;
		public Texture mapAlbedo;
		public Texture mapEmission;
		public Color? mapEmissionColor;
		public List<GameObject> mapParticlesPrefabs;
	}
}
