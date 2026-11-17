using DiskCardGame;
using InscryptionAPI.Regions;

namespace APIPlugin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Obsolete("Use RegionManager instead", true)]
public class NewRegion
{
    public NewRegion(RegionData region, int tier)
    {
        RegionManager.Add(region, tier);
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member