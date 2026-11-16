using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.TalkingCards.Helpers;
using System.Reflection;
using UnityEngine;

#nullable enable
#pragma warning disable Publicizer001

namespace InscryptionAPI.TalkingCards.Animation;

/// <summary>
/// Helper class for creating talking card portraits.
/// </summary>
public static class GeneratePortrait
{
    internal static AssetBundle? PortraitBundle;
    private static GameObject? FacePrefab;
    private static Transform? APIPortraits;

    #region FaceInfoDefault
    /// <summary>
    /// Default voice ID.
    /// </summary>
    public const string VoiceId = "female1_voice";
    /// <summary>
    /// Default blink rate.
    /// </summary>
    public const float BlinkRate = 1.5f;
    /// <summary>
    /// Default voice pitch.
    /// </summary>
    public const float VoicePitch = 1f;
    #endregion

    /// <summary>
    /// ID for a dummy talking card special ability.
    /// </summary>
    public static SpecialTriggeredAbility DialogueDummy = CreateDummyAbility();

    #region Sprites
    /// <summary>
    /// Sprite for an empty card portrait.
    /// </summary>
    public static readonly Sprite EmptyPortrait = AssetHelpers.MakeSprite(AssetHelpers.EmptyAndTransparent());

    /// <summary>
    /// Tuple containing two empty card portrait sprites.
    /// </summary>
    public static readonly (Sprite, Sprite) EmptyPortraitTuple
        = (EmptyPortrait, EmptyPortrait);
    #endregion

    #region Init
    private static bool hasInit = false;
    private static void InitTalkingCards()
    {
        LoadPrefab();
        Portrait();
        hasInit = true;
    }
    #endregion

#pragma warning disable CS8600
    internal static GameObject New()
    {
        if (!hasInit) InitTalkingCards();
        GameObject portrait = UnityObject.Instantiate(FacePrefab);
        UnityObject.DontDestroyOnLoad(portrait);
        portrait!.transform.SetParent(APIPortraits);
        return portrait;
    }
#pragma warning restore CS8600

    #region InitGenericPortrait
    private static void LoadPrefab()
    {
        byte[] assetBundle = LoadResource("TalkingCardGenericPrefab");
        PortraitBundle = AssetBundle.LoadFromMemory(assetBundle);
    }

    private static byte[] LoadResource(string resourceName)
    {
        Assembly target = InscryptionAPIPlugin.APIAssembly;
        using (Stream resourceStream = target.GetManifestResourceStream(resourceName))
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                resourceStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    private static SpecialTriggeredAbility CreateDummyAbility()
        => SpecialTriggeredAbilityManager.Add(
                InscryptionAPIPlugin.ModGUID,
                "TalkingCardAPI_",
                typeof(DialogueDummy)
            ).Id;

    private static void Portrait()
    {
        FacePrefab = PortraitBundle!.LoadAsset<GameObject>("TalkingCardGenericPrefab");

        Transform Anim = FacePrefab.transform.Find("Anim");
        Transform Body = Anim.transform.Find("Body");
        Transform Eyes = Body.Find("Eyes");
        Transform Mouth = Body.Find("Mouth");

        CharacterFace face = FacePrefab.AddComponent<CharacterFace>();
        face.anim = Anim.gameObject.GetComponent<Animator>();
        face.eyes = Eyes.gameObject.AddComponent<CharacterEyes>();
        face.mouth = Mouth.gameObject.AddComponent<CharacterMouth>();
        face.face = Body.gameObject.GetComponent<SpriteRenderer>();

        face.face.sprite = EmptyPortrait;
        Eyes.GetComponent<SpriteRenderer>().sprite = EmptyPortrait;
        Mouth.GetComponent<SpriteRenderer>().sprite = EmptyPortrait;

        face.emotionSprites = new List<CharacterFace.EmotionSprites>
         {
            new CharacterFace.EmotionSprites
            {
                emotion = Emotion.Neutral,
                face = EmptyPortrait,
                eyesOpen = EmptyPortrait,
                mouthClosed = EmptyPortrait,
                eyesClosed = EmptyPortrait,
                eyesOpenEmission = EmptyPortrait,
                mouthOpen = EmptyPortrait,
            }};

        face.eyes.blinkRate = BlinkRate;
        face.voiceSoundId = VoiceId;
        face.voiceSoundPitch = VoicePitch;

        int offscreen = LayerMask.NameToLayer("CardOffscreen");
        foreach (Transform t in FacePrefab.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = offscreen;
        }

        FacePrefab.layer = offscreen;

        face.eyes.emissionRenderer = face.eyes.transform.Find("Emission").GetComponent<SpriteRenderer>();
        face.eyes.emissionRenderer.gameObject.layer = LayerMask.NameToLayer("CardOffscreenEmission");
        face.eyes.emissionRenderer.sprite = EmptyPortrait;

        GameObject apiPortraits = new GameObject("API_Portraits");
        GameObject.DontDestroyOnLoad(apiPortraits);
        APIPortraits = apiPortraits.transform;
    }
    #endregion
}
