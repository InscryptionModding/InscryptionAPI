using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Saves;

[HarmonyPatch]
public static class ModdedSaveManager
{
    private static readonly string SaveFilePath = Path.Combine(BepInEx.Paths.BepInExRootPath, "ModdedSaveFile.gwsave");

    public static ModdedSaveData SaveData { get; private set; }

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
    public static void SaveDataToFile()
    {
        Tuple<Dictionary<string, Dictionary<string, string>>, Dictionary<string, Dictionary<string, string>>> saveData = new(SaveData.SaveData, RunState.SaveData);
        string moddedSaveData = SaveManager.ToJSON(saveData);
        File.WriteAllText(SaveFilePath, moddedSaveData);
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

        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            var (saveData, runStateSaveData) 
                = SaveManager.FromJSON<Tuple<Dictionary<string, Dictionary<string, string>>, Dictionary<string, Dictionary<string, string>>>>(json);

            SaveData ??= new();

            RunState ??= new();

            SaveData.SaveData = saveData;
            RunState.SaveData = runStateSaveData;
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