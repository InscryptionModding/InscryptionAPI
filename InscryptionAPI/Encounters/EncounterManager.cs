using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Encounters;

[HarmonyPatch]
public static class EncounterManager
{
    public static readonly ReadOnlyCollection<EncounterBlueprintData> BaseGameEncounters = new(Resources.LoadAll<EncounterBlueprintData>("Data"));
    private static readonly ObservableCollection<EncounterBlueprintData> NewEncounters = new();

    public static event Func<List<EncounterBlueprintData>, List<EncounterBlueprintData>> ModifyEncountersList;

    internal static void SyncEncounterList()
    {
        var encounters = BaseGameEncounters.Concat(NewEncounters).Select(x => (EncounterBlueprintData)UnityEngine.Object.Internal_CloneSingle(x)).ToList();
        //var encounters = BaseGameEncounters.Concat(NewEncounters).ToList();
        AllEncountersCopy = ModifyEncountersList?.Invoke(encounters) ?? encounters;
    }

    static EncounterManager()
    {
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScriptableObjectLoader<UnityObject>), nameof(ScriptableObjectLoader<UnityObject>.LoadData))]
    [SuppressMessage("Member Access", "Publicizer001", Justification = "Need to set internal list of encounters")]
    private static void EncounterLoadPrefix()
    {
        ScriptableObjectLoader<EncounterBlueprintData>.allData = AllEncountersCopy;
    }
}