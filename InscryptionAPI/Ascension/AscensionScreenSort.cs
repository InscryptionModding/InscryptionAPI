namespace InscryptionAPI.Ascension;

/// <summary>
/// Decorates a custom screen and marks how it should be sorted.
/// </summary>
public class AscensionScreenSort : System.Attribute
{
    /// <summary>
    /// Where in the new run sequence a custom screen should appear.
    /// </summary>
    public enum Direction : int
    {
        /// <summary>
        /// Requires being the starting screen when selecting a new run.
        /// </summary>
        RequiresStart = 1,
        /// <summary>
        /// Prefers being the starting screen when selecting a new run.
        /// </summary>
        PrefersStart = 2,
        /// <summary>
        /// Does not care where in the line up of screen this screen appears.
        /// </summary>
        NoPreference = 3,
        /// <summary>
        /// Prefers being the final screen when selecting a new run.
        /// </summary>
        PrefersEnd = 4,
        /// <summary>
        /// Requires being the starting screen when selecting a new run.
        /// </summary>
        RequiresEnd = 5,
    }

    /// <summary>
    /// The preferred position for this custom screen.
    /// </summary>
    public Direction preferredDirection;

    /// <summary>
    /// Marks a custom screen with its preferred direction.
    /// </summary>
    /// <param name="preferredDirection"></param>
    public AscensionScreenSort(Direction preferredDirection = Direction.NoPreference)
    {
        this.preferredDirection = preferredDirection;
    }
}