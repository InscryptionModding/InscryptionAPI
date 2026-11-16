using BepInEx;
using DiskCardGame;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Guid;
using Sirenix.Utilities;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Pelts;

/// <summary>
/// Manager class for managing new pelt cards that can be bought and traded in Act 1.
/// </summary>
public static class PeltManager
{
    private class VanillaPeltData : PeltData
    {
        public override int BuyPrice => GetBasePeltData().Find((a) => a.Item1 == peltCardName).Item2;
    }

    /// <summary>
    /// Utility class containing all data about pelt cards needed for them to be used in-game at the Trapper and Trader.
    /// </summary>
    public class PeltData
    {
        /// <summary>
        /// The GUID of the mod adding this pelt.
        /// </summary>
        public string pluginGuid;
        /// <summary>
        /// The internal name of the actual pelt card that will be added to the player's deck and then traded away.
        /// </summary>
        public string peltCardName;
        /// <summary>
        /// The internal name of the pelt's tier.
        /// </summary>
        public string peltTierName = null;
        /// <summary>
        /// If the pelt card can be bought at the Trapper.
        /// </summary>
        public bool isSoldByTrapper = true;

        /// <summary>
        /// Function for determining what cards the Trader will offer in exchange for this pelt.
        /// </summary>
        public Func<List<CardInfo>> CardChoices;
        /// <summary>
        /// Function for determining how much this pelt's price will change when buying it at the Trapper.
        /// </summary>
        public Func<int, int> BuyPriceAdjustment = (int basePrice) => basePrice + RunState.CurrentRegionTier;
        /// <summary>
        /// Action that's called to modify card choices offered by the Trader (eg., change stats, etc.).
        /// </summary>
        public Action<CardInfo> ModifyCardChoiceAtTrader = null;

        /// <summary>
        /// The minimum cost of this pelt when offered by the Trapper.
        /// </summary>
        public int baseBuyPrice;
        /// <summary>
        /// The maximum cost of this pelt when offered by the Trapper.
        /// </summary>
        public int maxBuyPrice;

        /// <summary>
        /// How many extra abilities should be added to card choices at the Trader.
        /// </summary>
        public int extraAbilitiesToAdd;

        /// <summary>
        /// How many cards choices the Trader will present to the player for this pelt.
        /// </summary>
        public int choicesOfferedByTrader = 8;
        /// <summary>
        /// How much the purchase price for this pelt should be reduced after beating the Trapper and Trader boss.
        /// </summary>
        public int bossDefeatedPriceReduction = 2;
        /// <summary>
        /// The price multiplier when the Expensive Pelts challenge is active. Default is double price.
        /// </summary>
        public int expensivePeltsPriceMultiplier = 2;

        /// <summary>
        /// The actual buy price for this pelt, implementing all multipliers, adjustments, and the set min and max price.
        /// </summary>
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

    internal static List<PeltData> AllNewPelts = new();
    private static List<PeltData> BasePelts = null;

    internal static string[] BasePeltNames { get; } = new string[]
    {
        "PeltHare",
        "PeltWolf",
        "PeltGolden"
    };

    internal static int[] BasePeltPrices
    {
        get // base w/o modifiers: 2, 4, 7
        {
            int expensivePeltsMult = AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.ExpensivePelts) ? 2 : 1;
            int defeatedTrapperMult = SpecialNodeHandler.Instance.buyPeltsSequencer.TrapperBossDefeated ? 2 : 1;
            return new int[]
            {
                2 / defeatedTrapperMult * expensivePeltsMult,
                (4 + RunState.CurrentRegionTier) / defeatedTrapperMult * expensivePeltsMult,
                Mathf.Min(20, (7 + RunState.CurrentRegionTier * 2) / defeatedTrapperMult * expensivePeltsMult)
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
        List<Tuple<string, int>> data = new();
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
        List<PeltData> pelts = new();

        for (int i = 0; i < BasePeltNames.Length; i++)
        {
            VanillaPeltData peltData = new()
            {
                peltCardName = BasePeltNames[i],
                choicesOfferedByTrader = 8,
                extraAbilitiesToAdd = 0,
                isSoldByTrapper = true,
                CardChoices = static () => CardLoader.GetUnlockedCards(CardMetaCategory.TraderOffer, CardTemple.Nature)
            };
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
    /// <param name="peltCardInfo">The <see cref="CardInfo"/> for the actual pelt card.</param>
    /// <param name="getCardChoices">The list of possible cards the Trader will offer for this pelt.</param>
    /// <param name="baseBuyPrice">The starting price of this pelt when buying from the Trapper.</param>
    /// <param name="extraAbilitiesToAdd">The number of extra sigils card choices will have when trading this pelt to the Trader.</param>
    /// <param name="choicesOfferedByTrader">How many cards to offer the player when trading the pelt.</param>
    /// <returns>The newly created CustomPeltData so a chain can continue.</returns>
    public static PeltData New(string pluginGuid, CardInfo peltCardInfo, Func<List<CardInfo>> getCardChoices, int baseBuyPrice, int extraAbilitiesToAdd = 0, int choicesOfferedByTrader = 8)
    {
        return New(pluginGuid, peltCardInfo, baseBuyPrice, extraAbilitiesToAdd, choicesOfferedByTrader, getCardChoices);
    }

    /// <summary>
    /// Creates a new instance of CustomPeltData then adds it to the game. Requires all parameter arguments be filled.
    /// </summary>
    /// <param name="pluginGuid">GUID of the mod adding this pelt.</param>
    /// <param name="peltCardInfo">The <see cref="CardInfo"/> for the actual pelt card.</param>
    /// <param name="getCardChoices">The list of possible cards the Trader will offer for this pelt.</param>
    /// <param name="baseBuyPrice">The starting price of this pelt when buying from the Trapper.</param>
    /// <param name="extraAbilitiesToAdd">The number of extra sigils card choices will have when trading this pelt to the Trader.</param>
    /// <param name="choicesOfferedByTrader">How many cards to offer the player when trading the pelt.</param>
    /// <returns>The newly created CustomPeltData so a chain can continue.</returns>
    public static PeltData New(string pluginGuid, CardInfo peltCardInfo, int baseBuyPrice, int extraAbilitiesToAdd, int choicesOfferedByTrader, Func<List<CardInfo>> getCardChoices)
    {
        if (getCardChoices == null)
        {
            throw new ArgumentNullException("CardChoices function cannot be null!");
        }

        PeltData peltData = new()
        {
            pluginGuid = pluginGuid,
            peltCardName = peltCardInfo.name,
            peltTierName = GetTierNameFromPelt(peltCardInfo.displayedName),
            CardChoices = getCardChoices,
            baseBuyPrice = baseBuyPrice,
            extraAbilitiesToAdd = extraAbilitiesToAdd,
            choicesOfferedByTrader = choicesOfferedByTrader
        };
        Add(peltData);

        return peltData;
    }

    /// <summary>
    /// Adds a <see cref="PeltData"/> to the game, enabling it to be usable with the Trapper and Trader.
    /// </summary>
    /// <param name="data">The <see cref="PeltData"/> to add.</param>
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

    /// <summary>
    /// Determines what pelts can be bought at the Trapper.
    /// </summary>
    /// <returns>A list of all pelts that can be bought at the Trapper.</returns>
    public static List<PeltData> AllPeltsAvailableAtTrader()
    {
        List<PeltData> peltNames = new();
        peltNames.AddRange(AllPelts().Where(x => x.isSoldByTrapper));
        return peltNames;
    }

    /// <summary>
    /// Gets the buy price of the pelt with the given name.
    /// </summary>
    /// <param name="peltName">The name of the pelt to search for.</param>
    /// <returns>The pelt's <see cref="PeltData.BuyPrice"/>, or 1 if the pelt could not be found.</returns>
    public static int GetCostOfPelt(string peltName)
    {
        PeltData pelt = GetPelt(peltName);
        if (pelt == null)
        {
            return 1;
        }

        return pelt.BuyPrice;
    }

    /// <summary>
    /// Gets the <see cref="PeltData"/> with the given name.
    /// </summary>
    /// <param name="peltName">Name of the pelt to retrieve.</param>
    /// <returns>The <see cref="PeltData"/> with the given name, or null if it couldn't be found.</returns>
    public static PeltData GetPelt(string peltName)
    {
        return AllPelts().Find((a) => a.peltCardName == peltName);
    }

    internal static void CreateDialogueEvents()
    {
        foreach (PeltData peltData in AllPeltsAvailableAtTrader())
        {
            string name = peltData.peltTierName ?? GetTierNameFromData(peltData);
            string dialogueId = "TraderPelts" + name;
            if (!DialogueManager.CustomDialogue.Exists(x => x.DialogueEvent.id == dialogueId))
            {
                if (name.Contains("pelt") || name.Contains("pelt"))
                {
                    DialogueManager.GenerateEvent(InscryptionAPIPlugin.ModGUID, dialogueId,
                        new()
                        {
                            name + "pelts..."
                        }
                    );
                }
                else
                {
                    DialogueManager.GenerateEvent(InscryptionAPIPlugin.ModGUID, dialogueId,
                        new()
                        {
                            name + "..."
                        }
                    );
                }
            }
        }
    }

    /// <summary>
    /// Determines the tier name for a pelt from its internal card name.
    /// </summary>
    /// <param name="cardName">The internal name of the pelt card.</param>
    /// <returns>The tier name for the pelt derived from the card name.</returns>
    public static string GetTierNameFromPelt(string cardName)
    {
        string result = "";
        if (cardName.Contains("pelt") || cardName.Contains("pelt"))
        {
            result = cardName.ToLowerInvariant().Replace("pelt", "").Replace("pelts", "");
            result = result.Split('_').Last().ToTitleCase();
        }
        else
        {
            result = cardName.ToLowerInvariant();
            result = result.Split('_').Last().ToTitleCase();
        }

        return result;
    }
    /// <summary>
    /// Retrieves the tier name of the pelt.
    /// </summary>
    /// <param name="peltData">The <see cref="PeltData"/> to access.</param>
    /// <returns>The tier name of the pelt. If <paramref name="peltData"/> has no defined tier name, <see cref="GetTierNameFromPelt(string)"/> will be called and used instead.</returns>
    public static string GetTierNameFromData(PeltData peltData)
    {
        return peltData.peltTierName ?? GetTierNameFromPelt(peltData.peltCardName);
    }
}
