using DiskCardGame;

namespace InscryptionAPI.Encounters;

/// <summary>
/// Utility class that builds a basic <see cref="EncounterBlueprintData"/> and associates it with the region it appears in.
/// </summary>
public class EncounterBuilderBlueprintData : EncounterBlueprintData
{
    internal RegionData region;

    /// <summary>
    /// Initialises the basic data of this blueprint and the given region.
    /// </summary>
    /// <param name="name">Name of this blueprint.</param>
    /// <param name="region">The <see cref="RegionData"/> that this encounter appears in.</param>
    public void SetBasic(string name, RegionData region)
    {
        this.region = region;
        this.name = name;
        region.encounters ??= new();
        region.encounters.Add(this);
    }

    /// <summary>
    /// Returns the <see cref="RegionData"/> of this blueprint.
    /// </summary>
    /// <returns>The <see cref="RegionData"/> for this blueprint.</returns>
    public RegionData Build() => region;
}

/// <summary>
/// Utility class that adds extension methods for aiding in constructing encounters.
/// </summary>
[Obsolete("Not used by the API.")]
public static class EncounterBuilder
{
    /// <summary>
    /// Returns the <see cref="RegionData"/> of this builder blueprint.
    /// </summary>
    /// <param name="blueprint">The <see cref="EncounterBuilderBlueprintData"/> to access.</param>
    /// <returns>The <see cref="RegionData"/> for this blueprint.</returns>
    public static RegionData Build(this EncounterBuilderBlueprintData blueprint)
    {
        return blueprint.region;
    }
}