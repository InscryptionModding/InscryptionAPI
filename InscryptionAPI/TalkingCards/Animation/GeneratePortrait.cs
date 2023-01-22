using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.TalkingCards.Helpers;
using System.Reflection;
using UnityEngine;

#nullable enable
#pragma warning disable Publicizer001

namespace InscryptionAPI.TalkingCards.Animation;

public static class GeneratePortrait
{
    internal static AssetBundle? PortraitBundle;
    private static GameObject? FacePrefab;
    private static Transform? APIPortraits;

    #region FaceInfoDefault
    public const string VoiceId = "female1_voice";
    public const float BlinkRate = 1.5f;
    public const float VoicePitch = 1f;
    #endregion

    public static SpecialTriggeredAbility DialogueDummy = CreateDummyAbility();

    #region Sprites
    public static readonly Sprite EmptyPortrait
        = AssetHelpers.MakeSprite(AssetHelpers.EmptyAndTransparent());

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
        GameObject portrait = GameObject.Instantiate(FacePrefab);
        GameObject.DontDestroyOnLoad(portrait);
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
        Assembly target = typeof(InscryptionAPIPlugin).Assembly;
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
