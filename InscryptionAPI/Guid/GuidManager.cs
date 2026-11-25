using InscryptionAPI.Saves;

namespace InscryptionAPI.Guid;

/// <summary>
/// This class is the GUID Manager used throughout the API.
/// </summary>
public static class GuidManager
{
    /// <summary>
    /// Gets the fully qualified name of <c>GUID_Value</c>.
    /// </summary>
    /// <param name="guid">The GUID or Identifier associated with the given Mod.</param>
    /// <param name="value">The Value being paired with the GUID.</param>
    /// <returns>The Fully Qualified name in the form of <c>GUID_Value</c></returns>
    public static string GetFullyQualifiedName(string guid, string value)
    {
        return $"{guid}_{value}";
    }

    /// <summary>
    /// A mapper of Int to Type in the form of a Dictionary.
    /// </summary>
    private static readonly Dictionary<int, Type> reverseMapper = new();

    /// <summary>
    /// Serves as a Lock within <see cref="GetEnumValue"/>
    /// </summary>
    private static readonly object lockObject = new object();

    /// <summary>
    /// The Start Index for our GUID's.
    /// </summary>
    public const int START_INDEX = 1000;

    /// <summary>
    /// The Maximum Data Value for the API after all Enum's have been passed.
    /// </summary>
    public const string MAX_DATA = "maximumStoredValueForEnum";

    /// <summary>
    /// Gets the Enum Type associated with a passed Integer.
    /// </summary>
    /// <param name="number">The Specific Number associated with the Type we are after from the <see cref="reverseMapper"/>.</param>
    /// <returns>The Type associated with the passed Number.</returns>
    public static Type GetEnumType(int number)
    {
        reverseMapper.TryGetValue(number, out var res);
        return res;
    }

    /// <summary>
    /// Gets a list of created Enum values of the provided Enum type.
    /// </summary>
    /// <typeparam name="T">The Enum type to search for.</typeparam>
    /// <returns>The list of Enum values created by the API.</returns>
    unsafe public static List<T> GetValues<T>() where T : unmanaged, System.Enum
    {
        List<T> itemList = new();
        foreach (T item in Enum.GetValues(typeof(T)))
            itemList.Add(item);

        string startKey = typeof(T).Name + "_";
        foreach (var item in ModdedSaveManager.SaveData.SaveData[InscryptionAPIPlugin.ModGUID])
        {
            if (item.Key.StartsWith(startKey))
            {
                int enumVal = int.Parse((string)item.Value);
                T convertedEnumVal = *(T*)&enumVal;
                itemList.Add(convertedEnumVal);
            }
        }

        return itemList;
    }

    /// <summary>
    /// Gets the Enum Value with the passed GUID and Value. 
    /// </summary>
    /// <returns>If successful returns the Enum Value</returns>
    /// <exception cref="NotSupportedException">Thrown when our GUID system does not support the associated Type.</exception>
    unsafe public static T GetEnumValue<T>(string guid, string value) where T : unmanaged, System.Enum
    {
        if (sizeof(T) != sizeof(int))
            throw new NotSupportedException($"Cannot manage values of type {typeof(T).Name} in GuidManager.GetEnumValue");

        string saveKey = $"{typeof(T).Name}_{guid}_{value}";

        int enumValue = ModdedSaveManager.SaveData.GetValueAsInt(InscryptionAPIPlugin.ModGUID, saveKey);
        if (enumValue == default)
        {
            lock (lockObject)
            {
                enumValue = ModdedSaveManager.SaveData.GetValueAsInt(InscryptionAPIPlugin.ModGUID, MAX_DATA);
                if (enumValue < START_INDEX)
                    enumValue = START_INDEX;

                ModdedSaveManager.SaveData.SetValue(InscryptionAPIPlugin.ModGUID, MAX_DATA, enumValue + 1);
                ModdedSaveManager.SaveData.SetValue(InscryptionAPIPlugin.ModGUID, saveKey, enumValue);

                ModdedSaveManager.isSystemDirty = true;
            }
        }

        reverseMapper[enumValue] = typeof(T);
        return *(T*)&enumValue;
    }

    /// <summary>
    /// Attempts to get the GUID and Key associated with an Enum Value's Name.
    /// </summary>
    /// <param name="value">The Value we want to get the GUID and Key from.</param>
    /// <param name="guid">The GUID found to be associated with the Value.</param>
    /// <param name="key">The Key found to be associated with the Passed in Value.</param>
    /// <typeparam name="T">The Type of the Enum.</typeparam>
    /// <returns>If True the GUID and Key associated with the Value were found successfully and are valid for use.</returns>
    public static bool TryGetGuidAndKeyEnumValue<T>(T value, out string guid, out string key) where T : System.Enum
    {
        string prefix = $"{typeof(T).Name}";
        return ModdedSaveManager.SaveData.TryGetGuidAndKey(prefix, value.ToString(), out guid, out key);
    }
}