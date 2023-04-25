using DiskCardGame;
using InscryptionAPI.Guid;
using System.Collections.ObjectModel;

namespace InscryptionAPI.Card;

public static class SpecialTriggeredAbilityManager
{
    public class FullSpecialTriggeredAbility
    {
        public readonly SpecialTriggeredAbility Id;
        public readonly Type AbilityBehaviour;

        public FullSpecialTriggeredAbility(SpecialTriggeredAbility id, Type abilityBehaviour)
        {
            Id = id;
            AbilityBehaviour = abilityBehaviour;

            TypeManager.Add(id.ToString(), abilityBehaviour);
        }
    }

    public readonly static ReadOnlyCollection<FullSpecialTriggeredAbility> BaseGameSpecialTriggers = new(GenBaseGameSpecialTriggersList());
    private readonly static ObservableCollection<FullSpecialTriggeredAbility> NewSpecialTriggers = new();

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
        var gameAsm = typeof(AbilityInfo).Assembly;
        foreach (SpecialTriggeredAbility ability in Enum.GetValues(typeof(SpecialTriggeredAbility)))
        {
            var name = ability.ToString();
            baseGame.Add(new(ability, gameAsm.GetType($"DiskCardGame.{name}")));
        }
        return baseGame;
    }

    public static FullSpecialTriggeredAbility Add(string guid, string abilityName, Type behavior)
    {
        FullSpecialTriggeredAbility full = new(GuidManager.GetEnumValue<SpecialTriggeredAbility>(guid, abilityName), behavior);
        NewSpecialTriggers.Add(full);
        return full;
    }

    public static void Remove(SpecialTriggeredAbility id) => NewSpecialTriggers.Remove(NewSpecialTriggers.FirstOrDefault(x => x.Id == id));
    public static void Remove(FullSpecialTriggeredAbility ability) => NewSpecialTriggers.Remove(ability);
}
