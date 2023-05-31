using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace InscryptionAPI.Sound;

/// <summary>
/// A static class that contains helper methods for working with audio.
/// </summary>
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

    internal static string GetAudioPath(string filename)
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


    /// <summary>
    /// A helper method for converting an audio file into an Unity <c>AudioClip</c>.
    /// </summary>
    /// <param name="guid">Your plugin's GUID.</param>
    /// <param name="path">The path to your audio file.</param>
    /// <returns>The audio file converted into an <c>AudioClip</c> object.</returns>
    public static AudioClip LoadAudioClip(string guid, string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = GetAudioPath(path);
        }

        string filename = Path.GetFileName(path);
        AudioType audioType = GetAudioType(path);

        if (audioType == AudioType.UNKNOWN)
        {
            ErrorLog($"Couldn't load file {filename ?? "(null)"} as AudioClip. AudioType is unknown.");
            return null;
        }

        return LoadAudioClip_Sync(path, audioType, guid);
    }

    /// <summary>
    /// A helper method for converting an audio file into an Unity <c>AudioClip</c>.
    /// </summary>
    /// <param name="path">The path to your audio file.</param>
    /// <returns>The audio file converted into an <c>AudioClip</c> object.</returns>
    public static AudioClip LoadAudioClip(string path)
    {
        return LoadAudioClip(string.Empty, path);
    }

    internal static AudioClip LoadAudioClip(GramophoneManager.TrackInfo trackInfo)
    {
        if (trackInfo.Clip != null) return trackInfo.Clip;
        return LoadAudioClip(trackInfo.Guid, trackInfo.FilePath);
    }

    private static AudioClip LoadAudioClip_Sync(string path, AudioType audioType, string guid = null)
    {
        guid ??= string.Empty;
        string filename = Path.GetFileName(path);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
        {
            www.SendWebRequest();
            while (!www.isDone) continue;

            if (www.isNetworkError || www.isHttpError)
            {
                ErrorLog($"Couldn't load file \'{filename ?? "(null)"}\' as AudioClip!");
                ErrorLog(www.error);
                return null;
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                if (audioClip != null)
                {
                    InfoLog($"Loaded \'{filename}\' as AudioClip. AudioType: {audioType}");
                    audioClip.name = $"{guid}_{filename}";
                }
                return audioClip;
            }
        }
    }
}
