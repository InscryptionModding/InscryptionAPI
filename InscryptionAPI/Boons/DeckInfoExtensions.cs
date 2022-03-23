using DiskCardGame;

namespace InscryptionAPI.Boons;

public static class DeckInfoExtensions
{
    public static void RemoveBoon(this DeckInfo self, BoonData.Type type)
    {
        self.boonIds.Remove(type);
        bool removedItem = false;
        self.Boons.RemoveAll(delegate (BoonData boonData)
        {
            if(boonData && boonData.type == type && !removedItem)
            {
                removedItem = true;
                return true;
            }
            return false;
        });
        List<BoonBehaviour> behaviors = BoonBehaviour.FindInstancesOfType(type);
        if(behaviors.Count > 0)
        {
            UnityObject.Destroy(behaviors[0].gameObject);
        }
    }

    public static void RemoveAllBoonsOfType(this DeckInfo self, BoonData.Type type)
    {
        self.boonIds.RemoveAll((x) => x == type);
        self.Boons.RemoveAll((boonData) => boonData && boonData.type == type);
        List<BoonBehaviour> behaviors = new List<BoonBehaviour>(BoonBehaviour.FindInstancesOfType(type)).Where(bh => bh && bh.gameObject).ToList();
        if (behaviors.Count > 0)
        {
            foreach (BoonBehaviour ins in behaviors)
            {
                UnityObject.Destroy(ins.gameObject);
            }
            BoonBehaviour.EnsureInstancesLoaded();
        }
    }
}