using DiskCardGame;
using InscryptionAPI.TalkingCards.Animation;
using InscryptionAPI.TalkingCards.Create;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards;

public static class TalkingCardManager
{
    // Add a talking card created through this API.
    // I'm using an interface so that it can all be kept in the same class. c:

    public static Sprite EmptyPortrait => GeneratePortrait.EmptyPortrait;

    /// <summary>
    /// Creates a talking card through this API.
    /// </summary>
    /// <typeparam name="T">A type that implements the ITalkingCard interface.</typeparam>
    public static void New<T>() where T : ITalkingCard, new()
    {
        ITalkingCard x = new T();

        FaceData faceData = new(
                x.CardName,
                x.Emotions,
                x.FaceInfo
            );

        TalkingCardCreator.New(faceData, x.DialogueAbility);
    }

    /// <summary>
    /// Create a talking card from a FaceData instance through this API.
    /// </summary>
    /// <param name="faceData">Your character's face data.</param>
    /// <param name="ability">The ability containing your character's dialogue events.</param>
    public static void Create(FaceData faceData, SpecialTriggeredAbility ability)
    {
        if (faceData?.CardName == null) return;
        TalkingCardCreator.New(faceData, ability);
    }
}