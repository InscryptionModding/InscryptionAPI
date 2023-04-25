using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Card.CostProperties;

[HarmonyPatch]
public static class CostProperties
{
    public static ConditionalWeakTable<CardInfo, List<PlayableCard>> CardInfoToCard = new();
    
    /// <summary>
    /// ChangeCardCostGetter patches BloodCost so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that CardInfo.BloodCost had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BloodCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static int OriginalBloodCost(CardInfo __instance) { return 0; }
    
    
    /// <summary>
    /// ChangeCardCostGetter patches BoneCost so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that CardInfo.BoneCost had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BonesCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static int OriginalBonesCost(CardInfo __instance) { return 0; }
    
    
    /// <summary>
    /// ChangeCardCostGetter patches BoneCost so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that CardInfo.BoneCost had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.GemsCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static List<GemType> OriginalGemsCost(CardInfo __instance) { return null; }
    
    public class RefreshCostMonoBehaviour : MonoBehaviour
    {
        private PlayableCard playableCard;
        private int cachedBloodCost = -1;
        private int cachedBoneCost = -1;
        private List<GemType> cachedGemsCost = new List<GemType>();
        private int cachedEnergyCost = -1;

        private void Awake()
        {
            playableCard = GetComponent<PlayableCard>();
        }
    
        private void LateUpdate()
        {
            bool refreshCost = DidCostsChangeThisFrame();
            if (refreshCost)
            {
                InscryptionAPIPlugin.Logger.LogError("[RefreshCostMonoBehaviour] Costs changed. Refreshing...");
                playableCard.RenderCard();
            }
        }
    
        private bool DidCostsChangeThisFrame()
        {
            bool refreshCost = false;

            int bloodCost = playableCard.BloodCost();
            if (bloodCost != cachedBloodCost)
            {
                cachedBloodCost = bloodCost;
                refreshCost = true;
            }

            int boneCost = playableCard.BonesCost();
            if (boneCost != cachedBoneCost)
            {
                cachedBoneCost = boneCost;
                refreshCost = true;
            }

            List<GemType> gemsCost = playableCard.GemsCost();
            if (!CompareLists(cachedGemsCost, gemsCost))
            {
                cachedGemsCost.Clear();
                cachedGemsCost.AddRange(gemsCost);
                refreshCost = true;
            }
            
            return refreshCost;
        }

        public bool CompareLists<T>(List<T> a, List<T> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}

[HarmonyPatch]
internal static class ChangeCardCostGetter
{
    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BloodCost), MethodType.Getter), HarmonyPrefix]
    public static bool BloodCost(CardInfo __instance, ref int __result)
    {
        PlayableCard card = __instance.GetPlayableCard();
        if (card == null)
        {
            return true;
        }
        
        __result = card.BloodCost();
        return false;
    }
    
    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BonesCost), MethodType.Getter), HarmonyPrefix]
    public static bool BoneCost(CardInfo __instance, ref int __result)
    {
        PlayableCard card = __instance.GetPlayableCard();
        if (card == null)
        {
            return true;
        }
        
        __result = card.BonesCost();
        return false;
    }
    
    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.GemsCost), MethodType.Getter), HarmonyPrefix]
    public static bool GemsCost(CardInfo __instance, ref List<GemType> __result)
    {
        PlayableCard card = __instance.GetPlayableCard();
        if (card == null)
        {
            return true;
        }
        
        __result = card.GemsCost();
        return false;
    }
}

[HarmonyPatch(typeof(DiskCardGame.Card), nameof(DiskCardGame.Card.Info), MethodType.Setter)]
internal static class Card_SetInfo
{
    public static void Postfix(DiskCardGame.Card __instance)
    {
        if (__instance is not PlayableCard playableCard)
        {
            return;
        }
        
        CardInfo info = playableCard.Info;
        
        if (CostProperties.CardInfoToCard.TryGetValue(info, out List<PlayableCard> cardList))
        {
            PlayableCard card = null;
            for (int i = 0; i < cardList.Count; i++)
            {
                if (cardList[i] == null)
                {
                    // NOTE: We store a list of cards so if we don't clear this list then it will fill up forever
                    cardList.RemoveAt(i--);
                    InscryptionAPIPlugin.Logger.LogInfo($"[Card_SetInfo] Removing Card from CardInfo at index {(i + 1)} {cardList.Count} left");
                }
                else if (cardList[i] == playableCard)
                {
                    card = playableCard;
                }
            }
            
            if (card == null)
            {
                cardList.Add(card);
            }
        }
        else
        {
            CostProperties.CardInfoToCard.Add(info, new List<PlayableCard>()
            {
                playableCard
            });
        }
    }
}

[HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Awake))]
internal static class PlayableCard_Awake
{
    public static void Postfix(PlayableCard __instance)
    {
        __instance.gameObject.AddComponent<CostProperties.RefreshCostMonoBehaviour>();
    }
}