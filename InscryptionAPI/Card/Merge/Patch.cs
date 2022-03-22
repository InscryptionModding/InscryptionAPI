using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Card.Merge
{
    [HarmonyPatch]
    public class Patch
    {
        [HarmonyPatch(typeof(BoardManager), "SacrificesCreateRoomForCard")]
        [HarmonyPostfix]
        public static void SacrificesCreateRoomForCard(BoardManager __instance, ref bool __result, PlayableCard card, List<CardSlot> sacrifices)
        {
            if (!__result)
            {
                foreach (CardSlot cardSlot in __instance.PlayerSlotsCopy)
                {
                    if (cardSlot.Card == null)
                    {
                        __result = true;
                        break;
                    }
                    if (card.Info.BloodCost >= 1 && sacrifices.Contains(cardSlot) && !cardSlot.Card.HasAbility(Ability.Sacrificial) && cardSlot.Card.CanBeSacrificed ||
                        (cardSlot != null && cardSlot.Card != null &&
                        (cardSlot?.Card?.TriggerHandler?.GetAllReceivers()?.FindAll(x => x is MergeBase)?.ConvertAll(x => (x as MergeBase).CanMergeWith(card)).Contains(true)).GetValueOrDefault()) ||
                        (cardSlot != null && cardSlot.Card != null && 
                        (card?.TriggerHandler?.GetAllReceivers()?.FindAll(x => x is PlaceAboveBase)?.ConvertAll(x => (x as PlaceAboveBase).CanPlaceAbove(cardSlot.Card)).Contains(true)).GetValueOrDefault()))
                    {
                        __result = true;
                        break;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerHand), "SelectSlotForCard")]
        [HarmonyPostfix]
        public static IEnumerator SelectSlotForCard(IEnumerator rat, PlayableCard card)
        {
            while (rat.MoveNext())
            {
                object current = rat.Current;
                if(current.GetType() == (type ??= (AccessTools.TypeByName("DiskCardGame.BoardManager+<ChooseSlot>d__75") ?? AccessTools.TypeByName("DiskCardGame.BoardManager+<ChooseSlot>d__81"))))
                {
                    List<CardSlot> emptySlots = Singleton<BoardManager>.Instance.PlayerSlotsCopy.FindAll((CardSlot x) => x != null && (x.Card == null || 
                        (x?.Card?.TriggerHandler?.GetAllReceivers()?.FindAll(x => x is MergeBase)?.ConvertAll(x => (x as MergeBase).CanMergeWith(card)).Contains(true)).GetValueOrDefault() ||
                        (card?.TriggerHandler?.GetAllReceivers()?.FindAll(x => x is PlaceAboveBase)?.ConvertAll(x2 => (x2 as PlaceAboveBase).CanPlaceAbove(x.Card)).Contains(true)).GetValueOrDefault()));
                    yield return BoardManager.Instance.ChooseSlot(emptySlots, card.Info.BloodCost <= 0);
                }
                else if(current.GetType() == (placeType ??= (AccessTools.TypeByName("DiskCardGame.PlayerHand+<PlayCardOnSlot>d__37") ?? AccessTools.TypeByName("DiskCardGame.PlayerHand+<PlayCardOnSlot>d__36"))))
                {
                    CardSlot slot = Singleton<BoardManager>.Instance.LastSelectedSlot;
                    MergeBase merge = null;
                    PlaceAboveBase placeAbove = null;
                    if (slot != null && slot.Card != null && slot.Card != null && slot.Card.TriggerHandler != null && slot.Card.TriggerHandler.GetAllReceivers() != null && 
                        slot.Card.TriggerHandler.GetAllReceivers().Exists(x => x is MergeBase))
                    {
                        merge = (MergeBase)slot.Card.TriggerHandler.GetAllReceivers().Find(x => x is MergeBase);
                    }
                    if(slot != null && card?.TriggerHandler?.GetAllReceivers()?.Find(x => x is PlaceAboveBase) != null)
                    {
                        placeAbove = (PlaceAboveBase)card?.TriggerHandler?.GetAllReceivers()?.Find(x => x is PlaceAboveBase);
                    }
                    PlayableCard slotCard = slot.Card;
                    bool hasMerge = false;
                    bool hasPlaceAbove = false;
                    /*bool willKillOriginal = false;
                    bool willKillPlaced = false;
                    bool willKillOriginalPA = false;
                    bool willKillPlacedPA = false;*/
                    void UpdateValues()
                    {
                        hasMerge = merge != null;
                        hasPlaceAbove = placeAbove != null && slotCard != null;
                    }
                    UpdateValues();
                    if (hasMerge)
                    {
                        foreach (MergeBase merge2 in slotCard.TriggerHandler.GetAllReceivers().FindAll(x => x is MergeBase))
                        {
                            UpdateValues();
                            if (hasMerge && merge2 != null && merge2.CanMergeWith(card))// && ((!willKillOriginal || merge2 is not MergeKillOther) && (!willKillPlaced || merge2 is not MergeKillSelf)))
                            {
                                /*if(merge2 is MergeKillOther)
                                {
                                    willKillPlaced = true;
                                }
                                else if(merge2 is MergeKillSelf)
                                {
                                    willKillOriginal = true;
                                }*/
                                GlobalTriggerHandler.Instance.StackSize++;
                                yield return merge2.OnPreMerge(card);
                                GlobalTriggerHandler.Instance.StackSize--;
                            }
                        }
                    }
                    UpdateValues();
                    if (hasPlaceAbove)
                    {
                        foreach (PlaceAboveBase placeAbove2 in card.TriggerHandler.GetAllReceivers().FindAll(x => x is PlaceAboveBase))
                        {
                            UpdateValues();
                            if (hasPlaceAbove && placeAbove2 != null && placeAbove2.CanPlaceAbove(slotCard))// && (!willKillOriginalPA || placeAbove2 is not PlaceAboveKillSelf) && (!willKillPlacedPA || placeAbove2 is not PlaceAboveKillOther))
                            {
                                /*if (placeAbove2 is PlaceAboveKillSelf)
                                {
                                    willKillPlacedPA = true;
                                }
                                else if (placeAbove2 is PlaceAboveKillOther)
                                {
                                    willKillOriginalPA = true;
                                }*/
                                GlobalTriggerHandler.Instance.StackSize++;
                                yield return placeAbove2.OnPrePlaceAbove(slotCard);
                                GlobalTriggerHandler.Instance.StackSize--;
                            }
                        }
                    }
                    yield return current;
                    UpdateValues();
                    if (hasMerge)
                    {
                        foreach (MergeBase merge2 in slotCard.TriggerHandler.GetAllReceivers().FindAll(x => x is MergeBase))
                        {
                            UpdateValues();
                            if (hasMerge && merge2 != null && merge2.CanMergeWith(card))// && ((!willKillOriginal || merge2 is not MergeKillOther) && (!willKillPlaced || merge2 is not MergeKillSelf)))
                            {
                                /*if (merge2 is MergeKillOther)
                                {
                                    willKillPlaced = true;
                                }
                                else if (merge2 is MergeKillSelf)
                                {
                                    willKillOriginal = true;
                                }*/
                                GlobalTriggerHandler.Instance.StackSize++;
                                yield return merge2.OnMerge(card);
                                GlobalTriggerHandler.Instance.StackSize--;
                            }
                        }
                    }
                    UpdateValues();
                    if (hasPlaceAbove)
                    {
                        foreach (PlaceAboveBase placeAbove2 in card.TriggerHandler.GetAllReceivers().FindAll(x => x is PlaceAboveBase))
                        {
                            UpdateValues();
                            if (hasPlaceAbove && placeAbove2 != null && placeAbove2.CanPlaceAbove(slotCard))// && ((!willKillOriginalPA || placeAbove2 is not PlaceAboveKillSelf) && (!willKillPlacedPA || placeAbove2 is not PlaceAboveKillOther)))
                            {
                                /*if (placeAbove2 is PlaceAboveKillSelf)
                                {
                                    willKillPlacedPA = true;
                                }
                                else if (placeAbove2 is PlaceAboveKillOther)
                                {
                                    willKillOriginalPA = true;
                                }*/
                                GlobalTriggerHandler.Instance.StackSize++;
                                yield return placeAbove2.OnPostPlaceAbove(slotCard);
                                GlobalTriggerHandler.Instance.StackSize--;
                            }
                        }
                    }
                }
                else
                {
                    yield return current;
                }
            }
            yield break;
        }

        public static Type type;
        public static Type placeType;
    }
}
