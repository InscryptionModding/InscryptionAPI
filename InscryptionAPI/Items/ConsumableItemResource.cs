using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Items;

public class ConsumableItemResource : ResourceLookup
{
    public Action<GameObject, ConsumableItemData> PreSetupCallback = null;
}
