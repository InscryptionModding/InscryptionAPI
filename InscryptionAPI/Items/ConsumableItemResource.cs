using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Items;

/// <summary>
/// Subclass of <see cref="ResourceLookup"/> specific to resources used as consumable item models.
/// </summary>
public class ConsumableItemResource : ResourceLookup
{
    /// <summary>
    /// Action used by the API when setting up custom item models.
    /// </summary>
    public Action<GameObject, ConsumableItemData> PreSetupCallback = null;
}
