using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;
using System.Reflection;

namespace InscryptionAPI.Card;

/// <summary>
/// Class for managing SpecialtriggeredAbilities created through the API.
/// </summary>
public static class SpecialTriggeredAbilityManager {
    /// <summary>
    /// A utility class that holds all of the required information about a special ability in order to be able to use it in-game.
    /// </summary>
    public class FullSpecialTriggeredAbility
    {
        /// <summary>
        /// The unique ID for this special ability
        /// </summary>
        public readonly SpecialTriggeredAbility Id;
        /// <summary>
        /// The guid of the mod that added this special ability
        /// </summary>
        public readonly string ModGUID;
        /// <summary>
        /// The name of this special ability. Used internally.
        /// </summary>
        public readonly string AbilityName;
        /// <summary>
        /// A subclass of SpecialCardBehaviour that implements the logic for this special ability
        /// </summary>
        public readonly Type AbilityBehaviour;

        /// <summary>
        /// Creates a new instance of FullSpecialTriggeredAbility and registers its behaviour type with the [TypeManager](InscryptionAPI.Guid.TypeManager).
        /// </summary>
        /// <param name="id">The unique ID for this special ability.</param>
        /// <param name="abilityBehaviour">A subclass of SpecialCardBehaviour that implements the logic for the ability.</param>
        [Obsolete("Use the constructor that takes a guid parameter instead")]
        public FullSpecialTriggeredAbility(SpecialTriggeredAbility id, Type abilityBehaviour) : this(null, abilityBehaviour.ToString(), id, abilityBehaviour)
        {

        }

        /// <summary>
        /// Creates a new instance of FullSpecialTriggeredAbility and registers its behaviour type with the [TypeManager](InscryptionAPI.Guid.TypeManager).
        /// </summary>
        /// <param name="guid">The GUID of the mod that added this.</param>
        /// <param name="name">The name of this special ability.</param>
        /// <param name="id">The unique ID for this special ability.</param>
        /// <param name="abilityBehaviour">A subclass of SpecialCardBehaviour that implements the logic for the ability.</param>
        public FullSpecialTriggeredAbility(string guid, string name, SpecialTriggeredAbility id, Type abilityBehaviour)
        {
            Id = id;
            ModGUID = guid;
            AbilityName = name;
            AbilityBehaviour = abilityBehaviour;

            TypeManager.Add(id.ToString(), abilityBehaviour);
        }
    }

    /// <summary>
    /// All of the vanilla game's special abilities.
    /// </summary>
    public readonly static ReadOnlyCollection<FullSpecialTriggeredAbility> BaseGameSpecialTriggers = new(GenBaseGameSpecialTriggersList());
    private readonly static ObservableCollection<FullSpecialTriggeredAbility> NewSpecialTriggers = new();

    /// <summary>
    /// The current processed list of all special abilities in the game, including vanilla and mod-added abilities.
    /// </summary>
    public static List<FullSpecialTriggeredAbility> AllSpecialTriggers { get; private set; } = BaseGameSpecialTriggers.ToList();

    static SpecialTriggeredAbilityManager()
    {
        NewSpecialTriggers.CollectionChanged += static (_, _) =>
        {
            AllSpecialTriggers = BaseGameSpecialTriggers.Concat(NewSpecialTriggers).ToList();
        };
    }

    private static List<FullSpecialTriggeredAbility> GenBaseGameSpecialTriggersList()
    {
        List<FullSpecialTriggeredAbility> baseGame = new();
        Assembly gameAsm = typeof(AbilityInfo).Assembly;
        foreach (SpecialTriggeredAbility ability in Enum.GetValues(typeof(SpecialTriggeredAbility)))
        {
            string name = ability.ToString();
            baseGame.Add(new(null, name, ability, gameAsm.GetType($"DiskCardGame.{name}")));
        }
        return baseGame;
    }

    /// <summary>
    /// Creates a new special ability and registers it to be able to be added to cards.
    /// </summary>
    /// <param name="guid">The guid of the mod adding the special ability.</param>
    /// <param name="abilityName">The name of the special ability.</param>
    /// <param name="behavior">A subclass of SpecialCardBehaviour that implements the logic for the ability.</param>
    public static FullSpecialTriggeredAbility Add(string guid, string abilityName, Type behavior)
    {
        FullSpecialTriggeredAbility full = new(guid, abilityName, GuidManager.GetEnumValue<SpecialTriggeredAbility>(guid, abilityName), behavior);
        NewSpecialTriggers.Add(full);
        return full;
    }

    /// <summary>
    /// Removes a special ability from the game based on its ID. Can only remove mod-added special abilities, not vanilla ones.
    /// </summary>
    /// <param name="id">The unique ID of the special ability to remove.</param>
    public static void Remove(SpecialTriggeredAbility id) => NewSpecialTriggers.Remove(NewSpecialTriggers.FirstOrDefault(x => x.Id == id));
    /// <summary>
    /// Removes a special ability from the game based on its FullSpecialTriggeredAbility. Can only remove mod-added special abilities, not vanilla ones.
    /// </summary>
    /// <param name="ability">The instance of the ability to remove.</param>
    public static void Remove(FullSpecialTriggeredAbility ability) => NewSpecialTriggers.Remove(ability);
}
