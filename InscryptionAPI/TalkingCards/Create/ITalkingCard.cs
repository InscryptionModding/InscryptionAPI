using DiskCardGame;
using System.Collections.Generic;

namespace InscryptionAPI.TalkingCards.Create;

// A class that wants to create a talking card through this API *must* implement for this interface.
// That's all the class needs to do. c:

public interface ITalkingCard
{
    public string CardName { get; }
    public List<EmotionData> Emotions { get; }
    public FaceInfo FaceInfo { get; }
    public SpecialTriggeredAbility DialogueAbility { get; }
}
