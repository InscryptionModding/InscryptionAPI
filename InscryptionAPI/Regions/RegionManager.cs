using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Regions;

[HarmonyPatch]
public static class RegionManager
{
    public static readonly ReadOnlyCollection<RegionData> BaseGameRegions = new(Resources.LoadAll<RegionData>("Data/Regions"));
    private static readonly ObservableCollection<Part1RegionData> NewRegions = new();

    public static event Func<List<RegionData>, List<RegionData>> ModifyRegionsList;

    internal static void SyncRegionList()
    {
        var regions = BaseGameRegions.Concat(NewRegions.Select(x => x.Region)).Select(x => (RegionData) UnityEngine.Object.Internal_CloneSingle(x)).ToList();
        //var regions = BaseGameRegions.Concat(NewRegions).ToList();
        AllRegionsCopy = ModifyRegionsList?.Invoke(regions) ?? regions;
    }

    static RegionManager()
    {
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
    [HarmonyPatch(typeof(ScriptableObjectLoader<UnityObject>), nameof(ScriptableObjectLoader<UnityObject>.LoadData))]
    [SuppressMessage("Member Access", "Publicizer001", Justification = "Need to set internal list of regions")]
    private static void RegionLoadPrefix()
    {
        ScriptableObjectLoader<RegionData>.allData = AllRegionsCopy;
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

        // TODO: Change to be more customizable
        if (tier < 3)
        {
            valid.Add(RegionProgression.Instance.regions[tier]);
            valid.AddRange(NewRegions.Where(x => x.Tier == tier).Select(x => x.Region));
            return valid[SeededRandom.Range(0, valid.Count, SaveManager.SaveFile.randomSeed + tier + 1)];
        }
        else
        {
            return RegionProgression.Instance.regions[tier];
        }
        
    }
}