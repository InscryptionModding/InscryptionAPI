using InscryptionAPI.Helpers;
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
        if (mask.MaterialOverrides == null || mask.MaterialOverrides.Count == 0)
        {
            return;
        }

        MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        Material[] materials = renderer.materials;
        if (mask.MaterialOverrides.Count > materials.Length)
        {
            InscryptionAPIPlugin.Logger.LogWarning($"{maskData.Name} has been given {mask.MaterialOverrides.Count} Textures to override with but the model only has {materials.Length}!");
        }

        for (int i = 0; i < Mathf.Min(materials.Length, mask.MaterialOverrides.Count); i++)
        {
            MaterialOverride materialOverride = mask.MaterialOverrides[i];
            if (materialOverride == null)
            {
                continue;
            }

            Material material = materials[i];
            if (materialOverride.OverrideMainTexture)
            {
                OverrideMainTexture(materialOverride, material);
            }

            if (materialOverride.OverrideEmission)
            {
                OverrideEmission(materialOverride, material);
            }

            if (materialOverride.OverrideMetallicMap)
            {
                OverrideMetallicMap(materialOverride, material);
            }

            if (materialOverride.OverrideHeightMap)
            {
                OverrideHeightMap(materialOverride, material);
            }

            if (materialOverride.OverrideDetailMask)
            {
                OverrideDetailMask(materialOverride, material);
            }

            if (materialOverride.OverrideOcclusionMap)
            {
                OverrideOcclusionMap(materialOverride, material);
            }

            if (materialOverride.OverrideNormalMap)
            {
                OverrideNormalMap(materialOverride, material);
            }

            if (materialOverride.Metallic.HasValue)
            {
                OverrideMetallic(materialOverride, material);
            }

            if (materialOverride.Smoothness.HasValue)
            {
                OverrideSmoothnessValue(materialOverride, material);
            }

            if (materialOverride.Height.HasValue)
            {
                OverrideHeightValue(materialOverride, material);
            }
        }
        renderer.materials = materials;
    }

    protected virtual void OverrideHeightValue(MaterialOverride materialOverride, Material material)
    {
        material.SetFloat("_Parallax", materialOverride.Height.Value);
    }

    protected virtual void OverrideSmoothnessValue(MaterialOverride materialOverride, Material material)
    {
        material.SetFloat("_Glossiness", materialOverride.Smoothness.Value);
    }

    protected virtual void OverrideMetallic(MaterialOverride materialOverride, Material material)
    {

        material.SetFloat("_Metallic", materialOverride.Metallic.Value);
    }

    protected virtual void OverrideNormalMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_BumpMap", materialOverride.Emission);
    }

    protected virtual void OverrideOcclusionMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_OcclusionMap", materialOverride.Emission);
    }

    protected virtual void OverrideDetailMask(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_DetailMask", materialOverride.Emission);
    }

    protected virtual void OverrideHeightMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_ParallaxMap", materialOverride.Emission);
    }

    protected virtual void OverrideMetallicMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_MetallicGlossMap", materialOverride.Emission);
    }

    protected virtual void OverrideEmission(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_EmissionMap", materialOverride.Emission);
    }

    protected virtual void OverrideMainTexture(MaterialOverride materialOverride, Material material)
    {
        material.mainTexture = materialOverride.MainTexture;
    }
}
