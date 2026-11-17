using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;
using System.Reflection;

namespace InscryptionAPI.Encounters;

/// <summary>
/// Manager class that handles the creation and management of mod-added special battle sequencers.
/// </summary>
public static class SpecialSequenceManager
{
    /// <summary>
    /// A utility class that holds all of the required information about a special battle sequencer in order to be able to use it in-game.
    /// </summary>
    public class FullSpecialSequencer
    {
        /// <summary>
        /// The unique ID of this sequencer.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// A subclass of <see cref="SpecialBattleSequencer"/> that implements the logic for the ability.
        /// </summary>
        public Type SpecialSequencer { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="FullSpecialSequencer"/> and registers its behaviour type with the <see cref="TypeManager"/>.
        /// </summary>
        /// <param name="id">The unique ID of this sequencer.</param>
        /// <param name="specialSequencer">A subclass of <see cref="SpecialBattleSequencer"/> that implements the logic for the ability.</param>
        public FullSpecialSequencer(string id, Type specialSequencer)
        {
            Id = id;
            SpecialSequencer = specialSequencer;

            TypeManager.Add(Id.ToString(), specialSequencer); // This is the magic that makes the patches work
        }
    }

    /// <summary>
    /// All of the vanilla game's special battle sequencers.
    /// </summary>
    public readonly static ReadOnlyCollection<FullSpecialSequencer> BaseGameSpecialSequencers = new(GenBaseGameSpecialSequencersList());
    private readonly static ObservableCollection<FullSpecialSequencer> NewSpecialSequencers = new();

    /// <summary>
    /// The current processed list of all special battle sequencers in the game, including vanilla and mod-added special battle sequencers.
    /// </summary>
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
        Assembly gameAsm = typeof(SpecialBattleSequencer).Assembly;
        foreach (Type sequencer in gameAsm.GetTypes().Where(type => type.IsSubclassOf(typeof(SpecialBattleSequencer))))
        {
            baseGame.Add(new(sequencer.Name, sequencer));
        }
        return baseGame;
    }

    /// <summary>
    /// Creates a new special battle sequencer and registers it to be able to be added to opponents.
    /// </summary>
    /// <param name="guid">The GUID of the mod adding the sequencer.</param>
    /// <param name="sequencerName">The internal name of the sequencer.</param>
    /// <param name="sequencer">A subclass of <see cref="SpecialBattleSequencer"/> that implements the logic for the sequencer.</param>
    /// <returns>An instance of <see cref="FullSpecialSequencer"/> containing all this sequencer's data.</returns>
    public static FullSpecialSequencer Add(string guid, string sequencerName, Type sequencer)
    {
        FullSpecialSequencer full = new("SpecialSequencer_" + GuidManager.GetFullyQualifiedName(guid, sequencerName), sequencer);
        NewSpecialSequencers.Add(full);
        return full;
    }
}
