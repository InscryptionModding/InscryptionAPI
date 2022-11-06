using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using HarmonyLib;
using DiskCardGame;

namespace InscryptionAPI.Sound;

[HarmonyPatch]
internal class GramophoneManager
{
    public static List<AudioClip> NewGramophoneTracks = new List<AudioClip>();

    public static List<TrackInfo> TracksToAdd = new List<TrackInfo>();

    public class TrackInfo
    {
        public string TrackName, Guid;
        public string GuidAndTrackName => Guid + TrackName;
        public TrackInfo(string trackName, string guid)
        {
            TrackName = trackName;
            Guid = guid ?? string.Empty;
        }
    }

    private static void InfoLog(string message) =>
        InscryptionAPIPlugin.Logger.LogInfo($"GramophoneManager: {message}");
    private static void ErrorLog(string message) =>
        InscryptionAPIPlugin.Logger.LogError($"GramophoneManager: {message}");

    public static void AddTrack(string guid, string filename)
    {
        string path = SoundManager.GetAudioFile(filename);
        TrackInfo trackInfo = new TrackInfo(guid, path);
        TracksToAdd.Add(trackInfo);
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    private static void LoadGramophoneTracks()
    {
        var audioTracks = TracksToAdd.Select(x => SoundManager.LoadAudioClip(x.TrackName, x.Guid)).ToList();
        NewGramophoneTracks.AddRange(audioTracks);

        // Add to TRACK_IDS
        foreach(TrackInfo x in TracksToAdd)
        {
            GramophoneInteractable.TRACK_IDS.Add(x.GuidAndTrackName);
            GramophoneInteractable.TRACK_VOLUMES.Add(1f);
        }
    }

    [HarmonyPatch(typeof(AudioController), nameof(AudioController.GetLoop))]
    [HarmonyPrefix]
    static void PatchGetLoop(List<AudioClip> ___Loops)
    {
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
    static void PatchGetLoopClip(List<AudioClip> ___Loops)
    {
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
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveToFile))]
    [HarmonyPrefix]
    static void PatchSaveToFile_Prefix(ref int __state)
    {
        __state = AscensionSaveData.Data.gramophoneTrackIndex;
        AscensionSaveData.Data.gramophoneTrackIndex = 0;
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveToFile))]
    [HarmonyPostfix]
    static void PatchSaveToFile_Postfix(ref int __state)
    {
        AscensionSaveData.Data.gramophoneTrackIndex = __state;
    }


    private static IEnumerator GramophoneAudioClip_Async(string guid, string path, AudioType audioType)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
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
