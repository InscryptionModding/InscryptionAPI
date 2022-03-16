using System.Collections.ObjectModel;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Regions;
using UnityEngine;

namespace InscryptionAPI.Encounters;

[HarmonyPatch]
public static class EncounterManager
{
    public static readonly ReadOnlyCollection<EncounterBlueprintData> BaseGameEncounters = new(Resources.LoadAll<EncounterBlueprintData>("Data"));
    private static readonly ObservableCollection<EncounterBlueprintData> NewEncounters = new();

    public static event Func<List<EncounterBlueprintData>, List<EncounterBlueprintData>> ModifyEncountersList;

    private static EncounterBlueprintData CloneAndReplace(this EncounterBlueprintData data)
    {
        EncounterBlueprintData returnValue = (EncounterBlueprintData)UnityObject.Internal_CloneSingle(data);
        returnValue.name = data.name;

        // Repair the blueprint if it is invalid
        if (returnValue.dominantTribes == null || returnValue.dominantTribes.Count == 0)
        {
            List<Tribe> tribes = returnValue.turns.SelectMany(l => l)
                .Select(cb => cb.card)
                .Concat(data.turns.SelectMany(l => l).Select(cb => cb.replacement))
                .Concat(data.randomReplacementCards)
                .Where(ci => ci != null && ci.tribes != null)
                .SelectMany(ci => ci.tribes)
                .ToList();

            returnValue.dominantTribes = tribes.Count > 0
                ? new() { tribes.GroupBy(t => t).OrderByDescending(g => g.Count()).First().Key }
                : new List<Tribe> { Tribe.Insect };
        }

        RegionManager.ReplaceBlueprintInCore(returnValue);
        return returnValue;
    }

    public static void SyncEncounterList()
    {
        var encounters = BaseGameEncounters.Concat(NewEncounters).Select(x => x.CloneAndReplace()).ToList();
        AllEncountersCopy = ModifyEncountersList?.Invoke(encounters) ?? encounters;
    }

    static EncounterManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(EncounterBlueprintData))
            {
                ScriptableObjectLoader<EncounterBlueprintData>.allData = AllEncountersCopy;
            }
        };
        NewEncounters.CollectionChanged += static (_, _) =>
        {
            SyncEncounterList();
        };
    }

    public static List<EncounterBlueprintData> AllEncountersCopy { get; private set; } = BaseGameEncounters.ToList();

    public static void Add(EncounterBlueprintData newEncounter)
    {
        if (!NewEncounters.Contains(newEncounter)) NewEncounters.Add(newEncounter);
    }
    public static void Remove(EncounterBlueprintData encounter) => NewEncounters.Remove(encounter);

    public static EncounterBlueprintData New(string name, bool addToPool = true)
    {
        EncounterBlueprintData returnValue = ScriptableObject.CreateInstance<EncounterBlueprintData>();
        returnValue.name = name;

        if (addToPool)
            Add(returnValue);

        return returnValue;
    }
}
