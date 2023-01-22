using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.TalkingCards.Animation;
using InscryptionAPI.TalkingCards.Helpers;
using UnityEngine;

#nullable enable
#pragma warning disable Publicizer001

namespace InscryptionAPI.TalkingCards.Create;

public static class TalkingCardCreator
{
    internal static List<string> AllDialogueAdded => DialogueDummy.AllDialogueAdded;

    internal static Dictionary<string, GameObject> AnimatedPortraits = new();

    #region AnimatedPortrait
    internal static FaceInfo BasicInfo()
    {
        FaceInfo faceInfo = new FaceInfo(
                GeneratePortrait.BlinkRate,
                GeneratePortrait.VoiceId,
                GeneratePortrait.VoicePitch
            );
        return faceInfo;
    }

    internal static void New(FaceData faceData, SpecialTriggeredAbility talkAbility)
    {
        if (AnimatedPortraits.ContainsKey(faceData.CardName))
        {
            LogHelpers.LogError($"An animated portrait has already been added for card \'{faceData.CardName}\'!");
            return;
        }

        GameObject portrait = GeneratePortrait.New();
        CharacterFace face = portrait.GetComponent<CharacterFace>();
        face.emotionSprites = faceData.Emotions.Select(x => x.MakeEmotion()).ToList();

        FaceInfo faceInfo = faceData.FaceInfo ?? BasicInfo();

        face.eyes.blinkRate = faceInfo.GetBlinkRate();
        face.voiceSoundId = faceInfo.GetVoiceId();
        face.voiceSoundPitch = faceInfo.GetVoicePitch();

        // FileLog.Log($"FaceInfo debug: Card: {faceData.CardName}, voicePitch: {face.voiceSoundPitch} , regular voicePitch variable: {faceInfo.voiceSoundPitch}");

        CardInfo? card = CardHelpers.Get(faceData.CardName);
        if (card == null) return;

        AnimatedPortraits.Add(faceData.CardName, portrait);
        card.AddAppearances(CardAppearanceBehaviour.Appearance.AnimatedPortrait);
        card.AddSpecialAbilities(talkAbility);
    }
    #endregion

    #region Dialogue
    public static void AddToDialogueCache(string? id)
    {
        if (id == null) return;
        AllDialogueAdded.Add(id);
    }
    #endregion

    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.AnimatedPortrait), MethodType.Getter)]
        [HarmonyPostfix]
        private static void GetFace(CardInfo __instance, ref GameObject __result)
        {
            if (!AnimatedPortraits.ContainsKey(__instance.name)) return;

            __instance.animatedPortrait = AnimatedPortraits[__instance.name];
            __result = AnimatedPortraits[__instance.name];
        }
    }
}