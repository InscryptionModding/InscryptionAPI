using DiskCardGame;

namespace InscryptionAPI.Regions;

public class Part1RegionData
{

    public int Tier { get; private set; }

    public RegionData Region { get; private set; }

    public Part1RegionData(RegionData region, int tier)
    {
        this.Region = region;
        this.Tier = tier;
    }
}
