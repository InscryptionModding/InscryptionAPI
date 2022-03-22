using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using Pixelplacement;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Card.Merge
{
    public abstract class MergeKillOther : MergeBase
    {
        public virtual bool IsActualDeath => true;

        public override sealed IEnumerator OnMerge(PlayableCard mergeCard)
        {
            yield return OnPreMergeDeath(mergeCard);
            if (!mergeCard.Dead)
            {
                if (IsActualDeath)
                {
                    CardSlot prevSlot = Card != null && !Card.Dead ? mergeCard.slot : null;
                    if (prevSlot != null)
                    {
                        prevSlot.Card = Card;
                        Card.Slot = prevSlot;
                    }
                    yield return mergeCard.Die(false, null, true);
                    if (prevSlot != null && Card != null && !Card.Dead)
                    {
                        prevSlot.Card = Card;
                        Card.Slot = prevSlot;
                    }
                }
                else
                {
                    CardSlot prevSlot = Card != null && !Card.Dead ? mergeCard.slot : null;
                    if (prevSlot != null)
                    {
                        prevSlot.Card = Card;
                        Card.Slot = prevSlot;
                    }
                    mergeCard.Dead = true;
                    mergeCard.UnassignFromSlot();
                    mergeCard.StartCoroutine(mergeCard.DestroyWhenStackIsClear());
                    if (prevSlot != null && Card != null && !Card.Dead)
                    {
                        prevSlot.Card = Card;
                        Card.Slot = prevSlot;
                    }
                }
            }
            yield break;
        }

        public sealed override IEnumerator OnPreMerge(PlayableCard mergeCard)
        {
            CustomCoroutine.WaitThenExecute(0.025f, delegate ()
            {
                Tween.Stop(mergeCard.transform.GetInstanceID(), Tween.TweenType.Position);
                Tween.Position(mergeCard.transform, Card.transform.position + Vector3.up * 0.2f, 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
                if (!IsActualDeath)
                {
                    Tween.LocalScale(transform, Vector3.zero, 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
                }
            }, false);
            yield return OnPreCreatureMerge(mergeCard);
            yield break;
        }

        public abstract IEnumerator OnPreCreatureMerge(PlayableCard mergeCard);

        public abstract IEnumerator OnPreMergeDeath(PlayableCard mergeCard);
    }
}
