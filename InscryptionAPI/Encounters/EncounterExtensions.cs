using DiskCardGame;

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
}