using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Card.Merge
{
    public abstract class MergeBase : AbilityBehaviour
    {
        public abstract IEnumerator OnPreMerge(PlayableCard mergeCard);
        public abstract IEnumerator OnMerge(PlayableCard mergeCard);
        public virtual bool CanMergeWith(PlayableCard mergeCard)
        {
            return true;
        }
    }
}
