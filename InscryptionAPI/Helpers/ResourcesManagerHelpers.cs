using DiskCardGame;
using InscryptionAPI.Card;

namespace InscryptionAPI.Helpers;

public static class ResourcesManagerHelpers
{
    public static int GemsOfType(this ResourcesManager instance, GemType gem)
    {
        return instance.gems.Count(x => x == gem);
    }
    public static int GemCount(bool playerGems, GemType gemToCheck)
    {
        if (playerGems)
            return ResourcesManager.Instance.GemsOfType(gemToCheck);
        else
            return OpponentGemsManager.Instance.GemsOfType(gemToCheck);
    }
    public static bool OwnerHasGems(bool playerGems, params GemType[] gems)
    {
        if (playerGems)
            return PlayerHasGems(gems);
        else
            return OpponentHasGems(gems);
    }
    public static bool OpponentHasGems(params GemType[] gems)
    {
        if (OpponentGemsManager.Instance == null)
            return false;

        foreach (GemType gem in gems)
        {
            if (!OpponentGemsManager.Instance.HasGem(gem))
                return false;
        }
        return true;
    }
    public static bool PlayerHasGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
        {
            if (!ResourcesManager.Instance.HasGem(gem))
                return false;
        }
        return true;
    }
}