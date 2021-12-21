using DiskCardGame;
using System;
using System.Linq;
using HarmonyLib;
using Mono.Collections.Generic;
using Sirenix.Serialization.Utilities;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace InscryptionAPI.Saves
{
    public class ModdedSaveData
    {
        internal Dictionary<string, Dictionary<string, string>> SaveData = new();

        public string GetValue(string guid, string key)
        {
            if (!SaveData.ContainsKey(guid))
                SaveData.Add(guid, new());

            if (!SaveData[guid].ContainsKey(key))
                SaveData[guid].Add(key, default(string));

            return SaveData[guid][key];
        }

        public int GetValueAsInt(string guid, string key)
        {
            string value = GetValue(guid, key);
            int result;
            int.TryParse(value, out result);
            return result;
        }

        public float GetValueAsFloat(string guid, string key)
        {
            string value = GetValue(guid, key);
            float result;
            float.TryParse(value, out result);
            return result;
        }

        public bool GetValueAsBoolean(string guid, string key)
        {
            string value = GetValue(guid, key);
            bool result;
            bool.TryParse(value, out result);
            return result;
        }

        public void SetValue(string guid, string key, object value)
        {
            if (!SaveData.ContainsKey(guid))
                SaveData.Add(guid, new());

            if (!SaveData[guid].ContainsKey(key))
                SaveData[guid].Add(key, value.ToString());
            else
                SaveData[guid][key] = value.ToString();
        }
    }
}