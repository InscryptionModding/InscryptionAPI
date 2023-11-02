using System.Collections;

namespace InscryptionAPI.Encounters;

/// <summary>
/// A sequencer interface for all custom sequencers compatible with the Inscryption API
/// </summary>
public interface ICustomNodeSequence
{
    /// <summary>
    /// Executes the sequence that plays when the player enters this particular map node.
    /// </summary>
    /// <param name="nodeData">The node data object.</param>
    /// <returns>An enumeration of Unity events.</returns>
    public IEnumerator ExecuteCustomSequence(CustomNodeData nodeData);
}