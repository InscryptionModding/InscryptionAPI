using DiskCardGame;

#nullable enable
namespace InscryptionAPI.TalkingCards.Create;

// A class that wants to create a talking card through this API *must* implement for this interface.
// That's all the class needs to do. c:

/// <summary>
/// An interface for the creation of a talking card through this API.
/// </summary>
public interface ITalkingCard
{
    /// <summary>
    /// The name of an existing card.
    /// </summary>
    public string CardName { get; }

    /// <summary>
    /// Your talking card's emotions.
    /// </summary>
    public List<EmotionData> Emotions { get; }

    /// <summary>
    /// A bit of info about your talking card: blink rate and voice.
    /// </summary>
    public FaceInfo FaceInfo { get; }

    /// <summary>
    /// The special ability that controls your talking card's dialogue.
    /// </summary>
    public SpecialTriggeredAbility DialogueAbility { get; }
}
