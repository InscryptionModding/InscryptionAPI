using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Masks;

/// <summary>
/// Class for implementinglogic for a custom mask and modifying its textures, emissions, etc..
/// </summary>
public class MaskBehaviour : MonoBehaviour
{
    /// <summary>
    /// The <see cref="CustomMask"/> tied to this behaviour.
    /// </summary>
    protected CustomMask maskData = null;

    /// <summary>
    /// Initialises the behaviour using the given <see cref="CustomMask"/>.
    /// </summary>
    /// <param name="mask">The <see cref="CustomMask"/> this behaviour will use.</param>
    public virtual void Initialize(CustomMask mask)
    {
        maskData = mask;
        OverrideTexture(mask);
    }

    /// <summary>
    /// Overrides the mask model's materials, if any overrides are present.
    /// </summary>
    /// <param name="mask">The <see cref="CustomMask"/> to reference for material overrides.</param>
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

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s parallax value using data the height value from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the height value to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideHeightValue(MaterialOverride materialOverride, Material material)
    {
        material.SetFloat("_Parallax", materialOverride.Height.Value);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s glossiness value using the smoothness value from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the smoothness value to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideSmoothnessValue(MaterialOverride materialOverride, Material material)
    {
        material.SetFloat("_Glossiness", materialOverride.Smoothness.Value);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s metallic value using data from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the metallic value to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideMetallic(MaterialOverride materialOverride, Material material)
    {
        material.SetFloat("_Metallic", materialOverride.Metallic.Value);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s bump map using the emission texture from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the emission texture to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideNormalMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_BumpMap", materialOverride.Emission);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s occlusion map value using the emission texture from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the emission texture to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideOcclusionMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_OcclusionMap", materialOverride.Emission);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s detail mask using the emission texture from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the emission texture to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideDetailMask(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_DetailMask", materialOverride.Emission);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s parallax map using the emission texture from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the emission texture to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideHeightMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_ParallaxMap", materialOverride.Emission);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s metallic map using the emission texture from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the emission texture to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideMetallicMap(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_MetallicGlossMap", materialOverride.Emission);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s emission map using the emission texture from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the emission texture to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideEmission(MaterialOverride materialOverride, Material material)
    {
        material.SetTexture("_EmissionMap", materialOverride.Emission);
    }

    /// <summary>
    /// Overrides the given <see cref="Material"/>'s main texture using the main texture from the override object.
    /// </summary>
    /// <param name="materialOverride">The <see cref="MaterialOverride"/> containing the main texture to override the material with.</param>
    /// <param name="material">The <see cref="Material"/> to override.</param>
    protected virtual void OverrideMainTexture(MaterialOverride materialOverride, Material material)
    {
        material.mainTexture = materialOverride.MainTexture;
    }
}
