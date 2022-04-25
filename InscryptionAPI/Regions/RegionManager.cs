using System.Collections.ObjectModel;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Regions;

[HarmonyPatch]
public static class RegionManager
{
    public static readonly ReadOnlyCollection<RegionData> BaseGameRegions = new(Resources.LoadAll<RegionData>("Data"));
    private static readonly ObservableCollection<Part1RegionData> NewRegions = new();

    public static event Func<List<RegionData>, List<RegionData>> ModifyRegionsList;

    internal static void ReplaceBlueprintInCore(EncounterBlueprintData newData)
    {
        foreach (RegionData region in NewRegions.Select(s => s.Region).Concat(BaseGameRegions))
        {
            if (region.encounters != null)
                for (int i = 0; i < region.encounters.Count; i++)
                    if (region.encounters[i].name.Equals(newData.name))
                        region.encounters[i] = newData;
            
            if (region.bossPrepEncounter != null)
                if (region.bossPrepEncounter.name.Equals(newData.name))
                    region.bossPrepEncounter = newData;
        }
    }

    private static RegionData CloneRegion(this RegionData data)
    {
        RegionData retval = (RegionData) UnityEngine.Object.Internal_CloneSingle(data);
        retval.name = data.name;
        return retval;
    }

    public static void SyncRegionList()
    {
        var regions = BaseGameRegions.Concat(NewRegions.Select(x => x.Region)).Select(x => x.CloneRegion()).ToList();
        AllRegionsCopy = ModifyRegionsList?.Invoke(regions) ?? regions;

        // Sync the regions to the RegionProgression
        for (int i = 0; i < RegionProgression.Instance.regions.Count; i++)
        {
            RegionData replacementRegion = AllRegionsCopy.FirstOrDefault(rd => rd.name.Equals(RegionProgression.Instance.regions[i].name));
            if (replacementRegion != null)
                RegionProgression.Instance.regions[i] = replacementRegion;
        }

        RegionData ascensionFinalRegion = AllRegionsCopy.FirstOrDefault(rd => rd.name.Equals(RegionProgression.Instance.ascensionFinalRegion.name));
        RegionProgression.Instance.ascensionFinalRegion = ascensionFinalRegion;

        RegionData specialFinalRegion = AllRegionsCopy.FirstOrDefault(rd => rd.name.Equals(RegionProgression.Instance.ascensionFinalBossRegion.name));
        RegionProgression.Instance.ascensionFinalBossRegion = specialFinalRegion;
    }

    static RegionManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(RegionData))
            {
                ScriptableObjectLoader<RegionData>.allData = AllRegionsCopy;
            }
        };
        NewRegions.CollectionChanged += static (_, _) =>
        {
            SyncRegionList();
        };
    }

    public static List<RegionData> AllRegionsCopy { get; private set; } = BaseGameRegions.ToList();

    public static void Add(RegionData newRegion, int tier) {
        if (!NewRegions.Select(x => x.Region).Contains(newRegion))
        {
            NewRegions.Add(new Part1RegionData(newRegion, tier));
        }
    }
    public static void Remove(RegionData region) => NewRegions.Remove(NewRegions.Where(x => x.Region == region).First());

    public static RegionData New(string name, int tier, bool addToPool = true)
    {
        RegionData retval = ScriptableObject.CreateInstance<RegionData>();
        retval.name = name;

        if (addToPool)
            Add(retval, tier);

        return retval;
    }

    /// <summary>
    /// Keeps encounters, cards, tribes, consumables
    /// </summary>
    public static RegionData FromTierFull(string name, int originalTier, int newTier, bool addToPool = true)
    {
        RegionProgression copy = ResourceBank.Get<RegionProgression>("Data/Map/RegionProgression");
        RegionData retval = (RegionData) RegionData.Internal_CloneSingle(copy.regions[originalTier]);
        retval.name = name;

        if (addToPool)
            Add(retval, newTier);

        return retval;
    }

    /// <summary>
    /// Keeps encounters, cards, tribes, consumables
    /// </summary>
    public static RegionData FromTierFull(string name, int originalTier, bool addToPool = true)
    {
        return FromTierFull(name, originalTier, originalTier, addToPool);
    }

    /// <summary>
    /// Removes all encounters, cards, tribes, consumables
    /// </summary>
    public static RegionData FromTierBasic(string name, int originalTier, int newTier, bool addToPool = true)
    {
        RegionData retval = FromTierFull(name, originalTier, newTier, addToPool);

        retval.encounters = new();
        retval.bossPrepCondition = null;
        retval.consumableItems = new();
        retval.dominantTribes = new();
        retval.likelyCards = new();
        retval.terrainCards = new();

        return retval;
    }

    /// <summary>
    /// Removes all encounters, cards, tribes, consumables
    /// </summary>
    public static RegionData FromTierBasic(string name, int originalTier, bool addToPool = true)
    {
        return FromTierBasic(name, originalTier, originalTier, addToPool);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunState), "CurrentMapRegion", MethodType.Getter)]
    private static bool CurrentMapPrefix(RunState __instance, ref RegionData __result)
    {
        if (SaveManager.SaveFile.IsPart3 || SaveManager.SaveFile.IsGrimora)
        {
            __result = ResourceBank.Get<RegionData>("Data/Map/Regions/!TEST_PART3");
            return false;
        }
        if (SaveFile.IsAscension)
        {
            if (RunState.Run.regionTier == RegionProgression.Instance.regions.Count - 1)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.FinalBoss))
                    __result = RegionProgression.Instance.ascensionFinalBossRegion;
                else
                    __result = RegionProgression.Instance.ascensionFinalRegion;
                return false;
            }
            __result = GetRandomRegionFromTier(RunState.Run.regionOrder[RunState.Run.regionTier]);
            return false;
        }
        __result = GetRandomRegionFromTier(RunState.Run.regionTier);
        return false;
    }

    public static RegionData GetRandomRegionFromTier(int tier)
    {
        List<RegionData> valid = new();
        if (RegionProgression.Instance.regions.Count > tier)
        {
            valid.Add(RegionProgression.Instance.regions[tier]);
        }
        valid.AddRange(NewRegions.Where(x => x.Tier == tier).Select(x => x.Region));
        if (valid.Count <= 0)
        {
            return null;
        }
        if (valid.Count == 1)
        {
            return valid[0];
        }
        int randomseed = 0;
        if (SaveManager.SaveFile != null && RunState.Run != null && (!SaveFile.IsAscension || AscensionSaveData.Data != null))
        {
            randomseed = SaveManager.SaveFile.randomSeed + (SaveFile.IsAscension ? AscensionSaveData.Data.currentRunSeed : (SaveManager.SaveFile.pastRuns.Count * 1000)) + (RunState.Run.regionTier + 1) * 100;
        }
        return valid[SeededRandom.Range(0, valid.Count, randomseed)];
    }
}