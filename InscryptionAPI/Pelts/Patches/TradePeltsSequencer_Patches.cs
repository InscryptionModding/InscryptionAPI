using DiskCardGame;
using HarmonyLib;
using InscryptionAPI;
using InscryptionAPI.Pelts;
using System.Reflection;
using System.Reflection.Emit;


namespace TradePeltAPI.Scripts.Patches;

[HarmonyPatch(typeof(TradePeltsSequencer), "GetTradeCardInfos", new Type[] { typeof(int), typeof(bool) })]
public class TradePeltsSequencer_GetTradeCardInfos
{
    /// <summary>
    /// Shows cards at the Trader to be traded for real cards
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this

        // List<CardInfo> list = new List<CardInfo>();
        // List<CardInfo> unlockedCards;
        // int numCards;
        // if (tier != 2)
        // {
        //     unlockedCards = CardLoader.GetUnlockedCards(CardMetaCategory.TraderOffer, CardTemple.Nature);
        //     numCards = 8;
        // }
        // else
        // {
        //     unlockedCards = CardLoader.GetUnlockedCards(CardMetaCategory.Rare, CardTemple.Nature);
        //     numCards = 4;
        // }
        // list = CardLoader.GetDistinctCardsFromPool(SaveManager.SaveFile.GetCurrentRandomSeed() + tier * 1000, numCards, unlockedCards, (tier == 1) ? 1 : 0, false);

        // === Into this

        // List<CardInfo> list = new List<CardInfo>();
        // List<CardInfo> unlockedCards;
        // int numCards;
        // if (tier != 2)
        // {
        //     unlockedCards = CardLoader.GetUnlockedCards(CardMetaCategory.TraderOffer, CardTemple.Nature);
        //     numCards = 8;
        // }
        // else
        // {
        //     unlockedCards = CardLoader.GetUnlockedCards(CardMetaCategory.Rare, CardTemple.Nature);
        //     numCards = 4;
        // }
        // GetCardOptions(tier, ref unlockedCards)
        // numCards(tier, ref numCards)
        // list = CardLoader.GetDistinctCardsFromPool(SaveManager.SaveFile.GetCurrentRandomSeed() + tier * 1000, numCards, unlockedCards, abilityCount((tier == 1) ? 1 : 0, tier), false);

        // ===
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        List<CardInfo> y = null;
        MethodInfo GetCardOptionsInfo = SymbolExtensions.GetMethodInfo(() => GetCardOptions(1, ref y));

        int t = 0;
        MethodInfo NumCardsInfo = SymbolExtensions.GetMethodInfo(() => numCards(1, ref t));
        MethodInfo AbilityCountInfo = SymbolExtensions.GetMethodInfo(() => abilityCount(1, 1));

        // Find index of
        // list = CardLoader.GetDistinctCardsFromPool(SaveManager.SaveFile.GetCurrentRandomSeed()
        int indexFinishedCalculations = -1;
        int numAbilitiesIndex = -1;
        CodeInstruction numAbilitiesInstruction = null;
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Call && codes[i].operand != null && codes[i].operand.ToString() == "SaveFile get_SaveFile()")
            {
                indexFinishedCalculations = i + 1;
            }

            if (codes[i].opcode == OpCodes.Call && codes[i].operand != null && codes[i].operand.ToString() == "System.Collections.Generic.List`1[DiskCardGame.CardInfo] GetDistinctCardsFromPool(Int32, Int32, System.Collections.Generic.List`1[DiskCardGame.CardInfo], Int32, Boolean)")
            {
                numAbilitiesIndex = i - 1;
                numAbilitiesInstruction = codes[i - 1];
            }
        }

        if (indexFinishedCalculations < 0)
        {
            InscryptionAPIPlugin.Logger.LogError("[TradePeltsSequencer_GetTradeCardInfos] Did not find index for indexFinishedCalculations!");
        }
        else if (numAbilitiesIndex < 0)
        {
            InscryptionAPIPlugin.Logger.LogError("[TradePeltsSequencer_GetTradeCardInfos] Did not find index for numAbilitiesIndex!");
        }
        else
        {
            int j = 0;

            // void GetCardOptions(int tier, ref List<CardInfo> cards)
            codes.Insert(indexFinishedCalculations + j++, new CodeInstruction(OpCodes.Ldarg_1)); // tier
            codes.Insert(indexFinishedCalculations + j++, new CodeInstruction(OpCodes.Ldloca, 1)); // list
            codes.Insert(indexFinishedCalculations + j++, new CodeInstruction(OpCodes.Call, GetCardOptionsInfo));

            // void numCards(int tier, ref int numCards)
            codes.Insert(indexFinishedCalculations + j++, new CodeInstruction(OpCodes.Ldarg_1)); // tier
            codes.Insert(indexFinishedCalculations + j++, new CodeInstruction(OpCodes.Ldloca, 2)); // numCards
            codes.Insert(indexFinishedCalculations + j++, new CodeInstruction(OpCodes.Call, NumCardsInfo));

            // int abilityCount(int tier, int abilityCount)
            j++;
            numAbilitiesInstruction.opcode = OpCodes.Ldarg_1; // tier
            codes.Insert(numAbilitiesIndex + j++, new CodeInstruction(OpCodes.Call, AbilityCountInfo));
            codes.Insert(numAbilitiesIndex + j++, new CodeInstruction(OpCodes.Ldc_I4_0)); // False
        }

        return codes;
    }

    private static void GetCardOptions(int tier, ref List<CardInfo> cards)
    {
        if (tier <= 2)
            return;

        PeltManager.CustomPeltData pelt = PeltManager.AllNewPelts[tier - 3];
        List<CardInfo> cardOptions = pelt.CardChoices;
        if (cardOptions.Count == 0)
        {
            InscryptionAPIPlugin.Logger.LogWarning("No cards specified for pelt '" + pelt.peltCardName + "', using fallback card.");
            cardOptions.Add(CardLoader.GetCardByName("Amalgam"));
        }

        cards = cardOptions;
    }

    private static void numCards(int tier, ref int numCards)
    {
        if (tier <= 2)
            return;

        PeltManager.CustomPeltData pelt = PeltManager.AllNewPelts[tier - 3];
        numCards = pelt.choicesOfferedByTrader;
    }

    private static int abilityCount(int abilityCount, int tier)
    {
        if (tier <= 2)
            return abilityCount;

        PeltManager.CustomPeltData pelt = PeltManager.AllNewPelts[tier - 3];
        int peltAbilityCount = pelt.extraAbilitiesToAdd;
        return peltAbilityCount;
    }
}