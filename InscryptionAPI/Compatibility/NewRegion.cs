using DiskCardGame;
using InscryptionAPI.Regions;

namespace APIPlugin
{
    [Obsolete("Use RegionManager instead", true)]
	public class NewRegion
	{
		public NewRegion(RegionData region, int tier)
		{
            RegionManager.Add(region, tier);
		}
	}
}
