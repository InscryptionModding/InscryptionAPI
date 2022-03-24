using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card
{
    [HarmonyPatch]
    public static class SniperFix
    {
        [HarmonyPatch(typeof(CombatPhaseManager), "SlotAttackSequence")]
        [HarmonyPostfix]
        public static IEnumerator SniperAttackFix(IEnumerator result, CombatPhaseManager __instance, CardSlot slot)
        {
            if (slot.Card.HasAbility(Ability.Sniper))
            {
                List<CardSlot> opposingSlots = slot.Card.GetOpposingSlots();
                Singleton<ViewManager>.Instance.SwitchToView(Singleton<BoardManager>.Instance.CombatView, false, false);
                Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
                int numAttacks = 1;
                if (slot.Card.HasAbility(Ability.SplitStrike))
                {
                    numAttacks += 1;
                }
                if (slot.Card.HasAbility(Ability.DoubleStrike))
                {
                    numAttacks += 1;
                }
                if (slot.Card.HasTriStrike())
                {
                    numAttacks += 2;
                }
                try
                {
                    List<Ability> list = new();
                    list.AddRange(slot.Card.Info.Abilities);
                    foreach (CardModificationInfo cardModificationInfo in slot.Card.TemporaryMods)
                    {
                        list.AddRange(cardModificationInfo.abilities);
                    }
                    list = AbilitiesUtil.RemoveNonDistinctNonStacking(list);
                    list.ForEach((x) => numAttacks += InscryptionAPI.Card.AbilityManager.GetOpposingSlotModifierFromAbility(slot.Card, x));
                }
                catch(Exception ex) { Debug.LogWarning("InscryptionAPI not accesible? " + ex); }
                try
                {
                    slot.Card.TriggerHandler.GetAllReceivers().FindAll(x => x is InscryptionAPI.Card.ExtendedAbilityBehaviour).ConvertAll(x => x as InscryptionAPI.Card.ExtendedAbilityBehaviour).ForEach(x =>
                        numAttacks += x.ExtraAttacks);
                }
                catch (Exception ex) { Debug.LogWarning("InscryptionAPI not accesible? " + ex); }
                opposingSlots.Clear();
                Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(Singleton<BoardManager>.Instance.ChoosingSlotViewMode, false);
                Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
                Part1SniperVisualizer visualizer = null;
                if ((SaveManager.SaveFile?.IsPart1).GetValueOrDefault())
                {
                    visualizer = __instance.GetComponent<Part1SniperVisualizer>() ?? __instance.gameObject.AddComponent<Part1SniperVisualizer>();
                }
                if (slot.Card.OpponentCard)
                {
                    List<CardSlot> slots = Singleton<BoardManager>.Instance.PlayerSlotsCopy;
                    List<PlayableCard> cards = slots.FindAll(x => x.Card != null).ConvertAll((x) => x.Card);
                    bool anyCards = cards.Count > 0;
                    CardSlot GetFirstAvailableOpenSlot()
                    {
                        return slots.Find(x => slot.Card.CanAttackDirectly(x) && !slots.Exists((x) => x.Card != null && x.Card.HasAbility(Ability.WhackAMole) && !CardIsAlreadyDead(x.Card) && !slot.Card.CanAttackDirectly(x)));
                    }
                    bool CanWin()
                    {
                        return LifeManager.Instance != null && LifeManager.Instance.Balance - (numAttacks * slot.Card.Attack) <= -5 && GetFirstAvailableOpenSlot() != null;
                    }
                    List<T> GetSorted<T>(List<T> unsorted, Comparison<T> sort)
                    {
                        List<T> toSort = new(unsorted);
                        toSort.Sort(sort);
                        return toSort;
                    }
                    bool CanKillCard(PlayableCard pc, int? overrideAttacks = null)
                    {
                        int realNumAttacks = overrideAttacks ?? numAttacks;
                        int availableAttacksToKill = pc.HasShield() ? realNumAttacks - 1 : realNumAttacks;
                        return slot.Card.HasAbility(Ability.Deathtouch) && !pc.HasAbility(Ability.MadeOfStone) ? availableAttacksToKill > 0 : availableAttacksToKill * slot.Card.Attack >= pc.Health &&
                            !slot.Card.CanAttackDirectly(pc.Slot);
                    }
                    int NumCardTargets(PlayableCard pc)
                    {
                        return opposingSlots.FindAll((x) => x != null && x.Card != null && x.Card == pc).Count;
                    }
                    bool CardIsAlreadyDead(PlayableCard pc)
                    {
                        return pc == null || pc.Dead || CanKillCard(pc, NumCardTargets(pc));
                    }
                    bool DeathFromSpiky(PlayableCard pc)
                    {
                        int attacksFromSpiky = pc.Info.Abilities.FindAll((Ability x) => x == Ability.Sharp).Count;
                        if(pc.HasAbility(Ability.Sharp) && attacksFromSpiky < 1)
                        {
                            attacksFromSpiky = 1;
                        }
                        if (slot.Card.HasShield())
                        {
                            attacksFromSpiky--;
                        }
                        attacksFromSpiky = Mathf.Max(attacksFromSpiky, 0);
                        return pc.HasAbility(Ability.Deathtouch) ? attacksFromSpiky > 0 : attacksFromSpiky >= slot.Card.Health;
                    }
                    PlayableCard GetFirstStrongestAttackableCard()
                    {
                        return anyCards ? GetSorted(cards.FindAll((x) => !slot.Card.CanAttackDirectly(x.Slot) && !DeathFromSpiky(x) && !CardIsAlreadyDead(x)), (x, x2) => x.PowerLevel - x2.PowerLevel).FirstOrDefault() : null;
                    }
                    PlayableCard GetFirstStrongestAttackableCardNoPreferences()
                    {
                        return anyCards ? GetSorted(cards.FindAll((x) => !slot.Card.CanAttackDirectly(x.Slot) && !CardIsAlreadyDead(x)), (x, x2) => x.PowerLevel - x2.PowerLevel).FirstOrDefault() : null;
                    }
                    PlayableCard GetStrongestKillableCard()
                    {
                        return anyCards ? GetSorted(cards.FindAll((x) => CanKillCard(x) && !DeathFromSpiky(x) && !CardIsAlreadyDead(x)), (x, x2) => x.PowerLevel - x2.PowerLevel).FirstOrDefault() : null;
                    }
                    for (int i = 0; i < numAttacks; i++)
                    {
                        CardSlot attackSlot = slot.opposingSlot;
                        if (anyCards)
                        {
                            PlayableCard strongestKillable = GetStrongestKillableCard();
                            PlayableCard strongestAttackable = GetFirstStrongestAttackableCard();
                            PlayableCard strongestAttackableNoPreferences = GetFirstStrongestAttackableCardNoPreferences();
                            if (CanWin())
                            {
                                attackSlot = GetFirstAvailableOpenSlot();
                            }
                            else if (strongestKillable != null)
                            {
                                attackSlot = strongestKillable.Slot;
                            }
                            else if(strongestAttackable != null)
                            {
                                attackSlot = strongestAttackable.Slot;
                            }
                            else if(strongestAttackableNoPreferences != null)
                            {
                                attackSlot = strongestAttackableNoPreferences.Slot;
                            }
                        }
                        opposingSlots.Add(attackSlot);
                        __instance.VisualizeConfirmSniperAbility(attackSlot);
                        visualizer?.VisualizeConfirmSniperAbility(attackSlot);
                        yield return new WaitForSeconds(0.25f);
                    }
                }
                else
                {
                    for (int i = 0; i < numAttacks; i++)
                    {
                        __instance.VisualizeStartSniperAbility(slot);
                        visualizer?.VisualizeStartSniperAbility(slot);
                        CardSlot cardSlot = Singleton<InteractionCursor>.Instance.CurrentInteractable as CardSlot;
                        if (cardSlot != null && opposingSlots.Contains(cardSlot))
                        {
                            __instance.VisualizeAimSniperAbility(slot, cardSlot);
                            visualizer?.VisualizeAimSniperAbility(slot, cardSlot);
                        }
                        yield return Singleton<BoardManager>.Instance.ChooseTarget(Singleton<BoardManager>.Instance.OpponentSlotsCopy, Singleton<BoardManager>.Instance.OpponentSlotsCopy,
                            delegate (CardSlot s)
                            {
                                opposingSlots.Add(s);
                                __instance.VisualizeConfirmSniperAbility(s);
                                visualizer?.VisualizeConfirmSniperAbility(s);
                            }, null, delegate (CardSlot s)
                            {
                                __instance.VisualizeAimSniperAbility(slot, s);
                                visualizer?.VisualizeAimSniperAbility(slot, s);
                            }, () => false, CursorType.Target);
                    }
                }
                Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(Singleton<BoardManager>.Instance.DefaultViewMode, false);
                Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
                foreach (CardSlot opposingSlot in opposingSlots)
                {
                    Singleton<ViewManager>.Instance.SwitchToView(Singleton<BoardManager>.Instance.CombatView, false, false);
                    yield return __instance.SlotAttackSlot(slot, opposingSlot, (opposingSlots.Count > 1) ? 0.1f : 0f);
                }
                __instance.VisualizeClearSniperAbility();
                visualizer?.VisualizeClearSniperAbility();
            }
            else
            {
                yield return result;
            }
            yield break;
        }
    }
}
