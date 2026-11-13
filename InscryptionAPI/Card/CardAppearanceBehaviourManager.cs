using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;
using System.Reflection;

namespace InscryptionAPI.Card;

/// <summary>
/// Class for managing custom CardAppearanceBehaviours.
/// </summary>
public static class CardAppearanceBehaviourManager {
    /// <summary>
    /// A utility class that holds all of the required information about an appearance behaviour
    /// in order to be able to use it in-game.
    /// </summary>
    public class FullCardAppearanceBehaviour
    {
        /// <summary>
        /// Appearance enum id for the appearance behaviour.
        /// </summary>
        public readonly CardAppearanceBehaviour.Appearance Id;
        /// <summary>
        /// Type of the appearance behaviour.
        /// </summary>
        public readonly Type AppearanceBehaviour;
        /// <summary>
        /// Creates a new instance of FullCardAppearanceBehaviour and registers its behaviour type with the <see cref="TypeManager"/>.
        /// </summary>
        /// <param name="id">Appearance enum id.</param>
        /// <param name="appearanceBehaviour">Type of the custom CardAppearanceBehaviour class.</param>
        public FullCardAppearanceBehaviour(CardAppearanceBehaviour.Appearance id, Type appearanceBehaviour)
        {
            Id = id;
            AppearanceBehaviour = appearanceBehaviour;

            TypeManager.Add(id.ToString(), appearanceBehaviour);
        }
    }

    /// <summary>
    /// A collection of all CardAppearanceBehaviours from the base game.
    /// </summary>
    public readonly static ReadOnlyCollection<FullCardAppearanceBehaviour> BaseGameAppearances = new(GenBaseGameAppearanceList());
    private readonly static ObservableCollection<FullCardAppearanceBehaviour> NewAppearances = new();

    /// <summary>
    /// All of CardAppearanceBehaviours, including vanilla and custom.
    /// </summary>
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
        Assembly gameAsm = typeof(CardAppearanceBehaviour).Assembly;
        foreach (CardAppearanceBehaviour.Appearance ability in Enum.GetValues(typeof(CardAppearanceBehaviour.Appearance)))
        {
            string name = ability.ToString();
            baseGame.Add(new FullCardAppearanceBehaviour(ability, gameAsm.GetType($"DiskCardGame.{name}")));
        }
        return baseGame;
    }
    /// <summary>
    /// Registers a custom CardAppearanceBehaviour with the API.
    /// </summary>
    /// <param name="guid">The GUID of the mod adding the behaviour.</param>
    /// <param name="abilityName">The internal name identifier for the behaviour.</param>
    /// <param name="behavior">The Type of the custom CardAppearanceBehaviour class.</param>
    /// <returns>A FullCardAppearanceBehaviour object containing all the custom CardAppearanceBehaviour's data.</returns>
    public static FullCardAppearanceBehaviour Add(string guid, string abilityName, Type behavior)
    {
        FullCardAppearanceBehaviour full = new(GuidManager.GetEnumValue<CardAppearanceBehaviour.Appearance>(guid, abilityName), behavior);
        NewAppearances.Add(full);
        return full;
    }

    /// <summary>
    /// Unregisters a CardAppearanceBehaviour from the API.
    /// </summary>
    /// <param name="id">The Appearance enum of the behaviour to remove.</param>
    public static void Remove(CardAppearanceBehaviour.Appearance id) => NewAppearances.Remove(NewAppearances.FirstOrDefault(x => x.Id == id));
    /// <summary>
    /// Unregisters a CardAppearanceBehaviour from the API.
    /// </summary>
    /// <param name="ability">The FullCardAppearanceBehaviour object of the behaviour to remove.</param>
    public static void Remove(FullCardAppearanceBehaviour ability) => NewAppearances.Remove(ability);
}
