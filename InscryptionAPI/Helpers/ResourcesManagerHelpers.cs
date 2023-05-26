using DiskCardGame;
using InscryptionAPI.Card;

namespace InscryptionAPI.Helpers;

public static class ResourcesManagerHelpers
{
    public static bool OwnerHasGems(bool playerGems, params GemType[] gems)
    {
        if (playerGems)
            return PlayerHasGems(gems);
        else
            return OpponentHasGems(gems);
    }
    public static bool OpponentHasGems(params GemType[] gems)
    {
        var component = ResourcesManager.Instance.GetComponent<OpponentGemsManager>();
        if (component == null)
            return false;

        foreach (GemType gem in gems)
        {
            if (!component.opponentGems.Contains(gem))
                return false;
        }
        return true;
    }
    public static bool PlayerHasGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
        {
            if (!ResourcesManager.Instance.gems.Contains(gem))
                return false;
        }
        return true;
    }
}