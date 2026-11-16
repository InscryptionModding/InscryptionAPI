using DiskCardGame;
using InscryptionAPI.Guid;
using System.Reflection;

namespace InscryptionAPI.Regions;

/// <summary>
/// Utility class containing all data needed to create a custom <see cref="RegionData"/> for use in Act 1.
/// </summary>
public class Part1RegionData
{
    /// <summary>
    /// The GUID of the mod adding this region.
    /// </summary>
    public string GUID { get; private set; }
    /// <summary>
    /// The tier for this region. Determines when this region will appear during a run (0 - 3).
    /// </summary>
    public int Tier { get => tier; private set => tier = value; }
    /// <summary>
    /// The description object for this region.
    /// </summary>
    public RegionData Region { get => region; private set => region = value; }
    /// <summary>
    /// The minimum amount of Terrain created on the board at the start of battles in this region.
    /// </summary>
    public int MinTerrain { get; set; }
    /// <summary>
    /// The maximum amount of Terrain cards created on the board at the start of battles in this region.
    /// </summary>
    public int MaxTerrain { get; set; }
    /// <summary>
    /// If created Terrain can appear on the opponent's side of the board.
    /// </summary>
    public bool AllowTerrainOnEnemySide { get; set; }
    /// <summary>
    /// If created Terrain can appear on the player's side of the board.
    /// </summary>
    public bool AllowTerrainOnPlayerSide { get; set; }
    /// <summary>
    /// If Terrain cards should not have the Reach sigil by default.
    /// </summary>
    public bool RemoveDefaultReachTerrain { get; set; }
    /// <summary>
    /// If Terrain cards should not be forced to have the Reach sigil.
    /// </summary>
    public bool DoNotForceReachTerrain { get; set; }
    /// <summary>
    /// If created Terain cards can pull from the pool of locked (not yet encountered) cards.
    /// </summary>
    public bool AllowLockedTerrainCards { get; set; }
    /// <summary>
    /// If created Terrain cards should pull from the pool of sacrificable cards.
    /// </summary>
    public bool AllowSacrificableTerrainCards { get; set; }
    /// <summary>
    /// If this region can appear randomly during a run or should only appear at its given tier.
    /// </summary>
    public bool CanAppearRandomly { get; set; }

    private int tier;
    private RegionData region;

    /// <summary>
    /// Creates a new instance of <see cref="Part1RegionData"/> and registers it with the <see cref="TypeManager"/>.
    /// </summary>
    /// <param name="region">The description object for the region.</param>
    /// <param name="tier">The tier of the region.</param>
    public Part1RegionData(RegionData region, int tier)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        GUID = TypeManager.GetModIdFromCallstack(callingAssembly);

        AllowSacrificableTerrainCards = false;
        AllowLockedTerrainCards = false;
        MinTerrain = -2;
        MaxTerrain = 3;
        AllowTerrainOnEnemySide = true;
        AllowTerrainOnPlayerSide = true;
        RemoveDefaultReachTerrain = false;
        DoNotForceReachTerrain = false;
        CanAppearRandomly = true;
        this.region = region;
        this.tier = tier;
    }
}