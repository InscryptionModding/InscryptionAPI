using UnityEngine;

namespace InscryptionAPI.Helpers;

/// <summary>
/// A Helper Object for Materials on an Object.
/// </summary>
public class MaterialOverride
{
    /// <summary>
    /// The Main Texture as a <see cref="Texture2D"/>.
    /// </summary>
    public Texture2D MainTexture = null;
    /// <summary>
    /// Whether the Main Texture should be Overwritten or not.
    /// </summary>
    public bool OverrideMainTexture = false;
    /// <summary>
    /// The Emission Texture as a <see cref="Texture2D"/>.
    /// </summary>
    public Texture2D Emission = null;
    /// <summary>
    /// Whether the Emission Texture should be Overwritten or not.
    /// </summary>
    public bool OverrideEmission = false;
    /// <summary>
    /// The Normal Map as a <see cref="Texture2D"/>.
    /// </summary>
    public Texture2D NormalMap = null;
    /// <summary>
    /// Whether the Normal Map should be Overwritten or not.
    /// </summary>
    public bool OverrideNormalMap = false;
    /// <summary>
    /// The Height Map as a <see cref="Texture2D"/>.
    /// </summary>
    public Texture2D HeightMap = null;
    /// <summary>
    /// Whether the Height Map should be Overwritten or not.
    /// </summary>
    public bool OverrideHeightMap = false;
    /// <summary>
    /// The Metallic Map as a <see cref="Texture2D"/>.
    /// </summary>
    public Texture2D MetallicMap = null;
    /// <summary>
    /// Whether the Metallic Map should be Overwritten or not.
    /// </summary>
    public bool OverrideMetallicMap = false;
    /// <summary>
    /// The Occlusion Map as a <see cref="Texture2D"/>.
    /// </summary>
    public Texture2D OcclusionMap = null;
    /// <summary>
    /// Whether the Occlusion Map should be Overwritten or not.
    /// </summary>
    public bool OverrideOcclusionMap = false;
    /// <summary>
    /// The Detail Mask as a <see cref="Texture2D"/>.
    /// </summary>
    public Texture2D DetailMask = null;
    /// <summary>
    /// Whether the Detail Mask should be Overwritten or not.
    /// </summary>
    public bool OverrideDetailMask = false;
    /// <summary>
    ///  The Metallic Surface Property. How Metallic should the Material be?
    /// </summary>
    public float? Metallic = null;
    /// <summary>
    /// Whether the Metallic Value should be Overwritten or not.
    /// </summary>
    public bool OverrideMetallic = false;
    /// <summary>
    /// The Surface Smoothness Property. How Smooth should the Material be?
    /// </summary>
    public float? Smoothness = null;
    /// <summary>
    /// Whether the Smoothness Value should be Overwritten or not.
    /// </summary>
    public bool OverrideSmoothness = false;
    /// <summary>
    /// The Displacement Height Property. How displaced should the Material be compared to the Model it is applied to?
    /// </summary>
    public float? Height = null;
    /// <summary>
    /// Whether the Height Value should be Overwritten or not.
    /// </summary>
    public bool OverrideHeight = false;

    /// <summary>
    /// A function for changing the <see cref="MainTexture"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="tex">The <see cref="Texture2D"/> in which to use to replace <see cref="MainTexture"/>.</param>
    /// <returns>An Overwritten <see cref="MainTexture"/> and <see cref="OverrideMainTexture"/> set to true.</returns>
    public MaterialOverride ChangeMainTexture(Texture2D tex)
    {
        MainTexture = tex;
        OverrideMainTexture = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="Emission"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="tex">The <see cref="Texture2D"/> in which to use to replace <see cref="Emission"/>.</param>
    /// <returns>An Overwritten <see cref="Emission"/> and <see cref="OverrideEmission"/> set to true.</returns>
    public MaterialOverride ChangeEmission(Texture2D tex)
    {
        Emission = tex;
        OverrideEmission = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="NormalMap"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="tex">The <see cref="Texture2D"/> in which to use to replace <see cref="NormalMap"/>.</param>
    /// <returns>An Overwritten <see cref="NormalMap"/> and <see cref="OverrideNormalMap"/> set to true.</returns>
    public MaterialOverride ChangeNormalMap(Texture2D tex)
    {
        NormalMap = tex;
        OverrideNormalMap = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="HeightMap"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="tex">The <see cref="Texture2D"/> in which to use to replace <see cref="HeightMap"/>.</param>
    /// <returns>An Overwritten <see cref="HeightMap"/> and <see cref="OverrideHeightMap"/> set to true.</returns>
    public MaterialOverride ChangeHeightMap(Texture2D tex)
    {
        HeightMap = tex;
        OverrideHeightMap = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="MetallicMap"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="tex">The <see cref="Texture2D"/> in which to use to replace <see cref="MetallicMap"/>.</param>
    /// <returns>An Overwritten <see cref="MetallicMap"/> and <see cref="OverrideMetallicMap"/> set to true.</returns>
    public MaterialOverride ChangeMetallicMap(Texture2D tex)
    {
        MetallicMap = tex;
        OverrideMetallicMap = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="OcclusionMap"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="tex">The <see cref="Texture2D"/> in which to use to replace <see cref="OcclusionMap"/>.</param>
    /// <returns>An Overwritten <see cref="OcclusionMap"/> and <see cref="OverrideOcclusionMap"/> set to true.</returns>
    public MaterialOverride ChangeOcclusionMap(Texture2D tex)
    {
        OcclusionMap = tex;
        OverrideOcclusionMap = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="DetailMask"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="tex">The <see cref="Texture2D"/> in which to use to replace <see cref="DetailMask"/>.</param>
    /// <returns>An Overwritten <see cref="DetailMask"/> and <see cref="OverrideDetailMask"/> set to true.</returns>
    public MaterialOverride ChangeDetailMask(Texture2D tex)
    {
        DetailMask = tex;
        OverrideMainTexture = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="Metallic"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="metallic">The <see cref="float"/> in which to use to replace <see cref="Metallic"/>.</param>
    /// <returns>An Overwritten <see cref="Metallic"/> and <see cref="OverrideMetallic"/> set to true.</returns>
    public MaterialOverride ChangeMetallic(float metallic)
    {
        Metallic = metallic;
        OverrideMetallic = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="Smoothness"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="smoothness">The <see cref="float"/> in which to use to replace <see cref="Smoothness"/>.</param>
    /// <returns>An Overwritten <see cref="Smoothness"/> and <see cref="OverrideSmoothness"/> set to true.</returns>
    public MaterialOverride ChangeSmoothness(float smoothness)
    {
        Smoothness = smoothness;
        OverrideSmoothness = true;
        return this;
    }
    /// <summary>
    /// A function for changing the <see cref="Height"/> of a <see cref="MaterialOverride"/>.
    /// </summary>
    /// <param name="height">The <see cref="float"/> in which to use to replace <see cref="Height"/>.</param>
    /// <returns>An Overwritten <see cref="Height"/> and <see cref="OverrideHeight"/> set to true.</returns>
    public MaterialOverride ChangeHeight(float height)
    {
        Height = height;
        OverrideHeight = true;
        return this;
    }
}
