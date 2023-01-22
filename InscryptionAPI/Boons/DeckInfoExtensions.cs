using DiskCardGame;

namespace InscryptionAPI.Boons;

/// <summary>
/// This contains helper extension methods for the DeckInfo class
/// </summary>
public static class DeckInfoExtensions
{
    /// <summary>
    /// Removes a single boon of a given type from the deck
    /// </summary>
    /// <param name="self">The deck to remove the boon from</param>
    /// <param name="type">The type of the boon to remove</param>
    public static void RemoveBoon(this DeckInfo self, BoonData.Type type)
    {
        self.boonIds.Remove(type);
        bool removedItem = false;
        self.Boons.RemoveAll(delegate (BoonData d)
        {
            if (d != null && d.type == type && !removedItem)
            {
                removedItem = true;
                return true;
            }
            return false;
        });
        List<BoonBehaviour> behaviors = BoonBehaviour.FindInstancesOfType(type).FindAll(x => x.GetComponent<DestroyingFlag>() == null);
        if (behaviors.Count > 0)
        {
            behaviors[0].gameObject.DestroyWhenStackClear();
            BoonBehaviour.EnsureInstancesLoaded();
        }
    }

    /// <summary>
    /// Removes all boons of a give type from the deck
    /// </summary>
    /// <param name="self">The deck to remove the boons from</param>
    /// <param name="type">The type of the boons to remove</param>
    public static void RemoveAllBoonsOfType(this DeckInfo self, BoonData.Type type)
    {
        self.boonIds.RemoveAll((x) => x == type);
        self.Boons.RemoveAll((x) => x != null && x.type == type);
        List<BoonBehaviour> behaviors = new(BoonBehaviour.FindInstancesOfType(type));
        if (behaviors.Count > 0)
        {
            foreach (BoonBehaviour ins in behaviors)
            {
                if (ins != null && ins.gameObject != null)
                {
                    ins.gameObject.DestroyWhenStackClear();
                }
            }
            BoonBehaviour.EnsureInstancesLoaded();
        }
    }
}