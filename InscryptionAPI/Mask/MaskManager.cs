using DiskCardGame;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using UnityEngine;

namespace InscryptionAPI.Masks;

public static class MaskManager
{
    public enum ModelType
    {
        Prospector = 1,
        Woodcarver = 2,
        Angler = 3,
        Trapper = 4,
        Trader = 5,
        Doctor = 6,

        FlatMask = 101,
        Sphere = 102,
    }

    public static LeshyAnimationController.Mask NoMask = (LeshyAnimationController.Mask)(-1);

    private static Dictionary<LeshyAnimationController.Mask, List<CustomMask>> MaskLookup = new();
    public static Dictionary<ModelType, ResourceLookup> TypeToPrefabLookup = new();

    public static List<CustomMask> BaseMasks = GenerateBaseMasks();
    public static List<CustomMask> CustomMasks = new List<CustomMask>();

    public static ModelType RegisterPrefab(string pluginGUID, string prefabName, ResourceLookup resource)
    {
        ModelType type = GuidManager.GetEnumValue<ModelType>(pluginGUID, prefabName);
        TypeToPrefabLookup[type] = resource;

        return type;
    }

    public static CustomMask Add(string guid, string name, string texturePath = null)
    {
        LeshyAnimationController.Mask maskType = GuidManager.GetEnumValue<LeshyAnimationController.Mask>(guid, name);

        CustomMask mask = AddCustomMask(guid, name, maskType, ModelType.FlatMask, false);
        if (!string.IsNullOrEmpty(texturePath))
        {
            MaterialOverride materialOverride = new MaterialOverride();
            materialOverride.ChangeMainTexture(TextureHelper.GetImageAsTexture(texturePath));
            mask.AddMaterialOverride(materialOverride);
        }

        return mask;
    }

    public static CustomMask AddRandom(string guid, string name, LeshyAnimationController.Mask maskType, string texturePath = null)
    {
        CustomMask mask = AddCustomMask(guid, name, maskType, ModelType.FlatMask, false);
        if (!string.IsNullOrEmpty(texturePath))
        {
            MaterialOverride materialOverride = new MaterialOverride();
            materialOverride.ChangeMainTexture(TextureHelper.GetImageAsTexture(texturePath));
            mask.AddMaterialOverride(materialOverride);
        }

        return mask;
    }

    public static CustomMask Override(string guid, string name, LeshyAnimationController.Mask maskType, string texturePath = null)
    {
        CustomMask mask = AddCustomMask(guid, name, maskType, ModelType.FlatMask, true);
        if (!string.IsNullOrEmpty(texturePath))
        {
            MaterialOverride materialOverride = new MaterialOverride();
            materialOverride.ChangeMainTexture(TextureHelper.GetImageAsTexture(texturePath));
            mask.AddMaterialOverride(materialOverride);
        }

        return mask;
    }

    /// <summary>
    /// Adds a custom mask to the game so you can tell leshy to put it on his face. Typically during a boss fight.
    /// </summary>
    /// <param name="guid">GUID of your mod</param>
    /// <param name="name">Name of the mask</param>
    /// <param name="maskType">The mask we want to add so we cna tell leshy to put on that specific mask.</param>
    /// <param name="modelType">The model the mask will use</param>
    /// <param name="isOverride"></param>
    /// <returns></returns>
    public static CustomMask AddCustomMask(string guid, string name, LeshyAnimationController.Mask maskType, ModelType modelType, bool isOverride)
    {
        CustomMask mask = new CustomMask(guid, name, maskType, isOverride);
        mask.SetModelType(modelType);

        CustomMasks.Add(mask);
        if (!MaskLookup.TryGetValue(maskType, out List<CustomMask> masks))
        {
            masks = new List<CustomMask>();
            MaskLookup[maskType] = masks;
        }
        masks.Add(mask);

        return mask;
    }

    internal static CustomMask GetRandomMask(LeshyAnimationController.Mask maskType)
    {
        if (!MaskLookup.TryGetValue(maskType, out List<CustomMask> masks))
        {
            InscryptionAPIPlugin.Logger.LogWarning("No mask defined of type: " + maskType);
            return BaseMasks[0];
        }

        if (masks.Count == 0)
        {
            InscryptionAPIPlugin.Logger.LogWarning("No masks found for type " + maskType);
            return BaseMasks[0];
        }

        CustomMask overrideMask = masks.FindLast((a) => a.Override);
        if (overrideMask == null)
        {
            // We have no overrides. Just get a random mask
            int index = UnityEngine.Random.RandomRangeInt(0, masks.Count);
            overrideMask = masks[index];
        }

        return overrideMask;
    }

    private static List<CustomMask> GenerateBaseMasks()
    {
        InitializeDefaultModel("maskFlat", "CustomMask", ModelType.FlatMask);

        List<CustomMask> list = new List<CustomMask>();
        foreach (LeshyAnimationController.Mask maskType in Enum.GetValues(typeof(LeshyAnimationController.Mask)))
        {
            ResourceLookup resourceLookup = new ResourceLookup();
            resourceLookup.FromResourceBank("Prefabs/Opponents/Leshy/Masks/Mask" + maskType);

            if (!Enum.TryParse(maskType.ToString(), out ModelType modelType))
            {
                InscryptionAPIPlugin.Logger.LogWarning("Could not get default mask for type " + maskType);
            }
            else
            {
                TypeToPrefabLookup[modelType] = resourceLookup;
            }

            CustomMask customMask = new CustomMask("", maskType.ToString(), maskType, false);
            customMask.SetModelType(modelType);

            list.Add(customMask);
            if (!MaskLookup.TryGetValue(maskType, out List<CustomMask> defaultMasks))
            {
                defaultMasks = new List<CustomMask>();
                MaskLookup[maskType] = defaultMasks;
            }
            defaultMasks.Add(customMask);
        }

        return list;
    }

    private static void InitializeDefaultModel(string assetBundlePath, string prefabName, ModelType modelType)
    {
        ResourceLookup resourceLookup = new ResourceLookup();
        resourceLookup.FromAssetBundleInAssembly<InscryptionAPIPlugin>(assetBundlePath, prefabName);

        TypeToPrefabLookup[modelType] = resourceLookup;
    }

    internal static LeshyAnimationController.Mask BossToMask(Opponent.Type opponentType)
    {
        switch (opponentType)
        {
            case Opponent.Type.ProspectorBoss:
                return LeshyAnimationController.Mask.Prospector;
            case Opponent.Type.AnglerBoss:
                return LeshyAnimationController.Mask.Angler;
            case Opponent.Type.WoodcarverBoss:
                return LeshyAnimationController.Mask.Woodcarver;
            case Opponent.Type.TrapperTraderBoss:
                return LeshyAnimationController.Mask.Trader;
            default:
                return LeshyAnimationController.Mask.Woodcarver;
        }
    }

    internal static void InitializeMaskClone(GameObject clone, CustomMask data)
    {
        if (!clone.TryGetComponent(out MaskBehaviour behaviour))
        {
            behaviour = (MaskBehaviour)clone.AddComponent(data.BehaviourType);
        }
        clone.SetActive(true);
        behaviour.Initialize(data);
    }
}