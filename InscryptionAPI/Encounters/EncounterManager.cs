using DiskCardGame;
using InscryptionAPI.Regions;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InscryptionAPI.Encounters;

public static class EncounterManager
{
    /// <summary>
    /// All of the vanilla game's encounters.
    /// </summary>
    public static readonly ReadOnlyCollection<EncounterBlueprintData> BaseGameEncounters = new(Resources.LoadAll<EncounterBlueprintData>("Data"));
    private static readonly ObservableCollection<EncounterBlueprintData> NewEncounters = new();

    /// <summary>
    /// This event runs every time the encounters list is resynced. By adding listeners to this event, you can modify encounters that have been added to the list after your mod was loaded.
    /// </summary>
    public static event Func<List<EncounterBlueprintData>, List<EncounterBlueprintData>> ModifyEncountersList;

    private static EncounterBlueprintData CloneAndReplace(this EncounterBlueprintData data)
    {
        EncounterBlueprintData retval = (EncounterBlueprintData)UnityObject.Internal_CloneSingle(data);
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

    /// <summary>
    /// Re-executes events and rebuilds the encounter pool.
    /// </summary>
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
                ScriptableObjectLoader<EncounterBlueprintData>.allData = AllEncountersCopy;

        };
        NewEncounters.CollectionChanged += static (_, _) =>
        {
            SyncEncounterList();
        };
    }

    /// <summary>
    /// A copy of all encounters in the encounter pool.
    /// </summary>
    /// <returns></returns>
    public static List<EncounterBlueprintData> AllEncountersCopy { get; private set; } = BaseGameEncounters.ToList();

    /// <summary>
    /// Adds a new encounter to the encounter pool.
    /// </summary>
    /// <param name="newEncounter">The encounter to add.</param>
    public static void Add(EncounterBlueprintData newEncounter)
    {
        if (!NewEncounters.Contains(newEncounter))
            NewEncounters.Add(newEncounter);
    }

    /// <summary>
    /// Removes a custom encounter from the encounter pool. Cannot be used to remove base encounters.
    /// </summary>
    /// <param name="encounter">The encounter to remove.</param>
    public static void Remove(EncounterBlueprintData encounter) => NewEncounters.Remove(encounter);

    /// <summary>
    /// Creates a new instance of EncounterBlueprintData.
    /// </summary>
    /// <param name="name">The internal name of your encounter - used to find and reference it.</param>
    /// <param name="addToPool">If true, the created instance will be added to the encounter pool.</param>
    /// <returns>The newly created card's CardInfo.</returns>
    public static EncounterBlueprintData New(string name, bool addToPool = true)
    {
        EncounterBlueprintData retval = ScriptableObject.CreateInstance<EncounterBlueprintData>();
        retval.name = name;

        if (addToPool)
            Add(retval);

        return retval;
    }
}