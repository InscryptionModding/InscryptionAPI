using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Items;

/// <summary>
/// Define how an asset should be retrieved so we can fetch it at any time.
/// </summary>
public class ResourceLookup : ICloneable
{
    public string AssetBundlePath { get; private set; }
    public string AssetBundlePrefabName { get; private set; }
    public string ResourcePath { get; private set; }
    public string ResourceBankID { get; private set; }
    public GameObject Prefab { get; private set; }

    public void FromAssetBundle(string assetBundlePath, string assetBundlePrefabName)
    {
        this.AssetBundlePath = assetBundlePath;
        this.AssetBundlePrefabName = assetBundlePrefabName;
    }
    
    public void FromResources(string resourcePath)
    {
        this.ResourcePath = resourcePath;
    }
    
    public void FromResourceBank(string resourceBankID)
    {
        this.ResourceBankID = resourceBankID;
    }
    
    public void FromPrefab(GameObject prefab)
    {
        this.Prefab = prefab;
    }
    
    public virtual T Get<T>() where T : UnityObject
    {
        if (!string.IsNullOrEmpty(AssetBundlePath))
        {
            byte[] resourceBytes = TextureHelper.GetResourceBytes(AssetBundlePath, typeof(InscryptionAPIPlugin).Assembly);
            if (AssetBundleHelper.TryGet(resourceBytes, AssetBundlePrefabName, out T prefab))
            {
                return prefab;
            }
            else
            {
                return null;
            }
        }

        if (!string.IsNullOrEmpty(ResourcePath))
        {
            return Resources.Load<T>(ResourcePath);
        }

        if (!string.IsNullOrEmpty(ResourceBankID))
        {
            return ResourceBank.Get<T>(ResourceBankID);
        }

        if (Prefab != null)
        {
            if (Prefab.GetType() == typeof(T))
            {
                return (T)(UnityObject)Prefab;
            }
            else if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                return Prefab.GetComponent<T>();
            }
            
            InscryptionAPIPlugin.Logger.LogError("No way to get Type " + typeof(T) + " from prefab " + Prefab.name);
            return null;
        }

        InscryptionAPIPlugin.Logger.LogError("ResourceLookup not setup correctly!");
        return default(T);
    }

    public override string ToString()
    {
        return $"ResourceLookup(AssetBundlePath:{AssetBundlePath}, AssetBundlePrefabName:{AssetBundlePrefabName}, ResourcePath:{ResourcePath})";
    }

    public object Clone()
    {
        ResourceLookup cardInfo = (ResourceLookup)base.MemberwiseClone();
        return cardInfo;
    }
}