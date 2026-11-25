using DiskCardGame;
using InscryptionAPI.Guid;

namespace APIPlugin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Obsolete("Use SpecialTriggeredAbilityManager instead", true)]
public class SpecialAbilityIdentifier
{
    internal string guid;
    internal string name;
    public SpecialTriggeredAbility id;
    internal SpecialTriggeredAbility specialTriggerID;
    internal SpecialStatIcon statIconID;
    internal bool ForStatIcon = false;

    private SpecialAbilityIdentifier(string guid, string name)
    {
        this.guid = guid;
        this.name = name;
        specialTriggerID = id = GuidManager.GetEnumValue<SpecialTriggeredAbility>(guid, name);
        statIconID = GuidManager.GetEnumValue<SpecialStatIcon>(guid, name);
    }

    public static SpecialAbilityIdentifier GetID(string guid, string name)
    {
        return new SpecialAbilityIdentifier(guid, name);
    }

    public override string ToString()
    {
        return $"{guid}({name})";
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member