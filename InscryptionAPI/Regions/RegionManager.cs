using DiskCardGame;
using HarmonyLib;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
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
        RegionData retval = (RegionData)UnityEngine.Object.Internal_CloneSingle(data);
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

    public static void Add(RegionData newRegion, int tier)
    {
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
        RegionData retval = (RegionData)RegionData.Internal_CloneSingle(copy.regions[originalTier]);
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
    
    #region Patches
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

    [HarmonyPatch(typeof(EncounterBuilder), nameof(EncounterBuilder.BuildTerrainCondition))]
    [HarmonyPrefix]
    private static bool ApplyTerrainCustomization(ref EncounterData.StartCondition __result, ref bool reachTerrainOnPlayerSide, int randomSeed)
    {
        var customregion = NewRegions.ToList().Find(x => x.Region == RunState.CurrentMapRegion);
        if (customregion != null)
        {
            reachTerrainOnPlayerSide &= !customregion.DoNotForceReachTerrain;
            __result = new EncounterData.StartCondition();
            int numTerrain = SeededRandom.Range(customregion.MinTerrain, customregion.MaxTerrain, randomSeed++);
            int playerTerrain = 0;
            int enemyTerrain = 0;
            for (int i = 0; i < numTerrain; i++)
            {
                bool terrainIsForPlayer;
                if (customregion.AllowTerrainOnEnemySide && customregion.AllowTerrainOnPlayerSide)
                {
                    terrainIsForPlayer = SeededRandom.Bool(randomSeed++);
                    if (terrainIsForPlayer && playerTerrain == 1)
                    {
                        terrainIsForPlayer = false;
                    }
                    else if (!terrainIsForPlayer && enemyTerrain == 1)
                    {
                        terrainIsForPlayer = true;
                    }
                }
                else
                {
                    terrainIsForPlayer = customregion.AllowTerrainOnPlayerSide;
                }
                CardInfo[] sameSideSlots = terrainIsForPlayer ? __result.cardsInPlayerSlots : __result.cardsInOpponentSlots;
                CardInfo[] otherSideSlots = terrainIsForPlayer ? __result.cardsInOpponentSlots : __result.cardsInPlayerSlots;
                int slotForTerrain = SeededRandom.Range(0, sameSideSlots.Length, randomSeed++);
                bool availableSpace = false;
                for (int j = 0; j < 4; j++)
                {
                    if (sameSideSlots[j] == null && otherSideSlots[j] == null)
                    {
                        availableSpace = true;
                    }
                }
                if (!availableSpace)
                {
                    break;
                }
                while (sameSideSlots[slotForTerrain] != null || otherSideSlots[slotForTerrain] != null)
                {
                    slotForTerrain = SeededRandom.Range(0, sameSideSlots.Length, randomSeed++);
                }
                if (terrainIsForPlayer && reachTerrainOnPlayerSide)
                {
                    CardInfo cardInfo = RunState.CurrentMapRegion.terrainCards.Find((CardInfo x) => x.HasAbility(Ability.Reach));
                    if (cardInfo == null && !customregion.RemoveDefaultReachTerrain)
                    {
                        cardInfo = CardLoader.GetCardByName("Tree");
                    }
                    if (cardInfo != null)
                    {
                        sameSideSlots[slotForTerrain] = CardLoader.GetCardByName(cardInfo.name);
                    }
                }
                else
                {
                    List<CardInfo> list = RunState.CurrentMapRegion.terrainCards.FindAll((CardInfo x) => (ConceptProgressionTree.Tree.CardUnlocked(x, true) || customregion.AllowLockedTerrainCards) &&
                        (x.traits.Contains(Trait.Terrain) || customregion.AllowSacrificableTerrainCards));
                    if (list.Count > 0)
                    {
                        sameSideSlots[slotForTerrain] = CardLoader.GetCardByName(list[SeededRandom.Range(0, list.Count, randomSeed++)].name);
                    }
                }
                if (terrainIsForPlayer)
                {
                    playerTerrain++;
                }
                else
                {
                    enemyTerrain++;
                }
            }
            return false;
        }
        return true;
    }
    
    [HarmonyPatch(typeof(MapDataReader), "SpawnMapObjects", new Type[] { typeof(MapData), typeof(int), typeof(Vector2) })]
    public class MapDataReader_SpawnMapObjects
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // === We want to turn this

            // gameObject.GetComponent<Renderer>();

            // === Into this

            // GetSpawnedMapObjectRenderer(gameObject);

            // ===
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            MethodInfo GetSpawnedMapObjectRendererInfo = SymbolExtensions.GetMethodInfo(() => GetSpawnedMapObjectRenderer(null));

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt &&
                    codes[i].operand.ToString() == "UnityEngine.Renderer GetComponent[Renderer]()")
                {
                    codes[i].operand = GetSpawnedMapObjectRendererInfo;
                }
            }

            return codes;
        }

        private static Renderer GetSpawnedMapObjectRenderer(GameObject gameObject)
        {
            // Props normally have the renderer on the object itself. But custom props sometimes will ot have this.
            if (gameObject.TryGetComponent(out Renderer renderer))
            {
                return renderer;
            }

            Renderer spawnedMapObjectRenderer = gameObject.GetComponentInChildren<Renderer>();
            return spawnedMapObjectRenderer;
        }
    }
    #endregion
}