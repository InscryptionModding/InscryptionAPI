using BepInEx;
using DiskCardGame;
using InscryptionAPI.Guid;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Pelts;

public static class PeltManager
{
    private class VanillaPeltData : PeltData
    {
        public override int BuyPrice => GetBasePeltData().Find((a) => a.Item1 == this.peltCardName).Item2;
    }

    public class PeltData
    {
        public string pluginGuid;
        public string peltCardName;

        public bool isSoldByTrapper = true;

        public Func<List<CardInfo>> CardChoices;
        public Func<int, int> BuyPriceAdjustment = (int basePrice) => basePrice + RunState.CurrentRegionTier;
        public Action<CardInfo> ModifyCardChoiceAtTrader = null;

        public int baseBuyPrice;
        public int maxBuyPrice;

        public int extraAbilitiesToAdd;

        public int choicesOfferedByTrader = 8;
        public int bossDefeatedPriceReduction = 2;
        public int expensivePeltsPriceMultiplier = 2;

        public virtual int BuyPrice
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
    }

    internal static List<PeltData> AllNewPelts = new List<PeltData>();
    private static List<PeltData> BasePelts = null;

    internal static string[] BasePeltNames { get; } = new string[]
    {
        "PeltHare",
        "PeltWolf",
        "PeltGolden"
    };

    internal static int[] BasePeltPrices
    {
        get
        {
            BuyPeltsSequencer instance = SpecialNodeHandler.Instance.buyPeltsSequencer;
            return new int[]
            {
                2 / (instance.TrapperBossDefeated ? 2 : 1) * (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 2 : 1),
                (4 + RunState.CurrentRegionTier) / (instance.TrapperBossDefeated ? 2 : 1) * (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 2 : 1),
                Mathf.Min(20, (7 + RunState.CurrentRegionTier * 2) / (instance.TrapperBossDefeated ? 2 : 1) * (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 2 : 1))
            };
        }
    }

    internal static List<PeltData> AllPelts()
    {
        BasePelts ??= CreateBasePelts();
        return BasePelts.Concat(AllNewPelts).ToList();
    }

    internal static List<Tuple<string, int>> GetBasePeltData()
    {
        List<Tuple<string, int>> data = new List<Tuple<string, int>>();
        for (int i = 0; i < BasePeltNames.Length; i++)
        {
            string peltName = BasePeltNames[i];
            data.Add(new Tuple<string, int>(peltName, BasePeltPrices[i]));
        }

        // Return cache
        return data;
    }

    private static List<PeltData> CreateBasePelts()
    {
        List<PeltData> pelts = new List<PeltData>();

        for (int i = 0; i < BasePeltNames.Length; i++)
        {
            var peltData = new VanillaPeltData()
            {
                peltCardName = BasePeltNames[i],
                choicesOfferedByTrader = 8,
                extraAbilitiesToAdd = 0,
                isSoldByTrapper = true,
            };
            peltData.CardChoices = static () => CardLoader.GetUnlockedCards(CardMetaCategory.TraderOffer, CardTemple.Nature);
            pelts.Add(peltData);
        }

        // Wolf Pelt
        pelts[1].extraAbilitiesToAdd = 1;

        // Golden Pelt
        pelts[2].choicesOfferedByTrader = 4;
        pelts[2].CardChoices = static () => CardLoader.GetUnlockedCards(CardMetaCategory.Rare, CardTemple.Nature);

        return pelts;
    }

    /// <summary>
    /// Creates a new instance of CustomPeltData then adds it to the game.
    /// </summary>
    /// <param name="pluginGuid">GUID of the mod adding this pelt.</param>
    /// <param name="peltCardInfo">The CardInfo for the actual pelt card.</param>
    /// <param name="baseBuyPrice">The starting price of this pelt when buying from the Trapper.</param>
    /// <param name="extraAbilitiesToAdd">The number of extra sigils card choices will have when trading this pelt to the Trader.</param>
    /// <param name="choicesOfferedByTrader">How many cards to offer the player when trading the pelt.</param>
    /// <param name="getCardChoices">The list of possible cards the Trader will offer for this pelt.</param>
    /// <returns>The newly created CustomPeltData so a chain can continue.</returns>
    public static PeltData New(string pluginGuid, CardInfo peltCardInfo, int baseBuyPrice, int extraAbilitiesToAdd, int choicesOfferedByTrader, Func<List<CardInfo>> getCardChoices)
    {
        PeltData peltData = new()
        {
            pluginGuid = pluginGuid,
            peltCardName = peltCardInfo.name,
            CardChoices = getCardChoices,
            baseBuyPrice = baseBuyPrice,
            extraAbilitiesToAdd = extraAbilitiesToAdd,
            choicesOfferedByTrader = choicesOfferedByTrader
        };
        Add(peltData);

        return peltData;
    }

    /// <summary>
    /// Adds a CustomPeltData to the game, enabling it to be usable with the Trapper and Trader.
    /// </summary>
    /// <param name="data">The CustomPeltData to add.</param>
    public static void Add(PeltData data)
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

    public static List<PeltData> AllPeltsAvailableAtTrader()
    {
        List<PeltData> peltNames = new List<PeltData>();
        foreach (PeltData data in AllPelts())
        {
            if (data.isSoldByTrapper)
            {
                peltNames.Add(data);
            }
        }

        return peltNames;
    }

    public static int GetCostOfPelt(string peltName)
    {
        PeltData pelt = GetPelt(peltName);
        if (pelt == null)
        {
            return 1;
        }

        return pelt.BuyPrice;
    }

    public static PeltData GetPelt(string peltName)
    {
        return AllPelts().Find((a) => a.peltCardName == peltName);
    }
}