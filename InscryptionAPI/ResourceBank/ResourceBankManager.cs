using HarmonyLib;
using Sirenix.Utilities;

namespace DiskCardGame;

public static class ResourceBankManager
{
    private class ResourceData
    {
        public ResourceBank.Resource Resource;
        public bool OverrideExistingResource;
    }
    
    private static List<ResourceData> customResources = new List<ResourceData>();

    public static void AddResource(ResourceBank.Resource resource, bool overrideExistingAsset=false)
    {
        if (resource == null)
        {
            InscryptionAPI.InscryptionAPIPlugin.Logger.LogError("Cannot add null resources!");
        }
        else if (string.IsNullOrEmpty(resource.path))
        {
            InscryptionAPI.InscryptionAPIPlugin.Logger.LogWarning($"Attempting to add resource with empty path! '{resource.path}' and asset {resource.asset}");
        }
        else
        {
            customResources.Add(new ResourceData()
            {
                Resource = resource,
                OverrideExistingResource = overrideExistingAsset
            });
        }
    }
    
    [HarmonyPatch(typeof (ResourceBank), "Awake", new System.Type[] {})]
    public class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
            Dictionary<string, ResourceBank.Resource> existingPaths = __instance.resources.ToDictionary((a)=>a.path, (a)=>a);
            foreach (ResourceData resourceData in customResources)
            {
                string resourcePath = resourceData.Resource.path;
                if (existingPaths.TryGetValue(resourcePath, out ResourceBank.Resource resource) && !resourceData.OverrideExistingResource)
                {
                    if (resourceData.OverrideExistingResource)
                    {
                        resource.asset = resourceData.Resource.asset;
                        continue;
                    }
                    else
                    {
                        InscryptionAPI.InscryptionAPIPlugin.Logger.LogWarning("Resource added with that already exists! " + resourcePath);
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
