using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Items;

/// <summary>
/// Define how an asset should be retrieved so we can fetch it at any time.
/// </summary>
public class ResourceLookup : ICloneable
{
    public string ResourcePath { get; private set; }
    public string ResourceBankID { get; private set; }
    public GameObject Prefab { get; private set; }

    public void FromAssetBundle(string assetBundlePath, string assetBundlePrefabName)
    {
        if (AssetBundleHelper.TryGet(assetBundlePath, assetBundlePrefabName, out GameObject go))
        {
            Prefab = go;
        }
    }

    public void FromAssetBundle(AssetBundle assetBundle, string assetBundlePrefabName)
    {
        if (AssetBundleHelper.TryGet(assetBundle, assetBundlePrefabName, out GameObject go))
        {
            Prefab = go;
        }
    }

    public void FromAssetBundleInAssembly<T>(string assetBundlePath, string assetBundlePrefabName)
    {
        byte[] resourceBytes = TextureHelper.GetResourceBytes(assetBundlePath, typeof(T).Assembly);
        if (AssetBundleHelper.TryGet(resourceBytes, assetBundlePrefabName, out GameObject go))
        {
            Prefab = go;
        }
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
        return $"ResourceLookup(ResourcePath:{ResourcePath}, ResourceBankID:{ResourceBankID}, Prefab:{Prefab})";
    }

    public object Clone()
    {
        ResourceLookup cardInfo = (ResourceLookup)base.MemberwiseClone();
        return cardInfo;
    }
}