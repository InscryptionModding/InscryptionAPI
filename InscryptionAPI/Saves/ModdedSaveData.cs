namespace InscryptionAPI.Saves;

public class ModdedSaveData
{
    internal Dictionary<string, Dictionary<string, string>> SaveData = new();

    public string GetValue(string guid, string key)
    {
        SaveData ??= new();

        if (!SaveData.ContainsKey(guid))
            SaveData.Add(guid, new());

        if (!SaveData[guid].ContainsKey(key))
            SaveData[guid].Add(key, default(string));

        return SaveData[guid][key];
    }

    public int GetValueAsInt(string guid, string key)
    {
        string value = GetValue(guid, key);
        int.TryParse(value, out int result);
        return result;
    }

    public float GetValueAsFloat(string guid, string key)
    {
        string value = GetValue(guid, key);
        float.TryParse(value, out float result);
        return result;
    }

    public bool GetValueAsBoolean(string guid, string key)
    {
        string value = GetValue(guid, key);
        bool.TryParse(value, out bool result);
        return result;
    }

    public void SetValue(string guid, string key, object value)
    {
        if (!SaveData.ContainsKey(guid))
            SaveData.Add(guid, new());

        string valueString = value?.ToString();

        if (!SaveData[guid].ContainsKey(key))
            SaveData[guid].Add(key, valueString);
        else
            SaveData[guid][key] = valueString;
    }
}