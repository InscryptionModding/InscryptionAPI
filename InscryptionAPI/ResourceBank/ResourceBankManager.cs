using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Resource;

public static class ResourceBankManager
{
    public class ResourceData
    {
        public string PluginGUID;
        public ResourceBank.Resource Resource;
        public bool OverrideExistingResource;
    }

    private static readonly List<ResourceData> CustomResources = new();

    public static ResourceData AddDecal(string pluginGUID, string resourceName, Texture decalTexture, bool overrideExistingAsset = false)
    {
        return Add(pluginGUID, new ResourceBank.Resource()
        {
            path = $"Art/Cards/Decals/{resourceName}",
            asset = decalTexture
        }, overrideExistingAsset);
    }

    public static ResourceData Add(string pluginGUID, string path, UnityObject unityObject, bool overrideExistingAsset = false)
    {
        return Add(pluginGUID, new ResourceBank.Resource()
        {
            path = path,
            asset = unityObject
        }, overrideExistingAsset);
    }

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

    [HarmonyPatch(typeof(ResourceBank), "Awake", new Type[] { })]
    public class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
            Dictionary<string, ResourceBank.Resource> existingPaths = new Dictionary<string, ResourceBank.Resource>();
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
