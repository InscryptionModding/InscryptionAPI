using DiskCardGame;
using HarmonyLib;
using System.Reflection;

namespace InscryptionAPI.Saves
{
    [HarmonyPatch]
    public static class ModdedSaveManager
    {
        private static readonly string saveFilePath = Path.Combine(BepInEx.Paths.BepInExRootPath, "ModdedSaveFile.gwsave");

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
            File.WriteAllText(saveFilePath, moddedSaveData);
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

            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                Tuple<Dictionary<string, Dictionary<string, string>>, Dictionary<string, Dictionary<string, string>>> saveData;
                saveData = SaveManager.FromJSON<Tuple<Dictionary<string, Dictionary<string, string>>, Dictionary<string, Dictionary<string, string>>>>(json);

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
}