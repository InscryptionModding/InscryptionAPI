using DiskCardGame;

namespace InscryptionAPI.Card;

public static partial class CardExtensions
{
    /// <summary>
    /// Gets a PlayableCards using this specific CardInfo.
    /// Inscryption often clones CardInfos and sometimes they're reused, meaning there can be more than 1 card using the same CardInfo object.
    /// </summary>
    /// <param name="cardInfo">CardInfo to check for.</param>
    /// <returns>The PlayableCard with the specified CardInfo. Checks the board, the player's hand and miscellaneous card displays.</returns>
    public static PlayableCard GetPlayableCard(this CardInfo cardInfo)
    {
        if (CostProperties.CostProperties.CardInfoToCard.TryGetValue(cardInfo, out List<WeakReference<PlayableCard>> cardList))
        {
            for (int i = cardList.Count - 1; i >= 0; i--)
            {
                if (cardList[i].TryGetTarget(out PlayableCard card) && card != null)
                    return card;

                cardList.RemoveAt(i);
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the Blood cost of a card.
    /// This function can be overridden if someone wants to inject new cost logic into a card's Blood cost.
    /// </summary>
    public static int BloodCost(this PlayableCard card)
    {
        //Debug.Log($"{card != null} {card?.Info != null} [{card?.GetType()}] {(card as DiskCardGame.Card)?.Info != null}");
        if (card && card.Info)
        {
            int originalBloodCost = CostProperties.CostProperties.OriginalBloodCost(card.Info);

            if (card.IsUsingBlueGem())
                originalBloodCost--;

            // add adjustments from temp mods
            foreach (CardModificationInfo mod in card.TemporaryMods)
                originalBloodCost += mod.bloodCostAdjustment;

            return originalBloodCost;
        }

        InscryptionAPIPlugin.Logger.LogError("[BloodCost] Couldn't find Card or CardInfo for blood cost??? How is this possible?");
        return 0;
    }

    /// <summary>
    /// Returns the Bone cost of a card.
    /// This function can be overridden if someone wants to inject new cost logic into a card's Bone cost.
    /// </summary>
    public static int BonesCost(this PlayableCard card)
    {
        if (card && card.Info)
        {
            int originalBonesCost = CostProperties.CostProperties.OriginalBonesCost(card.Info);
            if (card.IsUsingBlueGem())
                originalBonesCost--;

            // add adjustments from temp mods
            foreach (CardModificationInfo mod in card.TemporaryMods)
                originalBonesCost += mod.bonesCostAdjustment;

            return originalBonesCost;
        }

        InscryptionAPIPlugin.Logger.LogError("Couldn't find Card or CardInfo for bone cost??? How is this possible?");
        return 0;
    }

    /// <summary>
    /// Returns the Gem cost of a card as a list.
    /// This function can be overridden if someone wants to inject new cost logic into a card's Gem cost.
    /// </summary>
    public static List<GemType> GemsCost(this PlayableCard card)
    {
        if (!card || !card.Info)
            return new();

        List<CardModificationInfo> mods = card.TemporaryMods.Concat(card.Info.Mods).ToList();
        // if gems are nullified, return a new list
        if (mods.Exists((CardModificationInfo x) => x.nullifyGemsCost))
            return new List<GemType>();

        List<GemType> gemsCost = new(card.Info.gemsCost);
        foreach (CardModificationInfo mod in mods)
        {
            if (mod.addGemCost == null)
                continue;

            foreach (GemType item in mod.addGemCost)
            {
                if (!gemsCost.Contains(item))
                    gemsCost.Add(item);
            }
        }

        if (gemsCost.Count > 0 && card.IsUsingBlueGem())
            gemsCost.RemoveAt(0);

        return gemsCost;
    }

    public static bool IsGemified(this PlayableCard card) => card.Info.Gemified || card.TemporaryMods.Exists((CardModificationInfo x) => x.gemify);
    public static bool OwnerHasBlueGem(this PlayableCard card)
    {
        return card.OpponentCard ? OpponentGemsManager.Instance.HasGem(GemType.Blue) : ResourcesManager.Instance.HasGem(GemType.Blue);
    }
    public static bool IsUsingBlueGem(this PlayableCard card) => card.IsGemified() && card.OwnerHasBlueGem();
}