namespace InscryptionAPI.Ascension;

// Decorates a custom screen and marks how it should be sorted
public class AscensionScreenSort : System.Attribute
{
    public enum Direction : int
    {
        RequiresStart = 1,
        PrefersStart = 2,
        NoPreference = 3,
        PrefersEnd = 4,
        RequiresEnd = 5,
    }

    public Direction preferredDirection;

    public AscensionScreenSort(Direction preferredDirection = Direction.NoPreference)
    {
        this.preferredDirection = preferredDirection;
    }
}