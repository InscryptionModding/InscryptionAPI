using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using BepInEx;
using BepInEx.Logging;
using System.Xml.Serialization;
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

        // Track index patch
        // AscensionSaveData.Data.gramophoneTrackIndex = TrackIndex;
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
        if (noNewTracks) return;

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
        if (noNewTracks) return;

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

        // if (noNewTracks) return;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveToFile))]
    [HarmonyPrefix]
    private static void PatchSaveToFile_Prefix(ref int __state)
    {
        if (noNewTracks || !isLeshyCabin) return;
        __state = AscensionSaveData.Data.gramophoneTrackIndex;
        /*if(__state < 8)
        {
            TrackIndex = __state;
        }*/
        TrackIndex = __state;
        AscensionSaveData.Data.gramophoneTrackIndex = 0;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveToFile))]
    [HarmonyPostfix]
    private static void PatchSaveToFile_Postfix(ref int __state)
    {
        if (noNewTracks || !isLeshyCabin) return;
        AscensionSaveData.Data.gramophoneTrackIndex = __state;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadFromFile))]
    [HarmonyPostfix]
    private static void PatchLoadFromFile()
    {
        AscensionSaveData.Data.gramophoneTrackIndex = TrackIndex;
        InfoLog(TrackIndex.ToString());
    }


    private static IEnumerator GramophoneAudioClip_Async(string guid, string path, AudioType audioType)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                ErrorLog($"Error loading file {Path.GetFileName(path)} as AudioClip!");
                ErrorLog(www.error);
                // TODO: Log a proper more descriptive error here.
            }
            else
            {
                AudioClip audioclip = DownloadHandlerAudioClip.GetContent(www);
                audioclip.name = guid + Path.GetFileName(path);
                NewGramophoneTracks.Add(audioclip);
            }
        }
    }
}
