using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;


namespace InscryptionAPI.Pelts.Patches;

[HarmonyPatch(typeof(TradePeltsSequencer), "GetTradeCardInfos", new Type[] { typeof(int), typeof(bool) })]
internal static class TradePeltsSequencer_GetTradeCardInfos
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

    public static void Postfix(ref List<CardInfo> __result, int tier, bool mergedPelt)
    {
        PeltManager.PeltData pelt = PeltManager.AllPelts()[tier];
        if (pelt.ModifyCardChoiceAtTrader != null)
        {
            foreach (CardInfo cardInfo in __result)
            {
                pelt.ModifyCardChoiceAtTrader(cardInfo);
            }
        }

    }

    private static void GetCardOptions(int tier, ref List<CardInfo> cards)
    {
        PeltManager.PeltData pelt = PeltManager.AllPelts()[tier];
        List<CardInfo> cardOptions = pelt.CardChoices();
        cardOptions.RemoveAll((a) => a.Health == 0);
        if (cardOptions.Count == 0)
        {
            InscryptionAPIPlugin.Logger.LogWarning("No cards specified for pelt '" + pelt.peltCardName + "', using fallback card.");
            cardOptions.Add(CardLoader.GetCardByName("Amalgam"));
        }

        cards = cardOptions;
    }

    private static void numCards(int tier, ref int numCards)
    {
        PeltManager.PeltData pelt = PeltManager.AllPelts()[tier];
        numCards = Mathf.Min(8, pelt.choicesOfferedByTrader);
    }

    private static int abilityCount(int abilityCount, int tier)
    {
        PeltManager.PeltData pelt = PeltManager.AllPelts()[tier];
        int peltAbilityCount = pelt.extraAbilitiesToAdd;
        return peltAbilityCount;
    }
}

[HarmonyPatch]
internal class TradePeltsSequencer_CreatePeltCards
{
    private static Type classType = Type.GetType("DiskCardGame.TradePeltsSequencer+<CreatePeltCards>d__19, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    private static Type display19ClassType = Type.GetType("DiskCardGame.TradePeltsSequencer+<>c__DisplayClass19_0, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(classType, "MoveNext");
    }

    /// <summary>
    /// Ensures you only get as many cards as you have pelts
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this

        // Cap the pelts i have
        // int numPelts = Mathf.Min((tier == 2) ? 4 : 8, cardInfos.Count);
        // int num;

        // === Into this

        // // Cap the pelts i have
        // int numPelts = Mathf.Min((tier == 2) ? 4 : 8, cardInfos.Count);
        // numCards(tier, ref numPelts)
        // int num;

        // ===

        int t = 0;
        MethodInfo NumPeltsMethod = SymbolExtensions.GetMethodInfo(() => numPelts(1, ref t));
        FieldInfo NumPeltsField = AccessTools.Field(classType, "<numPelts>5__3");
        FieldInfo TierField = AccessTools.Field(display19ClassType, "tier");

        // Find index of
        // list = CardLoader.GetDistinctCardsFromPool(SaveManager.SaveFile.GetCurrentRandomSeed()
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            CodeInstruction code = codes[i];
            if (code.opcode == OpCodes.Stfld && code.operand == NumPeltsField)
            {
                // void numCards(int tier, ref int numCards)
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldloc_2)); // tier
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldfld, TierField)); // tier
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0)); // numCards
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldflda, NumPeltsField)); // numCards
                codes.Insert(++i, new CodeInstruction(OpCodes.Call, NumPeltsMethod));
                break;
            }
        }

        return codes;
    }

    private static void numPelts(int tier, ref int numPelts)
    {
        PeltManager.PeltData pelt = PeltManager.AllPelts()[tier];
        numPelts = Mathf.Min(numPelts, Mathf.Min(8, pelt.choicesOfferedByTrader));
    }
}