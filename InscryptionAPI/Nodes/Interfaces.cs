using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    /// <summary>
    /// Interface for triggering node sequences.
    /// </summary>
    public interface ICustomNodeSequencer
    {
        /// <summary>
        /// Trigger the custom node sequence.
        /// </summary>
        /// <param name="nodeData">Node that triggered this sequence.</param>
        /// <returns></returns>
        public IEnumerator DoCustomSequence(CustomSpecialNodeData nodeData);
    }

    /// <summary>
    /// Interface for destroying the node sequencer at the end.
    /// </summary>
    public interface IDestroyOnEnd
    {
        /// <summary>
        /// If this returns true, the node sequencer will be destroyed at the end.
        /// </summary>
        /// <param name="nodeData">Node that triggered this sequence.</param>
        /// <returns></returns>
        public bool ShouldDestroyOnEnd(CustomSpecialNodeData nodeData);
    }

    /// <summary>
    /// Interface for not returning to the map after the node sequencer ends.
    /// </summary>
    public interface IDoNotReturnToMapOnEnd
    {
        /// <summary>
        /// If this returns true, the player will not be returned to the map after the node sequencer ends
        /// </summary>
        /// <param name="nodeData">Node that triggered this sequence.</param>
        /// <returns></returns>
        public bool ShouldNotReturnToMapOnEnd(CustomSpecialNodeData nodeData);
    }

    /// <summary>
    /// Interface for inheriting from other node sequencers and setup before the node sequence starts.
    /// </summary>
    public interface IInherit
    {
        /// <summary>
        /// Used to inherit from another node sequencer and setup everything needed for the sequence to work.
        /// </summary>
        /// <param name="nodeData">Node that triggered this sequence.</param>
        public void Inherit(CustomSpecialNodeData nodeData);
    }
}
