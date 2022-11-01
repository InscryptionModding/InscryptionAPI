using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Items;

public class ResourceLookup : ICloneable
{
    public string AssetBundlePath = null;
    public string AssetBundlePrefabName = null;
    public string ResourcePath = null;
    public string ResourceBankID = null;
    public GameObject Prefab = null;

    public Action<UnityObject> PreSetupCallback = null;

    public object Clone()
    {
        ResourceLookup cardInfo = (ResourceLookup)base.MemberwiseClone();
        return cardInfo;
    }
    
    public T Get<T>() where T : UnityObject
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
}