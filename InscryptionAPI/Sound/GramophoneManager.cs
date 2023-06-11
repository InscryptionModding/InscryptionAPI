using BepInEx;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InscryptionAPI.Sound;

/// <summary>
/// A static class that contains helper methods for adding tracks to the Gramophone in Leshy's cabin.
/// </summary>
[HarmonyPatch]
public static class GramophoneManager
{
    private static string APIGuid = InscryptionAPIPlugin.ModGUID;
    internal static int TrackIndex
    {
        get { return ModdedSaveManager.SaveData.GetValueAsInt(APIGuid, "GramophoneIndex"); }
        set { ModdedSaveManager.SaveData.SetValue(APIGuid, "GramophoneIndex", value); }
    }

    internal static List<AudioClip> NewGramophoneTracks = new List<AudioClip>();

    internal static List<TrackInfo> TracksToAdd = new List<TrackInfo>();

    internal static List<string> AlreadyAddedTracks = new List<string>();
    private static bool noNewTracks => NewGramophoneTracks.Count == 0;
    private static bool isLeshyCabin => SceneManager.GetActiveScene().name == "Part1_Cabin";

    internal class TrackInfo
    {
        public string FilePath, Guid;
        public AudioClip Clip;
        public float Volume;
        // public bool CanSkip; // Unused at the time! 
        public string AudioClipName => $"{Guid}_{TrackName}";
        public string TrackName => Clip != null ? Clip.name : Path.GetFileName(FilePath);

        public TrackInfo(string guid, string filePath, float volume = 1f)
        {
            Guid = guid ?? string.Empty;
            FilePath = filePath;
            Volume = Mathf.Clamp(volume, 0, 1f);
        }

        public TrackInfo(string guid, AudioClip clip, float volume = 1f)
        {
            Guid = guid ?? string.Empty;
            Clip = clip;
            Volume = Mathf.Clamp(volume, 0, 1f);
        }
    }

    private static void InfoLog(string message) =>
        InscryptionAPIPlugin.Logger.LogInfo($"GramophoneManager: {message}");

    private static void ErrorLog(string message) =>
        InscryptionAPIPlugin.Logger.LogError($"GramophoneManager: {message}");

    /// <summary>
    /// A helper for adding a music track to the Gramophone in Leshy's cabin.
    /// </summary>
    /// <param name="guid">Your plugin's GUID.</param>
    /// <param name="path">The name of the audio file.</param>
    /// <param name="volume">The volume of your track, from 0 to 1.</param>
    public static void AddTrack(string guid, string path, float volume = 1f)
    {
        string filePath = Path.IsPathRooted(path) ? path : SoundManager.GetAudioPath(path);

        if (filePath.IsNullOrWhiteSpace())
        {
            ErrorLog($"Couldn't load audio track: File \'{filePath ?? "(null)"}\' not found!");
            return;
        }
        TrackInfo trackInfo = new TrackInfo(guid, filePath, volume);
        TracksToAdd.Add(trackInfo);
    }

    /// <summary>
    /// A helper for adding a music track to the Gramophone in Leshy's cabin.
    /// </summary>
    /// <param name="guid">Your plugin's GUID.</param>
    /// <param name="clip">The AudioClip of your track.</param>
    /// <param name="volume">The volume of your track, from 0 to 1.</param>
    public static void AddTrack(string guid, AudioClip clip, float volume = 1f)
    {
        TrackInfo trackInfo = new TrackInfo(guid, clip, volume);
        TracksToAdd.Add(trackInfo);
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    private static void LoadGramophoneTracks()
    {
        if (TracksToAdd.Count == 0) return;

        //InfoLog(TrackIndex.ToString());

        List<TrackInfo> newTracks = TracksToAdd
            .Where(x => !AlreadyAddedTracks.Contains(x.AudioClipName))
            .ToList();

        if (newTracks.Count == 0) return;

        foreach (TrackInfo info in newTracks)
        {
            AudioClip track = SoundManager.LoadAudioClip(info);
            if (track == null) continue;
            if (!NewGramophoneTracks.Contains(track))
            {
                NewGramophoneTracks.Add(track);
                GramophoneInteractable.TRACK_IDS.Add(track.name);
                GramophoneInteractable.TRACK_VOLUMES.Add(info.Volume);

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
        if (noNewTracks) return;
        AscensionSaveData.Data.gramophoneTrackIndex = TrackIndex;
    }
}
