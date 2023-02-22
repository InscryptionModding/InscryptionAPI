using DiskCardGame;
using InscryptionAPI.Card;
using static DiskCardGame.EncounterBlueprintData;

namespace InscryptionAPI.Encounters;

public class TurnBuilder<T> where T : EncounterBlueprintData
{
    internal T blueprint;
    internal List<CardBlueprint> cards;

    public TurnBuilder()
    {
        cards = new();
    }

    public void SetBlueprint(T blueprint)
    {
        this.blueprint = blueprint;
        blueprint.turns ??= new();
        blueprint.turns.Add(cards);
    }

}

public static class TurnExtensions
{
    /// <summary>
    /// Adds a card blueprint to this turn.
    /// </summary>
    /// <param name="card">The default card. Can be null for no card.</param>
    /// <param name="randomReplaceChance">The integer probability of this card getting replaced by a card from the encounter's <c>randomReplacementCards</c></param>
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

    public static T Build<T>(this TurnBuilder<T> turnBuilder) where T : EncounterBlueprintData
    {
        return turnBuilder.blueprint;
    }
}