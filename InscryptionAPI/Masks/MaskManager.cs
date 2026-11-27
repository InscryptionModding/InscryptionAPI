using DiskCardGame;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using UnityEngine;

namespace InscryptionAPI.Masks;

/// <summary>
/// Manager class for managing custom masks added through the API.
/// </summary>
public static class MaskManager
{
    /// <summary>
    /// IDs of all mask models.
    /// </summary>
    public enum ModelType {
        /// <summary>
        /// Mask worn during the Prospector fight.
        /// </summary>
        Prospector = 1,
        /// <summary>
        /// Mask worn during the Woodcarver node.
        /// </summary>
        Woodcarver = 2,
        /// <summary>
        /// Mask worn during the Angler fight.
        /// </summary>
        Angler = 3,
        /// <summary>
        /// Mask worn during the Trapper node and first phase of the Trapper-Trader fight.
        /// </summary>
        Trapper = 4,
        /// <summary>
        /// Mask worn during the Trader node and the second phase of the Trapper-Trader fight.
        /// </summary>
        Trader = 5,
        /// <summary>
        /// Mask worn during the Mycologist node.
        /// </summary>
        Doctor = 6,

        /// <summary>
        /// API-added mask representing a flat nondescript face.
        /// </summary>
        FlatMask = 101,
        /// <summary>
        /// API-added mask representing a nondescript sphere.
        /// </summary>
        Sphere = 102,
    }

    /// <summary>
    /// <see cref="LeshyAnimationController.Mask"/> ID for when Leshy should have no mask.
    /// </summary>
    public static LeshyAnimationController.Mask NoMask = (LeshyAnimationController.Mask)(-1);

    private static Dictionary<LeshyAnimationController.Mask, List<CustomMask>> MaskLookup = new();
    /// <summary>
    /// Dictionary that maps new mask model IDs to their prefab <see cref="ResourceLookup"/> object.
    /// </summary>
    public static Dictionary<ModelType, ResourceLookup> TypeToPrefabLookup = new();

    /// <summary>
    /// All processed vanilla masks.
    /// </summary>
    public static List<CustomMask> BaseMasks = GenerateBaseMasks();
    /// <summary>
    /// All processed mod-added masks.
    /// </summary>
    public static List<CustomMask> CustomMasks = new List<CustomMask>();

    /// <summary>
    /// Registers a new mask prefab with the API for use when creating custom masks.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the mod adding the mask prefab.</param>
    /// <param name="prefabName">The internal name of the mask prefab.</param>
    /// <param name="resource">The <see cref="ResourceLookup"/> object containing the data needed to find and load the mask prefab.</param>
    /// <returns>The ID of the newly registered prefab for use when creating masks.</returns>
    public static ModelType RegisterPrefab(string pluginGUID, string prefabName, ResourceLookup resource)
    {
        ModelType type = GuidManager.GetEnumValue<ModelType>(pluginGUID, prefabName);
        TypeToPrefabLookup[type] = resource;

        return type;
    }

    /// <summary>
    /// Creates a simple flat mask with a texture on its front and adds it to the API.
    /// </summary>
    /// <param name="guid">The GUID of the mod adding this mask.</param>
    /// <param name="name">The internal name of this mask.</param>
    /// <param name="texturePath">The file path of the texture to display on front of this mask.</param>
    /// <returns>The <see cref="CustomMask"/> object containing all the data of this mask.</returns>
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

    /// <summary>
    /// Creates a simple flat mask with a texture on its front and adds it to the API as a random option that can appear when selecting masks with the same ID.
    /// </summary>
    /// <param name="guid">The GUID of the mod adding this mask.</param>
    /// <param name="name">The internal name of this mask.</param>
    /// <param name="maskType">The ID of this mask. This mask will be added to the pool of masks with this same ID and may be randomly selected.</param>
    /// <param name="texturePath">The file path of the texture to display on front of this mask.</param>
    /// <returns>The <see cref="CustomMask"/> object containing all the data of this mask.</returns>
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

    /// <summary>
    /// Adds a simple mask that overrides masks with the given ID.
    /// <br/>
    /// The mask will use the <see cref="ModelType.FlatMask"/> model type, which will display the given texture on its front.
    /// </summary>
    /// <param name="guid">The GUID of the mod adding this mask.</param>
    /// <param name="name">The internal name of this mask.</param>
    /// <param name="maskType">The ID of masks to override with this mask.</param>
    /// <param name="texturePath">The file path of the texture to display on front of this mask.</param>
    /// <returns>The <see cref="CustomMask"/> object containing all the data of this mask.</returns>
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
    /// <param name="guid">GUID of your mod.</param>
    /// <param name="name">Name of the mask.</param>
    /// <param name="maskType">The mask we want to add so we can tell leshy to put on that specific mask.</param>
    /// <param name="modelType">The model the mask will use.</param>
    /// <param name="isOverride">.</param>
    /// <returns>.</returns>
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