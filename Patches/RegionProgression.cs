using System.Collections.Generic;
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
}
