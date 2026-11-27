using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace InscryptionAPI.Totems;

/// <summary>
/// Subclass of <see cref="CompositeTotemPiece"/> with extended functionalities for creating totem tops using simple texture icons.
/// </summary>
public class CustomIconTotemTopPiece : CompositeTotemPiece {
    /// <summary>
    /// The name for the game object that contains the icon renderer.
    /// </summary>
    protected virtual string IconGameObjectName => "Icon";

    /// <summary>
    /// Sets the icon and tribe data for this totem top piece.
    /// </summary>
    /// <param name="data">The data needed for this totem top to function. Must be convertable to a <see cref="TotemTopData"/>.</param>
    public override void SetData(ItemData data)
    {
        base.SetData(data);

        TotemTopData topData = (TotemTopData)data;

        // Get texture to apply
        Texture2D texture2D = TribeManager.GetTribeIcon(topData.prerequisites.tribe);

        // Populate icon
        GameObject icon = this.gameObject.FindChild(IconGameObjectName);
        if (icon != null)
        {
            emissiveRenderer = icon.GetComponent<Renderer>();
            if (emissiveRenderer != null)
            {
                emissiveRenderer.material.mainTexture = texture2D;
            }
            else
            {
                InscryptionAPIPlugin.Logger.LogError($"Could not find Renderer on Icon GameObject to assign tribe icon!");
            }
        }
        else
        {
            InscryptionAPIPlugin.Logger.LogError($"Could not find Icon GameObject to assign tribe icon or emission!");
        }
    }
}
