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
    internal static Dictionary<string, SpecialTriggeredAbility> TalkingAbilities = new();

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

    internal static void Remove(FaceData faceData)
    {
        if (faceData.CardName == null) return;
        Remove(faceData.CardName);
    }

    internal static void Remove(string cardName)
    {
        if (!AnimatedPortraits.ContainsKey(cardName)) return;

        CardInfo? card = CardHelpers.Get(cardName);
        if (card == null) return;

        card.RemoveAppearances(CardAppearanceBehaviour.Appearance.AnimatedPortrait);
        card.RemoveSpecialAbilities(TalkingAbilities[cardName]);

        GameObject.Destroy(AnimatedPortraits[cardName]);
        AnimatedPortraits.Remove(cardName);
        TalkingAbilities.Remove(cardName);
    }

    internal static void New(FaceData faceData, SpecialTriggeredAbility talkAbility, bool diskTalkingCard)
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
        TalkingAbilities.Add(faceData.CardName, talkAbility);
        card.AddSpecialAbilities(talkAbility);

        // Special behaviour for the talking cards
        if (diskTalkingCard)
        {
            card.AddAppearances(CardAppearanceBehaviour.Appearance.DynamicPortrait);
            foreach (var rend in card.AnimatedPortrait.GetComponentsInChildren<SpriteRenderer>())
                rend.color = new(0f, 0.755f, 1f);

            RecursiveSetLayer(card.AnimatedPortrait, "CardOffscreenEmission");

            // Rescale the portrait for tech cards
            card.AnimatedPortrait.transform.localScale = new(1f, 1f, 1f);
            card.AnimatedPortrait.transform.Find("Anim/Body").localPosition = new(0f, 0.2f, 0f);

            // Add the dialogue text displayer
            GameObject.Instantiate(CardLoader.GetCardByName("Angler_Talking").AnimatedPortrait.transform.Find("DialogueText").gameObject, card.AnimatedPortrait.transform);
        }
        else
        {
            card.AddAppearances(CardAppearanceBehaviour.Appearance.AnimatedPortrait);
        }
    }
    internal static void New(FaceData faceData, SpecialTriggeredAbility talkAbility)
    {
        New(faceData, talkAbility, CardHelpers.Get(faceData.CardName)?.temple == CardTemple.Tech);
    }

    private static void RecursiveSetLayer(GameObject obj, string layerName)
    {
        if (obj == null)
            return;

        obj.layer = LayerMask.NameToLayer(layerName);
        for (int i = 0; i < obj.transform.childCount; i++)
            RecursiveSetLayer(obj.transform.GetChild(i)?.gameObject, layerName);
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