using DiskCardGame;
using InscryptionAPI.Guid;

namespace APIPlugin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Obsolete("Use AbilityManager instead", true)]
public class AbilityIdentifier
{
    internal string guid;
    private string name;
    public Ability id; // This creates a point of fragility - I need to ensure this never changes
    internal Ability realID; // As such, I will actually just use this field

    private AbilityIdentifier(string guid, string name)
    {
        this.guid = guid;
        this.name = name;
        realID = id = GuidManager.GetEnumValue<Ability>(guid, name);
    }

    public static AbilityIdentifier GetAbilityIdentifier(string guid, string name)
    {
        return GetID(guid, name);
    }

    public static AbilityIdentifier GetID(string guid, string name)
    {
        return new AbilityIdentifier(guid, name);
    }

    public override string ToString()
    {
        return $"{guid}({name})";
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member