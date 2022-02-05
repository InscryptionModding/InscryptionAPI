using DiskCardGame;
using InscryptionAPI.Card;
using static DiskCardGame.EncounterBlueprintData;

namespace InscryptionAPI.Encounters;

public static class EncounterExtensions
{
    public static OpponentManager.FullOpponent OpponentById(this IEnumerable<OpponentManager.FullOpponent> opponents, Opponent.Type id)
    {
        return opponents.FirstOrDefault(o => o.Id == id);
    }

    public static OpponentManager.FullOpponent SetOpponent(this OpponentManager.FullOpponent opp, Type opponentType)
    {
        opp.Opponent = opponentType;
        return opp;
    }

    public static OpponentManager.FullOpponent SetSequencer(this OpponentManager.FullOpponent opp, string sequenceId)
    {
        opp.SpecialSequencerId = sequenceId;
        return opp;
    }

    public static OpponentManager.FullOpponent SetNewSequencer(this OpponentManager.FullOpponent opp, string pluginGuid, string sequencerName, Type sequencerType)
    {
        var newSequencer = SpecialSequenceManager.Add(pluginGuid, sequencerName, sequencerType);
        opp.SpecialSequencerId = newSequencer.Id;
        return opp;
    }

    public static T SetDifficulty<T>(this T blueprint, int min, int max) where T : EncounterBlueprintData
    {
        blueprint.minDifficulty = min;
        blueprint.maxDifficulty = max;
        return blueprint;
    }

    public static T AddDominantTribes<T>(this T blueprint, params Tribe[] tribes) where T : EncounterBlueprintData
    {
        blueprint.dominantTribes = blueprint.dominantTribes ?? new();
        foreach (Tribe tribe in tribes)
        {
            blueprint.dominantTribes.Add(tribe);
        }
        return blueprint;
    }

    [Obsolete("regionSpecific is unused.")]
    public static T SetRegionSpecific<T>(this T blueprint, bool enabled) where T : EncounterBlueprintData
    {
        blueprint.regionSpecific = enabled;
        return blueprint;
    }

    public static T AddRandomReplacementCards<T>(this T blueprint, params string[] cards) where T : EncounterBlueprintData
    {
        blueprint.randomReplacementCards = blueprint.randomReplacementCards ?? new();
        foreach (string card in cards)
        {
            CardInfo cardInfo = CardManager.AllCardsCopy.CardByName(card);
            if (!blueprint.randomReplacementCards.Contains(cardInfo))
            {
                blueprint.randomReplacementCards.Add(cardInfo);
            }
        }
        return blueprint;
    }

    public static T SetRedundantAbilities<T>(this T blueprint, params Ability[] abilities) where T : EncounterBlueprintData
    {
        blueprint.redundantAbilities = abilities.ToList();
        return blueprint;
    }

    public static T SetUnlockedCardPrerequisites<T>(this T blueprint, params string[] cards) where T : EncounterBlueprintData
    {
        blueprint.unlockedCardPrerequisites = blueprint.unlockedCardPrerequisites ?? new();
        foreach (string card in cards)
        {
            CardInfo cardInfo = CardManager.AllCardsCopy.CardByName(card);
            if (!blueprint.unlockedCardPrerequisites.Contains(cardInfo))
            {
                blueprint.unlockedCardPrerequisites.Add(cardInfo);
            }
        }
        return blueprint;
    }

    public static T AddTurnMods<T>(this T blueprint, params TurnModBlueprint[] turnMods) where T : EncounterBlueprintData
    {
        blueprint.turnMods = turnMods.ToList();
        return blueprint;
    }

    public static T AddTurn<T>(this T blueprint, params CardBlueprint[] turn) where T : EncounterBlueprintData
    {
        blueprint.turns.Add(turn.ToList());
        return blueprint;
    }

    public static TurnBuilder<T> CreateTurn<T>(this T blueprint) where T : EncounterBlueprintData
    {
        TurnBuilder<T> turnBuilder = new();
        turnBuilder.SetBlueprint(blueprint);
        return turnBuilder;
    }
}