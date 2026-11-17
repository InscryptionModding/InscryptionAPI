using DiskCardGame;
using InscryptionAPI.Card;

namespace APIPlugin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

        NewCardAppearanceBehaviour backgroundBehaviour = new NewCardAppearanceBehaviour();
        backgroundBehaviour.Appearance = fab.Id;
        backgroundBehaviour.Name = name;
        backgroundBehaviour.Behaviour = type;

        return backgroundBehaviour;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member