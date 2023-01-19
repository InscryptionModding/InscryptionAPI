using BepInEx;
using DiskCardGame;
using InscryptionAPI.Guid;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Pelts;

public static class PeltManager
{
    public class CustomPeltData
    {
        public string pluginGuid;
        public string peltCardName;

        public bool isSoldByTrapper = true;

        public Func<List<CardInfo>> GetCardChoices;
        public Func<int, int> BuyPriceAdjustment = (int basePrice) => basePrice + RunState.CurrentRegionTier;

        public int baseBuyPrice;
        public int maxBuyPrice;

        public int extraAbilitiesToAdd;

        public int choicesOfferedByTrader = 8;
        public int bossDefeatedPriceReduction = 2;
        public int expensivePeltsPriceMultiplier = 2;

        public int BuyPrice
        {
            get
            {
                int bossDefeatMult = !StoryEventsData.EventCompleted(StoryEvent.TrapperTraderDefeated) ? 1 : (bossDefeatedPriceReduction <= 0 ? 1 : bossDefeatedPriceReduction);
                int challengeMult = !AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 1 : (expensivePeltsPriceMultiplier <= 0 ? 1 : expensivePeltsPriceMultiplier);
                int finalPrice = BuyPriceAdjustment(baseBuyPrice) / bossDefeatMult * challengeMult;
                if (maxBuyPrice > 0)
                    finalPrice = Mathf.Min(maxBuyPrice, finalPrice);
                return Mathf.Max(1, finalPrice);
            }
        }
        public List<CardInfo> CardChoices => GetCardChoices();
    }

    public static List<CustomPeltData> AllNewPelts = new();

    /// <summary>
    /// Creates a new instance of CustomPeltData then adds it to the game.
    /// </summary>
    /// <param name="peltCardInfo">The CardInfo for the actual pelt card.</param>
    /// <param name="getCardChoices">The list of possible cards the Trader will offer for this pelt.</param>
    /// <param name="baseBuyPrice">The starting price of this pelt when buying from the Trapper.</param>
    /// <param name="extraAbilitiesToAdd">The number of extra sigils card choices will have when trading this pelt to the Trader.</param>
    /// <param name="isSoldByTrapper">Whether this type of pelt can be sold by the Trapper.</param>
    /// <returns>The newly created CustomPeltData so a chain can continue.</returns>
    public static CustomPeltData New(CardInfo peltCardInfo, Func<List<CardInfo>> getCardChoices, int baseBuyPrice, int extraAbilitiesToAdd, bool isSoldByTrapper = true)
    {
        CustomPeltData peltData = new()
        {
            peltCardName = peltCardInfo.name,
            GetCardChoices = getCardChoices,
            baseBuyPrice = baseBuyPrice,
            extraAbilitiesToAdd = extraAbilitiesToAdd,
            isSoldByTrapper = isSoldByTrapper
        };
        Add(peltData);

        return peltData;
    }

    /// <summary>
    /// Adds a CustomPeltData to the game, enabling it to be usable with the Trapper and Trader.
    /// </summary>
    /// <param name="data">The CustomPeltData to add.</param>
    public static void Add(CustomPeltData data)
    {
        if (data.peltCardName.IsNullOrWhiteSpace())
        {
            InscryptionAPIPlugin.Logger.LogError("Couldn't create CustomPeltData - missing card name!");
            return;
        }

        data.pluginGuid ??= TypeManager.GetModIdFromCallstack(Assembly.GetCallingAssembly());

        if (!AllNewPelts.Contains(data))
            AllNewPelts.Add(data);
    }

    public static string[] AllPeltsAvailableAtTrader()
    {
        List<string> peltNames = new();
        foreach (string peltName in CardLoader.PeltNames)
        {
            CustomPeltData data = AllNewPelts.Find((a) => a.peltCardName == peltName);
            if (data == null || data.isSoldByTrapper)
                peltNames.Add(peltName);
        }

        return peltNames.ToArray();
    }
}