using DiskCardGame;
using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Card.Merge
{
    public abstract class MergeKillSelf : MergeBase
    {
        public virtual bool IsActualDeath => false;

        public override sealed IEnumerator OnMerge(PlayableCard mergeCard)
        {
            yield return OnPreMergeDeath(mergeCard);
            if (!Card.Dead)
            {
                if (IsActualDeath)
                {
                    yield return Card.Die(false, null, true);
                }
                else
                {
                    Card.Dead = true;
                    Card.UnassignFromSlot();
                    Card.StartCoroutine(Card.DestroyWhenStackIsClear());
                }
            }
            yield break;
        }

        public sealed override IEnumerator OnPreMerge(PlayableCard mergeCard)
        {
            if (!IsActualDeath)
            {
                Tween.LocalScale(transform, Vector3.zero, 0.2f, 0f, null, Tween.LoopType.None, null, null, true);
            }
            else
            {
                Tween.Position(transform, transform.position - Vector3.up * -0.02f, 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
            }
            yield return OnPreCreatureMerge(mergeCard);
            yield break;
        }

        public abstract IEnumerator OnPreCreatureMerge(PlayableCard mergeCard);

        public abstract IEnumerator OnPreMergeDeath(PlayableCard mergeCard);
    }
}
