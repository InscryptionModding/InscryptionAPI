using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Card.Merge
{
    public abstract class PlaceAboveBase : AbilityBehaviour
    {
        public virtual bool CanPlaceAbove(PlayableCard card)
        {
            return card.CanBeSacrificed;
        }
        public abstract IEnumerator OnPrePlaceAbove(PlayableCard placeAboveCard);
        public abstract IEnumerator OnPostPlaceAbove(PlayableCard placeAboveCard);
    }
}
