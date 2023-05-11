using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers.Extensions;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using EncounterBuilder = DiskCardGame.EncounterBuilder;

namespace InscryptionAPI.Regions;

[HarmonyPatch]
public static class RegionManager
{
    public static readonly ReadOnlyCollection<RegionData> BaseGameRegions = new(ReorderBaseRegions());

    /* Order of BaseGameRegions before reordering
     * !TEST_PART3
     * Alpine
     * Forest
     * Midnight
     * Midnight_Ascension
     * Pirateville
     * Wetlands
     */

    private static List<RegionData> ReorderBaseRegions()
    {
        List<RegionData> baseRegions = Resources.LoadAll<RegionData>("Data").ToList();
        List<RegionData> orderedRegions = new()
        {
            baseRegions[2], baseRegions[6], baseRegions[1],
            baseRegions[3], baseRegions[4], baseRegions[5], baseRegions[0]
        };

        return orderedRegions;
    }
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
        RegionData retval = (RegionData)UnityObject.Internal_CloneSingle(data);
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
                ScriptableObjectLoader<RegionData>.allData = AllRegionsCopy;
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
            NewRegions.Add(new Part1RegionData(newRegion, tier));
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
        RegionData retval = (RegionData)UnityObject.Internal_CloneSingle(copy.regions[originalTier]);
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
            // InscryptionAPIPlugin.Logger.LogInfo(RunState.Run.regionOrder[RunState.Run.regionTier]);
            if (RunState.Run.regionTier == RegionProgression.Instance.regions.Count - 1)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.FinalBoss))
                    __result = RegionProgression.Instance.ascensionFinalBossRegion;
                else
                    __result = RegionProgression.Instance.ascensionFinalRegion;

                return false;
            }
            __result = AllRegionsCopy[RunState.Run.regionOrder[RunState.Run.regionTier]];
            return false;
        }

        __result = AllRegionsCopy[RunState.Run.regionTier];

        return false;
    }

    [HarmonyPatch(typeof(EncounterBuilder), nameof(EncounterBuilder.BuildTerrainCondition))]
    [HarmonyPrefix]
    private static bool ApplyTerrainCustomization(ref EncounterData.StartCondition __result, ref bool reachTerrainOnPlayerSide, int randomSeed)
    {
        var customregion = NewRegions.ToList()?.Find(x => x.Region == RunState.CurrentMapRegion);
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
                        terrainIsForPlayer = false;

                    else if (!terrainIsForPlayer && enemyTerrain == 1)
                        terrainIsForPlayer = true;
                }
                else
                {
                    terrainIsForPlayer = customregion.AllowTerrainOnPlayerSide;
                }
                CardInfo[] sameSideSlots = terrainIsForPlayer ? __result.cardsInPlayerSlots : __result.cardsInOpponentSlots;
                CardInfo[] otherSideSlots = terrainIsForPlayer ? __result.cardsInOpponentSlots : __result.cardsInPlayerSlots;
                int slotForTerrain = SeededRandom.Range(0, sameSideSlots.Length, randomSeed++);
                bool availableSpace = false;
                for (int j = 0; j < (sameSideSlots.Length >= otherSideSlots.Length ? sameSideSlots.Length : otherSideSlots.Length); j++)
                {
                    if (sameSideSlots[j] == null && otherSideSlots[j] == null)
                        availableSpace = true;
                }
                if (!availableSpace)
                    break;

                while (sameSideSlots[slotForTerrain] != null || otherSideSlots[slotForTerrain] != null)
                    slotForTerrain = SeededRandom.Range(0, sameSideSlots.Length, randomSeed++);

                if (terrainIsForPlayer && reachTerrainOnPlayerSide)
                {
                    CardInfo cardInfo = RunState.CurrentMapRegion.terrainCards.Find((CardInfo x) => x.HasAbility(Ability.Reach));
                    if (cardInfo == null && !customregion.RemoveDefaultReachTerrain)
                        cardInfo = CardLoader.GetCardByName("Tree");

                    if (cardInfo != null)
                        sameSideSlots[slotForTerrain] = CardLoader.GetCardByName(cardInfo.name);
                }
                else
                {
                    List<CardInfo> list = RunState.CurrentMapRegion.terrainCards.FindAll((CardInfo x) => (ConceptProgressionTree.Tree.CardUnlocked(x, true) || customregion.AllowLockedTerrainCards) &&
                        (x.traits.Contains(Trait.Terrain) || customregion.AllowSacrificableTerrainCards));
                    if (list.Count > 0)
                        sameSideSlots[slotForTerrain] = CardLoader.GetCardByName(list[SeededRandom.Range(0, list.Count, randomSeed++)].name);
                }

                if (terrainIsForPlayer)
                    playerTerrain++;
                else
                    enemyTerrain++;
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
                return renderer;

            Renderer spawnedMapObjectRenderer = gameObject.GetComponentInChildren<Renderer>();
            if (spawnedMapObjectRenderer == null)
                InscryptionAPIPlugin.Logger.LogError($"[RegionManager] Map object {gameObject.name} does not have a renderer attached!");

            return spawnedMapObjectRenderer;
        }
    }

    [HarmonyPatch(typeof(MapDataReader), nameof(MapDataReader.SpawnAndPlaceElement))]
    [HarmonyPrefix]
    private static bool MapDataReader_SpawnAndPlaceElement(ref MapDataReader __instance, ref GameObject __result, MapElementData data, Vector2 sampleRange, bool isScenery)
    {
        // NOTE: We just want logging so if anyone incorrect sets any props we know what went wrong  

        GameObject gameObject = null;
        string prefabPath = __instance.GetPrefabPath(data);
        GameObject original = ResourceBank.Get<GameObject>(prefabPath);
        if (original == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"[RegionManager] Could not find object {prefabPath} to spawn in region!");
            original = Resources.Load<GameObject>("prefabs/map/mapscenery/TreasureChest");
        }

        if (!isScenery)
            gameObject = UnityObject.Instantiate(original);
        else
        {
            MapElement mapElement = original.GetComponent<MapElement>();
            if (mapElement == null)
            {
                InscryptionAPIPlugin.Logger.LogError($"[RegionManager] {original.name} at path {prefabPath} does not have a mapElement component!");
                mapElement = original.AddComponent<MapElement>();
            }
            gameObject = mapElement.GetPooledInstance<MapElement>().gameObject;
        }

        gameObject.transform.SetParent(isScenery ? __instance.sceneryParent : __instance.nodesParent);
        gameObject.transform.localPosition = __instance.GetRealPosFromDataPos(data.position, sampleRange);
        if (isScenery)
        {
            MapElement component = gameObject.GetComponent<MapElement>();
            __instance.scenery.Add(component);
            component.Data = data;
        }

        __result = gameObject;
        return false;
    }

    private static List<RegionData> GetAllRegionsForMapGeneration()
    {
        List<RegionData> allRegions = new(RegionProgression.Instance.regions);
        allRegions.RemoveAt(allRegions.Count - 1); // Remove midnight region
        allRegions.AddRange(NewRegions.Select(a => a.Region)); // New Regions
        return allRegions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AscensionSaveData), "RollCurrentRunRegionOrder")]
    private static bool RollCurrentRunRegionOrder(AscensionSaveData __instance)
    {
        // Get all regions to choose from
        List<RegionData> allRegions = GetAllRegionsForMapGeneration();
        allRegions = allRegions.Randomize().ToList();

        List<RegionData> selectedRegions = new();
        List<Opponent.Type> selectedOpponents = new();
        for (int i = 0; i < allRegions.Count; i++)
        {
            RegionData regionData = allRegions[i];
            Opponent.Type opponentType = GetRandomAvailableOpponent(regionData, selectedOpponents);
            if (opponentType != Opponent.Type.Default)
            {
                // Add a region that doesn't conflict with the already selected ones
                selectedRegions.Add(regionData);
                selectedOpponents.Add(opponentType);
            }

            if (selectedRegions.Count == 3)
                break;
        }

        // Safety check: Make sure we have 3 regions!
        while (selectedRegions.Count < 3)
        {
            List<RegionData> unusedRegions = allRegions.Where((a) => !selectedRegions.Contains(a)).ToList();
            RegionData selectedRegion = null;
            if (unusedRegions.Count == 0)
                selectedRegion = allRegions[0];
            else
                selectedRegion = unusedRegions[0];

            selectedRegions.Add(selectedRegion);
            Opponent.Type opponentType = GetRandomAvailableOpponent(selectedRegion, selectedOpponents);
            if (opponentType == Opponent.Type.Default)
                opponentType = selectedRegion.bosses.GetRandom();

            selectedOpponents.Add(opponentType);
        }

        int[] regions = new int[3];
        for (int i = 0; i < selectedRegions.Count; i++)
        {
            RegionData region = selectedRegions[i];
            int indexOf = AllRegionsCopy.FindIndex((a) => a.name == region.name);
            if (indexOf == -1)
            {
                InscryptionAPIPlugin.Logger.LogError($"Could not get index of region {region.name} in all regions list!");
                foreach (RegionData data in AllRegionsCopy)
                {
                    InscryptionAPIPlugin.Logger.LogError(" " + data.name);
                }
                indexOf = 0;
            }
            regions[i] = indexOf;
        }

        OpponentManager.RunStateOpponents = selectedOpponents;
        __instance.currentRun.regionOrder = regions;
        return false;
    }

    private static Opponent.Type GetRandomAvailableOpponent(RegionData regionData, List<Opponent.Type> selectedOpponents)
    {
        List<Opponent.Type> enumerable = regionData.bosses.Where((a) => !selectedOpponents.Contains(a)).ToList();
        if (enumerable.Count == 0)
            return Opponent.Type.Default;

        return enumerable.GetRandom();
    }

    #endregion
}