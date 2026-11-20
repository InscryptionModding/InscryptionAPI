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
    /// <summary>
    /// The <see cref="EmotionData"/> for the neutral emotion expression.
    /// </summary>
    public EmotionData Neutral => Emotions.Find(x => x.Emotion == Emotion.Neutral);

    /// <summary>
    /// A bit of info about your talking card: blink rate and voice.
    /// </summary>
    public FaceInfo? FaceInfo { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="FaceData"/>.
    /// </summary>
    /// <param name="cardName">The internal name of the talking card this data is for.</param>
    /// <param name="emotions">All emotions the talking card can express.</param>
    /// <param name="faceInfo">The description object for the talking card's animated face.</param>
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
    /// How often your character should blink, measured in seconds.
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

    /// <summary>
    /// Creates a new instance of <see cref="FaceInfo"/>.
    /// </summary>
    /// <param name="blinkRate">How many seconds must pass between each blink.</param>
    /// <param name="voiceId">The ID of the voice this talking card should use. Represents an audio file.</param>
    /// <param name="voiceSoundPitch">How much the talking card's voice should be pitched up or down.</param>
    /// <param name="customVoice">The ID of the custom voice this talking card should use, if any.</param>
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

    /// <summary>
    /// Creates a new instance of <see cref="EmotionData"/>.
    /// </summary>
    /// <param name="emotion">The emotion this data is used for.</param>
    /// <param name="face">The face sprite this emotion will use.</param>
    /// <param name="eyes">The eye sprites this emotion will use.</param>
    /// <param name="mouth">The mouth sprites this emotion will use.</param>
    /// <param name="emission">The emission sprites this emotion will use.</param>
    public EmotionData(Emotion emotion, Sprite face, FaceAnim eyes, FaceAnim mouth, FaceAnim emission)
    {
        Emotion = emotion;
        Face = face.PivotBottom();
        Eyes = eyes;
        Mouth = mouth;
        Emission = emission;
    }


    #region Constructors
    /// <summary>
    /// Creates a new instance of <see cref="EmotionData"/>.
    /// </summary>
    /// <param name="emotion">The emotion this data is used for.</param>
    /// <param name="face">File path for the face sprite for this emotion.</param>
    /// <param name="eyes"><see cref="Tuple"/> of file paths for the open and closed eye sprites for this emotion.</param>
    /// <param name="mouth"><see cref="Tuple"/> of file paths for the open and closed mouth sprites for this emotion.</param>
    /// <param name="emission"><see cref="Tuple"/> of file paths for the open and closed eye emission sprites for this emotion.</param>
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

    /// <summary>
    /// Creates a new instance of <see cref="EmotionData"/>.
    /// </summary>
    /// <param name="emotion">The emotion this data is used for.</param>
    /// <param name="face">File path for the face sprite for this emotion.</param>
    /// <param name="eyes"><see cref="Tuple"/> of file paths for the open and closed eye sprites for this emotion.</param>
    /// <param name="mouth"><see cref="Tuple"/> of file paths for the open and closed mouth sprites for this emotion.</param>
    /// <param name="emission"><see cref="Tuple"/> of file paths for the open and closed eye emission sprites for this emotion.</param>
    /// <remarks>Intended for use with JSONLoader, please use the other constructors if using C#.</remarks>
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

    /// <summary>
    /// Implicit operator to convert a <see cref="Tuple"/> of a <see cref="DiskCardGame.Emotion"/> and sprite into an instance of <see cref="FaceData"/>. 
    /// </summary>
    /// <param name="x"><see cref="Tuple"/> of a <see cref="DiskCardGame.Emotion"/> and sprite.</param>
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
public class FaceAnim {
    /// <summary>
    /// Sprite for when this the face is 'open' (ie., eyes and mouth).
    /// </summary>
    public Sprite Open { get; }
    /// <summary>
    /// Sprite for when this the face is 'close' (ie., eyes and mouth).
    /// </summary>
    public Sprite Closed { get; }

    /// <summary>
    /// Creates a new instance of <see cref="FaceAnim"/>.
    /// </summary>
    /// <param name="open">The sprite for the open sprite. If invalid, creates an empty portrait.</param>
    /// <param name="closed">The sprite for the closed sprite. If invalid, creates an empty portrait.</param>
    public FaceAnim(Sprite? open, Sprite? closed)
    {
        Open = open?.PivotBottom() ?? GeneratePortrait.EmptyPortrait;
        Closed = closed?.PivotBottom() ?? GeneratePortrait.EmptyPortrait;
    }

    /// <summary>
    /// Creates a new instance of <see cref="FaceAnim"/>.
    /// </summary>
    /// <param name="open">The file path for the <see cref="Open"/> sprite. If invalid, creates an empty portrait.</param>
    /// <param name="closed">The file path for the <see cref="Closed"/> sprite. If invalid, creates an empty portrait.</param>
    public FaceAnim(string? open, string? closed)
    {
        Open = AssetHelpers.MakeSprite(open) ?? GeneratePortrait.EmptyPortrait;
        Closed = AssetHelpers.MakeSprite(closed) ?? GeneratePortrait.EmptyPortrait;
    }

    /// <summary>
    /// Implicit operator to convert this <see cref="FaceAnim"/> to a sprite. Converts it to its <see cref="Closed"/> sprite.
    /// </summary>
    /// <param name="x">The <see cref="FaceAnim"/> being converted.</param>
    public static implicit operator Sprite(FaceAnim x) => x.Closed;

    /// <summary>
    /// Implicit operator to convert a <see cref="Tuple"/> of two sprite representing the open and closed parts of this <see cref="FaceAnim"/> respectively.
    /// </summary>
    /// <param name="x"><see cref="Tuple"/> of two sprite representing the open and closed parts of this <see cref="FaceAnim"/> respectively.</param>
    public static implicit operator FaceAnim((Sprite? open, Sprite? closed) x)
        => new FaceAnim(x.open, x.closed);
}