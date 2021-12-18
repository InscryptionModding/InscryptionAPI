﻿using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;

namespace API.Patches
{
	[HarmonyPatch(typeof(RegionProgression), "Instance", MethodType.Getter)]
	public class RegionProgression_get_Instance
	{
		public static void Prefix(ref RegionProgression ___instance)
		{
			if (___instance == null)
			{
				RegionProgression official = ResourceBank.Get<RegionProgression>("Data/Map/RegionProgression");

				// If no custom region is defined for the default region, but custom encounters for that region exist, create one
				for (int t = 0; t <= 3; t++)
				{
					if (NewEncounter.encounters.Exists(encounter => (encounter.regionName == official.regions[t].name)))
					{
						if (!CustomRegion.regions.Exists(region => (region.tier == t && region.name == official.regions[t].name)))
						{
							new CustomRegion(official.regions[t].name);
						}
					}
				}

				foreach (CustomRegion region in CustomRegion.regions)
				{
					bool found = false;
					foreach (RegionData officialRegion in official.regions)
					{
						if (officialRegion != null && region.name == officialRegion.name)
						{
							region.AdjustRegion(officialRegion);
							List<NewEncounter> customEncounters = NewEncounter.encounters.FindAll(x => x.regionName == region.name);
							foreach (NewEncounter encounter in customEncounters)
							{
								if (encounter.regular)
								{
									officialRegion.encounters.Add(encounter.encounterBlueprintData);
								}
								if (encounter.bossPrep)
								{
									Plugin.Log.LogInfo($"Loaded custom encounter {encounter.name} as boss prep encounter into region {region.name}");
									officialRegion.bossPrepEncounter = encounter.encounterBlueprintData;
								}
							}
							if (customEncounters.Count > 0)
							{
								Plugin.Log.LogInfo($"Loaded {customEncounters.Count} custom encounters into region {region.name}");
							}
							Plugin.Log.LogInfo($"Loaded modified region {officialRegion.name} into data");
							found = true;
							break;
						}
					}
					if (!found)
					{
						Plugin.Log.LogInfo($"Could not find region {region.name} to modify");
					}
				}

				foreach (NewRegion region in NewRegion.regions)
				{
					official.regions.Add(region.region);

					List<NewEncounter> customEncounters = NewEncounter.encounters.FindAll(x => x.regionName == region.region.name);
					foreach (NewEncounter encounter in customEncounters)
					{
						region.region.encounters.Add(encounter.encounterBlueprintData);
					}

					if (customEncounters.Count > 0)
					{
						Plugin.Log.LogInfo($"Loaded {customEncounters.Count} custom encounters into new custom region {region.region.name}");
					}
				}

				// The final tier is reserved for ascensionFinalRegion
				official.regions.Add(official.ascensionFinalRegion);

				___instance = official;
				Plugin.Log.LogInfo($"Loaded {CustomRegion.regions.Count} new custom regions into data");
			}
		}
	}
}
