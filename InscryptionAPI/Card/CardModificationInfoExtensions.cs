using DiskCardGame;
using InscryptionAPI.CardCosts;
using Sirenix.Utilities;
using static InscryptionAPI.Card.CardModificationInfoManager;
namespace InscryptionAPI.Card;

/// <summary>
/// Class of helper extensions for CardModificationInfo objects.
/// </summary>
public static class CardModificationInfoExtensions
{
    /// <summary>
    /// Determines if the given CardModificationInfo contains info for a death card.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>True if the CardModificationInfo has death card info.</returns>
    public static bool HasDeathCardInfo(this CardModificationInfo mod) => mod.deathCardInfo != null;

    #region Adders
    /// <summary>
    /// Adds decal ID strings to the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="decalIds">The decal IDs to add.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo AddDecalIds(this CardModificationInfo mod, params string[] decalIds)
    {
        foreach (string decalId in decalIds)
        {
            if (!mod.DecalIds.Contains(decalId))
                mod.DecalIds.Add(decalId);
        }
        return mod;
    }
    /// <summary>
    /// Adds the given abilities to the given CardModificationInfos list of negated abilities.
    /// THIS WILL NOT NEGATE THE GIVEN ABILITIES ON ITS OWN.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="negateAbilities">The list of abilities to negate.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo AddNegateAbilities(this CardModificationInfo mod, params Ability[] negateAbilities)
    {
        mod.negateAbilities ??= new();
        mod.negateAbilities.AddRange(negateAbilities);
        return mod;
    }
    #endregion

    #region Setters
    /// <summary>
    /// Sets the name replacement field for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="name">The name replacement.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetNameReplacement(this CardModificationInfo mod, string name = null)
    {
        mod.nameReplacement = name;
        return mod;
    }
    /// <summary>
    /// Sets the singleton ID field for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="id">The singleton ID. If null, does not set it.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetSingletonId(this CardModificationInfo mod, string id = null)
    {
        if (id != null)
            mod.singletonId = id;
        return mod;
    }
    /// <summary>
    /// Sets the attack and health adjustment fields for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="attack">How much attack this mod should give. Can be negative.</param>
    /// <param name="health">How much health this mod should give. Can be negative.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetAttackAndHealth(this CardModificationInfo mod, int attack = 0, int health = 0)
    {
        mod.attackAdjustment = attack;
        mod.healthAdjustment = health;
        return mod;
    }
    /// <summary>
    /// Sets the cost adjustment fields for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="blood">The amount the mod should modify a card's blood cost. Can be negative.</param>
    /// <param name="bones">The amount the mod should modify a card's bones cost. Can be negative.</param>
    /// <param name="energy">The amount the mod should modify a card's energy cost. Can be negative.</param>
    /// <param name="gems">The amount the mod should modify a card's gems cost.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetCosts(this CardModificationInfo mod, int blood = 0, int bones = 0, int energy = 0, List<GemType> gems = null)
    {
        return mod.SetBloodCost(blood).SetBonesCost(bones).SetEnergyCost(energy).SetGemsCost(gems);
    }
    /// <summary>
    /// Sets the blood cost adjustment field for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="cost">The amount the mod should modify a card's blood cost. Can be negative.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetBloodCost(this CardModificationInfo mod, int cost = 0)
    {
        mod.bloodCostAdjustment = cost;
        return mod;
    }
    /// <summary>
    /// Sets the bones cost adjustment field for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="cost">The amount the mod should modify a card's bones cost. Can be negative.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetBonesCost(this CardModificationInfo mod, int cost = 0)
    {
        mod.bonesCostAdjustment = cost;
        return mod;
    }
    /// <summary>
    /// Sets the energy cost adjustment field for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="cost">The amount the mod should modify a card's energy cost. Can be negative.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetEnergyCost(this CardModificationInfo mod, int cost = 0)
    {
        mod.energyCostAdjustment = cost;
        return mod;
    }
    /// <summary>
    /// Sets the addGemsCost field for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="cost">List of gems cost this mod should add to a card.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetGemsCost(this CardModificationInfo mod, List<GemType> cost = null)
    {
        if (cost != null)
            mod.addGemCost = cost;
        return mod;
    }
    /// <summary>
    /// Sets the addGemsCost field for the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModification to access.</param>
    /// <param name="cost">Collection of gems cost this mod should add to a card.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetGemsCost(this CardModificationInfo mod, params GemType[] cost)
    {
        if (cost.Length > 0)
            mod.addGemCost = cost.ToList();
        return mod;
    }
    /// <summary>
    /// Adds the given abilities to the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="abilities"></param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo AddAbilities(this CardModificationInfo mod, params Ability[] abilities)
    {
        mod.abilities.AddRange(abilities);
        return mod;
    }
    /// <summary>
    /// Sets the given CardModificationInfo's deathCardInfo field with a new death card portrait.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="headType">What head the portrait should use.</param>
    /// <param name="mouthIndex">What mouth the portrait should use.</param>
    /// <param name="eyesIndex">What eyes the portrait should use.</param>
    /// <param name="lostEye">If the portrait should be missing an eye.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetDeathCardPortrait(this CardModificationInfo mod,
        CompositeFigurine.FigurineType headType = CompositeFigurine.FigurineType.SettlerMan,
        int mouthIndex = 0, int eyesIndex = 0, bool lostEye = false)
    {
        mod.deathCardInfo = new(headType, mouthIndex, eyesIndex) { lostEye = lostEye };
        return mod;
    }

    #endregion

    /// <summary>
    /// Used by the API to set and remove temporary decal mods, since it's not done automatically.
    /// </summary>
    public static void SetTemporaryDecal(this CardModificationInfo mod) => mod.SetExtendedProperty("API:TemporaryDecal", true);
    /// <summary>
    /// Determines if the given CardModificationInfo is an API temporary decal.
    /// </summary>
    public static bool IsTemporaryDecal(this CardModificationInfo mod) => mod.GetExtendedPropertyAsBool("API:TemporaryDecal") ?? false;

    #region Singleton ID

    /// <summary>
    /// Removes the custom properties field from a CardModificationInfo's singletonId.
    /// </summary>
    /// <param name="mod">The CardModificationInfo object whose properties we want to clear.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo ClearCustomPropertiesId(this CardModificationInfo mod)
    {
        string properties = GetCustomPropertiesIdString(mod);
        if (properties != null)
            mod.singletonId = mod.singletonId.Replace(properties, "");

        return mod;
    }
    /// <summary>
    /// Removes the custom costs ID field from a CardModificationInfo's singletonId.
    /// </summary>
    /// <param name="mod">The CardModificationInfo object whose custom costs ID we want to clear.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    public static CardModificationInfo ClearCustomCostsId(this CardModificationInfo mod)
    {
        string costs = GetCustomCostsIdString(mod);
        if (costs != null)
            mod.singletonId = mod.singletonId.Replace(costs, "");

        return mod;
    }

    #region Properties
    /// <summary>
    /// Determines if the given CardModificationInfo has a singleton ID containing an API custom (extended) property.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="propertyName">What custom property to look for.</param>
    /// <returns>True if the given property is present, false if it isn't present or there is no custom property section.</returns>
    public static bool HasCustomPropertyId(this CardModificationInfo mod, string propertyName) => HasCustomPropertiesId(mod.singletonId) && mod.singletonId.Contains(propertyName);
    /// <summary>
    /// Determines if the given string is a custom (extended) properties string ID.
    /// </summary>
    /// <param name="singletonId">The string to access.</param>
    /// <param name="propertyName">What custom property to look for.</param>
    /// <returns>True if the given property is present, false if it isn't present or there is no custom property section.</returns>
    public static bool HasCustomPropertyId(this string singletonId, string propertyName) => HasCustomPropertiesId(singletonId) && singletonId.Contains(propertyName);
    /// <summary>
    /// Determines if the given CardModificationInfo has any custom (extended) properties in its singleton ID.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>True if there are any custom properties, false otherwise.</returns>
    public static bool HasCustomPropertiesId(this CardModificationInfo mod) => !mod.singletonId.IsNullOrWhitespace() && mod.singletonId.Contains(PROPERTIES);
    /// <summary>
    /// Determines if the given string has any custom (extended) properties in it.
    /// </summary>
    /// <param name="singletonId">The string to access.</param>
    /// <returns>True if there are any custom properties, false otherwise.</returns>
    public static bool HasCustomPropertiesId(this string singletonId) => !singletonId.IsNullOrWhitespace() && singletonId.Contains(PROPERTIES);
    /// <summary>
    /// Creates and/or modifies the custom properties section inside the given CardModificationInfo's singleton ID.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="propertyName">The name of the property to add.</param>
    /// <param name="value">What value to set for the property. Will be converted to a string.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo SetPropertiesId(this CardModificationInfo mod, string propertyName, object value)
    {
        string valuePair = $"{propertyName},{value}";
        if (mod.HasCustomPropertiesId())
        {
            string newProperties;
            string oldProperties = GetCustomPropertiesIdString(mod); // get the current string of properties

            // if the property is already set
            if (oldProperties.Contains(propertyName))
            {
                string oldValue = mod.GetCustomPropertyId(propertyName);
                string currentPair = GetIdKeyPair(oldProperties, propertyName);

                newProperties = oldProperties.Replace(currentPair, currentPair.Replace(oldValue, value.ToString()));
            }
            else
            {
                newProperties = oldProperties.Remove(oldProperties.Length - 1) + ";" + valuePair + "]"; // add the new value pair to the end
            }

            mod.singletonId = mod.singletonId.Replace(oldProperties, newProperties);
        }
        else if (mod.singletonId.IsNullOrWhitespace())
        {
            mod.singletonId = PROPERTIES + valuePair + "]";
        }
        else
        {
            mod.singletonId += PROPERTIES + valuePair + "]";
        }
        return mod;
    }

    /// <summary>
    /// Determines if the given CardModificationInfo has a custom property with the given name. Checks extended properties before checking the singleton ID.
    /// </summary>
    /// <param name="mod">The CardModificationInfo</param>
    /// <param name="propertyName">The custom property to search for.</param>
    /// <returns>True if the CardModificationInfo has an extended property or custom property ID with the given name. False otherwise.</returns>
    public static bool HasCustomProperty(this CardModificationInfo mod, string propertyName) => mod.GetCustomProperty(propertyName) != null;
    /// <summary>
    /// Gets the custom property value from the given CardModificationInfo using the given property name. Checks extended properties before checking the singleton ID.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="propertyName">The custom property's name.</param>
    /// <returns>The value of the custom property, or null.</returns>
    public static string GetCustomProperty(this CardModificationInfo mod, string propertyName)
    {
        return mod.GetExtendedProperty(propertyName) ?? mod.GetCustomPropertyId(propertyName);
    }
    /// <summary>
    /// Retrieves the value of the given custom property ID in the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="propertyName">The custom property whose value we want.</param>
    /// <returns>The value of the custom property, or null if there is none.</returns>
    public static string GetCustomPropertyId(this CardModificationInfo mod, string propertyName)
    {
        if (!mod.HasCustomPropertyId(propertyName))
            return null;

        string currentProperties = GetCustomPropertiesIdString(mod);
        int costIdx = currentProperties.IndexOf(propertyName + ",");
        if (costIdx == -1)
            return null;

        string pairString = currentProperties.Substring(costIdx);
        int valueIdx = pairString.IndexOf(';');
        if (valueIdx == -1) // if this is the last valuePair in the string
            valueIdx = pairString.IndexOf(']');

        return pairString.Substring(valueIdx).Replace(propertyName + ",", "");
    }

    #endregion

    #region CustomCosts
    /// <summary>
    /// Determines if the given CardModificationInfo adds the given custom cost.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="costName">The name of the custom cost to search for.</param>
    /// <returns>True if the CardModification adds the given card cost.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static bool HasCustomCostId(this CardModificationInfo mod, string costName) => HasCustomCostsId(mod.singletonId) && mod.singletonId.Contains(costName);

    /// <summary>
    /// Determines if the given singleton ID string has the given custom cost in its custom costs substring.
    /// </summary>
    /// <param name="singletonId">The CardModificationInfo to access.</param>
    /// <param name="costName">The name of the custom cost to search for.</param>
    /// <returns>True if the string has the given card cost in its custom costs substring.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static bool HasCustomCostId(this string singletonId, string costName) => HasCustomCostsId(singletonId) && singletonId.Contains(costName);

    /// <summary>
    /// Determines if the given CardModificationInfo adds any custom costs.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>True if the CardModification adds any card costs.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static bool HasCustomCostsId(this CardModificationInfo mod) => !mod.singletonId.IsNullOrWhitespace() && mod.singletonId.Contains(CUSTOM_COSTS);

    /// <summary>
    /// Determines if the given singleton ID string has the custom costs substring.
    /// </summary>
    /// <param name="singletonId">The CardModificationInfo to access.</param>
    /// <returns>True if the string has the custom costs substring.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static bool HasCustomCostsId(this string singletonId) => !singletonId.IsNullOrWhitespace() && singletonId.Contains(CUSTOM_COSTS);

    /// <summary>
    /// Sets the custom cost ID and play amount to the given CardModificationInfo's singleton ID.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="costName"></param>
    /// <param name="value"></param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static CardModificationInfo SetCustomCostId(this CardModificationInfo mod, string costName, object value)
    {
        string valuePair = $"{costName},{value}";
        if (mod.HasCustomCostsId())
        {
            string newCosts;
            string oldCosts = GetCustomCostsIdString(mod); // get the current string of properties
            if (oldCosts.Contains(costName))
            {
                string oldValue = mod.GetCustomCostId(costName);
                string currentPair = GetIdKeyPair(oldCosts, costName);

                newCosts = oldCosts.Replace(currentPair, currentPair.Replace(oldValue, value.ToString()));
            }
            else
            {
                newCosts = oldCosts.Remove(oldCosts.Length - 1) + ";" + valuePair + "]"; // add the new value pair to the end
            }

            mod.singletonId = mod.singletonId.Replace(oldCosts, newCosts);
        }
        else if (mod.singletonId.IsNullOrWhitespace())
        {
            mod.singletonId = CUSTOM_COSTS + valuePair + "]";
        }
        else
        {
            mod.singletonId += CUSTOM_COSTS + valuePair + "]";
        }
        return mod;
    }

    /// <summary>
    /// Whetehr or not the given CardModificationInfo has the given custom cost.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="costName">The name of the custom cost to find.</param>
    /// <returns>True if the CardModificationInfo uses the given custom cost. False otherwise;</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static bool HasCustomCost(this CardModificationInfo mod, string costName) => (mod.GetExtendedProperty(costName) ?? mod.GetCustomCostId(costName)) != null;
    /// <summary>
    /// Retrieves the play amount of the given custom cost on the given CardModificationInfo.
    /// Checks extended properties before checking custom cost IDs.
    /// </summary>
    /// <param name="mod">The CardModificationinfo to access.</param>
    /// <param name="costName">The name of the custom cost to find.</param>
    /// <returns>The custom cost's play amount as an int.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static int GetCustomCost(this CardModificationInfo mod, string costName)
    {
        return mod.GetExtendedPropertyAsInt(costName) ?? mod.GetCustomCostIdValue(costName);
    }
    /// <summary>
    /// Retrieves the amount of the given custom cost the given CardModificationInfo costs as a string.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="costName">The name of the custom cost to find.</param>
    /// <returns>The string representation of the custom cost's play amount, or null if the custom cost ID is not present.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static string GetCustomCostId(this CardModificationInfo mod, string costName)
    {
        if (!mod.HasCustomCostId(costName))
            return null;

        string currentCosts = GetCustomCostsIdString(mod);
        int costIdx = currentCosts.IndexOf(costName + ",");
        if (costIdx == -1)
            return null;

        string pairString = currentCosts.Substring(costIdx);

        int valueIdx = pairString.IndexOf(';');
        if (valueIdx == -1) // if this is the last valuePair in the string
            valueIdx = pairString.IndexOf(']');

        return pairString.Substring(0, valueIdx).Replace(costName + ",", "");
    }
    /// <summary>
    /// Retrieves the cost amount of the given custom cost on the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="costName">Name of the custom cost to check for.</param>
    /// <returns>How much of the custom cost this CardModificationInfo needs, or 0 if the cost is not present.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static int GetCustomCostIdValue(this CardModificationInfo mod, string costName)
    {
        string valueStr = mod.GetCustomCostId(costName);
        if (valueStr == null)
            return 0;

        return int.TryParse(valueStr, out int result) ? result : 0;
    }
    /// <summary>
    /// Retrieves a list of all custom costs that the given CardModificationInfo adds.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>A List of all FullCardCosts the given CardModificationInfo adds to the attached card.</returns>
    /// <remarks>
    /// <b>
    /// Avoid using CardModificationInfos to add custom costs to cards. Instead use CardInfo.SetCustomCost and the much better [CardCostManager](InscryptionAPI.CardCosts.CardCostManager).
    /// <br/><br/>
    /// Only use this if you're trying to do cost adjustments like you can with vanilla costs and CardModificationInfos.
    /// </b>
    /// </remarks>
    // [Obsolete("Custom cost IDs on CardModInfos have been deprecated, please use the CardCostManager instead.")]
    public static List<CardCostManager.FullCardCost> GetCustomCostsFromMod(this CardModificationInfo mod)
    {
        List<CardCostManager.FullCardCost> costs = new();
        foreach (string key in mod.GetCardModExtensionTable().Keys)
        {
            if (CardCostManager.AllCustomCosts.Exists(x => x.CostName == key))
                costs.Add(CardCostManager.AllCustomCosts.Find(x => x.CostName == key));
        }
        if (mod.HasCustomPropertiesId())
        {
            List<string> modCosts = new();
            CardModificationInfoManager.GetCustomCostsFromId(mod.singletonId).ForEach(x => modCosts.Add(x.Split(',')[0]));
            costs.AddRange(CardCostManager.AllCustomCosts.Where(x => !costs.Contains(x) && modCosts.Contains(x.CostName)));
        }
        return costs;
    }

    #endregion

    #region ID Getters
    /// <summary>
    /// Creates a copy of the given CardModificationInfo's singleton ID without any of the API's custom property or cost substrings.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>A new string that is a copy of the CardModificationInfo's singleton ID but without any API-added substrings.</returns>
    public static string CleanId(this CardModificationInfo mod) => mod.IdWithoutCosts().IdWithoutProperties();
    /// <summary>
    /// Creates a new string that is a copy of the given CardModificationInfo's singleton ID string without the custom properties ID substring.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>A clean copy of the CardModificationInfo's singleton ID without the custom properties ID substring.</returns>
    public static string IdWithoutProperties(this CardModificationInfo mod) => mod.singletonId.IdWithoutProperties();
    /// <summary>
    /// Creates a new string that is a copy of the given string without the custom properties ID substring.
    /// </summary>
    /// <param name="singletonId">The string to access.</param>
    /// <returns>A clean copy of the singleton ID without the custom properties ID substring.</returns>
    public static string IdWithoutProperties(this string singletonId)
    {
        if (!singletonId.HasCustomPropertiesId())
            return singletonId;

        string properties = GetCustomPropertiesIdString(singletonId);
        return singletonId.Replace(properties, "");
    }
    /// <summary>
    /// Creates a new string that is a copy of the given CardModificationInfo's singleton ID string without the custom costs ID substring.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>A clean copy of the CardModificationInfo's singleton ID without the custom costs substring.</returns>
    public static string IdWithoutCosts(this CardModificationInfo mod) => mod.singletonId.IdWithoutCosts();
    /// <summary>
    /// Creates a new string that is a copy of the given string without the custom costs substring.
    /// </summary>
    /// <param name="singletonId">The string to access.</param>
    /// <returns>A clean copy of the singleton ID without the custom costs substring.</returns>
    public static string IdWithoutCosts(this string singletonId)
    {
        if (!singletonId.HasCustomCostsId())
            return singletonId;

        string costs = GetCustomCostsIdString(singletonId);
        return singletonId.Replace(costs, "");
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

    #region RemoveGems
    /// <summary>
    /// Removes all gem cost negations on the CardModificationInfo.
    /// Only considers extended properties.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    public static void ClearAllRemovedGemsCosts(this CardModificationInfo mod)
    {
        if (mod.HasRemovedGreenGemCost())
            mod.RemoveGreenGemCost(false);

        if (mod.HasRemovedOrangeGemCost())
            mod.RemoveOrangeGemCost(false);

        if (mod.HasRemoveBlueGemCost())
            mod.RemoveBlueGemCost(false);
    }
    /// <summary>
    /// Determines which gem costs have been removed/negated by the given CardModificationInfo using extended properties.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>A List of all removed gem cost colours.</returns>
    public static List<GemType> RemovedGemsCosts(this CardModificationInfo mod)
    {
        List<GemType> gems = new(3);
        if (mod.HasRemovedGreenGemCost())
            gems.Add(GemType.Green);

        if (mod.HasRemovedOrangeGemCost())
            gems.Add(GemType.Orange);

        if (mod.HasRemoveBlueGemCost())
            gems.Add(GemType.Blue);

        return gems;
    }
    /// <summary>
    /// Clears extended properties on the given CardModificationInfo that remove single gem costs with the given types.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="gemTypes">A collection of all gem colours that should be re-added/stop being negated.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo RemoveGemsCost(this CardModificationInfo mod, params GemType[] gemTypes)
    {
        if (gemTypes.Contains(GemType.Green))
            mod.RemoveGreenGemCost(true);

        if (gemTypes.Contains(GemType.Orange))
            mod.RemoveOrangeGemCost(true);

        if (gemTypes.Contains(GemType.Blue))
            mod.RemoveBlueGemCost(true);

        return mod;
    }
    /// <summary>
    /// Clears all extended properties on the given CardModificationInfo that remove single gem costs.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="gemTypes">A collection of all gem colours that should be re-added/stop being negated.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    public static CardModificationInfo ClearRemovedGemsCost(this CardModificationInfo mod, params GemType[] gemTypes)
    {
        if (gemTypes.Contains(GemType.Green))
            mod.RemoveGreenGemCost(false);

        if (gemTypes.Contains(GemType.Orange))
            mod.RemoveOrangeGemCost(false);

        if (gemTypes.Contains(GemType.Blue))
            mod.RemoveBlueGemCost(false);

        return mod;
    }
    /// <summary>
    /// Uses extended properties to make the given CardModificationInfo negate a single green gem cost.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="remove">Whether the given CardModificationInfo should remove a single green gem from the play cost.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    /// <remarks>If you want to negate multiple gems of the same colour, you'll need to create multiple CardModificationInfos.</remarks>
    public static CardModificationInfo RemoveGreenGemCost(this CardModificationInfo mod, bool remove) => mod.SetExtendedProperty("RemoveGreenGem", remove, true);
    /// <summary>
    /// Uses extended properties to make the given CardModificationInfo negate a single orange gem cost.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="remove">Whether the given CardModificationInfo should remove a single orange gem from the play cost.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    /// <remarks>If you want to negate multiple gems of the same colour, you'll need to create multiple CardModificationInfos.</remarks>
    public static CardModificationInfo RemoveOrangeGemCost(this CardModificationInfo mod, bool remove) => mod.SetExtendedProperty("RemoveOrangeGem", remove, true);
    /// <summary>
    /// Uses extended properties to make the given CardModificationInfo negate a single blue gem cost.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="remove">Whether the given CardModificationInfo should remove a single blue gem from the play cost.</param>
    /// <returns>The same CardModificationInfo so a chain can continue.</returns>
    /// <remarks>If you want to negate multiple gems of the same colour, you'll need to create multiple CardModificationInfos.</remarks>
    public static CardModificationInfo RemoveBlueGemCost(this CardModificationInfo mod, bool remove) => mod.SetExtendedProperty("RemoveBlueGem", remove, true);
    /// <summary>
    /// Determines if the given CardModificationInfo removes a green gems cost.
    /// Separate from the vanilla nullifyGemsCost, which negates all gem costs.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>True if the CardModificationInfo negates a green gem.</returns>
    public static bool HasRemovedGreenGemCost(this CardModificationInfo mod) => mod.GetExtendedPropertyAsBool("RemoveGreenGem") ?? false;
    /// <summary>
    /// Determines if the given CardModificationInfo removes a orange gems cost.
    /// Separate from the vanilla nullifyGemsCost, which negates all gem costs.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>True if the CardModificationInfo negates a orange gem.</returns>
    public static bool HasRemovedOrangeGemCost(this CardModificationInfo mod) => mod.GetExtendedPropertyAsBool("RemoveOrangeGem") ?? false;
    /// <summary>
    /// Determines if the given CardModificationInfo removes a blue gems cost.
    /// Separate from the vanilla nullifyGemsCost, which negates all gem costs.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>True if the CardModificationInfo negates a blue gem.</returns>
    public static bool HasRemoveBlueGemCost(this CardModificationInfo mod) => mod.GetExtendedPropertyAsBool("RemoveBlueGem") ?? false;
    /// <summary>
    /// Determines if the given CardModificationInfo removes any gem costs using the API's extended properties.
    /// Does not check nullifyGemsCost.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <returns>True if the CardModificationInfo negates any single gem.</returns>
    public static bool HasRemovedAnyGemCost(this CardModificationInfo mod) => mod.HasRemovedGreenGemCost() || mod.HasRemovedOrangeGemCost() || mod.HasRemoveBlueGemCost();
    /// <summary>
    /// Determines if the given CardModificationInfo removes a gems cost of the given colour.
    /// Separate from the vanilla nullifyGemsCost, which negates all gem costs.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="gemType">The colour gem to search for.</param>
    /// <returns>True if the CardModificationInfo negates a gem with the given colour.</returns>
    public static bool HasRemovedGemCost(this CardModificationInfo mod, GemType gemType)
    {
        return gemType switch
        {
            GemType.Green => mod.HasRemovedGreenGemCost(),
            GemType.Orange => mod.HasRemovedOrangeGemCost(),
            GemType.Blue => mod.HasRemoveBlueGemCost(),
            _ => false,
        };
    }
    #endregion
}
