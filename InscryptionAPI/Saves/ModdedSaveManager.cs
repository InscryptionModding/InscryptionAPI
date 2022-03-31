using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Saves;

[HarmonyPatch]
public static class ModdedSaveManager
{
    [Obsolete("Use 'saveFilePath' instead")]
    private static readonly string oldSaveFilePath = Path.Combine(BepInEx.Paths.BepInExRootPath, "ModdedSaveFile.gwsave");

    private static readonly string saveFilePath = Path.Combine(BepInEx.Paths.GameRootPath, "ModdedSaveFile.gwsave");

    /// <summary>
    /// Current modded SaveData.
    /// </summary>
    public static ModdedSaveData SaveData { get; private set; }

    /// <summary>
    /// Current modded RunState.
    /// </summary>
    public static ModdedSaveData RunState { get; private set; }

    internal static bool isSystemDirty = false; // This set whenever we save important system data
    // This only happens during initialization
    // We use it to make sure that loading data doesn't overwrite system data.

    /// <summary>
    /// If we are using the old save file path, this will return true.
    /// </summary>
    public static bool isOldPath = false;

    static ModdedSaveManager()
    {
        isOldPath = File.Exists(oldSaveFilePath);
        ReadDataFromFile();
    }

    [HarmonyPatch(typeof(SaveManager), "SaveToFile")]
    [HarmonyPostfix]
    public static void SaveDataToFile()
    {
        var saveData = (SaveData.SaveData, RunState.SaveData);
        string moddedSaveData = SaveManager.ToJSON(saveData);
        File.WriteAllText(isOldPath ? oldSaveFilePath : saveFilePath, moddedSaveData);
    }

    [HarmonyPatch(typeof(SaveManager), "LoadFromFile")]
    [HarmonyPostfix]
    public static void ReadDataFromFile()
    {
        if (isSystemDirty)
        {
            SaveDataToFile();
            isSystemDirty = false;
        }

        // If old save file exists, Use the old save file.
        if (File.Exists(saveFilePath) || isOldPath)
        {
            string json = isOldPath ? File.ReadAllText(oldSaveFilePath) : File.ReadAllText(saveFilePath);
            var saveData = SaveManager.FromJSON<(Dictionary<string, Dictionary<string, object>>, Dictionary<string, Dictionary<string, object>>)>(json);

            if (SaveData == null)
                SaveData = new();

            if (RunState == null)
                RunState = new();

            SaveData.SaveData = saveData.Item1;
            RunState.SaveData = saveData.Item2;
        }
        else
        {
            SaveData = new();
            RunState = new();
        }
    }

    [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
    [HarmonyPostfix]
    public static void ResetRunStateOnNewAscensionRun()
    {
        RunState = new();
    }

    [HarmonyPatch(typeof(SaveFile), "NewPart1Run")]
    [HarmonyPrefix]
    public static void ResetRunStateOnPart1Run()
    {
        RunState = new();
    }
}