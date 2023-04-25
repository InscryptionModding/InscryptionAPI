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

    static ModdedSaveManager()
    {
        ReadDataFromFile();
    }

    [HarmonyPatch(typeof(SaveManager), "SaveToFile")]
    [HarmonyPostfix]
    private static void SaveDataToFile()
    {
        var saveData = (SaveData.SaveData, RunState.SaveData);
        string moddedSaveData = SaveManager.ToJSON(saveData);
        File.WriteAllText(saveFilePath, moddedSaveData);
    }

    [HarmonyPatch(typeof(SaveManager), "LoadFromFile")]
    [HarmonyPostfix]
    private static void ReadDataFromFile()
    {
        if (isSystemDirty)
        {
            SaveDataToFile();
            isSystemDirty = false;
        }

        var oldSaveFileExist = File.Exists(oldSaveFilePath);
        var newSaveFileExist = File.Exists(saveFilePath);

        // If both old and new file exists, Delete the new one and move the old one to new path
        if (newSaveFileExist || oldSaveFileExist)
        {
            if (newSaveFileExist && oldSaveFileExist) File.Delete(saveFilePath);
            if (oldSaveFileExist) File.Move(oldSaveFilePath, saveFilePath);
            string json = File.ReadAllText(saveFilePath);
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
    [HarmonyPrefix]
    private static void ResetRunStateOnNewAscensionRun()
    {
        RunState = new();
    }

    [HarmonyPatch(typeof(SaveFile), "NewPart1Run")]
    [HarmonyPrefix]
    private static void ResetRunStateOnPart1Run()
    {
        RunState = new();
    }
}