using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using DiskCardGame;
using GBC;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Card.CostProperties;

[HarmonyPatch]
public static class CostProperties
{
    public static ConditionalWeakTable<CardInfo, List<WeakReference<PlayableCard>>> CardInfoToCard = new();
    
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
    /// ChangeCardCostGetter patches GemsCost so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that CardInfo.GemsCost had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.GemsCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static List<GemType> OriginalGemsCost(CardInfo __instance) { return null; }
    
    public class RefreshCostMonoBehaviour : MonoBehaviour
    {
        private PlayableCard playableCard;
        private int cachedBloodCost = -1;
        private int cachedBoneCost = -1;
        private List<GemType> cachedGemsCost = new List<GemType>();

        private void Awake()
        {
            playableCard = GetComponent<PlayableCard>();
        }
    
        private void LateUpdate()
        {
            bool refreshCost = DidCostsChangeThisFrame();
            if (refreshCost)
            {
                InscryptionAPIPlugin.Logger.LogInfo($"Refreshing cost for {playableCard.Info.displayedName}");
                playableCard.RenderCard();
            }
        }
    
        private bool DidCostsChangeThisFrame()
        {
            bool refreshCost = false;

            int bloodCost = playableCard.BloodCost();
            if (bloodCost != cachedBloodCost)
            {
                InscryptionAPIPlugin.Logger.LogInfo($"{playableCard.Info.displayedName} blood cost chanced!");
                cachedBloodCost = bloodCost;
                refreshCost = true;
            }

            int boneCost = playableCard.BonesCost();
            if (boneCost != cachedBoneCost)
            {
                InscryptionAPIPlugin.Logger.LogInfo($"{playableCard.Info.displayedName} bone cost chanced!");
                cachedBoneCost = boneCost;
                refreshCost = true;
            }

            InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.DidCostsChangeThisFrame] {playableCard.Info.displayedName} Getting gems");
            List<GemType> gemsCost = playableCard.GemsCost();
            InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.DidCostsChangeThisFrame] {playableCard.Info.displayedName} Comparing gems {string.Join(",", cachedGemsCost)} with {string.Join(",", gemsCost)}");
            if (!CompareLists(cachedGemsCost, gemsCost))
            {
                InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.DidCostsChangeThisFrame] ++++{playableCard.Info.displayedName} gem cost changed from {string.Join(",", cachedGemsCost)} to {string.Join(",", gemsCost)}");
                cachedGemsCost.Clear();
                cachedGemsCost.AddRange(gemsCost);
                refreshCost = true;
            }
            else
            {
                InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.DidCostsChangeThisFrame] {playableCard.Info.displayedName} gems {string.Join(",", gemsCost)}");
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
            InscryptionAPIPlugin.Logger.LogInfo($"[BloodCost] {__instance.name} has no playable card.");
            return true;
        }
        
        int num = card.BloodCost();
        if (IsUsingBlueGem(card))
        {
            num--;
        }
        
        __result = Mathf.Max(0, num);
        InscryptionAPIPlugin.Logger.LogInfo($"[BloodCost] {__instance.name} has a blood cost of {__result}");
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
        
        int num = card.BonesCost();
        if (IsUsingBlueGem(card))
        {
            num--;
        }
        
        __result = Mathf.Max(0, num);
        return false;
    }
    
    [HarmonyDebug]
    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.GemsCost), MethodType.Getter), HarmonyPrefix]
    public static bool GemsCost(CardInfo __instance, ref List<GemType> __result)
    {
        InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.GemsCost] {__instance.displayedName}");
        
        PlayableCard card = __instance.GetPlayableCard();
        if (card == null)
        {
            InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.GemsCost] {__instance.displayedName} has no playable card.");
            return true;
        }
        
        List<GemType> gems = new  List<GemType>(card.GemsCost());
        InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.GemsCost] {card.Info.displayedName} gemscost {string.Join(",", gems)}");
        if (gems.Count > 0 && IsUsingBlueGem(card))
        {
            gems.RemoveAt(0);
            InscryptionAPIPlugin.Logger.LogInfo($"[CostProperties.GemsCost] {card.Info.displayedName} is gemified and reduced cost to {string.Join(",", gems)}");
        }

        __result = gems;
        return false;
    }
    
    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.EnergyCost), MethodType.Getter), HarmonyPostfix]
    public static void EnergyCost(PlayableCard __instance, ref int __result)
    {
        if (!Singleton<ResourcesManager>.Instance.HasGem(GemType.Blue))
        {
            return;
        }

        if (__instance.Info.Gemified)
        {
            // --1 already applied by CardInfo logic
            return;
        }

        if (__instance.TemporaryMods.Exists((CardModificationInfo x) => x.gemify))
        {
            // --1 because we added it as a temporary mod
            __result = Mathf.Max(0, __result - 1);
        };
    }

    private static bool IsUsingBlueGem(PlayableCard card)
    {
        if (!Singleton<ResourcesManager>.Instance.HasGem(GemType.Blue))
        {
            InscryptionAPIPlugin.Logger.LogInfo($"{card.Info.displayedName} no gem on the board");
            return false;
        }

        if (card.Info.Gemified)
        {
            InscryptionAPIPlugin.Logger.LogInfo($"{card.Info.displayedName} is gemified cost chanced!");
            return true;
        }
        
        InscryptionAPIPlugin.Logger.LogInfo($"{card.Info.displayedName} has no blue gem!");
        return card.TemporaryMods.Exists((CardModificationInfo x) => x.gemify);
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
        
        if (CostProperties.CardInfoToCard.TryGetValue(info, out List<WeakReference<PlayableCard>> cardList))
        {
            PlayableCard card = null;
            for (int i = cardList.Count - 1; i >= 0; i--)
            {
                if (!cardList[i].TryGetTarget(out PlayableCard innerCard) || innerCard == null)
                {
                    // NOTE: We store a list of cards so if we don't clear this list then it will fill up forever
                    cardList.RemoveAt(i);
                    InscryptionAPIPlugin.Logger.LogInfo($"[Card_SetInfo] Removing playable card from list. {info.displayedName}");
                }
                else if(innerCard == playableCard)
                {
                    card = innerCard;
                    InscryptionAPIPlugin.Logger.LogInfo($"[Card_SetInfo] Card already exists in list! {info.displayedName} to existing list.");
                }
            }
            
            if (card == null)
            {
                cardList.Add(new WeakReference<PlayableCard>(playableCard));
                if (cardList.Count > 1)
                {
                    InscryptionAPIPlugin.Logger.LogWarning($"More than 1 card are using the same card info. This can cause unexpected problems with dynamic costs! {info.displayedName}");
                }
                else
                {
                    InscryptionAPIPlugin.Logger.LogInfo($"[Card_SetInfo] Added {info.displayedName} to existing list.");
                }
            }
        }
        else
        {
            CostProperties.CardInfoToCard.Add(info, new List<WeakReference<PlayableCard>>()
            {
                new WeakReference<PlayableCard>(playableCard)
            });
            InscryptionAPIPlugin.Logger.LogInfo($"[Card_SetInfo] Added {info.displayedName} to list.");
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

[HarmonyPatch]
internal static class TurnManager_CleanupPhase
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TurnManager), nameof(TurnManager.CleanupPhase));
        yield return AccessTools.Method(typeof(GBCEncounterManager), nameof(GBCEncounterManager.LoadOverworldScene));
    }
    
    public static void Postfix()
    {
        // NOTE: This is a hack to clear the table
        CostProperties.CardInfoToCard = new ConditionalWeakTable<CardInfo, List<WeakReference<PlayableCard>>>(); 
    }
}