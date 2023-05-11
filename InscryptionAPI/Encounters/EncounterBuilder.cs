using DiskCardGame;

namespace InscryptionAPI.Encounters;

public class EncounterBuilderBlueprintData : EncounterBlueprintData
{
    internal RegionData region;

    public void SetBasic(string name, RegionData region)
    {
        this.region = region;
        this.name = name;
        region.encounters ??= new();
        region.encounters.Add(this);
    }
}

public static class EncounterBuilder
{
    public static RegionData Build(this EncounterBuilderBlueprintData blueprint)
    {
        return blueprint.region;
    }
}