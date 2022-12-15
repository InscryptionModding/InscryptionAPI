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
    
    public static CustomMask AddCustomMask<T>(string guid, string name, ModelType modelType, string textureOverride=null) where T : MaskBehaviour
    {
        LeshyAnimationController.Mask maskType = GuidManager.GetEnumValue<LeshyAnimationController.Mask>(guid, name);

        List<string> list = new List<string>() { textureOverride };
        return AddCustomMask<T>(guid, name, maskType, modelType, true, list);
    }
    
    public static CustomMask AddCustomMask<T>(string guid, string name, ModelType modelType, List<string> textureOverrideList=null) where T : MaskBehaviour
    {
        LeshyAnimationController.Mask maskType = GuidManager.GetEnumValue<LeshyAnimationController.Mask>(guid, name);

        return AddCustomMask<T>(guid, name, maskType, modelType, true, textureOverrideList);
    }
    
    public static CustomMask OverrideCustomMask<T>(string guid, string name, LeshyAnimationController.Mask maskType, ModelType modelType, string textureOverride=null) where T : MaskBehaviour
    {
        List<string> list = new List<string>() { textureOverride };
        return AddCustomMask<T>(guid, name, maskType, modelType, false, list);
    }
    
    public static CustomMask OverrideCustomMask<T>(string guid, string name, LeshyAnimationController.Mask maskType, ModelType modelType, List<string> textureOverrideList=null) where T : MaskBehaviour
    {
        return AddCustomMask<T>(guid, name, maskType, modelType, false, textureOverrideList);
    }
    
    private static CustomMask AddCustomMask<T>(string guid, string name, LeshyAnimationController.Mask maskType, ModelType modelType, bool newModel, List<string> textureOverrideList=null) where T : MaskBehaviour
    {
        CustomMask mask = new CustomMask()
        {
            ID = maskType,
            Name = name,
            GUID = guid,
            TextureOverrides = textureOverrideList != null ? textureOverrideList.Select((a)=>TextureHelper.GetImageAsTexture(a)).ToList() : null,
            ModelType = modelType,
            BehaviourType = typeof(T),
            Override = !newModel,
        };
        
        CustomMasks.Add(mask);
        if (!MaskLookup.TryGetValue(maskType, out List<CustomMask> masks))
        {
            masks = new List<CustomMask>();
            MaskLookup[maskType] = masks;
        }
        masks.Add(mask);
        
        InscryptionAPIPlugin.Logger.LogInfo("Added CustomMask " + mask.Name + " with type " + maskType);
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
        
        int index = UnityEngine.Random.RandomRangeInt(0, masks.Count);
        CustomMask customMask = masks[index];
        InscryptionAPIPlugin.Logger.LogInfo("Got random mask " + customMask.Name + " from type " + maskType);
        return customMask;
    }
    
    private static List<CustomMask> GenerateBaseMasks()
    {
        InscryptionAPIPlugin.Logger.LogInfo("[GenerateBaseMasks] Start");
        InitializeDefaultModel("maskFlat", "CustomMask", ModelType.FlatMask);
        
        List<CustomMask> list = new List<CustomMask>();
        foreach (LeshyAnimationController.Mask maskType in Enum.GetValues(typeof(LeshyAnimationController.Mask)))
        {
            InscryptionAPIPlugin.Logger.LogInfo("[GenerateBaseMasks] Loading mask for " + maskType);
            ResourceLookup resourceLookup = new ResourceLookup();
            resourceLookup.FromResourceBank("Prefabs/Opponents/Leshy/Masks/Mask" + maskType);
            
            if(!Enum.TryParse(maskType.ToString(), out ModelType modelType))
            {
                InscryptionAPIPlugin.Logger.LogWarning("Could not get default mask for type " + maskType);
            }
            else
            {
                TypeToPrefabLookup[modelType] = resourceLookup;
            }

            CustomMask customMask = new CustomMask()
            {
                ID = maskType,
                Name = maskType.ToString(),
                ModelType = modelType,
                GUID = "",
                TextureOverrides = null,
                BehaviourType = typeof(MaskBehaviour)
            };
            list.Add(customMask);
            if (!MaskLookup.TryGetValue(maskType, out List<CustomMask> defaultMasks))
            {
                defaultMasks = new List<CustomMask>();
                MaskLookup[maskType] = defaultMasks;
            }
            defaultMasks.Add(customMask);
            
            InscryptionAPIPlugin.Logger.LogInfo("[GenerateBaseMasks] Done loading mask for " + maskType);
        }

        InscryptionAPIPlugin.Logger.LogInfo("[GenerateBaseMasks] Done");
        return list;
    }
    
    private static void InitializeDefaultModel(string assetBundlePath, string prefabName, ModelType modelType)
    {
        ResourceLookup resourceLookup = new ResourceLookup();
        resourceLookup.FromAssetBundleInAssembly<InscryptionAPIPlugin>(assetBundlePath, prefabName);

        TypeToPrefabLookup[modelType] = resourceLookup;
        InscryptionAPIPlugin.Logger.LogInfo("[GenerateBaseMasks] Added " + assetBundlePath + " " + prefabName);
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
        MaskBehaviour behaviour = (MaskBehaviour)clone.AddComponent(data.BehaviourType);
        behaviour.Initialize(data);
        clone.SetActive(true);

        PrintActive(clone, "[InitializeMaskClone] ");
        
        InscryptionAPIPlugin.Logger.LogInfo("[InitializeMaskClone] " + clone);
        foreach (Renderer renderer in clone.GetComponentsInChildren<Renderer>())
        {
            InscryptionAPIPlugin.Logger.LogInfo("[InitializeMaskClone][renderer] " + renderer.gameObject.name);
            InscryptionAPIPlugin.Logger.LogInfo("\t" + renderer.gameObject.activeSelf);
            InscryptionAPIPlugin.Logger.LogInfo("\t" + renderer.materials[0]);
            if(renderer.TryGetComponent(typeof(MeshFilter), out Component c))
            {
                InscryptionAPIPlugin.Logger.LogInfo("\tMesh Filter: " + ((MeshFilter)c).mesh);
            }
            
        }
        
    }

    internal static void PrintActive(GameObject o, string prefix)
    {
        InscryptionAPIPlugin.Logger.LogInfo(prefix + o.name + " " + o.activeSelf);
        for (int i = 0; i < o.transform.childCount; i++)
        {
            Transform t = o.transform.GetChild(i);
            PrintActive(t.gameObject, prefix + "\t");
        }
        
    }
}