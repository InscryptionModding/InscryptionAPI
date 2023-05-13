using DiskCardGame;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Card;

public static class CardModificationInfoExtensions
{
    public static bool HasDeathCardInfo(this CardModificationInfo mod) => mod.deathCardInfo != null;
    public static bool HasCustomCosts(this string singletonId) => singletonId != null && singletonId.Contains("[CustomCosts:");
    public static bool HasCustomProperties(this string singletonId) => singletonId != null && singletonId.Contains("[Properties:");

    public static bool HasCustomCosts(this CardModificationInfo mod) => mod.singletonId.HasCustomCosts();
    public static bool HasCustomProperties(this CardModificationInfo mod) => mod.singletonId.HasCustomProperties();

    public static CardModificationInfo AddCustomCost(this CardModificationInfo mod, string costName, object value)
    {
        mod.singletonId ??= "";
        if (mod.HasCustomCosts())
        {
            int start = mod.singletonId.IndexOf("[CustomCosts:");
            string currentCosts = mod.singletonId.Substring(start).Replace("[CustomCosts:", "");
            int end = currentCosts.IndexOf("]");
            currentCosts = currentCosts.Substring(0, end + 1).Replace("]", "");
            string newCosts = $"{currentCosts}_{costName},{value}";
            mod.singletonId.Replace(currentCosts, newCosts);
        }
        else
            mod.singletonId += $"[CustomCosts:{costName},{value}]";
        return mod;
    }
    public static string CleanSingletonId(this CardModificationInfo mod) => mod.SingletonIdWithoutCosts().SingletonIdWithoutProperties();
    public static string SingletonIdWithoutCosts(this CardModificationInfo mod) => mod.singletonId.SingletonIdWithoutCosts();
    public static string SingletonIdWithoutProperties(this CardModificationInfo mod) => mod.singletonId.SingletonIdWithoutProperties();
    public static string SingletonIdWithoutCosts(this string singletonId)
    {
        if (singletonId.IsNullOrWhitespace() || !singletonId.HasCustomCosts())
            return singletonId;

        int start = singletonId.IndexOf("[CustomCosts:");
        string singleton = singletonId.Substring(start);
        int end = singleton.IndexOf("]");
        singleton = singleton.Substring(0, end + 1).Replace(singleton, "");

        return singleton;
    }
    public static string SingletonIdWithoutProperties(this string singletonId)
    {
        if (singletonId.IsNullOrWhitespace() || !singletonId.HasCustomProperties())
            return singletonId;

        int start = singletonId.IndexOf("[Properties:");
        string singleton = singletonId.Substring(start);
        int end = singleton.IndexOf("]");
        singleton = singleton.Substring(0, end + 1).Replace(singleton, "");

        return singleton;
    }

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
        {
            mod.singletonId ??= "";
            if (mod.HasCustomProperties())
            {
                int start = mod.singletonId.IndexOf("[Properties:");
                string currentCosts = mod.singletonId.Substring(start).Replace("[Properties:", "");
                int end = currentCosts.IndexOf("]");
                currentCosts = currentCosts.Substring(0, end + 1).Replace("]", "");
                string newCosts = $"{currentCosts}_{propertyName},{value}";
                mod.singletonId.Replace(currentCosts, newCosts);
            }
            else
                mod.singletonId += $"[Properties:{propertyName},{value}]";
        }
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
}
