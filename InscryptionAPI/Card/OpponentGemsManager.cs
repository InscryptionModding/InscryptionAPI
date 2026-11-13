using DiskCardGame;

namespace InscryptionAPI.Card;
/// <summary>
/// Class for handling gems currently possessed by the opponent.
/// Allows for abilities and other behaviours that rely on owned gems to correctly work on both sides of the board.
/// </summary>
public class OpponentGemsManager : Singleton<OpponentGemsManager> {
    /// <summary>
    /// List of gems currently owned by the opponent.
    /// </summary>
    public List<GemType> opponentGems = new();

    /// <summary>
    /// How many of a certain gem type the opponent possesses.
    /// </summary>
    /// <param name="gem">The gem type to search for.</param>
    /// <returns>How many of the given gem type are owned by the opponent</returns>
    public int GemsOfType(GemType gem) => opponentGems.Count(x => x == gem);
    /// <summary>
    /// Determines if the opponent owns a given gem.
    /// </summary>
    /// <param name="gem">The gem type to look for.</param>
    /// <returns>True if the opponent owns at least one gem of the given type. False otherwise.</returns>
    public bool HasGem(GemType gem) => opponentGems.Contains(gem);
    /// <summary>
    /// Adds a gem of the given type to the List of opponent-owned gems.
    /// </summary>
    /// <param name="gem">Type of gem to add to the opponent's possession.</param>
    public void AddGem(GemType gem) => opponentGems.Add(gem);
    /// <summary>
    /// Removes a gem of the given type from the List of opponent-owned gems.
    /// </summary>
    /// <param name="gem">Type of gem to remove from the opponent's possession.</param>
    public void LoseGem(GemType gem) => opponentGems.Remove(gem);
    /// <summary>
    /// Adds gems of the given types to the List of opponent-owned gems.
    /// </summary>
    /// <param name="gems">Types of gems to add to the opponent's possession.</param>
    public void AddGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
            opponentGems.Add(gem);
    }
    /// <summary>
    /// Removes gems of the given types from the List of opponent-owned gems.
    /// </summary>
    /// <param name="gems">Types of gems to remove from the opponent's possession.</param>
    public void LoseGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
            opponentGems.Remove(gem);
    }

    /// <summary>
    /// Forcefully updates the list of opponent-owned gems based on gems owned on the opponent's side of the board.
    /// </summary>
    public void ForceGemsUpdate()
    {
        opponentGems.Clear();
        foreach (CardSlot slot in Singleton<BoardManager>.Instance.GetSlots(getPlayerSlots: false))
        {
            if (slot.Card != null && !slot.Card.Dead)
            {
                GainGem[] components = slot.Card.GetComponents<GainGem>();
                foreach (GainGem gainGem in components)
                    opponentGems.Add(gainGem.Gem);

                GainGemTriple[] components2 = slot.Card.GetComponents<GainGemTriple>();
                for (int i = 0; i < components2.Length; i++)
                {
                    _ = components2[i];
                    opponentGems.Add(GemType.Green);
                    opponentGems.Add(GemType.Orange);
                    opponentGems.Add(GemType.Blue);
                }
            }
        }
    }
}
