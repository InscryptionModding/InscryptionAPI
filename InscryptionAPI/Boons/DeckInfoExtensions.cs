using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Boons
{
    public static class DeckInfoExtensions
    {
        public static void RemoveBoon(this DeckInfo self, BoonData.Type type)
        {
            self.boonIds.Remove(type);
            bool removedItem = false;
            self.Boons.RemoveAll(delegate (BoonData d)
            {
                if(d != null && d.type == type && !removedItem)
                {
                    removedItem = true;
                    return true;
                }
                return false;
            });
            List<BoonBehaviour> behaviors = BoonBehaviour.FindInstancesOfType(type);
            if(behaviors.Count > 0)
            {
                UnityEngine.Object.Destroy(behaviors[0].gameObject);
            }
        }

        public static void RemoveAllBoonsOfType(this DeckInfo self, BoonData.Type type)
        {
            self.boonIds.RemoveAll((x) => x == type);
            self.Boons.RemoveAll((x) => x != null && x.type == type);
            List<BoonBehaviour> behaviors = new List<BoonBehaviour>(BoonBehaviour.FindInstancesOfType(type));
            if (behaviors.Count > 0)
            {
                foreach (BoonBehaviour ins in behaviors)
                {
                    if (ins != null && ins.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(ins.gameObject);
                    }
                }
                BoonBehaviour.EnsureInstancesLoaded();
            }
        }
    }
}
