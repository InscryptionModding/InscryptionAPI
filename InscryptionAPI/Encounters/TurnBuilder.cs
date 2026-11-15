using DiskCardGame;
using InscryptionAPI.Card;
using static DiskCardGame.EncounterBlueprintData;

namespace InscryptionAPI.Encounters;

/// <summary>
/// Utility class for easily creating turns in an encounter blueprint.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TurnBuilder<T> where T : EncounterBlueprintData
{
    internal T blueprint;
    internal List<CardBlueprint> cards;

    /// <summary>
    /// Default constructor that populates <c>cards</c> with an empty list.
    /// </summary>
    public TurnBuilder()
    {
        cards = new();
    }

    /// <summary>
    /// Sets the <see cref="EncounterBlueprintData"/> that this builder is helping construct.
    /// </summary>
    /// <param name="blueprint"></param>
    public void SetBlueprint(T blueprint)
    {
        this.blueprint = blueprint;
        blueprint.turns ??= new();
        blueprint.turns.Add(cards);
    }

}

/// <summary>
/// Utility class that adds extensions to aid in the creation of encounter blueprint turns.
/// </summary>
public static class TurnExtensions
{
    /// <summary>
    /// Adds a card blueprint to this turn.
    /// </summary>
    /// <typeparam name="T">The <see cref="EncounterBlueprintData"/> type.</typeparam>
    /// <param name="turnBuilder">The <see cref="TurnBuilder{T}"/> to access.</param>
    /// <param name="card">The default card. Can be null for no card.</param>
    /// <param name="randomReplaceChance">The integer probability of this card getting replaced by a card from the encounter's <c>randomReplacementCards</c>.</param>
    /// <param name="minDifficulty">The minimum difficulty for this card to appear.</param>
    /// <param name="maxDifficulty">The maximum difficulty for this card to appear.</param>
    /// <param name="difficultyReplace">Whether to replace this card when a certain difficulty threshold is met.</param>
    /// <param name="difficultyReplaceReq">The difficulty threshold for the <c>replacement</c> card to be used instead.</param>
    /// <param name="replacement">The replacement card for the difficulty replacement.</param>
    public static TurnBuilder<T> AddCardBlueprint<T>(this TurnBuilder<T> turnBuilder, string card, int randomReplaceChance = 0,
        int minDifficulty = 1, int maxDifficulty = 20,
        bool difficultyReplace = false, int difficultyReplaceReq = 0, string replacement = null)
        where T : EncounterBlueprintData
    {
        turnBuilder.cards.Add(new CardBlueprint()
        {
            card = CardManager.AllCardsCopy.CardByName(card),
            randomReplaceChance = randomReplaceChance,
            minDifficulty = minDifficulty,
            maxDifficulty = maxDifficulty,
            difficultyReplace = difficultyReplace,
            difficultyReq = difficultyReplaceReq,
            replacement = CardManager.AllCardsCopy.CardByName(replacement)
        });
        return turnBuilder;
    }

    /// <summary>
    /// Finishes building the blueprint and returns it for use.
    /// </summary>
    /// <typeparam name="T">The <see cref="EncounterBlueprintData"/> type.</typeparam>
    /// <param name="turnBuilder">The <see cref="TurnBuilder{T}"/> to access.</param>
    /// <returns>The <see cref="EncounterBlueprintData"/> being constructed by this builder.</returns>
    public static T Build<T>(this TurnBuilder<T> turnBuilder) where T : EncounterBlueprintData
    {
        return turnBuilder.blueprint;
    }
}