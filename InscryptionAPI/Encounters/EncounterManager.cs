using DiskCardGame;
using InscryptionAPI.Regions;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InscryptionAPI.Encounters;

public static class EncounterManager
{
    public static readonly ReadOnlyCollection<EncounterBlueprintData> BaseGameEncounters = new(Resources.LoadAll<EncounterBlueprintData>("Data"));
    private static readonly ObservableCollection<EncounterBlueprintData> NewEncounters = new();

    public static event Func<List<EncounterBlueprintData>, List<EncounterBlueprintData>> ModifyEncountersList;

    private static EncounterBlueprintData CloneAndReplace(this EncounterBlueprintData data)
    {
        EncounterBlueprintData retval = (EncounterBlueprintData)UnityEngine.Object.Internal_CloneSingle(data);
        retval.name = data.name;

        // Repair the blueprint if it is invalid
        if (retval.dominantTribes == null || retval.dominantTribes.Count == 0)
        {
            List<Tribe> tribes = retval.turns.SelectMany(l => l).Select(cb => cb.card)
                                  .Concat(data.turns.SelectMany(l => l).Select(cb => cb.replacement))
                                  .Concat(data.randomReplacementCards)
                                  .Where(ci => ci != null && ci.tribes != null)
                                  .SelectMany(ci => ci.tribes)
                                  .ToList();

            if (tribes.Count > 0)
                retval.dominantTribes = new() { tribes.GroupBy(t => t).OrderByDescending(g => g.Count()).First().Key };
            else
                retval.dominantTribes = new List<Tribe>() { Tribe.Insect };
        }

        RegionManager.ReplaceBlueprintInCore(retval);
        return retval;
    }

    public static void SyncEncounterList()
    {
        var encounters = BaseGameEncounters.Concat(NewEncounters).Select(x => x.CloneAndReplace()).ToList();
        //var encounters = BaseGameEncounters.Concat(NewEncounters).ToList();
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

    public static void Add(EncounterBlueprintData newEncounter) { if (!NewEncounters.Contains(newEncounter)) NewEncounters.Add(newEncounter); }
    public static void Remove(EncounterBlueprintData encounter) => NewEncounters.Remove(encounter);

    public static EncounterBlueprintData New(string name, bool addToPool = true)
    {
        EncounterBlueprintData retval = ScriptableObject.CreateInstance<EncounterBlueprintData>();
        retval.name = name;

        if (addToPool)
            Add(retval);

        return retval;
    }
}