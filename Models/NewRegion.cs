using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
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
}
