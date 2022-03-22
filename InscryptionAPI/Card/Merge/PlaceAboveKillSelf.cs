using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using UnityEngine;
using System.Collections;
using Pixelplacement;

namespace InscryptionAPI.Card.Merge
{
    public abstract class PlaceAboveKillSelf : PlaceAboveBase
    {
        public virtual bool IsActualDeath => true;

        public override sealed IEnumerator OnPostPlaceAbove(PlayableCard placeAboveCard)
        {
            yield return OnPrePlaceAboveDeath(placeAboveCard);
            if (!Card.Dead)
            {
                if (IsActualDeath)
                {
                    CardSlot prevSlot = placeAboveCard != null && !placeAboveCard.Dead ? Card.slot : null;
                    if (prevSlot != null)
                    {
                        prevSlot.Card = placeAboveCard;
                        placeAboveCard.Slot = prevSlot;
                    }
                    yield return Card.Die(false, null, true);
                    if (prevSlot != null && placeAboveCard != null && !placeAboveCard.Dead)
                    {
                        prevSlot.Card = placeAboveCard;
                        placeAboveCard.Slot = prevSlot;
                    }
                }
                else
                {
                    CardSlot prevSlot = placeAboveCard != null && !placeAboveCard.Dead ? Card.slot : null;
                    if (prevSlot != null)
                    {
                        prevSlot.Card = placeAboveCard;
                        placeAboveCard.Slot = prevSlot;
                    }
                    Card.Dead = true;
                    Card.UnassignFromSlot();
                    Card.StartCoroutine(Card.DestroyWhenStackIsClear());
                    if (prevSlot != null && placeAboveCard != null && !placeAboveCard.Dead)
                    {
                        prevSlot.Card = placeAboveCard;
                        placeAboveCard.Slot = prevSlot;
                    }
                }
            }
            yield break;
        }

        public sealed override IEnumerator OnPrePlaceAbove(PlayableCard placeAboveCard)
        {
            CustomCoroutine.WaitThenExecute(0.025f, delegate ()
            {
                Tween.Stop(Card.transform.GetInstanceID(), Tween.TweenType.Position);
                Tween.Position(Card.transform, placeAboveCard.transform.position + Vector3.up * 0.2f, 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
                if (!IsActualDeath)
                {
                    Tween.LocalScale(transform, Vector3.zero, 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
                }
            }, false);
            yield return OnPreCreaturePlaceAbove(placeAboveCard);
            yield break;
        }

        public abstract IEnumerator OnPreCreaturePlaceAbove(PlayableCard mergeCard);
        public abstract IEnumerator OnPrePlaceAboveDeath(PlayableCard mergeCard);
    }
}
