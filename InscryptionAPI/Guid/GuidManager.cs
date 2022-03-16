using InscryptionAPI.Saves;

namespace InscryptionAPI.Guid;

public static class GuidManager
{
    public static string GetFullyQualifiedName(string guid, string value)
    {
        return $"{guid}_{value}";
    }

    private static readonly Dictionary<int, Type> ReverseMapper = new();

    private static readonly object LockObject = new object();

    public const int START_INDEX = 1000;

    public const string MAX_DATA = "maximumStoredValueForEnum";

    public static Type GetEnumType(int number)
    {
        return !ReverseMapper.ContainsKey(number)
            ? default(Type)
            : ReverseMapper[number];

    }

    unsafe public static List<T> GetValues<T>() where T : unmanaged, System.Enum
    {
        List<T> itemList = Enum.GetValues(typeof(T)).Cast<T>().ToList();

        string startKey = typeof(T).Name + "_";
        foreach (var item in ModdedSaveManager.SaveData.SaveData[InscryptionAPIPlugin.ModGUID])
        {
            if (item.Key.StartsWith(startKey))
            {
                int enumVal = int.Parse(item.Value);
                T convertedEnumVal = *(T*)&enumVal;
                itemList.Add(convertedEnumVal);
            }
        }

        return itemList;
    }

    unsafe public static T GetEnumValue<T>(string guid, string value) where T : unmanaged, System.Enum
    {
        if (sizeof(T) != sizeof(int))
            throw new NotSupportedException($"Cannot manage values of type {typeof(T).Name} in GuidManager.GetEnumValue");

        string saveKey = $"{typeof(T).Name}_{guid}_{value}";

        int enumValue = ModdedSaveManager.SaveData.GetValueAsInt(InscryptionAPIPlugin.ModGUID, saveKey);

        lock (LockObject)
        {
            if (enumValue > 0)
            {
                if (!ReverseMapper.ContainsKey(enumValue))
                    ReverseMapper.Add(enumValue, typeof(T));

                return *(T*)&enumValue;
            }

            enumValue = ModdedSaveManager.SaveData.GetValueAsInt(InscryptionAPIPlugin.ModGUID, MAX_DATA) + 1;
            if (enumValue < START_INDEX)
                enumValue = START_INDEX;

            ModdedSaveManager.SaveData.SetValue(InscryptionAPIPlugin.ModGUID, MAX_DATA, enumValue);
            ModdedSaveManager.SaveData.SetValue(InscryptionAPIPlugin.ModGUID, saveKey, enumValue);

            ModdedSaveManager.isSystemDirty = true;

            ReverseMapper.Add(enumValue, typeof(T));

            return *(T*)&enumValue;
        }
    }
}
