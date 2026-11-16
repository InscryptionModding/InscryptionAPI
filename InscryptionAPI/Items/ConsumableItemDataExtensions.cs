using DiskCardGame;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Items.Extensions;

/// <summary>
/// Utiliy class that manages custom <see cref="ConsumableItemData"/> objects, and adds extensions for that purpose.
/// </summary>
public static class ConsumableItemDataExtensions
{
    // Needs to be defined first so the implicit static constructor works correctly
    private class ConsumableItemDataExt
    {
        public readonly Dictionary<Type, object> TypeMap = new();
        public readonly Dictionary<string, string> StringMap = new();
    }
    private static readonly ConditionalWeakTable<ConsumableItemData, ConsumableItemDataExt> ExtensionProperties = new();

    /// <summary>
    /// Sets the powerlevel of this consumable item; determines the item's rarity when obtaining it from nodes.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="powerLevel">The power level to set for this item.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    /// <remarks>
    /// Vanilla power level for items ranges from [0, 5].
    /// </remarks>
    public static ConsumableItemData SetPowerLevel(this ConsumableItemData data, int powerLevel)
    {
        data.powerLevel = powerLevel;
        return data;
    }

    /// <summary>
    /// Sets the text shown when encountering this item for the first time.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="description">The text to display.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetLearnItemDescription(this ConsumableItemData data, string description)
    {
        data.description = description;
        return data;
    }

    /// <summary>
    /// Sets the metacategory that controls what rulebook this item's entry appears in.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="rulebookCategory">The metacategory for the rulebook this item's entry can appear.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetRulebookCategory(this ConsumableItemData data, AbilityMetaCategory rulebookCategory)
    {
        data.rulebookCategory = rulebookCategory;
        return data;
    }
    /// <summary>
    /// Adds additional metacategories so this item's entry can appear in multiple acts.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="rulebookCategories">The metacategories for the rulebooks this item's entry can appear in.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData AddExtraRulebookCategories(this ConsumableItemData data, params AbilityMetaCategory[] rulebookCategories)
    {
        ConsumableItemManager.FullConsumableItemData fullItem = data.GetFullConsumableItemData();
        if (fullItem != null)
        {
            foreach (AbilityMetaCategory abilityMetaCategory in rulebookCategories)
            {
                if (!fullItem.rulebookMetaCategories.Contains(abilityMetaCategory))
                    fullItem.rulebookMetaCategories.Add(abilityMetaCategory);
            }
        }
        return data;
    }

    /// <summary>
    /// Retrieves the FullConsumableItemData associated with the given ConsumableItemData.
    /// </summary>
    /// <param name="data">The ConsumableItemData we want to find the FullConsumableItemData of.</param>
    /// <param name="fallBackToName">If the initial retrieval returns null and this is true, search again for the FullConsumableItemData using the data's mod prefix and rulebookName.</param>
    /// <returns>The FullConsumableItemData associated with the given COnsumableItemData, or null if it does not exist.</returns>
    public static ConsumableItemManager.FullConsumableItemData GetFullConsumableItemData(this ConsumableItemData data, bool fallBackToName = true)
    {
        ConsumableItemManager.FullConsumableItemData fullItem = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData == data);

        if (fullItem == null && fallBackToName)
            fullItem = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData.GetModPrefix() == data.GetModPrefix() && x.itemData.rulebookName == data.rulebookName);

        return fullItem;
    }

    /// <summary>
    /// Sets the name of this item that appears in the rulebook.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="rulebookName">The title of this item's rulebook entry.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetRulebookName(this ConsumableItemData data, string rulebookName)
    {
        data.rulebookName = rulebookName;
        return data;
    }

    /// <summary>
    /// Sets the description for this item when it appears in the rulebook.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="rulebookDescription">The text to display in the rulebook.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetRulebookDescription(this ConsumableItemData data, string rulebookDescription)
    {
        data.rulebookDescription = rulebookDescription;
        return data;
    }

    /// <summary>
    /// Sets the Sprite used for this item's rulebook entry. Should be <c>100 x 150</c> px in size.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="rulebookSprite">The Sprite to display on this item's rulebook entry.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetRulebookSprite(this ConsumableItemData data, Sprite rulebookSprite)
    {
        data.rulebookSprite = rulebookSprite;
        return data;
    }

    /// <summary>
    /// Sets whether or not this item should only appear in a specific region.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="regionSpecific">If this item only appears in a specific region.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    /// <remarks>
    /// By default, <see cref="ConsumableItemData.regionSpecific"/> being true just removes the item from the pool of region-specific items.
    /// </remarks>
    public static ConsumableItemData SetRegionSpecific(this ConsumableItemData data, bool regionSpecific)
    {
        data.regionSpecific = regionSpecific;
        return data;
    }

    /// <summary>
    /// Sets whether or not this item can appear in the pool of obtainable items at item choice nodes.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="notRandomlyGiven">If true, this item can be obtained randomly at item choice nodes.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetNotRandomlyGiven(this ConsumableItemData data, bool notRandomlyGiven)
    {
        data.notRandomlyGiven = notRandomlyGiven;
        return data;
    }

    /// <summary>
    /// Sets this item's extended property <c>ComponentType</c> to allow the given component to be added to the item's in-game object.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="type">The Type of the component this item should have.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetComponentType(this ConsumableItemData data, Type type)
    {
        data.SetExtendedProperty("ComponentType", $"{type.FullName}, {type.Assembly.FullName}");
        return data;
    }

    /// <summary>
    /// Sets whether or not this item can be activated outside of card battles.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="alwaysActivatable">If true, this item can be interacted with and used even outside of battle.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetCanActivateOutsideBattles(this ConsumableItemData data, bool alwaysActivatable)
    {
        data.SetExtendedProperty("CanActivateOutsideBattles", alwaysActivatable);
        return data;
    }

    /// <summary>
    /// Sets the ID of the prefab this item uses for its in-game model.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="prefabID">The ID of the prefab object to use for this item's model.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetPrefabID(this ConsumableItemData data, string prefabID)
    {
        data.prefabId = prefabID;
        return data;
    }

    /// <summary>
    /// Sets the ID of the sound played when this item is picked up (chosen at a choice node or used).
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="pickupSoundId">The ID of the sound played when this item is picked up.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetPickupSoundId(this ConsumableItemData data, string pickupSoundId)
    {
        data.pickupSoundId = pickupSoundId;
        return data;
    }

    /// <summary>
    /// Sets the ID of the sound played when this item is placed down (added to your inventory or has its usage cancelled).
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="placedSoundId">The ID of the sound played when this item is placed down.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetPlacedSoundId(this ConsumableItemData data, string placedSoundId)
    {
        data.placedSoundId = placedSoundId;
        return data;
    }

    /// <summary>
    /// Sets the ID of the sound played when this item is hovered over with the mouse.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="examineSoundId">The ID of the sound played when this item is examined.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetExamineSoundId(this ConsumableItemData data, string examineSoundId)
    {
        data.examineSoundId = examineSoundId;
        return data;
    }

    /// <summary>
    /// Sets the item ready to be accessed in Act 1.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetAct1(this ConsumableItemData data)
    {
        data.rulebookCategory = AbilityMetaCategory.Part1Rulebook;
        return data;
    }

    /// <summary>
    /// Sets the item ready to be accessed in Act 3.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetAct3(this ConsumableItemData data)
    {
        data.rulebookCategory = AbilityMetaCategory.Part3Rulebook;
        return data;
    }

    /// <summary>
    /// Sets the item ready to be accessed in Ascension.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetAscensionUnlocked(this ConsumableItemData data)
    {
        data.rulebookCategory = AbilityMetaCategory.AscensionUnlocked;
        return data;
    }

    /// <summary>
    /// Sets the card that is contained within a bottle-type item (eg. Frozen Opposum).
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="cardName">The internal name of the card that will be contained in this item.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetCardWithinBottle(this ConsumableItemData data, string cardName)
    {
        data.SetExtendedProperty("CardWithinBottle", cardName);
        return data;
    }
    /// <summary>
    /// Gets the card that is contained within a bottle-type item (eg. Frozen Opposum).
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>The internal name of the card contained in this bottle item, or null if this isn't a bottle item.</returns>
    public static string GetCardWithinBottle(this ConsumableItemData data)
    {
        return data.GetExtendedProperty("CardWithinBottle");
    }

    /// <summary>
    /// Sets the prefab model type for this item. Use if you don't have a custom prefab or want to use one of the API's default models.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="modelType">The ID for the default model to use.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetPrefabModelType(this ConsumableItemData data, ConsumableItemManager.ModelType modelType)
    {
        data.SetExtendedProperty("PrefabModelType", (int)modelType);
        return data;
    }

    /// <summary>
    /// Gets the prefab model type for this item.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>The ID of the model used, or 0 if a default model is not being used.</returns>
    public static ConsumableItemManager.ModelType GetPrefabModelType(this ConsumableItemData data)
    {
        int? i = data.GetExtendedPropertyAsInt("PrefabModelType");
        return i.HasValue ? (ConsumableItemManager.ModelType)i : 0;
    }

    /// <summary>
    /// Gets the component type for this item.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>The component Type.</returns>
    public static Type GetComponentType(this ConsumableItemData data)
    {
        return Type.GetType(data.GetExtendedProperty("ComponentType"));
    }

    /// <summary>
    /// Gets whether or not this item can be used outside card battles.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>Whether or not this item can be used outside battles. Defaults to false if no value is present.</returns>
    public static bool CanActivateOutsideBattles(this ConsumableItemData data)
    {
        bool? extendedPropertyAsBool = data.GetExtendedPropertyAsBool("CanActivateOutsideBattles");
        return extendedPropertyAsBool.HasValue && extendedPropertyAsBool.Value;
    }

    #region ModPrefixesAndTags

    /// <summary>
    /// Sets the prefix of the mod adding this item.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="modPrefix"></param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    internal static ConsumableItemData SetModPrefix(this ConsumableItemData data, string modPrefix)
    {
        data.SetExtendedProperty("ModPrefix", modPrefix);
        return data;
    }

    /// <summary>
    /// Gets the prefix of the mod that added this item.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <returns>The prefix of the mod that added this item.</returns>
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
    /// Adds a custom property value to the <see cref="ConsumableItemData"/>.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetExtendedProperty(this ConsumableItemData data, string propertyName, object value)
    {
        data.GetConsumableItemDataExtensionTable()[propertyName] = value?.ToString();
        return data;
    }

    /// <summary>
    /// Gets a custom property value from the <see cref="ConsumableItemData"/>.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="propertyName">The name of the property to get the value of.</param>
    /// <returns>.</returns>
    public static string GetExtendedProperty(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var ret);
        return ret;
    }

    /// <summary>
    /// Gets a custom property as an nullable int.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int.</returns>
    public static int? GetExtendedPropertyAsInt(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var str);
        return int.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a nullable float.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float.</returns>
    public static float? GetExtendedPropertyAsFloat(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var str);
        return float.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a nullable boolean.
    /// </summary>
    /// <param name="data">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean.</returns>
    public static bool? GetExtendedPropertyAsBool(this ConsumableItemData data, string propertyName)
    {
        data.GetConsumableItemDataExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }
    #endregion
}
