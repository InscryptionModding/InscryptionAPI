using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Resource;

/// <summary>
/// Manager class for adding new resources to the <see cref="ResourceBank"/>.
/// </summary>
public static class ResourceBankManager {
    /// <summary>
    /// Utility class containing all data need to add and use custom resources.
    /// </summary>
    public class ResourceData
    {
        /// <summary>
        /// The GUID of the mod adding this resource.
        /// </summary>
        public string PluginGUID;
        /// <summary>
        /// The description object for this resource.
        /// </summary>
        public ResourceBank.Resource Resource;
        /// <summary>
        /// If this resource is overriding an existing resource.
        /// </summary>
        public bool OverrideExistingResource;
    }

    private static readonly List<ResourceData> CustomResources = new();

    /// <summary>
    /// Adds a new resource to the API that can be retrieved from the <see cref="ResourceBank"/>.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the mod adding the resource.</param>
    /// <param name="path">The path the resource will be located at in the <see cref="ResourceBank"/>.</param>
    /// <param name="unityObject">The <see cref="UnityObject"/> that will be stored as a resource.</param>
    /// <param name="overrideExistingAsset">If the resource will override an existing resource asset.</param>
    /// <returns>The <see cref="ResourceData"/> for the newly created resource.</returns>
    public static ResourceData Add(string pluginGUID, string path, UnityObject unityObject, bool overrideExistingAsset = false)
    {
        return Add(pluginGUID, new ResourceBank.Resource()
        {
            path = path,
            asset = unityObject
        }, overrideExistingAsset);
    }

    /// <summary>
    /// Adds a new resource to the API that can be retrieved from the <see cref="ResourceBank"/>.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the mod adding the resource.</param>
    /// <param name="resource">The resource to add to the <see cref="ResourceBank"/>.</param>
    /// <param name="overrideExistingAsset">If the resource will override an existing resource asset.</param>
    /// <returns>The <see cref="ResourceData"/> for the newly created resource, or null if <paramref name="resource"/> is null.</returns>
    public static ResourceData Add(string pluginGUID, ResourceBank.Resource resource, bool overrideExistingAsset = false)
    {
        if (resource == null)
        {
            InscryptionAPIPlugin.Logger.LogError(pluginGUID + " cannot add null resources!");
            return null;
        }
        if (string.IsNullOrEmpty(resource.path))
        {
            InscryptionAPIPlugin.Logger.LogError($"{pluginGUID} Attempting to add resource with empty path! '{resource.path}' and asset {resource.asset}");
            return null;
        }

        ResourceData resourceData = new()
        {
            PluginGUID = pluginGUID,
            Resource = resource,
            OverrideExistingResource = overrideExistingAsset
        };

        CustomResources.Add(resourceData);
        return resourceData;
    }

    /// <summary>
    /// Adds a custom <see cref="GameObject"/> resource located at the path Inscryption uses to retrieve Prefabs when instantiating weights for the scales.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the plugin adding the resource.</param>
    /// <param name="resourceName">The name used to identify the resource. Used when retrieving it from the <see cref="ResourceBank"/>.</param>
    /// <param name="prefab">The scales weight <see cref="GameObject"/>.</param>
    /// <param name="overrideExistingAsset">If we should override any existing asset that shares this resource's path.</param>
    public static ResourceData AddScaleWeightPrefab(string pluginGUID, string resourceName, GameObject prefab, bool overrideExistingAsset = false) {
        return Add(pluginGUID, "Prefabs/Environment/ScaleWeights/" + resourceName, prefab, overrideExistingAsset);
    }

    /// <summary>
    /// Adds a custom <see cref="GameObject"/> resource located at the path Inscryption uses to retrieve Prefabs when instantiating card battle idle events, eg, the spider.
    /// </summary>
    /// <remarks>
    /// Note: Object must have a <see cref="CardBattleIdleEvent"/> component attached.
    /// </remarks>
    /// <param name="pluginGUID">The GUID of the plugin adding the resource.</param>
    /// <param name="resourceName">The name used to identify the resource. Used when retrieving it from the <see cref="ResourceBank"/>.</param>
    /// <param name="eventPrefab">The <see cref="GameObject"/> for the <see cref="CardBattleIdleEvent"/>.</param>
    /// <param name="overrideExistingAsset">If we should override any existing asset that shares this resource's path.</param>
    public static ResourceData AddCardBattleIdleEvent(string pluginGUID, string resourceName, GameObject eventPrefab, bool overrideExistingAsset = false) {
        return Add(pluginGUID, "Prefabs/Environment/CardBattleIdleEvents/" + resourceName, eventPrefab, overrideExistingAsset);
    }

    /// <summary>
    /// Adds a custom <see cref="GameObject"/> resource located at the path Inscryption uses to retrieve Prefabs when instantiating first person animations.
    /// </summary>
    /// <remarks>
    /// Note: The game checks the Prefab's children for the <see cref="Animator"/> and <see cref="FirstPersonAnimatorObject"/> components.
    /// </remarks>
    /// <param name="pluginGUID">The GUID of the plugin adding the resource.</param>
    /// <param name="resourceName">The name used to identify the resource. Used when retrieving it from the <see cref="ResourceBank"/>.</param>
    /// <param name="animPrefab">The <see cref="GameObject"/> for the first person animation.</param>
    /// <param name="overrideExistingAsset">If we should override any existing asset that shares this resource's path.</param>
    public static ResourceData AddFirstPersonAnimation(string pluginGUID, string resourceName, GameObject animPrefab, bool overrideExistingAsset = false) {
        return Add(pluginGUID, "Prefabs/FirstPersonAnimations/" + resourceName, animPrefab, overrideExistingAsset);
    }

    /// <summary>
    /// Adds a custom <see cref="GameObject"/> resource located at the path Inscryption uses to retrieve Prefabs when instantiating table effects, ie, during boss fights.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the plugin adding the resource.</param>
    /// <param name="resourceName">The name used to identify the resource. Used when retrieving it from the <see cref="ResourceBank"/>.</param>
    /// <param name="tableEffectPrefab">The <see cref="GameObject"/> for the table effect.</param>
    /// <param name="overrideExistingAsset">If we should override any existing asset that shares this resource's path.</param>
    public static ResourceData AddTableEffect(string pluginGUID, string resourceName, GameObject tableEffectPrefab, bool overrideExistingAsset = false) {
        return Add(pluginGUID, "Prefabs/Environment/TableEffects/" + resourceName, tableEffectPrefab, overrideExistingAsset);
    }

    /// <summary>
    /// Adds a custom <see cref="GameObject"/> resource located at the path Inscryption uses to retrieve Prefabs when generating map scenery.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the plugin adding the resource.</param>
    /// <param name="resourceName">The name used to identify the resource. Used when retrieving it from the <see cref="ResourceBank"/>.</param>
    /// <param name="sceneryPrefab">The map scenery <see cref="GameObject"/>.</param>
    /// <param name="overrideExistingAsset">If we should override any existing asset that shares this resource's path.</param>
    public static ResourceData AddMapScenery(string pluginGUID, string resourceName, GameObject sceneryPrefab, bool overrideExistingAsset = false) {
        return Add(pluginGUID, SceneryData.PREFABS_ROOT + resourceName, sceneryPrefab, overrideExistingAsset);
    }

    /// <summary>
    /// Adds a custom Texture resource located at the path Inscryption uses to retrieve ability icons.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the plugin adding the resource.</param>
    /// <param name="resourceName">The name used to identify the resource. Used when retrieving it from the <see cref="ResourceBank"/>.</param>
    /// <param name="iconTexture">The ability icon's texture.</param>
    /// <param name="overrideExistingAsset">If we should override any existing asset that shares this resource's path.</param>
    public static ResourceData AddAbilityIcon(string pluginGUID, string resourceName, Texture iconTexture, bool overrideExistingAsset = false) {
        return Add(pluginGUID, "Art/Cards/AbilityIcons/" + resourceName, iconTexture, overrideExistingAsset);
    }

    /// <summary>
    /// Adds a custom Texture resource located at the path Inscryption uses to retrieve card decal textures.
    /// </summary>
    /// <param name="pluginGUID">The GUID of the plugin adding the resource.</param>
    /// <param name="resourceName">The name used to identify the resource. Used when retrieving it from the <see cref="ResourceBank"/>.</param>
    /// <param name="decalTexture">The decal texture.</param>
    /// <param name="overrideExistingAsset">If we should override any existing asset that shares this resource's path.</param>
    public static ResourceData AddDecal(string pluginGUID, string resourceName, Texture decalTexture, bool overrideExistingAsset = false) {
        return Add(pluginGUID, "Art/Cards/Decals/" + resourceName, decalTexture, overrideExistingAsset);
    }

    [HarmonyPatch(typeof(ResourceBank), "Awake", new Type[] { })]
    internal class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
            Dictionary<string, ResourceBank.Resource> existingPaths = new();
            foreach (ResourceBank.Resource resource in __instance.resources)
            {
                string resourcePath = resource.path;
                if (!existingPaths.ContainsKey(resourcePath))
                    existingPaths[resourcePath] = resource;
            }

            foreach (ResourceData resourceData in CustomResources)
            {
                string resourcePath = resourceData.Resource.path;
                if (existingPaths.TryGetValue(resourcePath, out ResourceBank.Resource resource))
                {
                    if (resourceData.OverrideExistingResource)
                    {
                        resource.asset = resourceData.Resource.asset;
                        continue;
                    }
                    else
                    {
                        InscryptionAPIPlugin.Logger.LogWarning($"Cannot add new resource at path {resourcePath} because it already exists with asset {resource.asset}!");
                    }
                }
                else
                {
                    existingPaths[resourcePath] = resourceData.Resource;
                }
                __instance.resources.Add(resourceData.Resource);
            }
        }
    }
}
