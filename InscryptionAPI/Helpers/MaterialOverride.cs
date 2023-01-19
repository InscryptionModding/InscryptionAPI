using UnityEngine;

namespace InscryptionAPI.Helpers;

public class MaterialOverride
{
    public Texture2D MainTexture = null;
    public bool OverrideMainTexture = false;

    public Texture2D Emission = null;
    public bool OverrideEmission = false;

    public Texture2D NormalMap = null;
    public bool OverrideNormalMap = false;

    public Texture2D HeightMap = null;
    public bool OverrideHeightMap = false;

    public Texture2D MetallicMap = null;
    public bool OverrideMetallicMap = false;

    public Texture2D OcclusionMap = null;
    public bool OverrideOcclusionMap = false;

    public Texture2D DetailMask = null;
    public bool OverrideDetailMask = false;

    public float? Metallic = null;
    public float? Smoothness = null;
    public float? Height = null;

    public MaterialOverride ChangeMainTexture(Texture2D tex)
    {
        MainTexture = tex;
        OverrideMainTexture = true;
        return this;
    }

    public MaterialOverride ChangeEmission(Texture2D tex)
    {
        Emission = tex;
        OverrideEmission = true;
        return this;
    }

    public MaterialOverride ChangeNormalMap(Texture2D tex)
    {
        NormalMap = tex;
        OverrideNormalMap = true;
        return this;
    }

    public MaterialOverride ChangeHeightMap(Texture2D tex)
    {
        HeightMap = tex;
        OverrideHeightMap = true;
        return this;
    }

    public MaterialOverride ChangeMetallicMap(Texture2D tex)
    {
        MetallicMap = tex;
        OverrideMetallicMap = true;
        return this;
    }

    public MaterialOverride ChangeOcclusionMap(Texture2D tex)
    {
        OcclusionMap = tex;
        OverrideOcclusionMap = true;
        return this;
    }

    public MaterialOverride ChangeDetailMask(Texture2D tex)
    {
        DetailMask = tex;
        OverrideMainTexture = true;
        return this;
    }
}
