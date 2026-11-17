using DiskCardGame;

namespace InscryptionAPI.Card;

/// <summary>
/// Custom trigger for determining if an ability trigger should activate when a card is face down.
/// </summary>
public interface IActivateWhenFacedown
{
    /// <summary>
    /// Whether or not the given trigger should be called on this ability when its card is face down.
    /// </summary>
    /// <param name="trigger">The trigger being checked.</param>
    /// <param name="otherArgs">Array of objects relating to the trigger being checked.</param>
    /// <returns>True if the given trigger should be activated while this ability's card is face down.</returns>
    public bool ShouldTriggerWhenFaceDown(Trigger trigger, object[] otherArgs);
    /// <summary>
    /// Whether or not the given trigger should be called on this ability when its card is face down.
    /// </summary>
    /// <param name="customTrigger">The Type of the custom API trigger being checked.</param>
    /// <returns>True if the given trigger should be activated while this ability's card is face down.</returns>
    public bool ShouldTriggerCustomWhenFaceDown(Type customTrigger);
}