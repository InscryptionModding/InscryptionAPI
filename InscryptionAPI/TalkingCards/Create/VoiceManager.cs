using HarmonyLib;
using InscryptionAPI.Sound;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards.Create;

[HarmonyPatch]
internal static class VoiceManager
{
    private static readonly List<AudioClip> Voices = new();

    public static readonly string[] ValidVoiceIds = new string[]
    {
        "female1_voice",
        "kobold_voice",
        "cat_voice"
    };

    private static bool IsInvalidVoiceId(string? id)
        => id == null || ValidVoiceIds.Contains(id);

    public static bool Add(string? soundPath, string? id)
    {
        if (string.IsNullOrWhiteSpace(soundPath)) return false;

        if (IsInvalidVoiceId(soundPath))
        {
            LogHelpers.LogError($"Invalid sound path: {soundPath ?? "(null)"}");
            return false;
        }

        if (IsInvalidVoiceId(id))
        {
            LogHelpers.LogError($"Error: Voice ID \"{id ?? "(null)"}\" isn't unique!");
            return false;
        }

        AudioClip voice = SoundManager.LoadAudioClip(soundPath);
        voice.name = id ?? soundPath;
        Voices.Add(voice);
        return true;
    }

    public static bool Add(string? path) => Add(path, path);

    [HarmonyPatch(typeof(AudioController), nameof(AudioController.GetAudioClip))]
    [HarmonyPrefix]
    private static void AddVoiceIds(ref List<AudioClip> ___SFX)
    {
        foreach (AudioClip voice in Voices)
        {
            if (!___SFX.Contains(voice))
            {
                ___SFX.Add(voice);
            }
        }
    }
}
