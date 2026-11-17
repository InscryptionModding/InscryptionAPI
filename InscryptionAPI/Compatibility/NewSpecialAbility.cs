using DiskCardGame;
using InscryptionAPI.Card;

namespace APIPlugin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Obsolete("Use SpecialTriggeredAbilityManager instead", true)]
public class NewSpecialAbility
{
    public static List<NewSpecialAbility> specialAbilities = new();
    public SpecialTriggeredAbility specialTriggeredAbility;
    public StatIconInfo statIconInfo;
    public Type abilityBehaviour;
    public SpecialAbilityIdentifier id;

    public NewSpecialAbility(
        Type abilityBehaviour,
        SpecialAbilityIdentifier id,
        StatIconInfo statIconInfo = null
    )
    {
        if (statIconInfo == null)
        {
            var fam = SpecialTriggeredAbilityManager.Add(id.guid, id.name, abilityBehaviour);
            this.specialTriggeredAbility = fam.Id;
            this.abilityBehaviour = abilityBehaviour;
            this.id = id;
        }

        if (statIconInfo != null)
        {
            StatIconManager.Add(id.guid, statIconInfo, abilityBehaviour);
            id.ForStatIcon = true;
            this.statIconInfo = statIconInfo;
            this.abilityBehaviour = abilityBehaviour;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member