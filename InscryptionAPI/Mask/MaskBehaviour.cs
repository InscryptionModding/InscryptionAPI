using UnityEngine;

namespace InscryptionAPI.Masks;

public class MaskBehaviour : MonoBehaviour
{
    protected CustomMask maskData = null;
    
    public virtual void Initialize(CustomMask mask)
    {
        maskData = mask;
        OverrideTexture(mask);
    }

    protected virtual void OverrideTexture(CustomMask mask)
    {
        if (mask.TextureOverrides == null || mask.TextureOverrides.Count == 0)
        {
            return;
        }
        
        MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        Material[] materials = renderer.materials;
        if (mask.TextureOverrides.Count > materials.Length)
        {
            InscryptionAPIPlugin.Logger.LogWarning($"{maskData.Name} has been given {mask.TextureOverrides.Count} Textures to override with but the model only has {materials.Length}!");
        }

        for (int i = 0; i < Mathf.Min(materials.Length, mask.TextureOverrides.Count); i++)
        {
            if (mask.TextureOverrides[i] != null)
            {
                materials[0].mainTexture = mask.TextureOverrides[i];
            }
        }
        renderer.materials = materials;
    }
}
