using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Pelts;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
#pragma warning disable CS0252, CS0253

[HarmonyPatch(typeof(BuyPeltsSequencer), "PeltPrices", MethodType.Getter)]
public class BuyPeltsSequencer_PeltPrices
{
    /// <summary>
    /// Adds new pelt costs so their price can be listed for purchase 
    /// </summary>
    public static void Postfix(CardLoader __instance, ref int[] __result)
    {
        List<int> list = new List<int>(__result);
        list.AddRange(PeltManager.AllNewPelts.Select((a) => a.BuyPrice));
        __result = list.ToArray();
    }
}

[HarmonyPatch]
internal class BuyPeltsSequencer_BuyPelts
{
    static Type BuyPeltsSequencerClass = Type.GetType("DiskCardGame.BuyPeltsSequencer, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    static Type BuyPeltsMethodClass = Type.GetType("DiskCardGame.BuyPeltsSequencer+<BuyPelts>d__22, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

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

        return true;
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
        int totalPelts = GetTotalPelts();
        for (int i = 0; i < totalPelts; i++)
        {
            peltsForSale.Add(null);
        }
    }

    private static int GetTotalPelts()
    {
        int peltNamesLength = PeltManager.AllPeltsAvailableAtTrader().Length;
        return peltNamesLength;
    }
}

[HarmonyPatch(typeof(BuyPeltsSequencer), "CreatePelt", new Type[] { typeof(int), typeof(int), typeof(float) })]
public class BuyPeltsSequencer_CreatePelt
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
        MethodInfo AllPeltsAvailableAtTraderInfo = SymbolExtensions.GetMethodInfo(() => PeltManager.AllPeltsAvailableAtTrader());

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

    private static void AdjustPosition(int index, ref Vector3 vector)
    {
        int totalCardsAtTrader = PeltManager.AllPeltsAvailableAtTrader().Length;

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