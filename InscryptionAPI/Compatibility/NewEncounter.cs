using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Encounters;
using InscryptionAPI.Regions;
using UnityEngine;

namespace APIPlugin;

[Obsolete("Use EncounterManager instead", true)]
public class NewEncounter
{
    private List<RegionData> SyncRegion(List<RegionData> region)
    {
        EncounterBlueprintData curClone = EncounterManager.AllEncountersCopy.FirstOrDefault(e => e.name.Equals(this.encounterBlueprintData.name));
        if (curClone == null)
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Could not attach encounter named {this.encounterBlueprintData.name}");
            return region;
        }

        if (curClone.regionSpecific) // Does this belong only to a single region?
        {
            if (!string.IsNullOrEmpty(this.regionName))
            {
                RegionData target = region.FirstOrDefault(r => r.name.Equals(this.regionName));
                if (target != null)
                {
                    if (!bossPrep && !target.encounters.Any(e => e.name.Equals(curClone.name)))
                    {
                        InscryptionAPIPlugin.Logger.LogDebug($"Attaching encounter named {this.encounterBlueprintData.name} to region {target.name}");
                        target.encounters.Add(curClone);
                    }

                    if (bossPrep)
                        target.bossPrepEncounter = curClone;
                }
            }
        }
        else
        {
            foreach (RegionData target in region)
            {
                if (!target.encounters.Any(e => e.name.Equals(curClone.name)))
                {
                    InscryptionAPIPlugin.Logger.LogDebug($"Attaching encounter named {this.encounterBlueprintData.name} to region {target.name}");
                    target.encounters.Add(curClone);
                }
            }
        }

        return region;
    }

    public NewEncounter(string name, EncounterBlueprintData encounterBlueprintData, string regionName, bool regular, bool bossPrep)
    {
        this.name = name;
        this.encounterBlueprintData = encounterBlueprintData;
        this.encounterBlueprintData.name = name;
        this.regionName = regionName;
        this.bossPrep = bossPrep;
        this.regular = regular;

        EncounterManager.Add(encounterBlueprintData);
        RegionManager.ModifyRegionsList += SyncRegion;
        RegionManager.SyncRegionList();
    }

    public static void Add(string name, string regionName, List<EncounterBlueprintData.TurnModBlueprint> turnMods = null, List<Tribe> dominantTribes = null, List<Ability> redundantAbilities = null, List<CardInfo> unlockedCardPrerequisites = null, bool regionSpecific = true, int minDifficulty = 0, int maxDifficulty = 30, List<CardInfo> randomReplacementCards = null, List<List<EncounterBlueprintData.CardBlueprint>> turns = null, bool regular = true, bool bossPrep = false, int oldPreviewDifficulty = 0)
    {
        EncounterBlueprintData encounterBlueprintData = ScriptableObject.CreateInstance<EncounterBlueprintData>();
        if (turnMods != null)
        {
            encounterBlueprintData.turnMods = turnMods;
        }
        if (dominantTribes != null)
        {
            encounterBlueprintData.dominantTribes = dominantTribes;
        }
        if (redundantAbilities != null)
        {
            encounterBlueprintData.redundantAbilities = redundantAbilities;
        }
        if (unlockedCardPrerequisites != null)
        {
            encounterBlueprintData.unlockedCardPrerequisites = unlockedCardPrerequisites;
        }
        encounterBlueprintData.regionSpecific = regionSpecific;
        encounterBlueprintData.minDifficulty = minDifficulty;
        encounterBlueprintData.maxDifficulty = maxDifficulty;
        if (randomReplacementCards != null)
        {
            encounterBlueprintData.randomReplacementCards = randomReplacementCards;
        }
        if (turns != null)
        {
            encounterBlueprintData.turns = turns;
        }
        encounterBlueprintData.oldPreviewDifficulty = oldPreviewDifficulty;
        new NewEncounter(name, encounterBlueprintData, regionName, regular, bossPrep);
    }

    public static List<NewEncounter> encounters = new List<NewEncounter>();
    public string name;
    public string regionName;
    public bool regular;
    public bool bossPrep;
    public EncounterBlueprintData encounterBlueprintData;
}