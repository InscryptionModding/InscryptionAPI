using DiskCardGame;
using InscryptionAPI.CardCosts;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// A snapshot of the costs a card renderer can display, independent of its model or display surface.
/// </summary>
public sealed class CardCostDisplayInfo
{
    public int BloodCost { get; }
    public int BonesCost { get; }
    public int EnergyCost { get; }
    public IReadOnlyList<GemType> GemsCost { get; }
    public IReadOnlyList<(CardCostManager.FullCardCost Definition, int Amount)> CustomCosts { get; }

    internal CardCostDisplayInfo(int blood, int bones, int energy, List<GemType> gems,
        IReadOnlyList<(CardCostManager.FullCardCost Definition, int Amount)> customCosts)
    {
        BloodCost = blood;
        BonesCost = bones;
        EnergyCost = energy;
        GemsCost = Array.AsReadOnly(gems.ToArray());
        CustomCosts = customCosts;
    }
}
