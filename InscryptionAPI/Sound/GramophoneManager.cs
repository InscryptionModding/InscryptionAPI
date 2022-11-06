using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using BepInEx;
using UnityEngine.SceneManagement;

namespace InscryptionAPI.Sound;

[HarmonyPatch]
public static class GramophoneManager
{
    private static string APIGuid = InscryptionAPIPlugin.ModGUID;
    internal static int TrackIndex
    {
        get { return ModdedSaveManager.SaveData.GetValueAsInt(APIGuid, "GramophoneIndex");  }
        set { ModdedSaveManager.SaveData.SetValue(APIGuid, "GramophoneIndex", value);       }
    }

    internal static List<AudioClip> NewGramophoneTracks = new List<AudioClip>();

    internal static List<TrackInfo> TracksToAdd = new List<TrackInfo>();
    internal static List<string> AlreadyAddedTracks = new List<string>();
    private static bool noNewTracks => NewGramophoneTracks.Count == 0;
    private static bool isLeshyCabin => SceneManager.GetActiveScene().name == "Part1_Cabin";

    public class TrackInfo
    {
        public string FilePath, Guid;
        public bool CanSkip;
        public string AudioClipName => Guid + TrackName;
        public string TrackName => Path.GetFileName(FilePath);
        public TrackInfo(string guid, string filePath, bool canSkip = false)
        {
            Guid = guid ?? string.Empty;
            FilePath = filePath;
            CanSkip = canSkip;
        }
    }

    private static void InfoLog(string message) =>
        InscryptionAPIPlugin.Logger.LogInfo($"GramophoneManager: {message}");

    private static void ErrorLog(string message) =>
        InscryptionAPIPlugin.Logger.LogError($"GramophoneManager: {message}");

    public static void AddTrack(string guid, string filename)
    {
        string path = SoundManager.GetAudioFile(filename);
        if (path.IsNullOrWhiteSpace())
        {
            ErrorLog($"Couldn't load audio track: File {filename} not found!");
            return;
        }
        TrackInfo trackInfo = new TrackInfo(guid, path);
        TracksToAdd.Add(trackInfo);
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    private static void LoadGramophoneTracks()
    {
        if (TracksToAdd.Count == 0) return;

        InfoLog(TrackIndex.ToString());

        List<TrackInfo> newTracks = TracksToAdd
            .Where(x => !AlreadyAddedTracks.Contains(x.AudioClipName))
            .ToList();

        if (newTracks.Count == 0) return;

        List<AudioClip> audioTracks = newTracks
            .Select(x => SoundManager.LoadAudioClip(x))
            .ToList();

        foreach (AudioClip track in audioTracks)
        {
            if (track == null) continue;
            if (!NewGramophoneTracks.Contains(track))
            {
                NewGramophoneTracks.Add(track);
                GramophoneInteractable.TRACK_VOLUMES.Add(1f);
                GramophoneInteractable.TRACK_IDS.Add(track.name);

                AlreadyAddedTracks.Add(track.name);
            }
        }
    }

    [HarmonyPatch(typeof(AudioController), nameof(AudioController.GetLoop))]
    [HarmonyPrefix]
    private static void PatchGetLoop(List<AudioClip> ___Loops)
    {
        if (noNewTracks || !isLeshyCabin) return;

        foreach (AudioClip track in NewGramophoneTracks)
        {
            if (!___Loops.Contains(track))
            {
                ___Loops.Add(track);
            }
        }
    }

    [HarmonyPatch(typeof(AudioController), nameof(AudioController.GetLoopClip))]
    [HarmonyPrefix]
    private static void PatchGetLoopClip(List<AudioClip> ___Loops)
    {
        if (noNewTracks || !isLeshyCabin) return;

        foreach (AudioClip track in NewGramophoneTracks)
        {
            if (!___Loops.Contains(track))
            {
                ___Loops.Add(track);
            }
        }
    }

    [HarmonyPatch(typeof(GramophoneInteractable), nameof(GramophoneInteractable.PlaySavedTrack))]
    [HarmonyPrefix]
    private static void PatchTrackIndex()
    {
        int index = AscensionSaveData.Data.gramophoneTrackIndex;
        if (index < 0 || index >= GramophoneInteractable.TRACK_IDS.Count)
        {
            AscensionSaveData.Data.gramophoneTrackIndex = 0;
            TrackIndex = 0;
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveToFile))]
    [HarmonyPrefix]
    private static void PatchSaveToFile_Prefix(ref int __state)
    {
        if (noNewTracks) return;
        __state = AscensionSaveData.Data.gramophoneTrackIndex;
        TrackIndex = __state;
        AscensionSaveData.Data.gramophoneTrackIndex = 0;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveToFile))]
    [HarmonyPostfix]
    private static void PatchSaveToFile_Postfix(ref int __state)
    {
        if (noNewTracks) return;
        AscensionSaveData.Data.gramophoneTrackIndex = __state;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadFromFile))]
    [HarmonyPostfix]
    private static void PatchLoadFromFile()
    {
        AscensionSaveData.Data.gramophoneTrackIndex = TrackIndex;
    }
}
