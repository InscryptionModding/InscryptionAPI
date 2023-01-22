using DiskCardGame;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Items.Extensions;

public static class ConsumableItemDataExtensions
{
    // Needs to be defined first so the implicit static constructor works correctly
    private class ConsumableItemDataExt
    {
        public readonly Dictionary<Type, object> TypeMap = new();
        public readonly Dictionary<string, string> StringMap = new();
    }
    private static readonly ConditionalWeakTable<ConsumableItemData, ConsumableItemDataExt> ExtensionProperties = new();

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetPowerLevel(this ConsumableItemData data, int powerLevel)
    {
        data.powerLevel = powerLevel;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetLearnItemDescription(this ConsumableItemData data, string description)
    {
        data.description = description;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetRulebookCategory(this ConsumableItemData data, AbilityMetaCategory rulebookCategory)
    {
        data.rulebookCategory = rulebookCategory;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetRulebookName(this ConsumableItemData data, string rulebookName)
    {
        data.rulebookName = rulebookName;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetRulebookDescription(this ConsumableItemData data, string rulebookDescription)
    {
        data.rulebookDescription = rulebookDescription;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetRulebookSprite(this ConsumableItemData data, Sprite rulebookSprite)
    {
        data.rulebookSprite = rulebookSprite;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetRegionSpecific(this ConsumableItemData data, bool regionSpecific)
    {
        data.regionSpecific = regionSpecific;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetNotRandomlyGiven(this ConsumableItemData data, bool notRandomlyGiven)
    {
        data.notRandomlyGiven = notRandomlyGiven;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetComponentType(this ConsumableItemData data, Type type)
    {
        data.SetExtendedProperty("ComponentType", $"{type.FullName}, {type.Assembly.FullName}");
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetCanActivateOutsideBattles(this ConsumableItemData data, bool alwaysActivatable)
    {
        data.SetExtendedProperty("CanActivateOutsideBattles", alwaysActivatable);
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetPrefabID(this ConsumableItemData data, string prefabID)
    {
        data.prefabId = prefabID;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetPickupSoundId(this ConsumableItemData data, string pickupSoundId)
    {
        data.pickupSoundId = pickupSoundId;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetPlacedSoundId(this ConsumableItemData data, string placedSoundId)
    {
        data.placedSoundId = placedSoundId;
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetExamineSoundId(this ConsumableItemData data, string examineSoundId)
    {
        data.examineSoundId = examineSoundId;
        return data;
    }

    /// <summary>
    /// Sets the item ready to be accessed in act 1
    /// </summary>
    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetAct1(this ConsumableItemData data)
    {
        data.rulebookCategory = AbilityMetaCategory.Part1Rulebook;
        return data;
    }

    /// <summary>
    /// Sets the item ready to be accessed in act 3
    /// </summary>
    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetAct3(this ConsumableItemData data)
    {
        data.rulebookCategory = AbilityMetaCategory.Part3Rulebook;
        return data;
    }

    /// <summary>
    /// Sets the item ready to be accessed in Ascension
    /// </summary>
    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetAscensionUnlocked(this ConsumableItemData data)
    {
        data.rulebookCategory = AbilityMetaCategory.AscensionUnlocked;
        return data;
    }

    /// <summary>
    /// Sets the card that is contained within a bottle
    /// </summary>
    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetCardWithinBottle(this ConsumableItemData data, string cardName)
    {
        data.SetExtendedProperty("CardWithinBottle", cardName);
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static string GetCardWithinBottle(this ConsumableItemData data)
    {
        return data.GetExtendedProperty("CardWithinBottle");
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    internal static ConsumableItemData SetPrefabModelType(this ConsumableItemData data, ConsumableItemManager.ModelType modelType)
    {
        data.SetExtendedProperty("PrefabModelType", (int)modelType);
        return data;
    }

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemManager.ModelType GetPrefabModelType(this ConsumableItemData data)
    {
        int? i = data.GetExtendedPropertyAsInt("PrefabModelType");
        return i.HasValue ? (ConsumableItemManager.ModelType)i : 0;
    }

    /// <returns>Mod Prefix</returns>
    public static Type GetComponentType(this ConsumableItemData data)
    {
        return Type.GetType(data.GetExtendedProperty("ComponentType"));
    }

    /// <returns>Mod Prefix</returns>
    public static bool CanActivateOutsideBattles(this ConsumableItemData data)
    {
        bool? extendedPropertyAsBool = data.GetExtendedPropertyAsBool("CanActivateOutsideBattles");
        return extendedPropertyAsBool.HasValue && extendedPropertyAsBool.Value;
    }

    #region ModPrefixesAndTags

    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    internal static ConsumableItemData SetModPrefix(this ConsumableItemData data, string modPrefix)
    {
        data.SetExtendedProperty("ModPrefix", modPrefix);
        return data;
    }

    /// <returns>Mod Prefix</returns>
    public static string GetModPrefix(this ConsumableItemData data)
    {
        return data.GetExtendedProperty("ModPrefix");
    }

    #endregion



    #region ExtendedProperties
    internal static Dictionary<string, string> GetConsumableItemDataExtensionTable(this ConsumableItemData data)
    {
        return ExtensionProperties.GetOrCreateValue(data).StringMap;
    }

    /// <summary>
    /// Adds a custom property value to the ConsumableItemData.
    /// </summary>
    /// <param name="data">Card to access</param>
    /// <param name="propertyName">The name of the property to set</param>
    /// <param name="value">The value of the property</param>
    /// <returns>The same ConsumableItemData so a chain can continue</returns>
    public static ConsumableItemData SetExtendedProperty(this ConsumableItemData data, string propertyName, object value)
    {
        data.GetConsumableItemDataExtensionTable()[propertyName] = value?.ToString();
        return data;
    }

    /// <summary>
    /// Gets a custom property value from the ConsumableItemData
    /// </summary>
    /// <param name="data">Card to access</param>
    /// <param name="propertyName">The name of the property to get the value of</param>
    /// <returns></returns>
    public static string GetExtendedProperty(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var ret);
        return ret;
    }

    /// <summary>
    /// Gets a custom property as an int (can by null)
    /// </summary>
    /// <param name="data">Card to access</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int</returns>
    public static int? GetExtendedPropertyAsInt(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var str);
        return int.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a float (can by null)
    /// </summary>
    /// <param name="data">Card to access</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float</returns>
    public static float? GetExtendedPropertyAsFloat(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var str);
        return float.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a boolean (can be null)
    /// </summary>
    /// <param name="data">Card to access</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean</returns>
    public static bool? GetExtendedPropertyAsBool(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }
    #endregion
}
