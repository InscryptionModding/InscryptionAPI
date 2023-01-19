using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;

namespace InscryptionAPI.Encounters;

public static class AIManager
{
    public class FullAI
    {
        public readonly string Id;
        public readonly Type AI;

        public FullAI(string id, Type aiType)
        {
            Id = id;
            AI = aiType;

            TypeManager.Add(Id.ToString(), AI);
        }
    }

    public readonly static ReadOnlyCollection<FullAI> BaseGameAIs = new(GenBaseGameAIsList());
    private readonly static ObservableCollection<FullAI> NewAIs = new();

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
        var gameAsm = typeof(AI).Assembly;
        foreach (Type aiType in gameAsm.GetTypes().Where(type => type.IsSubclassOf(typeof(AI))))
        {
            baseGame.Add(new(aiType.Name, aiType));
        }
        return baseGame;
    }

    public static FullAI Add(string guid, string aiName, Type sequencer)
    {
        FullAI full = new("AI_" + GuidManager.GetFullyQualifiedName(guid, aiName), sequencer);
        NewAIs.Add(full);
        return full;
    }
}
