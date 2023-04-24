using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Regions;
using System.Collections.ObjectModel;
using UnityEngine;
using static DiskCardGame.EncounterBlueprintData;

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

    /// <summary>
    /// Creates a new CardBlueprint.
    /// </summary>
    /// <param name="cardName">The internal name of the card to use.</param>
    /// <param name="randomReplaceChance">The integer probability of this card getting replaced by a card from the encounter's <c>randomReplacementCards</c>.</param>
    /// <param name="difficultyReplace">Whether to replace this card when a certain difficulty threshold is met.</param>
    /// <param name="difficultyReplaceReq">The difficulty threshold for the <c>replacement</c> card to be used instead.</param>
    /// <param name="replacement">The name of the replacement card for the difficulty replacement.</param>
    /// <param name="minDifficulty">The minimum difficulty for this card to appear.</param>
    /// <param name="maxDifficulty">The maximum difficulty for this card to appear.</param>
    /// <returns>The newly created CardBlueprint.</returns>
    public static CardBlueprint NewCardBlueprint(string cardName, int randomReplaceChance = 0,
        bool difficultyReplace = false, int difficultyReplaceReq = 0, string replacement = null,
        int minDifficulty = 1, int maxDifficulty = 20)
    {
        return new()
        {
            card = CardManager.AllCardsCopy.CardByName(cardName),
            randomReplaceChance = randomReplaceChance,
            minDifficulty = minDifficulty,
            maxDifficulty = maxDifficulty,
            difficultyReplace = difficultyReplace,
            difficultyReq = difficultyReplaceReq,
            replacement = CardManager.AllCardsCopy.CardByName(replacement)
        };
    }

    /// <summary>
    /// Creates a new turn using the provided card name.
    /// </summary>
    /// <param name="cardName">The name of the card that will be played this turn.</param>
    /// <returns>The newly created list so a chain can continue.</returns>
    public static List<CardBlueprint> CreateTurn(string cardName) => CreateTurn(NewCardBlueprint(cardName));

    /// <summary>
    /// Creates a new turn using the provided card names.
    /// </summary>
    /// <param name="cardNames">The names of the cards that will be played this turn.</param>
    /// <returns>The newly created list so a chain can continue.</returns>
    public static List<CardBlueprint> CreateTurn(params string[] cardNames)
    {
        List<CardBlueprint> cards = new();
        foreach (string cardName in cardNames)
            cards.Add(NewCardBlueprint(cardName));

        return CreateTurn(cards.ToArray());
    }

    /// <summary>
    /// Creates a new turn using the provided CardBlueprint.
    /// </summary>
    /// <param name="card">The CardBlueprint that will be used this turn. If null, creates an empty turn.</param>
    /// <returns>The newly created list so a chain can continue.</returns>
    public static List<CardBlueprint> CreateTurn(CardBlueprint card = null)
    {
        List<CardBlueprint> list = new();
        if (card != null)
            list.Add(card);

        return list;
    }

    /// <summary>
    /// Creates a new turn using the provided CardBlueprints.
    /// </summary>
    /// <param name="cards">The CardBlueprints that will be used this turn.</param>
    /// <returns>The newly created list so a chain can continue.</returns>
    public static List<CardBlueprint> CreateTurn(params CardBlueprint[] cards)
    {
        List<CardBlueprint> retval = new();
        foreach (CardBlueprint card in cards)
            retval.Add(card);

        return retval;
    }
}