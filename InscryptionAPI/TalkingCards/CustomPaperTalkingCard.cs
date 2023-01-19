using DiskCardGame;
using InscryptionAPI.TalkingCards.Create;
using System.Collections;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards;

/// <summary>
/// An abstract class for the creation of talking cards through this API.
/// It inherits from PaperTalkingCard and implements ITalkingCard.
/// </summary>
public abstract class CustomPaperTalkingCard : PaperTalkingCard, ITalkingCard
{
    public override DialogueEvent.Speaker SpeakerType => DialogueEvent.Speaker.Stoat;
    public abstract string CardName { get; }
    public abstract List<EmotionData> Emotions { get; }
    public abstract FaceInfo FaceInfo { get; }
    public abstract SpecialTriggeredAbility DialogueAbility { get; }

    public override string OnDrawnFallbackDialogueId => OnDrawnDialogueId;
    public override IEnumerator OnShownForCardSelect(bool forPositiveEffect)
    {
        yield return new WaitForEndOfFrame();
        yield return base.OnShownForCardSelect(forPositiveEffect);
        yield break;
    }
}
