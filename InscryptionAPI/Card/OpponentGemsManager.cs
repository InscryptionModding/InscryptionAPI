using DiskCardGame;

namespace InscryptionAPI.Card;
public class OpponentGemsManager : Singleton<OpponentGemsManager>
{
    public List<GemType> opponentGems = new();

    public bool HasGem(GemType gem) => opponentGems.Contains(gem);
    public void AddGem(GemType gem) => opponentGems.Add(gem);
    public void LoseGem(GemType gem) => opponentGems.Remove(gem);
    public void AddGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
            opponentGems.Add(gem);
    }
    public void LoseGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
            opponentGems.Remove(gem);
    }

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