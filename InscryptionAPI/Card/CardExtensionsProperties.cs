using DiskCardGame;

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
}