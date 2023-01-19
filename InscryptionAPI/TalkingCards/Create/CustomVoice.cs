using HarmonyLib;
using InscryptionAPI.Sound;
using UnityEngine;

#nullable enable
namespace InscryptionAPI.TalkingCards.Create;

[HarmonyPatch]
internal static class CustomVoice
{
    private static readonly List<AudioClip> _voices = new();

    // This is all you have to care about!! 
    public static void RegisterVoice(string filename) // <-- The name of your file. Pass it as argument.
    {
        AudioClip audio = SoundManager.LoadAudioClip(filename);
        audio.name = filename;
        _voices.Add(audio);
    }

    [HarmonyPatch(typeof(AudioController), nameof(AudioController.GetAudioClip))]
    [HarmonyPrefix]
    private static void AddVoiceIds(ref List<AudioClip> ___SFX)
    {
        foreach (AudioClip voice in _voices)
        {
            if (!___SFX.Contains(voice))
            {
                ___SFX.Add(voice);
            }
        }
    }
}
