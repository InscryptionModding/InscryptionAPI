using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace InscryptionAPI.Sound;
public static class SoundManager
{
    public static Dictionary<string, AudioType> AudioTypes = new Dictionary<string, AudioType>()
    {
        { ".mp3", AudioType.MPEG },
        { ".wav", AudioType.WAV },
        { ".ogg", AudioType.OGGVORBIS },
        { ".aiff", AudioType.AIFF }, // WHO USES AIFF.
        { ".aif", AudioType.AIFF } // I'M UNSURE IF I SHOULD EVEN INCLUDE AIFF. WHO USES AIFF.
    };

    internal static string GetAudioFile(string filename)
    {
        string[] files = Directory.GetFiles(Paths.PluginPath, filename, SearchOption.AllDirectories);
        return files.FirstOrDefault();
    }

    internal static AudioType GetAudioType(string filename)
    {
        string extension = Path.GetExtension(filename)?.ToLower();
        return AudioTypes.ContainsKey(extension ?? "") ? AudioTypes[extension] : AudioType.UNKNOWN;
    }

    private static void InfoLog(string message) =>
        InscryptionAPIPlugin.Logger.LogInfo($"SoundManager: {message}");
    private static void ErrorLog(string message) =>
        InscryptionAPIPlugin.Logger.LogError($"SoundManager: {message}");

    public static AudioClip LoadAudioClip(string path, string guid = null)
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

        return LoadAudioClip_Sync(path, audioType, guid);
    }

    public static AudioClip LoadAudioClip(GramophoneManager.TrackInfo trackInfo)
    {
        return LoadAudioClip(trackInfo.FilePath, trackInfo.Guid);
    }

    private static AudioClip LoadAudioClip_Sync(string path, AudioType audioType, string guid = null)
    {
        guid ??= string.Empty;
        string filename = Path.GetFileName(path);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
        {
            www.SendWebRequest();
            while (!www.isDone) continue;

            if(www.isNetworkError || www.isHttpError)
            {
                ErrorLog($"Couldn't load file {filename} as AudioClip!");
                ErrorLog(www.error);
                return null;
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                if(audioClip != null)
                {
                    InfoLog($"Loaded file {filename} as an AudioClip successfully!");
                }
                audioClip.name = guid + filename;
                return audioClip;
            }
        }
    }
}
