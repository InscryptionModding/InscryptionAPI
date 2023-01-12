using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DiskCardGame;
using InscryptionAPI.TalkingCards.Animation;
using InscryptionAPI.TalkingCards.Create;
using InscryptionAPI.TalkingCards.Helpers;

namespace InscryptionAPI.TalkingCards.JSON;

[Serializable]
public class TalkingJSONData
{
    public string cardName { get; set; }
    public string faceSprite { get; set; }
    public FaceImages? eyeSprites { get; set; }
    public FaceImages? mouthSprites { get; set; }
    public string? emissionSprite { get; set; }
    public EmotionImages[]? emotions { get; set; }

    public FaceInfo? faceInfo { get; set; }
    public DialogueEventStrings[] dialogueEvents { get; set; }

    public TalkingJSONData(string cardName, string faceSprite,
        FaceImages? eyeSprites = null, FaceImages? mouthSprites = null,
        string? emissionSprite = null, EmotionImages[]? emotions = null,
        FaceInfo? faceInfo = null, DialogueEventStrings[]? dialogueEvents = null)
    {
        this.cardName = cardName;
        this.faceSprite = faceSprite;
        this.eyeSprites = eyeSprites;
        this.mouthSprites = mouthSprites;
        this.emissionSprite = emissionSprite;
        this.emotions = emotions;
        this.faceInfo = faceInfo;
        this.dialogueEvents = dialogueEvents ?? new DialogueEventStrings[0];
    }

    public List<EmotionData> GetEmotions()
    {
        List<EmotionData> emotionList = new();

        #region NeutralEmotion
        Emotion neutralEmotion = Emotion.Neutral;
        Sprite face = AssetHelpers.MakeSprite(faceSprite)
            ?? GeneratePortrait.EmptyPortrait;
        FaceAnim eyes = eyeSprites?.GetSprites()
            ?? GeneratePortrait.EmptyPortraitTuple;
        FaceAnim mouth = mouthSprites?.GetSprites()
            ?? GeneratePortrait.EmptyPortraitTuple;
        Sprite emission = AssetHelpers.MakeSprite(emissionSprite)
            ?? GeneratePortrait.EmptyPortrait;

        EmotionData neutral = new(neutralEmotion, face, eyes, mouth, emission);

        emotionList.Add(neutral);
        #endregion

        IEnumerable<EmotionData?>? moreEmotions = emotions
            ?.Select(x => x.MakeEmotion(neutral));
        if (moreEmotions == null) return emotionList;

        foreach(EmotionData? data in moreEmotions)
        {
            if (data != null) emotionList.Add(data);            
        }
        return emotionList;
    }

    public FaceData GetFaceData() => new(cardName, GetEmotions(), faceInfo);
    public List<DialogueEvent?> MakeDialogueEvents()
        => dialogueEvents.Select(x => x?.CreateEvent(cardName)).ToList();
}

[Serializable]
public class FaceImages
{
    public string? open { get; set; }
    public string? closed { get; set; }

    public FaceImages(string open, string? closed)
    {
        this.open = open;
        this.closed = closed;
    }

    public (Sprite open, Sprite closed) GetSprites()
    {
        Sprite open = AssetHelpers.MakeSprite(this.open) ?? GeneratePortrait.EmptyPortrait;
        Sprite? closed = AssetHelpers.MakeSprite(this.closed);
        return (open, closed ?? open);
    }

    public static implicit operator FaceImages((string open, string closed) x)
        => new FaceImages(x.open, x.closed);
}

[Serializable]
public class EmotionImages
{
    public string emotion { get; set; }
    public string? faceSprite { get; set; }
    public FaceImages? eyeSprites { get; set; }
    public FaceImages? mouthSprites { get; set; }
    public string? emissionSprite { get; set; }

    public EmotionImages(string emotion, string? faceSprite = null, FaceImages? eyeSprites = null, FaceImages? mouthSprites = null, string? emissionSprite = null)
    {
        this.emotion = emotion;
        this.faceSprite = faceSprite;
        this.eyeSprites = eyeSprites;
        this.mouthSprites = mouthSprites;
        this.emissionSprite = emissionSprite;
    }

    public EmotionData? MakeEmotion(EmotionData neutralEmotion)
    {
        // Emotion
        Emotion emotionValue = (Emotion)Enum.Parse(typeof(Emotion), emotion.SentenceCase());
        if (emotionValue == Emotion.Neutral) return null;

        // Sprites
        Sprite face = AssetHelpers.MakeSprite(faceSprite)
            ?? neutralEmotion.Face;
        
        FaceAnim eyes = eyeSprites != null
            ? eyeSprites.GetSprites()
            : neutralEmotion.Eyes;
        
        FaceAnim mouth = mouthSprites != null
            ? mouthSprites.GetSprites()
            : neutralEmotion.Mouth;
       
        Sprite emission = AssetHelpers.MakeSprite(emissionSprite)
            ?? neutralEmotion.Emission;

        return new EmotionData(emotionValue, face, eyes, mouth, emission);
    }
}