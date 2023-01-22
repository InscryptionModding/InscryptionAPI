using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace InscryptionAPI.Totems;

public class CustomIconTotemTopPiece : CompositeTotemPiece
{
    protected virtual string IconGameObjectName => "Icon";

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
