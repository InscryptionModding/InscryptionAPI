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

    /// <summary>
    /// Sets the difficulty range of this encounter.<br/>
    /// Difficulty is determined by the formula (6 * <c>tier</c>) + <c>battle#</c> + <c>modifier</c>.
    /// </summary>
    /// <param name="blueprint">The blueprint to access.</param>
    /// <param name="min">The minimum difficulty.</param>
    /// <param name="max">The maximum difficulty.</param>
    public static T SetDifficulty<T>(this T blueprint, int min, int max) where T : EncounterBlueprintData
    {
        blueprint.minDifficulty = min;
        blueprint.maxDifficulty = max;
        return blueprint;
    }

    /// <summary>
    /// Adds dominant tribes to this region.<br/>
    /// The dominant tribes list determines the totems for this battle.
    /// </summary>
    /// <param name="blueprint">The blueprint to access.</param>
    /// <param name="tribes">The tribes to add.</param>
    public static T AddDominantTribes<T>(this T blueprint, params Tribe[] tribes) where T : EncounterBlueprintData
    {
        blueprint.dominantTribes ??= new();
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

    /// <summary>
    /// Adds random replacement cards to this region.<br/>
    /// A card from this list is selected whenever a card is randomly replaced by <c>randomReplaceChance</c>.
    /// </summary>
    /// <param name="blueprint">The blueprint to access.</param>
    /// <param name="cards">The cards to add.</param>
    public static T AddRandomReplacementCards<T>(this T blueprint, params string[] cards) where T : EncounterBlueprintData
    {
        blueprint.randomReplacementCards ??= new();
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

    /// <summary>
    /// Adds redundant abilities to this region.<br/>
    /// Redundant abilities will not be used on totems for this encounter.
    /// </summary>
    /// <param name="blueprint">The blueprint to access.</param>
    /// <param name="abilities">The abilities to add.</param>
    public static T SetRedundantAbilities<T>(this T blueprint, params Ability[] abilities) where T : EncounterBlueprintData
    {
        blueprint.redundantAbilities = abilities.ToList();
        return blueprint;
    }

    public static T SetUnlockedCardPrerequisites<T>(this T blueprint, params string[] cards) where T : EncounterBlueprintData
    {
        blueprint.unlockedCardPrerequisites ??= new();
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

    /// <summary>
    /// Creates a new turn for this encounter and returns the builder.
    /// </summary>
    /// <param name="blueprint">The blueprint to access.</param>
    public static TurnBuilder<T> CreateTurn<T>(this T blueprint) where T : EncounterBlueprintData
    {
        TurnBuilder<T> turnBuilder = new();
        turnBuilder.SetBlueprint(blueprint);
        return turnBuilder;
    }
}