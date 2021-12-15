using DiskCardGame;
using System.Collections.Generic;
using System.Linq;
using APIPlugin;
using System;

namespace API.Utils
{
    public class RegionUtils
    {
        public static int GetRandomRegionFromTier(int tier)
        {
            List<int> regions = new List<int>();
            if (tier < 4)
            {
                regions.Add(tier);
            }
            List<NewRegion> newRegions = NewRegion.regions;
            regions.AddRange(Enumerable.Range(4, newRegions.Count).Where(i => newRegions[i - 4].tier == tier));
            if (regions.Count == 0)
            {
                Plugin.Log.LogError($"No regions have been defined for tier {tier}");
                return 3;
            }
            return regions[SeededRandom.Range(0, regions.Count, SaveManager.SaveFile.GetCurrentRandomSeed())];
        }

        public static int TrueTier()
        {
            return RunState.Run.regionTier < 4 ? RunState.Run.regionTier + 1 : NewRegion.regions[RunState.Run.regionTier - 4].tier + 1;
        }
    }
}
