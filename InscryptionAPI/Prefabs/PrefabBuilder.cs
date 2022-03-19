using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Prefabs
{
    public static class PrefabBuilder
    {
        public static GameObject BuildGameObjectPrefab(string name)
        {
            AssetBundle bundl = AssetBundle.LoadFromMemory(AssetBundleBuilder.BuildBundleFile(builtBundles.Count));
            builtBundles.Add(bundl);
            GameObject obj = bundl.LoadAsset<GameObject>("object");
            obj.name = name;
            builtObjects.Add(obj);
            return obj;
        }

        private static readonly List<AssetBundle> builtBundles = new();
        public static readonly List<GameObject> builtObjects = new();
    }
}
