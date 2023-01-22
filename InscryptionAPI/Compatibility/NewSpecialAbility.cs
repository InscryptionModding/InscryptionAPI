using DiskCardGame;
using InscryptionAPI.Card;

namespace APIPlugin;

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