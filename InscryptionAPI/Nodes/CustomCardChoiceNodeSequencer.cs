using DiskCardGame;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Nodes;

/// <summary>
/// Core class for custom card choice nodes that implements all node-related interfaces for you. ONLY WORKS IN ACT 1.
/// </summary>
public abstract class CustomCardChoiceNodeSequencer : CardChoicesSequencer, ICustomNodeSequencer, IInherit, IDestroyOnEnd, IDoNotReturnToMapOnEnd
{
    /// <summary>
    /// Trigger the custom node sequence.
    /// </summary>
    /// <param name="node">Node that triggered this sequence.</param>
    /// <returns></returns>
    public abstract IEnumerator DoCustomSequence(CustomSpecialNodeData node);
    /// <summary>
    /// CardChoicesSequencer this should inherit. Defaults to the normal card choice.
    /// </summary>
    public virtual CardChoicesSequencer InheritTarget => EasyAccess.CardSingleChoices;
    /// <summary>
    /// Position offset from the position of the inherit target.
    /// </summary>
    public virtual Vector3 PositionOffset => Vector3.zero;
    /// <summary>
    /// Rotation offset from the rotation of the inherit target.
    /// </summary>
    public virtual Quaternion RotationOffset => Quaternion.identity;
    /// <summary>
    /// Position offset for the deck pile.
    /// </summary>
    public virtual Vector3 DeckPilePositionOffset => Vector3.zero;
    /// <summary>
    /// Rotation offset for the deck pile.
    /// </summary>
    public virtual Quaternion DeckPileRotationOffset => Quaternion.identity;
    /// <summary>
    /// True if deck pile should inherit PositionOffset and RotationOffset together with DeckPilePositionOffset and DeckPileRotationOffset. Defaults to false.
    /// </summary>
    public virtual bool DeckPileInheritsOffsets => false;
    /// <summary>
    /// True if this should inherit from InheritTarget. Defaults to true.
    /// </summary>
    public virtual bool ShouldInherit => true;
    /// <summary>
    /// Inherits this from InheritTarget, copying the deck pile, the selectable card prefab and gamepad grid.
    /// </summary>
    /// <param name="node">Node that triggered this sequence.</param>
    public virtual void Inherit(CustomSpecialNodeData node)
    {
        var target = InheritTarget;
        if (ShouldInherit && target != null)
        {
            transform.position = target.transform.position + PositionOffset;
            transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles + RotationOffset.eulerAngles);
            if (target.deckPile != null)
            {
                deckPile = Instantiate(target.deckPile, target.deckPile.transform.position, target.deckPile.transform.rotation);
                Vector3 position = deckPile.transform.position;
                Quaternion rotation = deckPile.transform.rotation;
                deckPile.transform.parent = transform;
                deckPile.transform.position = position + DeckPilePositionOffset + (DeckPileInheritsOffsets ? PositionOffset : Vector3.zero);
                deckPile.transform.rotation = Quaternion.Euler(rotation.eulerAngles + DeckPileRotationOffset.eulerAngles + (DeckPileInheritsOffsets ? RotationOffset.eulerAngles : Vector3.zero));
            }
            selectableCardPrefab = target.selectableCardPrefab;
            if (target.gamepadGrid != null)
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
    /// <summary>
    /// Returns true if this sequencer should be destroyed after ending. Defaults to false.
    /// </summary>
    /// <param name="node">Node that triggered this sequence.</param>
    /// <returns>True if this sequencer should be destroyed after ending.</returns>
    public virtual bool ShouldDestroyOnEnd(CustomSpecialNodeData node) => false;
    /// <summary>
    /// Returns true if this sequencer shouldn't return the player to the map after ending. Defaults to false.
    /// </summary>
    /// <param name="node">Node that triggered this sequence.</param>
    /// <returns>True if this sequencer shouldn't return the player to the map after ending.</returns>
    public virtual bool ShouldNotReturnToMapOnEnd(CustomSpecialNodeData node) => false;
}