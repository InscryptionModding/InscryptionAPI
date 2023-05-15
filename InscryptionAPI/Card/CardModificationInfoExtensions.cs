using DiskCardGame;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Card;

public static class CardModificationInfoExtensions
{
    public static bool HasDeathCardInfo(this CardModificationInfo mod) => mod.deathCardInfo != null;

    #region Setters
    public static CardModificationInfo SetNameReplacement(this CardModificationInfo mod, string name = null)
    {
        if (name != null)
            mod.nameReplacement = name;
        return mod;
    }
    public static CardModificationInfo SetSingletonId(this CardModificationInfo mod, string id = null)
    {
        if (id != null)
            mod.singletonId = id;
        return mod;
    }
    public static CardModificationInfo SetAttackAndHealth(this CardModificationInfo mod, int attack = 0, int health = 0)
    {
        mod.attackAdjustment = attack;
        mod.healthAdjustment = health;
        return mod;
    }
    public static CardModificationInfo SetCosts(this CardModificationInfo mod, int blood = 0, int bones = 0, int energy = 0, List<GemType> gems = null)
    {
        return mod.SetBloodCost(blood).SetBonesCost(bones).SetEnergyCost(energy).SetGemsCost(gems);
    }
    public static CardModificationInfo SetBloodCost(this CardModificationInfo mod, int cost = 0)
    {
        mod.bloodCostAdjustment = cost;
        return mod;
    }
    public static CardModificationInfo SetBonesCost(this CardModificationInfo mod, int cost = 0)
    {
        mod.bonesCostAdjustment = cost;
        return mod;
    }
    public static CardModificationInfo SetEnergyCost(this CardModificationInfo mod, int cost = 0)
    {
        mod.energyCostAdjustment = cost;
        return mod;
    }
    public static CardModificationInfo SetGemsCost(this CardModificationInfo mod, List<GemType> cost = null)
    {
        if (cost != null)
            mod.addGemCost = cost;
        return mod;
    }
    public static CardModificationInfo SetGemsCost(this CardModificationInfo mod, params GemType[] cost)
    {
        if (cost.Length > 0)
            mod.addGemCost = cost.ToList();
        return mod;
    }
    public static CardModificationInfo AddAbilities(this CardModificationInfo mod, params Ability[] abilities)
    {
        mod.abilities.AddRange(abilities);
        return mod;
    }
    public static CardModificationInfo SetDeathCardPortrait(this CardModificationInfo mod,
        CompositeFigurine.FigurineType headType = CompositeFigurine.FigurineType.SettlerMan,
        int mouthIndex = 0, int eyesIndex = 0, bool lostEye = false)
    {
        mod.deathCardInfo = new(headType, mouthIndex, eyesIndex) { lostEye = lostEye };
        return mod;
    }

    #endregion

    #region Singleton ID
    public static bool HasCustomCostId(this CardModificationInfo mod, string costName) => mod.singletonId.HasCustomCostId(costName);
    public static bool HasCustomPropertyId(this CardModificationInfo mod, string propertyName) => mod.singletonId.HasCustomPropertyId(propertyName);

    public static bool HasCustomCostId(this string singletonId, string costName) => singletonId.HasCustomCostsId() && singletonId.Contains(costName);
    public static bool HasCustomPropertyId(this string singletonId, string propertyName) => singletonId.HasCustomPropertiesId() && singletonId.Contains(propertyName);

    public static bool HasCustomCostsId(this CardModificationInfo mod) => mod.singletonId.HasCustomCostsId();
    public static bool HasCustomPropertiesId(this CardModificationInfo mod) => mod.singletonId.HasCustomPropertiesId();

    public static bool HasCustomCostsId(this string singletonId) => !singletonId.IsNullOrWhitespace() && singletonId.Contains("[CustomCosts:");
    public static bool HasCustomPropertiesId(this string singletonId) => !singletonId.IsNullOrWhitespace() && singletonId.Contains("[Properties:");

    public static CardModificationInfo SetCustomCostId(this CardModificationInfo mod, string costName, object value)
    {
        string valuePair = costName + "," + value;
        if (mod.singletonId.IsNullOrWhitespace())
        {
            mod.singletonId = $"[CustomCosts:{valuePair}]";
            return mod;
        }
        if (!mod.HasCustomCostsId())
        {
            mod.singletonId += $"[CustomCosts:{valuePair}]";
            return mod;
        }
        int startOf = mod.singletonId.IndexOf("[CustomCosts:");
        string currentCosts = mod.singletonId.Substring(startOf);
        int endOf = currentCosts.IndexOf("]");
        currentCosts = currentCosts.Substring(0, endOf + 1);
        if (mod.HasCustomCostId(costName)) // update cost value
        {
            int costIdx = currentCosts.IndexOf(costName + ",");
            int valueIdx = currentCosts.Substring(costIdx).IndexOf(';'); // assume there are other costs
            if (valueIdx == -1)
                valueIdx = currentCosts.Substring(costIdx).IndexOf(']'); // this is the only cost

            string valueToReplace = currentCosts.Substring(costIdx, valueIdx).Replace(costName + ",", "");
            mod.singletonId.Replace(currentCosts, currentCosts.Replace(valueToReplace, value.ToString()));
            return mod;
        }
        currentCosts = currentCosts.Replace("]", "");
        string newCosts = $"{currentCosts};{valuePair}]";
        mod.singletonId.Replace(currentCosts, newCosts);

        return mod;
    }
    public static string GetCustomCostId(this CardModificationInfo mod, string costName)
    {
        if (!mod.HasCustomCostId(costName))
            return null;

        int startOf = mod.singletonId.IndexOf("[CustomCosts:");
        string currentCosts = mod.singletonId.Substring(startOf);
        int endOf = currentCosts.IndexOf("]");
        currentCosts = currentCosts.Substring(0, endOf + 1);

        int costIdx = currentCosts.IndexOf(costName + ",");
        int valueIdx = currentCosts.Substring(costIdx).IndexOf(';'); // assume there are other costs
        if (valueIdx == -1)
            valueIdx = currentCosts.Substring(costIdx).IndexOf(']'); // this is the only cost

        string value = currentCosts.Substring(costIdx, valueIdx).Replace(costName + ",", "");
        return value;
    }
    public static CardModificationInfo SetPropertiesId(this CardModificationInfo mod, string propertyName, object value)
    {
        string valuePair = propertyName + "," + value;
        if (mod.singletonId.IsNullOrWhitespace())
        {
            mod.singletonId = $"[Properties:{valuePair}]";
            return mod;
        }
        if (!mod.HasCustomPropertiesId())
        {
            mod.singletonId += $"[Properties:{valuePair}]";
            return mod;
        }
        int startOf = mod.singletonId.IndexOf("[Properties:");
        string currentCosts = mod.singletonId.Substring(startOf);
        int endOf = currentCosts.IndexOf("]");
        currentCosts = currentCosts.Substring(0, endOf + 1);
        if (mod.HasCustomPropertyId(propertyName)) // update property value
        {
            int costIdx = currentCosts.IndexOf(propertyName + ",");
            int valueIdx = currentCosts.Substring(costIdx).IndexOf(';'); // assume there are other costs
            if (valueIdx == -1)
                valueIdx = currentCosts.Substring(costIdx).IndexOf(']'); // this is the only cost

            string valueToReplace = currentCosts.Substring(costIdx, valueIdx).Replace(propertyName + ",", "");
            mod.singletonId.Replace(currentCosts, currentCosts.Replace(valueToReplace, value.ToString()));
            return mod;
        }
        currentCosts = currentCosts.Replace("]", "");
        string newCosts = $"{currentCosts};{valuePair}]";
        mod.singletonId.Replace(currentCosts, newCosts);

        return mod;
    }
    public static string GetCustomPropertyId(this CardModificationInfo mod, string costName)
    {
        if (!mod.HasCustomPropertyId(costName))
            return null;

        int startOf = mod.singletonId.IndexOf("[Properties:");
        string currentCosts = mod.singletonId.Substring(startOf);
        int endOf = currentCosts.IndexOf("]");
        currentCosts = currentCosts.Substring(0, endOf + 1);

        int costIdx = currentCosts.IndexOf(costName + ",");
        int valueIdx = currentCosts.Substring(costIdx).IndexOf(';'); // assume there are other costs
        if (valueIdx == -1)
            valueIdx = currentCosts.Substring(costIdx).IndexOf(']'); // this is the only cost

        string value = currentCosts.Substring(costIdx, valueIdx).Replace(costName + ",", "");
        return value;
    }

    #region ID Getters
    public static string CleanId(this CardModificationInfo mod) => mod.IdWithoutCosts().IdWithoutProperties();
    public static string IdWithoutCosts(this CardModificationInfo mod) => mod.singletonId.IdWithoutCosts();
    public static string IdWithoutProperties(this CardModificationInfo mod) => mod.singletonId.IdWithoutProperties();
    public static string IdWithoutCosts(this string singletonId)
    {
        if (!singletonId.HasCustomCostsId())
            return singletonId;

        int start = singletonId.IndexOf("[CustomCosts:");
        string singleton = singletonId.Substring(start);
        int end = singleton.IndexOf("]");
        singleton = singleton.Substring(0, end + 1).Replace(singleton, "");

        return singleton;
    }
    public static string IdWithoutProperties(this string singletonId)
    {
        if (!singletonId.HasCustomPropertiesId())
            return singletonId;

        int start = singletonId.IndexOf("[Properties:");
        string singleton = singletonId.Substring(start);
        int end = singleton.IndexOf("]");
        singleton = singleton.Substring(0, end + 1).Replace(singleton, "");

        return singleton;
    }

    #endregion

    #endregion

    #region Extended Properties

    /// <summary>
    /// Adds a custom property value to the CardModificationInfo.
    /// </summary>
    /// <param name="mod">CardModificationInfo to access.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value of the property.</param>
    /// <param name="addToSingletonId">Appends a string containing the custom property name and value to the mod's singletonId in the forme of "[ExtProperty:{propertyName},{value}:ExtProperty]".</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetExtendedProperty(this CardModificationInfo mod, string propertyName, object value, bool addToSingletonId = false)
    {
        mod.GetCardModExtensionTable()[propertyName] = value?.ToString();
        if (addToSingletonId)
            mod.SetPropertiesId(propertyName, value);

        return mod;
    }

    /// <summary>
    /// Gets a custom property value from the card.
    /// </summary>
    /// <param name="mod">CardModificationInfo to access.</param>
    /// <param name="propertyName">The name of the property to get the value of.</param>
    /// <returns>The custom property value as a string. If it doesn't exist, returns null.</returns>
    public static string GetExtendedProperty(this CardModificationInfo mod, string propertyName)
    {
        mod.GetCardModExtensionTable().TryGetValue(propertyName, out var ret);
        return ret;
    }

    /// <summary>
    /// Gets a custom property as an int (can be null).
    /// </summary>
    /// <param name="mod">CardModificationInfo to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int.</returns>
    public static int? GetExtendedPropertyAsInt(this CardModificationInfo mod, string propertyName)
    {
        mod.GetCardModExtensionTable().TryGetValue(propertyName, out var str);
        return int.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a float (can be null).
    /// </summary>
    /// <param name="mod">CardModificationInfo to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float.</returns>
    public static float? GetExtendedPropertyAsFloat(this CardModificationInfo mod, string propertyName)
    {
        mod.GetCardModExtensionTable().TryGetValue(propertyName, out var str);
        return float.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a boolean (can be null).
    /// </summary>
    /// <param name="mod">CardModificationInfo to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean.</returns>
    public static bool? GetExtendedPropertyAsBool(this CardModificationInfo mod, string propertyName)
    {
        mod.GetCardModExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }
#endregion
}
