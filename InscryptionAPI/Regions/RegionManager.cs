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

    [HarmonyPatch(typeof(EncounterBuilder), nameof(EncounterBuilder.BuildTerrainCondition))]
    [HarmonyPrefix]
    private static bool ApplyTerrainCustomization(ref EncounterData.StartCondition __result, ref bool reachTerrainOnPlayerSide, int randomSeed)
    {
        var customregion = NewRegions.ToList().Find(x => x.Region == RunState.CurrentMapRegion);
        if(customregion != null)
        {
            reachTerrainOnPlayerSide &= !customregion.DoNotForceReachTerrain;
            __result = new EncounterData.StartCondition();
            int num = SeededRandom.Range(customregion.MinTerrain, customregion.MaxTerrain, randomSeed++);
            int num2 = 0;
            int num3 = 0;
            for (int i = 0; i < num; i++)
            {
                bool flag;
                if (customregion.AllowTerrainOnEnemySide && customregion.AllowTerrainOnPlayerSide)
                {
                    flag = SeededRandom.Bool(randomSeed++);
                    if (flag && num2 == 1)
                    {
                        flag = false;
                    }
                    else if (!flag && num3 == 1)
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = customregion.AllowTerrainOnPlayerSide;
                }
                CardInfo[] array = flag ? __result.cardsInPlayerSlots : __result.cardsInOpponentSlots;
                CardInfo[] array2 = flag ? __result.cardsInOpponentSlots : __result.cardsInPlayerSlots;
                int num4 = SeededRandom.Range(0, array.Length, randomSeed++);
                bool availableSpace = false;
                for (int j = 0; j < 4; j++)
                {
                    if (array[j] == null && array2[j] == null)
                    {
                        availableSpace = true;
                    }
                }
                if (!availableSpace)
                {
                    break;
                }
                while (array[num4] != null || array2[num4] != null)
                {
                    num4 = SeededRandom.Range(0, array.Length, randomSeed++);
                }
                if (flag && reachTerrainOnPlayerSide)
                {
                    CardInfo cardInfo = RunState.CurrentMapRegion.terrainCards.Find((CardInfo x) => x.HasAbility(Ability.Reach));
                    if (cardInfo == null && !customregion.RemoveDefaultReachTerrain)
                    {
                        cardInfo = CardLoader.GetCardByName("Tree");
                    }
                    if (cardInfo != null)
                    {
                        array[num4] = CardLoader.GetCardByName(cardInfo.name);
                    }
                }
                else
                {
                    List<CardInfo> list = RunState.CurrentMapRegion.terrainCards.FindAll((CardInfo x) => (ConceptProgressionTree.Tree.CardUnlocked(x, true) || customregion.AllowLockedTerrainCards) &&
                        (x.traits.Contains(Trait.Terrain) || customregion.AllowSacrificableTerrainCards));
                    if (list.Count > 0)
                    {
                        array[num4] = CardLoader.GetCardByName(list[SeededRandom.Range(0, list.Count, randomSeed++)].name);
                    }
                }
                if (flag)
                {
                    num2++;
                }
                else
                {
                    num3++;
                }
            }
            return false;
        }
        return true;
    }
}