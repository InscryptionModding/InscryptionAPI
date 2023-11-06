using System.Collections;

namespace InscryptionAPI.Nodes;

/// <summary>
/// Core class for miscellaneous non-card related nodes that implements all node-related interfaces for you.
/// </summary>
public abstract class CustomNodeSequencer : ManagedBehaviour, ICustomNodeSequencer, IInherit, IDestroyOnEnd, IDoNotReturnToMapOnEnd
{
    /// <summary>
    /// Trigger the custom node sequence.
    /// </summary>
    /// <param name="node">Node that triggered this sequence.</param>
    /// <returns>.</returns>
    public abstract IEnumerator DoCustomSequence(CustomSpecialNodeData node);
    /// <summary>
    /// Used to inherit from another node sequencer and setup everything needed for the sequence to work.
    /// </summary>
    /// <param name="node">Node that triggered this sequence.</param>
    public virtual void Inherit(CustomSpecialNodeData node) { }
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