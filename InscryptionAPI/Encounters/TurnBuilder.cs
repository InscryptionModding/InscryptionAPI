using DiskCardGame;
using InscryptionAPI.Card;
using static DiskCardGame.EncounterBlueprintData;

namespace InscryptionAPI.Encounters
{
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
            blueprint.turns = blueprint.turns ?? new();
            blueprint.turns.Add(cards);
        }

    }

    public static class TurnExtensions
    {

        public static TurnBuilder<T> AddCard<T>(this TurnBuilder<T> turnBuilder, string card, int randomReplaceChance = 0,
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
                replacement = CardManager.AllCardsCopy.CardByName(card)
            });
            return turnBuilder;
        }

        public static T Build<T>(this TurnBuilder<T> turnBuilder) where T : EncounterBlueprintData
        {
            return turnBuilder.blueprint;
        }
    }
}
