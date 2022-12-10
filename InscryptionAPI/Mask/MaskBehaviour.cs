using UnityEngine;

namespace InscryptionAPI.Masks;

public class MaskBehaviour : MonoBehaviour
{
    public virtual void Initialize(CustomMask mask)
    {
        OverrideTexture(mask);
    }

    protected void OverrideTexture(CustomMask mask)
    {
        if (mask.TextureOverride == null)
        {
            return;
        }
        
        MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        Material[] materials = renderer.materials;
        materials[0].mainTexture = mask.TextureOverride;
        renderer.materials = materials;
    }
}
