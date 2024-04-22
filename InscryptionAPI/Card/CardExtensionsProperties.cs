using DiskCardGame;
using InscryptionAPI.CardCosts;
using static InscryptionAPI.CardCosts.CardCostManager;

namespace InscryptionAPI.Card;

public static partial class CardExtensions
{
    /// <summary>
    /// Adds a custom property value to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetExtendedProperty(this CardInfo info, string propertyName, object value)
    {
        info.GetCardExtensionTable()[propertyName] = value?.ToString();
        return info;
    }

    /// <summary>
    /// Gets a custom property value from the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">The name of the property to get the value of.</param>
    /// <returns>The custom property value as a string. If it doesn't exist, returns null.</returns>
    public static string GetExtendedProperty(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var ret);
        return ret;
    }

    /// <summary>
    /// Gets a custom property as a nullable int.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int.</returns>
    public static int? GetExtendedPropertyAsInt(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return int.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a nullable float.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float.</returns>
    public static float? GetExtendedPropertyAsFloat(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return float.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a nullable boolean.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean.</returns>
    public static bool? GetExtendedPropertyAsBool(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }

    #region Tidal Lock
    /// <summary>
    /// Checks if this card will be killed by the effect of Tidal Lock.
    /// </summary>
    /// <param name="item">PlayableCard to access.</param>
    /// <returns>True if the card is affected by Tidal Lock.</returns>
    public static bool IsAffectedByTidalLock(this PlayableCard item) => item.Info.IsAffectedByTidalLock();
    /// <summary>
    /// Checks if this card info will be killed by the effect of Tidal Lock.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>True if the card info is affected by Tidal Lock.</returns>
    public static bool IsAffectedByTidalLock(this CardInfo info) => info.GetExtendedPropertyAsBool("AffectedByTidalLock") ?? false;

    /// <summary>
    /// Sets whether the card should be killed by Tidal Lock's effect.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAffectedByTidalLock(this CardInfo info, bool affectedByTidalLock = true)
    {
        info.SetExtendedProperty("AffectedByTidalLock", affectedByTidalLock);
        return info;
    }
    #endregion

    #region TransformerBeastCardId
    /// <summary>
    /// Gets the string value of the extended property TransformerCardId. Can be null.
    /// </summary>
    /// <param name="item">PlayableCard to access.</param>
    /// <returns>The string value of the extended property TransformerCardId.</returns>
    public static string GetTransformerCardId(this PlayableCard item) => item.Info.GetTransformerCardId();
    /// <summary>
    /// Gets the string value of the extended property TransformerCardId. Can be null.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The string value of the extended property TransformerCardId.</returns>
    public static string GetTransformerCardId(this CardInfo info) => info.GetExtendedProperty("TransformerCardId");

    /// <summary>
    /// Sets whether the card should be killed by Tidal Lock's effect.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTransformerCardId(this CardInfo info, string transformerCardId)
    {
        info.SetExtendedProperty("TransformerCardId", transformerCardId);
        return info;
    }

    #endregion

    #region Custom Costs

    public static bool HasCustomCost(this CardInfo info, FullCardCost customCost) => info.GetCustomCost(customCost) != 0;

    /// <summary>
    /// Sets a card's custom cost property using the provided FullCardCost object as a reference.
    /// </summary>
    /// <param name="info">The CardInfo to modify.</param>
    /// <param name="customCost">The FullCardCost representing the cost we want to set.</param>
    /// <param name="amount">How much of the custom cost the CardInfo should need to be played.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCustomCost(this CardInfo info, FullCardCost customCost, int amount) => info.SetCustomCost(customCost.CostName, amount);

    /// <summary>
    /// A variant of SetExtendedProperty intended to be used to set properties representing custom costs. Primarily provided for clarity of purpose.
    /// </summary>
    /// <param name="info">The CardInfo to modify.</param>
    /// <param name="costName">The name of the cost we want to set.</param>
    /// <param name="amount">How much of the custom cost the CardInfo should need to be played.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCustomCost(this CardInfo info, string costName, int amount) => info.SetExtendedProperty(costName, amount);
    public static CardInfo SetCustomCost<T>(this CardInfo info, int amount) where T : CustomCardCost
    {
        FullCardCost cost = CardCostManager.AllCustomCosts.Find(x => x.CostBehaviour == typeof(T));
        return info.SetCustomCost(cost.CostName, amount);
    }

    /// <summary>
    /// Variant of GetCustomCost that automatically retrieves the given cost's canBeNegative value.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="costName"></param>
    /// <returns></returns>
    public static int GetCustomCostAmount(this PlayableCard card, string costName) => card.Info.GetCustomCostAmount(costName);
    public static int GetCustomCostAmount(this CardInfo cardInfo, string costName) => cardInfo.GetCustomCost(costName, AllCustomCosts.Find(x => x.CostName == costName).CanBeNegative);

    public static int GetCustomCost(this PlayableCard card, string costName, bool canBeNegative = false) => card.Info.GetCustomCost(costName, canBeNegative);
    public static int GetCustomCost(this CardInfo cardInfo, string costName, bool canBeNegative = false)
    {
        int retval = cardInfo.GetExtendedPropertyAsInt(costName) ?? 0;
        foreach (CardModificationInfo mod in cardInfo.Mods)
        {
            retval += (mod.GetExtendedPropertyAsInt(costName) ?? 0) + mod.GetCustomCostIdValue(costName);
        }
        if (!canBeNegative && retval < 0)
            retval = 0;

        return retval;
    }

    public static int GetCustomCost<T>(this PlayableCard card, bool canBeNegative = false) where T : CustomCardCost
    {
        FullCardCost cost = CardCostManager.AllCustomCosts.Find(x => x.CostBehaviour == typeof(T));
        return card.Info.GetCustomCost(cost.CostName, canBeNegative);
    }

    public static int GetCustomCost<T>(this CardInfo cardInfo, bool canBeNegative = false) where T : CustomCardCost
    {
        FullCardCost cost = CardCostManager.AllCustomCosts.Find(x => x.CostBehaviour == typeof(T));
        return cardInfo.GetCustomCost(cost.CostName, canBeNegative);
    }

    public static int GetCustomCostAmount<T>(this PlayableCard card) where T : CustomCardCost
    {
        FullCardCost cost = CardCostManager.AllCustomCosts.Find(x => x.CostBehaviour == typeof(T));
        return card.GetCustomCost(cost);
    }

    public static int GetCustomCostAmount<T>(this CardInfo cardInfo) where T : CustomCardCost
    {
        FullCardCost cost = CardCostManager.AllCustomCosts.Find(x => x.CostBehaviour == typeof(T));
        return cardInfo.GetCustomCost(cost);
    }

    public static int GetCustomCost(this PlayableCard card, FullCardCost fullCardCost)
    {
        return card.GetCustomCostAmount(fullCardCost.CostName);
    }
    public static int GetCustomCost(this CardInfo cardInfo, FullCardCost fullCardCost)
    {
        return cardInfo.GetCustomCostAmount(fullCardCost.CostName);
    }

    public static List<CustomCardCost> GetCustomCardCosts(this DiskCardGame.Card card)
    {
        InscryptionAPIPlugin.Logger.LogDebug($"[GetCustomCardCosts] {card != null}");
        CustomCardCost[] components = card?.GetComponents<CustomCardCost>();
        InscryptionAPIPlugin.Logger.LogDebug($"[GetCustomCardCosts] {components != null} {components?.Length}");
        return components?.ToList() ?? new();
    }
    public static List<FullCardCost> GetCustomCosts(this CardInfo info)
    {
        List<FullCardCost> costs = new();
        foreach (string key in info.GetCardExtensionTable().Keys)
        {
            if (AllCustomCosts.Exists(x => x.CostName == key))
                costs.Add(AllCustomCosts.Find(x => x.CostName == key));
        }
        foreach (CardModificationInfo mod in info.Mods)
        {
            costs.AddRange(mod.GetCustomCostsFromMod());
        }
        return costs;
    }

    #endregion
}