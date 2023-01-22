using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;

namespace InscryptionAPI.Encounters;

public static class SpecialSequenceManager
{
    public class FullSpecialSequencer
    {
        public readonly string Id;

        private Type _specialSequencer;
        public Type SpecialSequencer { get; private set; }

        public FullSpecialSequencer(string id, Type specialSequencer)
        {
            Id = id;
            SpecialSequencer = specialSequencer;

            TypeManager.Add(Id.ToString(), specialSequencer); // This is the magic that makes the patches work
        }
    }

    public readonly static ReadOnlyCollection<FullSpecialSequencer> BaseGameSpecialSequencers = new(GenBaseGameSpecialSequencersList());
    private readonly static ObservableCollection<FullSpecialSequencer> NewSpecialSequencers = new();

    public static List<FullSpecialSequencer> AllSpecialSequencers { get; private set; } = BaseGameSpecialSequencers.ToList();

    static SpecialSequenceManager()
    {
        NewSpecialSequencers.CollectionChanged += static (_, _) =>
        {
            AllSpecialSequencers = BaseGameSpecialSequencers.Concat(NewSpecialSequencers).ToList();
        };
    }

    private static List<FullSpecialSequencer> GenBaseGameSpecialSequencersList()
    {
        List<FullSpecialSequencer> baseGame = new();
        var gameAsm = typeof(SpecialBattleSequencer).Assembly;
        foreach (Type sequencer in gameAsm.GetTypes().Where(type => type.IsSubclassOf(typeof(SpecialBattleSequencer))))
        {
            baseGame.Add(new(sequencer.Name, sequencer));
        }
        return baseGame;
    }

    public static FullSpecialSequencer Add(string guid, string sequencerName, Type sequencer)
    {
        FullSpecialSequencer full = new("SpecialSequencer_" + GuidManager.GetFullyQualifiedName(guid, sequencerName), sequencer);
        NewSpecialSequencers.Add(full);
        return full;
    }
}
