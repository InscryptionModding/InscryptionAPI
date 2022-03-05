using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
        EncounterBlueprintData retval = (EncounterBlueprintData)UnityEngine.Object.Internal_CloneSingle(data);
        retval.name = data.name;
        RegionManager.ReplaceBlueprintInCore(retval);
        return retval;
    }

    internal static void SyncEncounterList()
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