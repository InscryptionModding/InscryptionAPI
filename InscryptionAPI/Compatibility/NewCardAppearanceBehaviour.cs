using DiskCardGame;
using InscryptionAPI.Card;

namespace APIPlugin;

[Obsolete("Use CardAppearanceBehaviourManager instead", true)]
public class NewCardAppearanceBehaviour
{
    public static Dictionary<CardAppearanceBehaviour.Appearance, NewCardAppearanceBehaviour> behaviours = new();
    public static List<NewCardAppearanceBehaviour> allBehaviours = new();

    public CardAppearanceBehaviour.Appearance Appearance;
    public string Name;
    public Type Behaviour;

    public static NewCardAppearanceBehaviour AddNewBackground(Type type, string name)
    {
        var fab = CardAppearanceBehaviourManager.Add(type.Namespace, name, type);

        NewCardAppearanceBehaviour backgroundBehaviour = new NewCardAppearanceBehaviour
        {
            Appearance = fab.Id,
            Name = name,
            Behaviour = type
        };

        return backgroundBehaviour;
    }
}