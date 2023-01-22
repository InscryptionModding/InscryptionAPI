using DiskCardGame;
using InscryptionAPI.TalkingCards.Animation;
using InscryptionAPI.TalkingCards.Helpers;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards.Create;

/// <summary>
/// Data for the creation of a talking card through this API.
/// </summary>
public class FaceData
{
    /// <summary>
    /// The name of an existing card.
    /// </summary>
    public string CardName { get; }

    /// <summary>
    /// Your talking card's emotions.
    /// </summary>
    public List<EmotionData> Emotions { get; }
    public EmotionData Neutral => Emotions.Find(x => x.Emotion == Emotion.Neutral);

    /// <summary>
    /// A bit of info about your talking card: blink rate and voice.
    /// </summary>
    public FaceInfo? FaceInfo { get; set; }

    public FaceData(string cardName, List<EmotionData> emotions, FaceInfo? faceInfo)
    {
        CardName = cardName;
        Emotions = emotions;
        FaceInfo = faceInfo;
    }
}

/// <summary>
/// A bit of information about a talking card: blink rate and voice.
/// </summary>
[Serializable]
public class FaceInfo
{
    private static string[] ValidVoiceIds => VoiceManager.ValidVoiceIds;

    /* Yes, I know these names should be capitalized.
     * However, camelCase looks better in JSON files, so I'm gonna make this compromise. :'3 */

    /// <summary>
    /// How often your character should blink.
    /// </summary>
    public float? blinkRate { get; set; }

    /// <summary>
    /// Your character's voice ID.
    /// </summary>
    public string? voiceId { get; set; }

    /// <summary>
    /// Your character's voice's pitch.
    /// </summary>
    public float? voiceSoundPitch { get; set; }

    /// <summary>
    /// The path to a short audio file to be used as your character's voice.
    /// </summary>
    public string? customVoice { get; set; }

    public FaceInfo(float? blinkRate = null, string? voiceId = null, float? voiceSoundPitch = null, string? customVoice = null)
    {
        this.blinkRate = blinkRate;
        this.voiceId = voiceId;
        this.voiceSoundPitch = voiceSoundPitch;
        this.customVoice = customVoice;
    }

    internal float GetBlinkRate()
        => Mathf.Clamp(blinkRate ?? GeneratePortrait.BlinkRate, 0.1f, 10f);

    internal float GetVoicePitch()
        => Mathf.Clamp(voiceSoundPitch ?? GeneratePortrait.VoicePitch, 0.1f, 10f);

    private string? CustomVoice()
    {
        bool result = VoiceManager.Add(customVoice);
        return result ? customVoice : null;
    }

    internal string GetVoiceId()
        => CustomVoice()
        ?? (
            (voiceId != null && ValidVoiceIds.Contains(voiceId))
            ? voiceId
            : ValidVoiceIds.First()
        );
}

/// <summary>
/// A talking card's emotion (a facial expression, basically). A container for sprites.
/// </summary>
public class EmotionData
{
    /// <summary>
    /// The chosen emotion.
    /// </summary>
    public Emotion Emotion { get; }

    /// <summary>
    /// A sprite for your talking card's face.
    /// </summary>
    public Sprite Face { get; }

    /// <summary>
    /// A pair of sprites for your talking card's eyes: open and closed, respectively.
    /// </summary>
    public FaceAnim Eyes { get; }

    /// <summary>
    /// A pair of sprites for your talking card's mouth: open and closed, respectively.
    /// </summary>
    public FaceAnim Mouth { get; }

    /// <summary>
    /// A pair of sprites for your talking card's emission: open and closed, respectively.
    /// </summary>
    public FaceAnim Emission { get; }

    public EmotionData(Emotion emotion, Sprite face, FaceAnim eyes, FaceAnim mouth, FaceAnim emission)
    {
        Emotion = emotion;
        Face = face.PivotBottom();
        Eyes = eyes;
        Mouth = mouth;
        Emission = emission;
    }


    #region Constructors
    public EmotionData(Emotion emotion, string? face, (string? open, string? closed)? eyes, (string? open, string? closed)? mouth, (string? open, string? closed)? emission)
    {
        Emotion = emotion;

        Face = AssetHelpers.MakeSprite(face)
            ?? GeneratePortrait.EmptyPortrait;

        Eyes = eyes != null
            ? AssetHelpers.MakeSpriteTuple(eyes)
            : GeneratePortrait.EmptyPortraitTuple;

        Mouth = mouth != null
            ? AssetHelpers.MakeSpriteTuple(mouth)
            : GeneratePortrait.EmptyPortraitTuple;

        Emission = emission != null
            ? AssetHelpers.MakeSpriteTuple(emission)
            : GeneratePortrait.EmptyPortraitTuple;
    }


    public EmotionData(string? emotion, string? face, (string? open, string? closed)? eyes, (string? open, string? closed)? mouth, (string? open, string? closed)? emission)
    {
        Emotion = AssetHelpers.ParseAsEnumValue<Emotion>(emotion?.SentenceCase());

        Face = AssetHelpers.MakeSprite(face)
            ?? GeneratePortrait.EmptyPortrait;

        Eyes = eyes != null
            ? AssetHelpers.MakeSpriteTuple(eyes)
            : GeneratePortrait.EmptyPortraitTuple;

        Mouth = mouth != null
            ? AssetHelpers.MakeSpriteTuple(mouth)
            : GeneratePortrait.EmptyPortraitTuple;

        Emission = emission != null
            ? AssetHelpers.MakeSpriteTuple(emission)
            : GeneratePortrait.EmptyPortraitTuple;
    }
    #endregion

    internal CharacterFace.EmotionSprites MakeEmotion() => new()
    {
        emotion = Emotion,
        face = Face,

        eyesOpen = Eyes.Open,
        eyesClosed = Eyes.Closed,

        mouthOpen = Mouth.Open,
        mouthClosed = Mouth.Closed,

        eyesOpenEmission = Emission.Open,
        eyesClosedEmission = Emission.Closed
    };

    public static implicit operator EmotionData((Emotion emotion, Sprite sprite) x)
        => new(
                x.emotion,
                x.sprite,
                GeneratePortrait.EmptyPortraitTuple,
                GeneratePortrait.EmptyPortraitTuple,
                GeneratePortrait.EmptyPortraitTuple
            );
}

/// <summary>
/// A container for sprites for a part of a talking card's face.
/// </summary>
public class FaceAnim
{
    public Sprite Open { get; }
    public Sprite Closed { get; }

    public FaceAnim(Sprite? open, Sprite? closed)
    {
        Open = open?.PivotBottom() ?? GeneratePortrait.EmptyPortrait;
        Closed = closed?.PivotBottom() ?? GeneratePortrait.EmptyPortrait;
    }

    public FaceAnim(string? open, string? closed)
    {
        Open = AssetHelpers.MakeSprite(open) ?? GeneratePortrait.EmptyPortrait;
        Closed = AssetHelpers.MakeSprite(closed) ?? GeneratePortrait.EmptyPortrait;
    }

    public static implicit operator Sprite(FaceAnim x)
        => x.Closed;

    public static implicit operator FaceAnim((Sprite? open, Sprite? closed) x)
        => new FaceAnim(x.open, x.closed);
}