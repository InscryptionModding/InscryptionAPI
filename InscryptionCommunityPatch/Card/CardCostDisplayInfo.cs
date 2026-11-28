using DiskCardGame;
using InscryptionAPI.CardCosts;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// A snapshot of the costs a card renderer can display, independent of its model or display surface.
/// </summary>
public sealed class CardCostDisplayInfo
{
    /// <summary>The number of sacrifices required.</summary>
    public int BloodCost { get; }

    /// <summary>The number of bones required.</summary>
    public int BonesCost { get; }

    /// <summary>The amount of energy required.</summary>
    public int EnergyCost { get; }

    /// <summary>The required gems, including repeated gems of the same type.</summary>
    public IReadOnlyList<GemType> GemsCost { get; }

    /// <summary>The registered custom cost definitions and their required amounts.</summary>
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
