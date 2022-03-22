using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using Pixelplacement;
using UnityEngine;
using System.Collections;

namespace InscryptionAPI.Card.Merge
{
    public abstract class PlaceAboveKillOther : PlaceAboveBase
    {
        public virtual bool IsActualDeath => false;

        public override sealed IEnumerator OnPostPlaceAbove(PlayableCard placeAboveCard)
        {
            yield return OnPrePlaceAboveDeath(placeAboveCard);
            if (!placeAboveCard.Dead)
            {
                if (IsActualDeath)
                {
                    yield return placeAboveCard.Die(false, null, true);
                }
                else
                {
                    placeAboveCard.Dead = true;
                    placeAboveCard.UnassignFromSlot();
                    placeAboveCard.StartCoroutine(placeAboveCard.DestroyWhenStackIsClear());
                }
            }
            yield break;
        }

        public sealed override IEnumerator OnPrePlaceAbove(PlayableCard placeAboveCard)
        {
            if (!IsActualDeath)
            {
                Tween.LocalScale(placeAboveCard.transform, Vector3.zero, 0.2f, 0f, null, Tween.LoopType.None, null, null, true);
            }
            else
            {
                Tween.Position(placeAboveCard.transform, placeAboveCard.transform.position + Vector3.up * -0.02f, 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
            }
            yield return OnPreCreaturePlaceAbove(placeAboveCard);
            yield break;
        }

        public abstract IEnumerator OnPreCreaturePlaceAbove(PlayableCard placeAboveCard);
        public abstract IEnumerator OnPrePlaceAboveDeath(PlayableCard placeAboveCard);
    }
}
