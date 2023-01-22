using DiskCardGame;
using static InscryptionAPI.Pelts.PeltManager;

namespace InscryptionAPI.Pelts.Extensions;

public static class PeltExtensions
{
    /// <summary>
    /// Sets the CustomPeltData's pluginGuid (this is done automatically so only use this if you know what you're doing!).
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="pluginGuid">The GUID of the plugin this pelt is associated with.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetPluginGuid(this CustomPeltData peltData, string pluginGuid)
    {
        peltData.pluginGuid = pluginGuid;
        return peltData;
    }

    #region Buy Price Fields
    /// <summary>
    /// Sets the pelt's base price, and optionally sets its maximum price.
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="baseBuyPrice">The base price of the pelt before calculations occur.</param>
    /// <param name="maxBuyPrice">The highest price the pelt can be offered for - only considers values above 0.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetBuyPrice(this CustomPeltData peltData, int baseBuyPrice, int maxBuyPrice = 0)
    {
        peltData.baseBuyPrice = baseBuyPrice;
        if (maxBuyPrice > 0)
            peltData.maxBuyPrice = maxBuyPrice;
        return peltData;
    }
    /// <summary>
    /// Sets the pelt's max possible price.
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="maxBuyPrice">The highest price the pelt can be offered for - only considers values above 0r.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetMaxBuyPrice(this CustomPeltData peltData, int maxBuyPrice)
    {
        if (maxBuyPrice > 0)
            peltData.maxBuyPrice = maxBuyPrice;
        return peltData;
    }
    /// <summary>
    /// Sets the buy price modifiers for when Trapper and Trader have been defeated or when the Expensive Pelts challenge is active.
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="bossPriceCut">The price divider for when the Trapper and Trader have been defeated. Default value halves the price.</param>
    /// <param name="challengePriceHike">The price multiplier for when the Expensive Pelts challenge is active. Default value doubles the price.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetBuyPriceModifiers(this CustomPeltData peltData, int bossPriceCut = 2, int challengePriceHike = 2)
    {
        peltData.bossDefeatedPriceReduction = bossPriceCut;
        peltData.expensivePeltsPriceMultiplier = challengePriceHike;
        return peltData;
    }
    /// <summary>
    /// Sets the function used to determine how this pelt's price will increase across a run.
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="buyPriceFunc">The Func used to calculate the price increase.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetBuyPriceAdjustment(this CustomPeltData peltData, Func<int, int> buyPriceAdjustment)
    {
        peltData.BuyPriceAdjustment = buyPriceAdjustment;
        return peltData;
    }
    #endregion

    /// <summary>
    /// Sets whether the pelt can be bought from the Trapper.
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="soldByTrapper">Whether the pelt should be sellable by the Trapper.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetIsSoldByTrapper(this CustomPeltData peltData, bool soldByTrapper = true)
    {
        peltData.isSoldByTrapper = soldByTrapper;
        return peltData;
    }
    /// <summary>
    /// Sets the number of card choices the Trader will present to the player in exchange for this pelt.
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="soldByTrapper">Whether the pelt should be sellable by the Trapper.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetNumberOfTraderChoices(this CustomPeltData peltData, int numOfChoices)
    {
        peltData.choicesOfferedByTrader = numOfChoices;
        return peltData;
    }
    /// <summary>
    /// Sets the function used to determine what cards can be offered by the Trader in exchange for this pelt.
    /// </summary>
    /// <param name="peltData">The CustomPeltData to access.</param>
    /// <param name="getChoicesList">The Func that decides what cards can be offered.</param>
    /// <returns>The same CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData SetCardChoices(this CustomPeltData peltData, Func<List<CardInfo>> getCardChoices)
    {
        peltData.GetCardChoices = getCardChoices;
        return peltData;
    }
}