using DiskCardGame;
using InscryptionAPI.TalkingCards.Create;
using System.Collections;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards;

/// <summary>
/// An abstract class for the creation of talking cards through the API.
/// It inherits from <see cref="PaperTalkingCard"/> and implements <see cref="ITalkingCard"/>.
/// </summary>
public abstract class CustomPaperTalkingCard : PaperTalkingCard, ITalkingCard {
    /// <summary>
    /// ID of the speaker used to determine the voice used when this card talks.
    /// </summary>
    public override DialogueEvent.Speaker SpeakerType => DialogueEvent.Speaker.Stoat;
    /// <summary>
    /// Internal name of the actual talking card.
    /// </summary>
    public abstract string CardName { get; }
    /// <summary>
    /// List of emotions this card can express.
    /// </summary>
    public abstract List<EmotionData> Emotions { get; }
    /// <summary>
    /// Description object for the talking card's portrait.
    /// </summary>
    public abstract FaceInfo FaceInfo { get; }
    /// <summary>
    /// ID of the special ability used to mark the talking card as a talking card.
    /// </summary>
    public abstract SpecialTriggeredAbility DialogueAbility { get; }

    /// <summary>
    /// ID of the <see cref="DialogueEvent"/> to play when the dialogue event for <see cref="TalkingCard.OnDrawnFallbackDialogueId"/> fails to play.
    /// </summary>
    public override string OnDrawnFallbackDialogueId => OnDrawnDialogueId;

    /// <summary>
    /// The base <see cref="TalkingCard.OnShownForCardSelect(bool)"/> with a frame delay before being called to fix some problems.
    /// </summary>
    public override IEnumerator OnShownForCardSelect(bool forPositiveEffect)
    {
        yield return new WaitForEndOfFrame();
        yield return base.OnShownForCardSelect(forPositiveEffect);
    }
}

/// <summary>
/// An abstract class for the creation of Act 3 talking cards through the API.
/// It inherits from <see cref="DiskTalkingCard"/> and implements <see cref="ITalkingCard"/>.
/// </summary>
public abstract class CustomDiskTalkingCard : DiskTalkingCard, ITalkingCard {
    /// <summary>
    /// ID of the speaker used to determine the voice used when this card talks.
    /// </summary>
    public override DialogueEvent.Speaker SpeakerType => DialogueEvent.Speaker.Stoat;
    /// <summary>
    /// Internal name of the actual talking card.
    /// </summary>
    public abstract string CardName { get; }
    /// <summary>
    /// List of emotions this card can express.
    /// </summary>
    public abstract List<EmotionData> Emotions { get; }
    /// <summary>
    /// Description object for the talking card's portrait.
    /// </summary>
    public abstract FaceInfo FaceInfo { get; }
    /// <summary>
    /// ID of the special ability used to mark the talking card as a talking card.
    /// </summary>
    public abstract SpecialTriggeredAbility DialogueAbility { get; }

    /// <summary>
    /// ID of the <see cref="DialogueEvent"/> to play when the dialogue event for <see cref="TalkingCard.OnDrawnFallbackDialogueId"/> fails to play.
    /// </summary>
    public override string OnDrawnFallbackDialogueId => OnDrawnDialogueId;

    /// <summary>
    /// The base <see cref="TalkingCard.OnShownForCardSelect(bool)"/> with a frame delay before being called to fix some problems.
    /// </summary>
    public override IEnumerator OnShownForCardSelect(bool forPositiveEffect)
    {
        yield return new WaitForEndOfFrame();
        yield return base.OnShownForCardSelect(forPositiveEffect);
        yield break;
    }
}
