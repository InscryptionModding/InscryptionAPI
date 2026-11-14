using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Card.CostProperties;

/// <summary>
/// Class that handles dynamic costs on <see cref="PlayableCard"/> objects.
/// </summary>
[HarmonyPatch]
public static class CostProperties {
    /// <summary>
    /// Class that's attached to <see cref="PlayableCard"/> objects to ensure their costs are updated appropriately.
    /// </summary>
    public class RefreshCostMonoBehaviour : MonoBehaviour {
        /// <summary>
        /// <see cref="PlayableCard"/> instance this behaviour is attached to.
        /// </summary>
        public PlayableCard playableCard;
        private int cachedBloodCost = -1, cachedBoneCost = -1;
        private readonly List<GemType> cachedGemsCost = new();

        private void LateUpdate()
        {
            if (DidCostsChangeThisFrame())
                playableCard.RenderCard();
        }

        private bool DidCostsChangeThisFrame()
        {
            // update all cachedCosts before returning boolean
            bool refreshCost = false;

            int bloodCost = playableCard?.BloodCost() ?? 0;
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
            if (GemsChanged(cachedGemsCost, gemsCost))
            {
                cachedGemsCost.Clear();
                cachedGemsCost.AddRange(gemsCost);
                refreshCost = true;
            }

            return refreshCost;
        }

        /// <summary>
        /// Determines if a card's list of Gem costs has been updated.
        /// </summary>
        /// <typeparam name="T">GemType.</typeparam>
        /// <param name="a">First list of gems.</param>
        /// <param name="b">Second list of gems.</param>
        /// <returns>True if <c>a</c> does not equal <c>b</c></returns>
        public bool GemsChanged<T>(List<T> a, List<T> b)
        {
            if (a.Count != b.Count)
                return true;

            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// A table that maps a <see cref="CardInfo"/> object to a list of weak references to every <see cref="PlayableCard"/> that uses that same <see cref="CardInfo"/>.
    /// </summary>
    public static ConditionalWeakTable<CardInfo, List<WeakReference<PlayableCard>>> CardInfoToCard = new();


    // duplicate of above, kept in case something breaks
    //public static ConditionalWeakTable<CardInfo, List<WeakReference<PlayableCard>>> CardInfoToPlayableCardReferences = new();


    /// <summary>
    /// <see cref="ChangeCardCostGetter"/> patches <see cref="CardInfo.BloodCost"/> so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that <see cref="CardInfo.BloodCost"/> had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BloodCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static int OriginalBloodCost(CardInfo __instance) { return 0; }

    /// <summary>
    /// <see cref="ChangeCardCostGetter"/> patches <see cref="CardInfo.BonesCost"/> so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that <see cref="CardInfo.BonesCost"/> had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BonesCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static int OriginalBonesCost(CardInfo __instance) { return 0; }

    /// <summary>
    /// <see cref="ChangeCardCostGetter"/> patches <see cref="CardInfo.GemsCost"/> so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that <see cref="CardInfo.GemsCost"/> had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.GemsCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static List<GemType> OriginalGemsCost(CardInfo __instance) { return null; }

    /// <summary>
    /// Improved version of <see cref="CardInfo.GemsCost"/> that accounts for <c>addGemCost</c> and <see cref="CardModificationInfoExtensions.RemovedGemsCosts(CardModificationInfo)"/>.
    /// </summary>
    /// <remarks>For consistency's sake, it's recommended you use this method over <see cref="OriginalGemsCost"/> in most cases.</remarks>
    /// <param name="instance"></param>
    public static List<GemType> ImprovedGemsCost(CardInfo instance)
    {
        if (instance.Mods.Exists(x => x.nullifyGemsCost))
            return new();

        List<GemType> gems = new(instance.gemsCost);
        foreach (CardModificationInfo mod in instance.Mods)
        {
            if (mod.addGemCost != null)
            {
                gems.AddRange(mod.addGemCost);
            }
            foreach (GemType gem in mod.RemovedGemsCosts())
            {
                gems.Remove(gem);
            }
        }
        return gems;
    }

    /// <summary>
    /// <see cref="ChangeCardCostGetter"/> patches <see cref="CardInfo.EnergyCost"/> so we can change the cost on the fly
    /// This reverse patch gives us access to the original method without any changes.
    /// This method has a copy of all the code that <see cref="CardInfo.EnergyCost"/> had so it doesn't result in a StackOverflow and freezing the game when called.
    /// </summary>
    [HarmonyReversePatch, HarmonyPatch(typeof(CardInfo), nameof(CardInfo.EnergyCost), MethodType.Getter), MethodImpl(MethodImplOptions.NoInlining)]
    public static int OriginalEnergyCost(CardInfo __instance) { return 0; }

    /// <summary>
    /// Determines whether or not Gemified should reduce the blood cost on a multicost card.
    /// </summary>
    /// <param name="card">The gemified <see cref="PlayableCard"/> we're checking.</param>
    /// <param name="bloodCost">The <see cref="PlayableCard"/>'s blood cost. Defaults to using the <see cref="CardInfo"/>'s blood cost if null.</param>
    /// <returns>True if Gemified should reduce the blood cost by 1.</returns>
    /// <remarks>
    /// Used for multicost cards to determine which cost Gemified should affect.
    /// Priority from first choice to last: Energy, Bones, Mox, Blood
    /// </remarks>
    public static bool ReduceGemifiedBlood(PlayableCard card, int? bloodCost = null)
    {
        return (bloodCost ?? OriginalBloodCost(card.Info)) > 0 && !ReduceGemifiedEnergy(card) && !ReduceGemifiedBones(card) && !ReduceGemifiedMox(card);
    }
    /// <summary>
    /// Determines whether or not Gemified should reduce the gems cost on a multicost card.
    /// </summary>
    /// <param name="card">The gemified <see cref="PlayableCard"/> we're checking.</param>
    /// <param name="gemsCost">The <see cref="PlayableCard"/>'s gems cost. Defaults to using the <see cref="CardInfo"/>'s gems cost if null.</param>
    /// <returns>True if Gemified should reduce the gems cost by 1.</returns>
    /// <remarks>
    /// Used for multicost cards to determine which cost Gemified should affect.
    /// Priority from first choice to last: Energy, Bones, Mox, Blood
    /// </remarks>
    public static bool ReduceGemifiedMox(PlayableCard card, List<GemType> gemsCost = null)
    {
        return (gemsCost?.Count ?? ImprovedGemsCost(card.Info).Count) > 0 && !ReduceGemifiedBones(card) && !ReduceGemifiedEnergy(card);
    }
    /// <summary>
    /// Determines whether or not Gemified should reduce the bone cost on a multicost card.
    /// </summary>
    /// <param name="card">The gemified <see cref="PlayableCard"/> we're checking.</param>
    /// <param name="bonesCost">The <see cref="PlayableCard"/>'s bone cost. Defaults to using the <see cref="CardInfo"/>'s bone cost if null.</param>
    /// <returns>True if Gemified should reduce the bone cost by 1.</returns>
    /// <remarks>
    /// Used for multicost cards to determine which cost Gemified should affect.
    /// Priority from first choice to last: Energy, Bones, Mox, Blood
    /// </remarks>
    public static bool ReduceGemifiedBones(PlayableCard card, int? bonesCost = null)
    {
        return (bonesCost ?? OriginalBonesCost(card.Info)) > 0 && !ReduceGemifiedEnergy(card);
    }
    /// <summary>
    /// Determines whether or not Gemified should reduce the energy cost on a multicost card.
    /// </summary>
    /// <param name="card">The gemified <see cref="PlayableCard"/> we're checking.</param>
    /// <param name="energyCost">The <see cref="PlayableCard"/>'s energy cost. Defaults to using the <see cref="CardInfo"/>'s energy cost if null.</param>
    /// <returns>True if Gemified should reduce the energy cost by 1.</returns>
    /// <remarks>
    /// Used for multicost cards to determine which cost Gemified should affect.
    /// Priority from first choice to last: Energy, Bones, Mox, Blood
    /// </remarks>
    public static bool ReduceGemifiedEnergy(PlayableCard card, int? energyCost = null)
    {
        return (energyCost ?? OriginalEnergyCost(card.Info)) > 0;
    }
}

[HarmonyPatch]
internal static class ChangeCardCostGetter
{
    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BloodCost), MethodType.Getter), HarmonyPrefix]
    private static bool BloodCost(CardInfo __instance, ref int __result)
    {
        PlayableCard card = __instance.GetPlayableCard();
        //Debug.Log($"{card != null}");
        __result = Mathf.Max(0, card?.BloodCost() ?? CostProperties.OriginalBloodCost(__instance));
        return false;
    }

    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.BonesCost), MethodType.Getter), HarmonyPrefix]
    private static bool BoneCost(CardInfo __instance, ref int __result)
    {
        PlayableCard card = __instance.GetPlayableCard();
        if (card == null)
            return true;

        __result = Mathf.Max(0, card.BonesCost());
        return false;
    }

    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.GemsCost), MethodType.Getter), HarmonyPrefix]
    private static bool GemsCost(CardInfo __instance, ref List<GemType> __result)
    {
        PlayableCard card = __instance.GetPlayableCard();
        __result = card?.GemsCost() ?? CostProperties.ImprovedGemsCost(__instance);
        return false;
    }

    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.EnergyCost), MethodType.Getter), HarmonyPrefix]
    private static bool EnergyCost(CardInfo __instance, ref int __result)
    {
        PlayableCard card = __instance.GetPlayableCard();
        __result = card?.EnergyCost ?? CostProperties.OriginalEnergyCost(__instance);
        return false;
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.EnergyCost), MethodType.Getter), HarmonyPrefix]
    private static bool DisableVanillaEnergyCost(PlayableCard __instance, ref int __result)
    {
        // patch this to follow the same pattern as the other cost methods
        int energyCost = CostProperties.OriginalEnergyCost(__instance.Info);
        if (__instance.IsUsingBlueGem() && CostProperties.ReduceGemifiedEnergy(__instance, energyCost))
            energyCost--;

        foreach (CardModificationInfo mod in __instance.TemporaryMods)
            energyCost += mod.energyCostAdjustment;

        __result = Mathf.Max(0, energyCost);
        return false;
    }

    //[HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.SetInfo))]
    //private static void GetCardInfoReferences(PlayableCard __instance) {

    //    // if this is a new PlayableCard being associated with the given CardInfo
    //    // returns true if the <see cref="CardInfo"/> is in the table
    //    if (CostProperties.CardInfoToPlayableCardReferences.TryGetValue(__instance.Info, out List<WeakReference<PlayableCard>> connectedCards)) {
    //        PlayableCard card = null;
    //        for (int i = connectedCards.Count - 1; i >= 0; i--) {
    //            // NOTE: We store a list of cards so if we don't clear this list then it will fill up forever
    //            // remove PlayableCard references that no longer point to anything
    //            if (!connectedCards[i].TryGetTarget(out PlayableCard cardReference) || cardReference == null) {
    //                connectedCards.RemoveAt(i);
    //            }
    //            else if (cardReference == __instance) {
    //                card = cardReference;
    //            }
    //        }

    //        if (card == null) {
    //            connectedCards.Add(new WeakReference<PlayableCard>(__instance));
    //            if (connectedCards.Count > 1) {
    //                InscryptionAPIPlugin.Logger.LogWarning($"More than 1 card are using the same card info. This can cause unexpected problems with dynamic costs! {__instance.Info.displayedName}");
    //            }
    //        }
    //    }
    //    else {
    //        // add CardInfo to table with reference to this PlayableCard
    //        CostProperties.CardInfoToPlayableCardReferences.Add(__instance.Info, new() { new(__instance) });
    //    }
    //}
}

[HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.SetInfo))]
internal static class AddRefreshBehaviourToCard
{
    private static void Postfix(PlayableCard __instance)
    {
        // add the refresh component if it doesn't exist, then set the Card to the calling instance
        CostProperties.RefreshCostMonoBehaviour refreshCostDisplay = __instance.GetComponent<CostProperties.RefreshCostMonoBehaviour>() ?? __instance.gameObject.AddComponent<CostProperties.RefreshCostMonoBehaviour>();
        refreshCostDisplay.playableCard = __instance;

        // if this is a new PlayableCard being associated with the given CardInfo
        // returns true if the <see cref="CardInfo"/> is in the table
        if (CostProperties.CardInfoToCard.TryGetValue(__instance.Info, out List<WeakReference<PlayableCard>> cardList)) {
            PlayableCard card = null;
            for (int i = cardList.Count - 1; i >= 0; i--) {
                // NOTE: We store a list of cards so if we don't clear this list then it will fill up forever
                // remove PlayableCard references that no longer point to anything
                if (!cardList[i].TryGetTarget(out PlayableCard innerCard) || innerCard == null)
                    cardList.RemoveAt(i);

                else if (innerCard == __instance)
                    card = innerCard;
            }

            if (card == null) {
                cardList.Add(new WeakReference<PlayableCard>(__instance));
                if (cardList.Count > 1) {
                    InscryptionAPIPlugin.Logger.LogWarning($"More than 1 card are using the same CardInfo. This can cause unexpected problems with dynamic costs! {__instance.Info.displayedName}");
                }
            }
        }
        else {
            // add CardInfo to table with reference to this PlayableCard
            CostProperties.CardInfoToCard.Add(__instance.Info, new() { new (__instance) });
        }
    }
}

[HarmonyPatch]
internal static class TurnManager_CleanupPhase
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TurnManager), nameof(TurnManager.CleanupPhase));
        yield return AccessTools.Method(typeof(GBCEncounterManager), nameof(GBCEncounterManager.LoadOverworldScene));
    }

    private static void Postfix()
    {
        // NOTE: This is a hack to clear the table
        CostProperties.CardInfoToCard = new ConditionalWeakTable<CardInfo, List<WeakReference<PlayableCard>>>();
    }
}