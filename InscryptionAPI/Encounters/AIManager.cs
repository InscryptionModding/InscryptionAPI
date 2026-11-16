using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;
using System.Reflection;

namespace InscryptionAPI.Encounters;

/// <summary>
/// Manager class for custom opponent AIs.
/// </summary>
public static class AIManager
{
    /// <summary>
    /// A utility class that holds all of the required information about an AI in order to be able to use it in-game.
    /// </summary>
    public class FullAI {
        /// <summary>
        /// The ID of the AI. Contains the GUID of the mod adding this AI.
        /// </summary>
        public readonly string Id;
        /// <summary>
        /// The <see cref="DiskCardGame.AI"/> subclass for this AI.
        /// </summary>
        public readonly Type AI;

        /// <summary>
        /// Creates a new instance of <see cref="FullAI"/> and registers its behaviour type with the <see cref="TypeManager"/>.
        /// </summary>
        /// <param name="id">The ID of this AI.</param>
        /// <param name="aiType">The <see cref="DiskCardGame.AI"/> subclass that implements this AI's logic.</param>
        public FullAI(string id, Type aiType)
        {
            Id = id;
            AI = aiType;

            TypeManager.Add(Id.ToString(), AI);
        }
    }

    /// <summary>
    /// All of the vanilla game's opponent AIs.
    /// </summary>
    public readonly static ReadOnlyCollection<FullAI> BaseGameAIs = new(GenBaseGameAIsList());
    private readonly static ObservableCollection<FullAI> NewAIs = new();

    /// <summary>
    /// The current processed list of all opponent AIs in the game, including vanilla and mod-added AIs.
    /// </summary>
    public static List<FullAI> AllAIs { get; private set; } = BaseGameAIs.ToList();

    static AIManager()
    {
        NewAIs.CollectionChanged += static (_, _) =>
        {
            AllAIs = BaseGameAIs.Concat(NewAIs).ToList();
        };
    }

    private static List<FullAI> GenBaseGameAIsList()
    {
        List<FullAI> baseGame = new();
        Assembly gameAsm = typeof(AI).Assembly;
        foreach (Type aiType in gameAsm.GetTypes().Where(type => type.IsSubclassOf(typeof(AI))))
        {
            baseGame.Add(new(aiType.Name, aiType));
        }
        return baseGame;
    }
    /// <summary>
    /// Creates a new AI and registers it to be able to be used for opponents.
    /// </summary>
    public static FullAI Add(string guid, string aiName, Type sequencer)
    {
        FullAI full = new("AI_" + GuidManager.GetFullyQualifiedName(guid, aiName), sequencer);
        NewAIs.Add(full);
        return full;
    }
}
