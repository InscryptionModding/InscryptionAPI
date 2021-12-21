using InscryptionAPI.Saves;

namespace InscryptionAPI.Guid
{
    public static class GuidManager
    {
        public static string GetFullyQualifiedName(string guid, string value)
        {
            return $"{guid}_{value}";
        }

        public static readonly int START_INDEX = 500;

        public const string MAX_DATA = "maximumStoredValueForEnum";

        public static int GetEnumValue<T>(string guid, string value) where T : System.Enum
        {
            string saveKey = GetFullyQualifiedName(guid, value);
            string saveGuid = GetFullyQualifiedName(InscryptionAPIPlugin.ModGUID, typeof(T).Name);
            
            int enumValue = ModdedSaveManager.SaveData.GetValueAsInt(saveGuid, saveKey);

            if (enumValue > 0)
                return enumValue;

            enumValue = ModdedSaveManager.SaveData.GetValueAsInt(saveGuid, MAX_DATA) + 1;
            if (enumValue < START_INDEX)
                enumValue = START_INDEX;
            
            ModdedSaveManager.SaveData.SetValue(saveGuid, MAX_DATA, enumValue);
            ModdedSaveManager.SaveData.SetValue(saveGuid, saveKey, enumValue);

            ModdedSaveManager.isSystemDirty = true;

            return enumValue;
        }
    }
}