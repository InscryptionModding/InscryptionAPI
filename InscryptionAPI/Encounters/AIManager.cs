using System.Collections.ObjectModel;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;

namespace InscryptionAPI.Encounters;

[HarmonyPatch]
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

            TypeManager.Add(Id, AI);
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
        var gameAsm = typeof(AI).Assembly;
        return gameAsm.GetTypes().Where(type => type.IsSubclassOf(typeof(AI))).Select(aiType => new FullAI(aiType.Name, aiType)).ToList();
    }

    public static FullAI Add(string guid, string aiName, Type sequencer)
    {
        FullAI full = new("AI_" + GuidManager.GetFullyQualifiedName(guid, aiName), sequencer);
        NewAIs.Add(full);
        return full;
    }
}
