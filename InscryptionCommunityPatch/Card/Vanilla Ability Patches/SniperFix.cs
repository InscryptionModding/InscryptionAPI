using DiskCardGame;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public class SniperFix
{
    [HarmonyPatch(typeof(CombatPhaseManager), nameof(CombatPhaseManager.SlotAttackSequence))]
    [HarmonyPrefix]
    private static bool MaybeOverrideAttack(CombatPhaseManager __instance, ref IEnumerator __result, CardSlot slot)
    {
        if (slot?.Card != null && slot.Card.HasAbility(Ability.Sniper))
        {
            __result = SniperAttack(__instance, slot);
            return false;
        }
        return true;
    }

    public static IEnumerator SniperAttack(CombatPhaseManager instance, CardSlot slot)
    {
        List<CardSlot> opposingSlots = new(); // the slots to attacks
        Singleton<ViewManager>.Instance.SwitchToView(Singleton<BoardManager>.Instance.CombatView, false, false);
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
        int numAttacks = GetAttackCount(slot.Card);
        Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(Singleton<BoardManager>.Instance.ChoosingSlotViewMode, false);
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
        Part1SniperVisualizer visualizer = null;
        if ((SaveManager.SaveFile?.IsPart1).GetValueOrDefault())
            visualizer = instance.GetComponent<Part1SniperVisualizer>() ?? instance.gameObject.AddComponent<Part1SniperVisualizer>();

        if (slot.Card.OpponentCard)
            yield return OpponentSniperLogic(instance, visualizer, opposingSlots, slot, numAttacks);
        else
            yield return PlayerSniperLogic(instance, visualizer, opposingSlots, slot, numAttacks);

        Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(Singleton<BoardManager>.Instance.DefaultViewMode, false);
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
        foreach (CardSlot opposingSlot in opposingSlots)
        {
            Singleton<ViewManager>.Instance.SwitchToView(Singleton<BoardManager>.Instance.CombatView, false, false);
            yield return instance.SlotAttackSlot(slot, opposingSlot, (opposingSlots.Count > 1) ? 0.1f : 0f);
        }
        instance.VisualizeClearSniperAbility();
        visualizer?.VisualizeClearSniperAbility();
        yield break;
    }
    public static IEnumerator PlayerSniperLogic(
        CombatPhaseManager instance, Part1SniperVisualizer visualizer,
        List<CardSlot> opposingSlots, CardSlot slot, int numAttacks)
    {
        for (int i = 0; i < numAttacks; i++)
        {
            instance.VisualizeStartSniperAbility(slot);
            visualizer?.VisualizeStartSniperAbility(slot);
            CardSlot cardSlot = Singleton<InteractionCursor>.Instance.CurrentInteractable as CardSlot;
            if (cardSlot != null && opposingSlots.Contains(cardSlot))
            {
                instance.VisualizeAimSniperAbility(slot, cardSlot);
                visualizer?.VisualizeAimSniperAbility(slot, cardSlot);
            }
            yield return Singleton<BoardManager>.Instance.ChooseTarget(Singleton<BoardManager>.Instance.OpponentSlotsCopy, Singleton<BoardManager>.Instance.OpponentSlotsCopy,
                delegate (CardSlot s)
                {
                    opposingSlots.Add(s);
                    instance.VisualizeConfirmSniperAbility(s);
                    visualizer?.VisualizeConfirmSniperAbility(s);
                }, null, delegate (CardSlot s)
                {
                    instance.VisualizeAimSniperAbility(slot, s);
                    visualizer?.VisualizeAimSniperAbility(slot, s);
                }, () => false, CursorType.Target);
        }
    }
    public static IEnumerator OpponentSniperLogic(
        CombatPhaseManager instance, Part1SniperVisualizer visualizer,
        List<CardSlot> opposingSlots, CardSlot slot, int numAttacks)
    {
        List<CardSlot> playerSlots = Singleton<BoardManager>.Instance.PlayerSlotsCopy;
        List<PlayableCard> playerCards = playerSlots.FindAll(x => x.Card != null).ConvertAll((x) => x.Card);
        bool anyCards = playerCards.Count > 0;

        for (int i = 0; i < numAttacks; i++)
        {
            CardSlot attackSlot = slot.opposingSlot;
            if (anyCards)
            {
                PlayableCard strongestKillable = GetStrongestKillableCard(anyCards, playerCards, opposingSlots, slot, numAttacks);
                PlayableCard strongestAttackable = GetFirstStrongestAttackableCard(anyCards, playerCards, opposingSlots, slot, numAttacks);
                PlayableCard strongestAttackableNoPreferences = GetFirstStrongestAttackableCardNoPreferences(anyCards, playerCards, opposingSlots, slot, numAttacks);

                if (CanWin(opposingSlots, playerSlots, slot, numAttacks))
                    attackSlot = GetFirstAvailableOpenSlot(opposingSlots, playerSlots, slot, numAttacks);

                else if (strongestKillable != null)
                    attackSlot = strongestKillable.Slot;

                else if (strongestAttackable != null)
                    attackSlot = strongestAttackable.Slot;

                else if (strongestAttackableNoPreferences != null)
                    attackSlot = strongestAttackableNoPreferences.Slot;

            }
            opposingSlots.Add(attackSlot);
            instance.VisualizeConfirmSniperAbility(attackSlot);
            visualizer?.VisualizeConfirmSniperAbility(attackSlot);
            yield return new WaitForSeconds(0.25f);
        }
    }
    public static List<T> GetSorted<T>(List<T> unsorted, Comparison<T> sort)
    {
        List<T> toSort = new(unsorted);
        toSort.Sort(sort);
        return toSort;
    }
    public static CardSlot GetFirstAvailableOpenSlot(
        List<CardSlot> opposingSlots, List<CardSlot> playerSlots,
        CardSlot slot, int numAttacks)
    {
        return playerSlots.Find(x => slot.Card.CanAttackDirectly(x) &&
            !playerSlots.Exists(y => y.Card != null &&
                y.Card.HasAbility(Ability.WhackAMole) &&
                !CardIsDeadOrHasRepulsive(y.Card, opposingSlots, slot, numAttacks) &&
                !slot.Card.CanAttackDirectly(y)));
    }
    public static bool CanWin(List<CardSlot> opposingSlots, List<CardSlot> playerSlots, CardSlot slot, int numAttacks)
    {
        return LifeManager.Instance != null &&
            LifeManager.Instance.Balance - (numAttacks * slot.Card.Attack) <= -5 &&
            GetFirstAvailableOpenSlot(opposingSlots, playerSlots, slot, numAttacks) != null;
    }
    public static bool CanKillCard(PlayableCard pc, int numAttacks, CardSlot slot, int? overrideAttacks = null)
    {
        int realNumAttacks = overrideAttacks ?? numAttacks;
        int availableAttacksToKill = pc.HasShield() ? realNumAttacks - 1 : realNumAttacks;
        return slot.Card.HasAbility(Ability.Deathtouch) && !pc.HasAbility(Ability.MadeOfStone) ? availableAttacksToKill > 0 : availableAttacksToKill * slot.Card.Attack >= pc.Health &&
            !slot.Card.CanAttackDirectly(pc.Slot);
    }
    public static int NumCardTargets(PlayableCard pc, List<CardSlot> opposingSlots)
    {
        return opposingSlots.FindAll((x) => x != null && x.Card != null && x.Card == pc).Count;
    }
    public static bool CardIsDeadOrHasRepulsive(PlayableCard pc, List<CardSlot> opposingSlots, CardSlot slot, int numAttacks)
    {
        return pc == null || pc.Dead || pc.HasAbility(Ability.PreventAttack) || CanKillCard(pc, numAttacks, slot, NumCardTargets(pc, opposingSlots));
    }
    public static bool WillDieFromSharp(PlayableCard pc, CardSlot slot)
    {
        int attacksFromSpiky = pc.Info.Abilities.FindAll((Ability x) => x == Ability.Sharp).Count;
        if (pc.HasAbility(Ability.Sharp) && attacksFromSpiky < 1)
            attacksFromSpiky = 1;

        if (slot.Card.HasShield())
            attacksFromSpiky--;

        attacksFromSpiky = Mathf.Max(0, attacksFromSpiky);
        return pc.HasAbility(Ability.Deathtouch) ? attacksFromSpiky > 0 : attacksFromSpiky >= slot.Card.Health;
    }
    public static PlayableCard GetFirstStrongestAttackableCard(
        bool anyCards,
        List<PlayableCard> playerCards, List<CardSlot> opposingSlots,
        CardSlot slot, int numAttacks)
    {
        if (!anyCards)
            return null;

        return GetSorted(playerCards.FindAll(x => !slot.Card.CanAttackDirectly(x.Slot) && !WillDieFromSharp(x, slot) && !CardIsDeadOrHasRepulsive(x, opposingSlots, slot, numAttacks)), (x, x2) => x.PowerLevel - x2.PowerLevel).FirstOrDefault();
    }
    public static PlayableCard GetStrongestKillableCard(bool anyCards,
    List<PlayableCard> playerCards, List<CardSlot> opposingSlots,
    CardSlot slot, int numAttacks)
    {
        if (!anyCards)
            return null;

        return anyCards ? GetSorted(playerCards.FindAll((x) => CanKillCard(x, numAttacks, slot) && !WillDieFromSharp(x, slot) && !CardIsDeadOrHasRepulsive(x, opposingSlots, slot, numAttacks)), (x, x2) => x.PowerLevel - x2.PowerLevel).FirstOrDefault() : null;
    }
    public static PlayableCard GetFirstStrongestAttackableCardNoPreferences(bool anyCards,
        List<PlayableCard> playerCards, List<CardSlot> opposingSlots,
        CardSlot slot, int numAttacks)
    {
        if (!anyCards)
            return null;

        return GetSorted(playerCards.FindAll((x) => !slot.Card.CanAttackDirectly(x.Slot) && !CardIsDeadOrHasRepulsive(x, opposingSlots, slot, numAttacks)), (x, x2) => x.PowerLevel - x2.PowerLevel).FirstOrDefault();
    }

    /// <summary>
    /// Counts the number of times that a card with sniper should attack
    /// </summary>
    /// <param name="card">The card with sniper on it</param>
    /// <returns>An integer indicating the number of times the card should attack</returns>
    public static int GetAttackCount(PlayableCard card)
    {
        // Let's do this the easiest way possible
        // Assign the card to each slot, count the number of opposing slots, and use the highest number
        // This way we always use the most up-to-date logic and patches for GetOpposingSlots
        CardSlot currentSlot = card.Slot;
        List<CardSlot> slots = card.OpponentCard ? BoardManager.Instance.OpponentSlotsCopy : BoardManager.Instance.PlayerSlotsCopy;
        int highestCount = 1;
        foreach (CardSlot slot in slots)
        {
            card.Slot = slot;
            int opposingSlots = card.GetOpposingSlots().Count;
            if (opposingSlots > highestCount)
                highestCount = opposingSlots;
        }
        card.Slot = currentSlot;
        return highestCount;
    }
}
