using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Nodes;
using Pixelplacement.TweenSystem;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.CardCosts;

/*public class TestCost : CustomCardCost
{
    public override string CostName => "TestCost";

    public static void Init()
    {
        CardCostManager.FullCardCost fullCardCost = CardCostManager.Register("api", "TestCost", typeof(TestCost), F, F);
    }

    public static Texture2D F(int cardCost, CardInfo info, PlayableCard playableCard)
    {
        return Texture2D.blackTexture;
    }

    public override bool CostSatisfied(int cardCost, PlayableCard card)
    {
        return cardCost <= (Singleton<ResourcesManager>.Instance.PlayerEnergy - card.EnergyCost);
    }

    public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
    {
        return "Eat your greens aby. " + card.Info.DisplayedNameLocalized;
    }

    public override IEnumerator OnPlayed(int cardCost, PlayableCard card)
    {
        yield return Singleton<ResourcesManager>.Instance.SpendEnergy(cardCost);
    }
}*/

[HarmonyPatch]
public static class CardCostManager
{
    public class FullCardCost
    {
        public string ModGUID;
        public string CostName;

        public Type CostBehaviour;

        /// <summary>
        /// Retrieves the correct texture corresponding to CardInfo's cost. Used in the 3D Acts (Act 1, Act 3, etc.).
        /// Parameters:
        ///     int: how much of the custom cost the CardInfo/PlayableCard needs to be played.
        ///     CardInfo: the base CardInfo of the current card.
        ///     PlayableCard: the PlayableCard possessing the base CardInfo. Can be null.
        /// Returns:
        ///     The texture representing the custom cost to display on the card.
        /// </summary>
        public Func<int, CardInfo, PlayableCard, Texture2D> GetCostTexture;
        /// <summary>
        /// Retrieves the correct texture corresponding to CardInfo's cost. Used in Act 2.
        /// Parameters:
        ///     int: how much of the custom cost the CardInfo/PlayableCard needs to be played.
        ///     CardInfo: the base CardInfo of the current card.
        ///     PlayableCard: the PlayableCard possessing the base CardInfo. Can be null.
        /// Returns:
        ///     The texture representing the custom cost to display on the card.
        /// </summary>
        public Func<int, CardInfo, PlayableCard, Texture2D> GetPixelCostTexture;

        public Func<CardInfo, int> GetCostTier;

        public FullCardCost(string modGUID, string costName, Type costBehaviour,
            Func<int, CardInfo, PlayableCard, Texture2D> getCostTexture,
            Func<int, CardInfo, PlayableCard, Texture2D> getPixelCostTexture)
        {
            ModGUID = modGUID;
            CostName = costName;
            CostBehaviour = costBehaviour;
            GetCostTexture = getCostTexture;
            GetPixelCostTexture = getPixelCostTexture;

            TypeManager.Add(costName, costBehaviour);
        }

        public FullCardCost Clone()
        {
            FullCardCost retval = new(this.ModGUID, this.CostName, this.CostBehaviour, this.GetCostTexture, this.GetPixelCostTexture);
            return retval;
        }
    }

    static CardCostManager()
    {
        NewCosts.CollectionChanged += static (_, _) =>
        {
            SyncCustomCostList();
        };
    }

    public static List<FullCardCost> AllCustomCosts { get; private set; } = new();

    internal readonly static ObservableCollection<FullCardCost> NewCosts = new();
    
    public static event Func<List<FullCardCost>, List<FullCardCost>> ModifyCustomCostList;
    public static void SyncCustomCostList()
    {
        AllCustomCosts = NewCosts.Select(a => a.Clone()).ToList();
        AllCustomCosts = ModifyCustomCostList?.Invoke(AllCustomCosts) ?? AllCustomCosts;
    }

    public static FullCardCost Register(string modGUID, string costName, Type costBehaviour,
        Func<int, CardInfo, PlayableCard, Texture2D> getCostTexture, Func<int, CardInfo, PlayableCard, Texture2D> getPixelCostTexture)
    {
        FullCardCost newCost = new(modGUID, costName, costBehaviour, getCostTexture, getPixelCostTexture);
        NewCosts.Add(newCost);
        return newCost;
    }

    #region Patches
    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.CanPlay))]
    private static void CanPlayCustomCosts(ref bool __result, ref PlayableCard __instance)
    {
        if (__result)
        {
            foreach (CustomCardCost cost in __instance.GetCustomCardCosts())
            {
                __result = __result && cost.CostSatisfied(__instance.GetCustomCost(cost.CostName), __instance);
                if (!__result)
                    break;
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(HintsHandler), nameof(HintsHandler.OnNonplayableCardClicked))]
    private static bool CannotAffordCustomCostHints(ref PlayableCard card)
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            foreach (CustomCardCost customCost in card.GetCustomCardCosts())
            {
                int value = card.GetCustomCost(customCost.CostName);
                if (!customCost.CostSatisfied(value, card))
                {
                    CustomCoroutine.Instance.StartCoroutine(TextBox.Instance.ShowUntilInput(
                        customCost.CostUnsatisfiedHint(value, card),
                        DialogueManager.GetStyleFromAmbition())
                        );
                    return false;
                }
            }
        }
        else if (TextDisplayer.m_Instance != null)
        {
            foreach (CustomCardCost customCost in card.GetCustomCardCosts())
            {
                int value = card.GetCustomCost(customCost.CostName);
                if (!customCost.CostSatisfied(value, card))
                {
                    if (customCost.PlayHintFrequency > 0 && customCost.playHintAttempts % customCost.PlayHintFrequency == 0)
                    {
                        CustomCoroutine.Instance.StartCoroutine(TextDisplayer.Instance.ShowUntilInput(
                                customCost.CostUnsatisfiedHint(value, card))
                            );
                    }
                    customCost.playHintAttempts++;
                    return false;
                }
            }
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(DiskCardGame.Card), nameof(DiskCardGame.Card.SetInfo))]
    private static void AttachCustomCardCosts(DiskCardGame.Card __instance, CardInfo info)
    {
        // destroy all pre-existing card costs
        __instance.GetComponents<CustomCardCost>().ForEach(x => UnityObject.Destroy(x));

        foreach (Type type in info.GetCustomCosts().Select(x => x.CostBehaviour))
        {
            __instance.gameObject.AddComponent(type);
            //InscryptionAPIPlugin.Logger.LogInfo($"AddCost: {type.Name}");
        }
    }

    #region SelectedCardToSlot transiler
    [HarmonyTranspiler, HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.SelectSlotForCard), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> FixDialogueSoftlock(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        //codes.LogCodeInscryptions();

        int methodIndex = codes.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == "System.Collections.IEnumerator PlayCardOnSlot(DiskCardGame.PlayableCard, DiskCardGame.CardSlot)");
        if (methodIndex > 0)
        {
            MethodInfo newPlayCardToSlot = AccessTools.Method(typeof(CardCostManager), nameof(CardCostManager.PlaySelectedCardToSlot),
                new Type[] { typeof(PlayerHand), typeof(PlayableCard), typeof(CardSlot) });

            codes[methodIndex] = new(OpCodes.Callvirt, newPlayCardToSlot);
            //InscryptionAPIPlugin.Logger.LogInfo($"Added PlaySelectedCardToSlot");
        }

        // offset to the start of the ilcode we want to remove
        int bonesIndex = codes.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == "Int32 get_BonesCost()") - 3;
        if (bonesIndex > 0)
        {
            int endIndex = codes.FindIndex(bonesIndex, x => x.opcode == OpCodes.Ldc_I4_6) + 4; // offset to the end of the code to remove
            codes.RemoveRange(bonesIndex, endIndex - bonesIndex);
            //InscryptionAPIPlugin.Logger.LogInfo("Removed dupe bone code");
        }
        int energyIndex = codes.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == "Int32 get_EnergyCost()") - 2;
        if (energyIndex > 0)
        {
            int endIndex = codes.FindIndex(energyIndex, x => x.opcode == OpCodes.Ldc_I4_7) + 4;
            codes.RemoveRange(energyIndex, endIndex - energyIndex);
            //InscryptionAPIPlugin.Logger.LogInfo($"Removed dupe energy code");
        }
        return codes;
    }
    /// <summary>
    /// Does two things:
    ///     1: Caches play costs before playing the card onto the board to prevent incorrect values being used should the card's info change.
    ///     2: Implements support for custom cards' OnPlay logic.
    /// </summary>
    /// <param name="instance">The PlayerHand object.</param>
    /// <param name="card">The PlayableCard being played to the board.</param>
    /// <param name="lastSelectedSlot">The last selected slot, equals BoardManage.Instance.LastSelectedSlot by default.</param>
    /// <returns></returns>
    public static IEnumerator PlaySelectedCardToSlot(PlayerHand instance, PlayableCard card, CardSlot lastSelectedSlot)
    {
        // keep track of the costs before the card is resolved on the board
        // this is to avoid situations where the card's info changes before we can actually spend resources
        // which would cause us to spend the incorrect amount or type
        int bonesCost = card.BonesCost(), energyCost = card.EnergyCost;
        Dictionary<string, int> customCosts = new();
        foreach (CustomCardCost cost1 in card.GetCustomCardCosts())
            customCosts.Add(cost1.CostName, card.GetCustomCost(cost1.CostName));

        yield return instance.PlayCardOnSlot(card, lastSelectedSlot);

        if (bonesCost > 0)
            yield return Singleton<ResourcesManager>.Instance.SpendBones(bonesCost);

        if (energyCost > 0)
            yield return Singleton<ResourcesManager>.Instance.SpendEnergy(energyCost);

        foreach (var cost2 in card.GetCustomCardCosts())
        {
            if (customCosts.ContainsKey(cost2.CostName))
                yield return cost2.OnPlayed(customCosts[cost2.CostName], card);
        }
    }
    #endregion

    #endregion
}

/// <summary>
/// An inheritable class for implementing custom card costs.
/// </summary>
public abstract class CustomCardCost : ManagedBehaviour
{
    /// <summary>
    /// The internal name of the cost. Used to check for the cost's extended property by the API.
    /// </summary>
    public abstract string CostName { get; }
    public virtual int PlayHintFrequency { get; } = 1;
    public int playHintAttempts = 0;

    /// <summary>
    /// Whether the current PlayableCard can be played.
    /// </summary>
    /// <param name="cardCost">How many of this cost the PlayableCard needs in order to be played.</param>
    /// <param name="playableCard">The PlayableCard currently being checked.</param>
    public virtual bool CostSatisfied(int cardCost, PlayableCard playableCard)
    {
        return true;
    }
    /// <summary>
    /// The dialogue string that will be played when you cannot play a card with this custom cost.
    /// </summary>
    /// <param name="cardCost">How many of this cost the PlayableCard needs in order to be played.</param>
    /// <param name="playableCard">The PlayableCard currently being checked.</param>
    public virtual string CostUnsatisfiedHint(int cardCost, PlayableCard playableCard)
    {
        return null;
    }
    /// <summary>
    /// The (stat point) SP value of the custom cost. Used in a few places by the game to determine how powerful a card can be, (eg Build-A-Card).
    /// </summary>
    /// <param name="cardCost">How many of this cost the PlayableCard needs in order to be played.</param>
    /// <param name="playableCard">The PlayableCard currently being checked.</param>
    public virtual int CostStatPointValue(int cardCost, PlayableCard playableCard)
    {
        return 0;
    }

    /// <summary>
    /// What the game should do when a card with this cost is played.
    /// Most common use case is implementing the logic for paying for this cost.
    /// </summary>
    /// <param name="cardCost">How many of this cost the PlayableCard needs in order to be played.</param>
    /// <param name="playableCard">The PlayableCard currently being checked.</param>
    public virtual IEnumerator OnPlayed(int cardCost, PlayableCard playableCard)
    {
        yield break;
    }
}