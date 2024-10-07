using DiskCardGame;
using System.Collections;

namespace InscryptionAPI.Encounters;

/// <summary>
/// An Opponent interface that implements custom logic when the player has exhausted both of their draw piles.
/// </summary>
public interface ICustomExhaustSequence
{
    public bool RespondsToCustomExhaustSequence(CardDrawPiles drawPiles);
    /// <summary>
    /// Executes the sequence that plays when the player exhausts their draw piles.
    /// </summary>
    /// <param name="drawPiles">The CardDrawPiles instance for this scene.</param>
    /// <returns>An enumeration of Unity events.</returns>
    public IEnumerator DoCustomExhaustSequence(CardDrawPiles drawPiles);
}