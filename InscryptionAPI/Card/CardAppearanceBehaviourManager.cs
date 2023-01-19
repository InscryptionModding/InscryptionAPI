using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;

namespace InscryptionAPI.Card;

public static class CardAppearanceBehaviourManager
{
    public class FullCardAppearanceBehaviour
    {
        public readonly CardAppearanceBehaviour.Appearance Id;
        public readonly Type AppearanceBehaviour;

        public FullCardAppearanceBehaviour(CardAppearanceBehaviour.Appearance id, Type appearanceBehaviour)
        {
            Id = id;
            AppearanceBehaviour = appearanceBehaviour;

            TypeManager.Add(id.ToString(), appearanceBehaviour);
        }
    }

    public readonly static ReadOnlyCollection<FullCardAppearanceBehaviour> BaseGameAppearances = new(GenBaseGameAppearanceList());
    private readonly static ObservableCollection<FullCardAppearanceBehaviour> NewAppearances = new();

    public static List<FullCardAppearanceBehaviour> AllAppearances { get; private set; } = BaseGameAppearances.ToList();

    static CardAppearanceBehaviourManager()
    {
        NewAppearances.CollectionChanged += static (_, _) =>
        {
            AllAppearances = BaseGameAppearances.Concat(NewAppearances).ToList();
        };
    }

    private static List<FullCardAppearanceBehaviour> GenBaseGameAppearanceList()
    {
        List<FullCardAppearanceBehaviour> baseGame = new();
        var gameAsm = typeof(CardAppearanceBehaviour).Assembly;
        foreach (CardAppearanceBehaviour.Appearance ability in Enum.GetValues(typeof(CardAppearanceBehaviour.Appearance)))
        {
            var name = ability.ToString();
            baseGame.Add(new FullCardAppearanceBehaviour(ability, gameAsm.GetType($"DiskCardGame.{name}")));
        }
        return baseGame;
    }

    public static FullCardAppearanceBehaviour Add(string guid, string abilityName, Type behavior)
    {
        FullCardAppearanceBehaviour full = new(GuidManager.GetEnumValue<CardAppearanceBehaviour.Appearance>(guid, abilityName), behavior);
        NewAppearances.Add(full);
        return full;
    }

    public static void Remove(CardAppearanceBehaviour.Appearance id) => NewAppearances.Remove(NewAppearances.FirstOrDefault(x => x.Id == id));
    public static void Remove(FullCardAppearanceBehaviour ability) => NewAppearances.Remove(ability);
}
