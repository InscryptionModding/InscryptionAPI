using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Nodes
{
    public abstract class CustomCardChoiceNodeSequencer : CardChoicesSequencer, ICustomNodeSequencer, IInherit, IDestroyOnEnd, IDoNotReturnToMapOnEnd
    {
        public abstract IEnumerator DoCustomSequence(CustomSpecialNodeData node);
        public virtual CardChoicesSequencer InheritTarget => EasyAccess.CardSingleChoices;
        public virtual Vector3 PositionOffset => Vector3.zero;
        public virtual Quaternion RotationOffset => Quaternion.identity;
        public virtual Vector3 DeckPilePositionOffset => Vector3.zero;
        public virtual Quaternion DeckPileRotationOffset => Quaternion.identity;
        public virtual bool DeckPileInheritsOffsets => false;
        public virtual bool ShouldInherit => true;
        public virtual void Inherit(CustomSpecialNodeData node)
        {
            var target = InheritTarget;
            if (ShouldInherit && target != null)
            {
                transform.position = target.transform.position + PositionOffset;
                transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles + RotationOffset.eulerAngles);
                if(target.deckPile != null)
                {
                    deckPile = Instantiate(target.deckPile, target.deckPile.transform.position, target.deckPile.transform.rotation);
                    Vector3 position = deckPile.transform.position;
                    Quaternion rotation = deckPile.transform.rotation;
                    deckPile.transform.parent = transform;
                    deckPile.transform.position = position + DeckPilePositionOffset + (DeckPileInheritsOffsets ? PositionOffset : Vector3.zero);
                    deckPile.transform.rotation = Quaternion.Euler(rotation.eulerAngles + DeckPileRotationOffset.eulerAngles + (DeckPileInheritsOffsets ? RotationOffset.eulerAngles : Vector3.zero));
                }
                selectableCardPrefab = target.selectableCardPrefab;
                if(target.gamepadGrid != null)
                {
                    gamepadGrid = Instantiate(target.gamepadGrid, target.gamepadGrid.transform.position, target.gamepadGrid.transform.rotation);
                    Vector3 position = gamepadGrid.transform.localPosition;
                    Quaternion rotation = gamepadGrid.transform.localRotation;
                    gamepadGrid.transform.parent = transform;
                    gamepadGrid.transform.localPosition = position;
                    gamepadGrid.transform.localRotation = rotation;
                }
            }
        }
        public virtual bool ShouldDestroyOnEnd(CustomSpecialNodeData node) { return false; }
        public virtual bool ShouldNotReturnToMapOnEnd(CustomSpecialNodeData node) { return false; }
    }
}
