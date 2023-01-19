using DiskCardGame;

namespace InscryptionAPI.Regions;

public class Part1RegionData
{
    private int tier;
    private RegionData region;

    public int Tier { get => tier; private set => tier = value; }
    public RegionData Region { get => region; private set => region = value; }
    public int MinTerrain { get; set; }
    public int MaxTerrain { get; set; }
    public bool AllowTerrainOnEnemySide { get; set; }
    public bool AllowTerrainOnPlayerSide { get; set; }
    public bool RemoveDefaultReachTerrain { get; set; }
    public bool DoNotForceReachTerrain { get; set; }
    public bool AllowLockedTerrainCards { get; set; }
    public bool AllowSacrificableTerrainCards { get; set; }

    public Part1RegionData(RegionData region, int tier)
    {
        AllowSacrificableTerrainCards = false;
        AllowLockedTerrainCards = false;
        MinTerrain = -2;
        MaxTerrain = 3;
        AllowTerrainOnEnemySide = true;
        AllowTerrainOnPlayerSide = true;
        RemoveDefaultReachTerrain = false;
        DoNotForceReachTerrain = false;
        this.region = region;
        this.tier = tier;
    }
}