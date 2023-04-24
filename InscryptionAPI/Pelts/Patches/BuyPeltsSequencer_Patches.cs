using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers.Extensions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

#pragma warning disable CS0252, CS0253
namespace InscryptionAPI.Pelts.Patches;

[HarmonyPatch(typeof(BuyPeltsSequencer), "PeltPrices", MethodType.Getter)]
internal class BuyPeltsSequencer_PeltPrices
{
    /// <summary>
    /// Adds new pelt costs so their price can be listed for purchase 
    /// </summary>
    public static void Postfix(CardLoader __instance, ref int[] __result)
    {
        __result = BuyPeltsSequencer_BuyPelts.PeltsAvailableAtTrader.Select((a) => a.BuyPrice).ToArray();
    }
}

[HarmonyPatch]
internal class BuyPeltsSequencer_BuyPelts
{
    static Type BuyPeltsSequencerClass = Type.GetType("DiskCardGame.BuyPeltsSequencer, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    static Type BuyPeltsMethodClass = Type.GetType("DiskCardGame.BuyPeltsSequencer+<BuyPelts>d__22, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

    public static List<PeltManager.PeltData> PeltsAvailableAtTrader = new List<PeltManager.PeltData>();

    public static BuyPeltsSequencer s_BuyPeltsSequencer
    {
        get
        {
            if (m_specialNodeHandler == null)
            {
                m_specialNodeHandler = SpecialNodeHandler.Instance.buyPeltsSequencer;
            }

            return m_specialNodeHandler;
        }
    }
    private static BuyPeltsSequencer m_specialNodeHandler;

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(BuyPeltsMethodClass, "MoveNext");
    }

    /// <summary>
    /// Setup how the board should be layed out
    /// If we have too many cards then we need to make space
    /// </summary>
    public static bool Prefix()
    {
        if (PeltManager.AllNewPelts.Count == 0)
        {
            // No custom pelts. Don't change anything!
            PeltsAvailableAtTrader.Clear();
            PeltsAvailableAtTrader.AddRange(PeltManager.AllPelts());
            return true;
        }

        // Move card pile off screen
        FieldInfo cardPileField = AccessTools.Field(BuyPeltsSequencerClass, "deckPile");
        CardPile cardPile = (CardPile)cardPileField.GetValue(s_BuyPeltsSequencer);
        Vector3 cardPilePosition = cardPile.transform.localPosition;
        cardPile.transform.localPosition = new Vector3(5.5f, cardPilePosition.y, cardPilePosition.z);

        // Move purchase pile to the left of all the cards
        FieldInfo purchasedPileField = AccessTools.Field(BuyPeltsSequencerClass, "purchasedPile");
        CardPile purchasedPile = (CardPile)purchasedPileField.GetValue(s_BuyPeltsSequencer);
        purchasedPile.transform.position = s_BuyPeltsSequencer.PELT_CARDS_ANCHOR - s_BuyPeltsSequencer.PELT_SPACING;

        // Move teeth anchor to the left
        FieldInfo weightOrganizeAnchorField = AccessTools.Field(BuyPeltsSequencerClass, "weightOrganizeAnchor");
        Transform weightOrganizeAnchor = (Transform)weightOrganizeAnchorField.GetValue(s_BuyPeltsSequencer);
        Vector3 weightedOranizeAnchorPos = weightOrganizeAnchor.transform.localPosition;
        weightOrganizeAnchor.transform.localPosition = new Vector3(-1.5f, weightedOranizeAnchorPos.y, weightedOranizeAnchorPos.z);

        // Create choices
        PeltsAvailableAtTrader.Clear();
        GeneratePeltChoices();

        return true;
    }
    private static void GeneratePeltChoices()
    {
        int randomseed = SaveManager.SaveFile.GetCurrentRandomSeed();

        List<PeltManager.PeltData> availableAtTrader = PeltManager.AllPeltsAvailableAtTrader();
        availableAtTrader.RemoveAll((a) => a.CardChoices().Count == 0);

        if (availableAtTrader.Count > 8)
        {
            // Ensure we have a PeltHare
            // We have at least 1 rare
            // Sort by cost
            List<PeltManager.PeltData> selectedCards = new List<PeltManager.PeltData>(8);
            selectedCards.Add(PeltManager.GetPelt("PeltHare"));

            List<PeltManager.PeltData> allCards = new(availableAtTrader);
            allCards.Remove(selectedCards[0]);

            List<PeltManager.PeltData> rares = allCards.FindAll((a) => CardLoader.GetCardByName(a.peltCardName).appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.GoldEmission)).ToList();
            List<PeltManager.PeltData> nonRares = allCards.FindAll((a) => !rares.Contains(a));

            // 3 rares
            for (int i = 0; i < 3 && rares.Count > 0; i++)
            {
                PeltManager.PeltData data = rares.GetSeededRandom(randomseed++);
                rares.Remove(data);
                selectedCards.Add(data);
            }

            // 4 non-rares
            while (selectedCards.Count < 8 && nonRares.Count > 0)
            {
                PeltManager.PeltData selected = nonRares.GetSeededRandom(randomseed++);
                nonRares.Remove(selected);
                selectedCards.Add(selected);
            }

            // Fill with rares
            while (selectedCards.Count < 8 && rares.Count > 0)
            {
                PeltManager.PeltData selected = rares.GetSeededRandom(randomseed++);
                rares.Remove(selected);
                selectedCards.Add(selected);
            }

            PeltsAvailableAtTrader.AddRange(selectedCards);
        }
        else
        {
            PeltsAvailableAtTrader.AddRange(availableAtTrader);
        }

        PeltsAvailableAtTrader.Sort(static (a, b) =>
        {
            // Sort by cost. Lowest to Highest
            int aCost = a.BuyPrice;
            int bCost = b.BuyPrice;
            int costDiff = aCost - bCost;
            if (costDiff != 0)
            {
                return costDiff;
            }

            // Sort by cost by which one has the most cards. Least to most
            int aCards = a.CardChoices().Count;
            int bCards = b.CardChoices().Count;
            return aCards - bCards;
        });
    }

    /// <summary>
    /// Fixes the sequence to support more than 3 pelts
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        FieldInfo peltsForSale = AccessTools.Field(BuyPeltsSequencerClass, "peltsForSale");
        FieldInfo forLoopI = AccessTools.Field(BuyPeltsMethodClass, "<i>5__2");

        List<SelectableCard> x = null;
        MethodInfo PopulatePeltsForSaleListInfo = SymbolExtensions.GetMethodInfo(() => PopulatePeltsForSaleList(ref x));
        MethodInfo GetTotalPeltsInfo = SymbolExtensions.GetMethodInfo(() => GetTotalPelts());

        // ================================================
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            // buyPeltsSequencer.peltsForSale = new List<SelectableCard> { null, null, null };
            // to
            // buyPeltsSequencer.peltsForSale = new List<SelectableCard> { null, null, null };
            // PopulatePeltsForSaleListInfo(ref buyPeltsSequencer.peltsForSale);
            CodeInstruction code = codes[i];
            if (code.opcode == OpCodes.Stfld && code.operand == peltsForSale)
            {
                // Fill peltsForSale with enough nulls to support extra pelt types
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldloc_1));
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldflda, peltsForSale));
                codes.Insert(++i, new CodeInstruction(OpCodes.Call, PopulatePeltsForSaleListInfo));
            }
            else if (code.opcode == OpCodes.Ldfld && code.operand == forLoopI)
            {
                CodeInstruction nextCode = codes[i + 1];
                if (nextCode.opcode == OpCodes.Ldc_I4_3)
                {
                    // Instead of pushing 3, push the total amount of pelts
                    nextCode.opcode = OpCodes.Call;
                    nextCode.operand = GetTotalPeltsInfo;
                }
            }
        }

        return codes;
    }

    private static void PopulatePeltsForSaleList(ref List<SelectableCard> peltsForSale)
    {
        peltsForSale.Clear();
        for (int i = 0; i < PeltsAvailableAtTrader.Count; i++)
        {
            peltsForSale.Add(null);
        }
    }

    private static int GetTotalPelts()
    {
        return PeltsAvailableAtTrader.Count;
    }
}

[HarmonyPatch(typeof(BuyPeltsSequencer), "CreatePelt", new Type[] { typeof(int), typeof(int), typeof(float) })]
internal class BuyPeltsSequencer_CreatePelt
{
    /// <summary>
    /// Moves the new pelt cards to their correct position
    /// and Adds the new pelts to be bought
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // ================================================

        Vector3 r = default;
        MethodInfo AdjustPositionInfo = SymbolExtensions.GetMethodInfo(() => AdjustPosition(1, ref r));
        MethodInfo PeltNamesInfo = AccessTools.PropertyGetter(typeof(CardLoader), "PeltNames");
        MethodInfo AllPeltsAvailableAtTraderInfo = SymbolExtensions.GetMethodInfo(() => GetCardByName());

        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            CodeInstruction code = codes[i];
            if (code.opcode == OpCodes.Stloc_3)
            {
                // Vector3 vector = this.PELT_CARDS_ANCHOR + this.PELT_SPACING * (float)index;
                // to
                // Vector3 vector = this.PELT_CARDS_ANCHOR + this.PELT_SPACING * (float)index;
                // AdjustPosition(index, ref vector);
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldarg_1)); // index
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldloca, 3)); // ref vector
                codes.Insert(++i, new CodeInstruction(OpCodes.Call, AdjustPositionInfo));
            }
            else if (code.opcode == OpCodes.Call && code.operand == PeltNamesInfo)
            {
                // CardInfo cardByName = CardLoader.GetCardByName(CardLoader.PeltNames[index]);
                // to
                // CardInfo cardByName = CardLoader.GetCardByName(PeltManager.AllPeltsAvailableAtTrader()[index]);
                code.operand = AllPeltsAvailableAtTraderInfo;
            }
        }

        return codes;
    }

    private static string[] GetCardByName()
    {
        return BuyPeltsSequencer_BuyPelts.PeltsAvailableAtTrader.Select((a) => a.peltCardName).ToArray();
    }

    private static void AdjustPosition(int index, ref Vector3 vector)
    {
        int totalCardsAtTrader = BuyPeltsSequencer_BuyPelts.PeltsAvailableAtTrader.Count;

        float peltsPerRow = 3;
        float xPadding = 1.6f;
        float zPadding = -2.0f;

        // If we have 7 or more pelts to buy then make more space!
        if (totalCardsAtTrader > 6)
        {
            peltsPerRow += 1;
            xPadding -= 0.3f;
        }


        // Vector3 vector = this.PELT_CARDS_ANCHOR + this.PELT_SPACING * (float)index;
        vector = BuyPeltsSequencer_BuyPelts.s_BuyPeltsSequencer.PELT_CARDS_ANCHOR;
        vector.x += xPadding * (index % (peltsPerRow));
        vector.z += zPadding * Mathf.FloorToInt(index / peltsPerRow);
    }
}

[HarmonyPatch]
internal class BuyPeltsSequencer_GiveFreePeltSequence
{
    static Type GiveFreePeltSequenceClass = Type.GetType("DiskCardGame.BuyPeltsSequencer+<GiveFreePeltSequence>d__24, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(GiveFreePeltSequenceClass, "MoveNext");
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo GainPeltInfo = SymbolExtensions.GetMethodInfo(() => SpecialNodeHandler.Instance.buyPeltsSequencer.GainPelt(null));
        MethodInfo ChangeFreeCardInfo = SymbolExtensions.GetMethodInfo(() => ChangeFreeCard(null));

        // ================================================
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            // this.GainPelt(this.peltsForSale[0]);
            // to
            // this.GainPelt(this.peltsForSale[0]);
            CodeInstruction code = codes[i];
            if (code.opcode == OpCodes.Call && code.operand == GainPeltInfo)
            {
                // Fill peltsForSale with enough nulls to support extra pelt types
                codes.Insert(i, new CodeInstruction(OpCodes.Call, ChangeFreeCardInfo));
            }
        }

        return codes;
    }

    private static CardInfo ChangeFreeCard(CardInfo currentFreeCard)
    {
        return CardLoader.GetCardByName("PeltHare");
    }
}