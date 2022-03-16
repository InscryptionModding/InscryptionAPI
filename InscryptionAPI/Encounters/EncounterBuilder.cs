using DiskCardGame;

namespace InscryptionAPI.Encounters;

public class EncounterBuilderBlueprintData : EncounterBlueprintData
{
    internal RegionData region;

    public void SetBasic(string regionName, RegionData regionData)
    {
        this.region = regionData;
        this.name = regionName;
        regionData.encounters ??= new();
        regionData.encounters.Add(this);
    }
}

public static class EncounterBuilder
{
    public static RegionData Build(this EncounterBuilderBlueprintData blueprint)
    {
        return blueprint.region;
    }
}
