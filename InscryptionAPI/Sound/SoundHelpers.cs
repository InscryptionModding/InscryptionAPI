using BepInEx;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using DiskCardGame;
using HarmonyLib;
using UnityEngine.Networking;
using UnityEngine;

namespace InscryptionAPI.Sound;
internal static class SoundHelpers
{
    public static Dictionary<string, AudioType> AudioTypes = new Dictionary<string, AudioType>()
    {
        { "mp3", AudioType.MPEG },
        { "wav", AudioType.WAV },
        { "ogg", AudioType.OGGVORBIS },
        { "aiff", AudioType.AIFF }, // WHO USES AIFF.
        { "aif", AudioType.AIFF } // I'M UNSURE IF I SHOULD EVEN INCLUDE AIFF. WHO USES AIFF.
    };

    public static Dictionary<string, AudioClip> AudioClipsCache = new Dictionary<string, AudioClip>();

    static string GetAudioFile(string filename)
    {
        string[] files = Directory.GetFiles(Paths.PluginPath, filename, SearchOption.AllDirectories);
        Regex isValid = new Regex(@".*\.(mp3|wav|ogg)");
        return files.Where(x => isValid.IsMatch(x)).FirstOrDefault();
    }

    static AudioType GetAudioType(string filename)
    {
        string extension = Path.GetExtension(filename)?.ToLower();
        return AudioTypes.ContainsKey(extension ?? "") ? AudioTypes[extension] : AudioType.UNKNOWN;
    }

    static void InfoLog(string message) =>
        InscryptionAPIPlugin.Logger.LogInfo($"SoundHelpers: {message}");
    static void ErrorLog(string message) =>
        InscryptionAPIPlugin.Logger.LogError($"SoundHelpers: {message}");

    public static AudioClip LoadAudioClip(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = GetAudioFile(path);
        }

        string filename = Path.GetFileName(path);
        AudioType audioType = GetAudioType(path);

        if (audioType == AudioType.UNKNOWN)
        {
            ErrorLog($"Couldn't load file {filename} as AudioClip. AudioType is unknown.");
            return null;
        }

        InfoLog($"Loading file \"{filename}\" as AudioClip. AudioType: {audioType}");

        return AudioClip_Sync(path, audioType);
    }

    static AudioClip AudioClip_Sync(string path, AudioType audioType)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
        {
            www.SendWebRequest();
            while (!www.isDone) continue;

            if(www.isNetworkError || www.isHttpError)
            {
                return null;
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                if(audioClip != null)
                {
                    InfoLog($"Loaded file {Path.GetFileName(path)} as an AudioClip successfully!");
                }
                return audioClip;
            }
        }
    }

    static IEnumerator AudioClip_Async(string path, AudioType audioType, string guid = null)
    {
        guid ??= string.Empty;

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
                AudioClipsCache.Add((guid + path), audioclip);
            }
        }
    }






}
