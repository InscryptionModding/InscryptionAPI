using System.Collections.ObjectModel;
using System.Reflection;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items.Extensions;
using UnityEngine;

namespace InscryptionAPI.Items;

    
public static class ConsumableItemManager
{
    internal enum ConsumableState
    {
        Vanilla,
        Custom,
        All
    }
    
    public enum ModelType
    {
        Basic = 1,
        BasicVeins = 2,
        Hover = 3
    }
    
#region Patches

    [HarmonyPatch(typeof(ResourceBank), "Awake", new System.Type[] { })]
    private class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
            InscryptionAPIPlugin.Logger.LogInfo("[ResourceBank_Awake] Postfix");
            Initialize();
        }
    }
    
    [HarmonyPatch(typeof(ItemsUtil), "AllConsumables", MethodType.Getter)]
    private class ItemsUtil_AllConsumables
    {
        public static void Postfix(ref List<ConsumableItemData> __result)
        {
            __result.AddRange(allNewItems);
        }
    }
    
#endregion
    
    private static GameObject defaultItemModel = null;
    private static ModelType defaultItemModelType = ModelType.Hover;
    private static Dictionary<ModelType, GameObject> typeToPrefabLookup = new();
    private static HashSet<ModelType> defaultModelTypes = new();
    private static List<ConsumableItemData> allNewItems = new();
    private static List<ConsumableItemData> baseConsumableItemsDatas = new();
    public static ReadOnlyCollection<ConsumableItemData> NewConsumableItemDatas = new(allNewItems);

    private static void InitializeDefaultModels()
    {
        LoadDefaultModel("runeroundedbottom", "RuneRoundedBottom", ModelType.Basic);
        LoadDefaultModel("customitem", "RuneRoundedBottomVeins", ModelType.BasicVeins);
        LoadDefaultModel("customhoveringitem", "RuneHoveringItem", ModelType.Hover);
        InscryptionAPIPlugin.Logger.LogInfo("[InitializeDefaultModels] Default items " + typeToPrefabLookup.Count);
    }

    private static void LoadDefaultModel(string assetBundlePath, string prefabName, ModelType type)
    {
        byte[] resourceBytes = TextureHelper.GetResourceBytes(assetBundlePath, typeof(InscryptionAPIPlugin).Assembly);
        if (AssetBundleHelper.TryGet(resourceBytes, prefabName, out GameObject prefab))
        {
            typeToPrefabLookup[type] = prefab;
            defaultModelTypes.Add(type);
        }
    }

    private static void Initialize()
    {
        InscryptionAPIPlugin.Logger.LogInfo("[InitializeDefaultModels] Initializing");
        baseConsumableItemsDatas.Clear();
        baseConsumableItemsDatas.AddRange(ItemsUtil.AllConsumables);
        if (InscryptionAPIPlugin.configCustomItemTypes.Value == ConsumableState.Vanilla)
        {
            // Don't change any items!
            return;
        }
        
        if (defaultItemModel == null)
        {
            // NOTE: Do not add a ConsumableItem component here so we can override default models with this!
            InitializeDefaultModels();
            defaultItemModel = typeToPrefabLookup[defaultItemModelType];
        }
        
        // Add all totem tops to the game
        foreach (ConsumableItemData item in allNewItems)
        {
            InitializeConsumableItemDataPrefab(item);
        }
        
        // Override vanilla items
        if (InscryptionAPIPlugin.configCustomItemTypes.Value == ConsumableState.All)
        {
            // Change all base items to use the fallback model!
            foreach (ConsumableItemData data in baseConsumableItemsDatas)
            {
                InscryptionAPIPlugin.Logger.LogInfo("[InitializeDefaultModels] Overriding data " + data.name);
                if (!CanUseBaseModel(data))
                {
                    continue;
                }
                
                string path = ("Prefabs/Items/" + data.prefabId).ToLowerInvariant();
                GameObject item = ResourceBank.Get<GameObject>(path);
                if (item == null || !item.TryGetComponent(out ConsumableItem prefabConsumableItem))
                {
                    InscryptionAPIPlugin.Logger.LogError($"Couldn't override item {data.rulebookName}. Couldn't get prefab from ResourceBank");
                    continue;
                }

                ConsumableItem clone = CloneAndSetupPrefab(data, defaultItemModel, prefabConsumableItem.GetType(), defaultItemModelType);

                bool replaced = false;
                for (int i = 0; i < ResourceBank.instance.resources.Count; i++)
                {
                    ResourceBank.Resource resource = ResourceBank.instance.resources[i];
                    if (resource.path.ToLowerInvariant() == path)
                    {
                        resource.asset = clone.gameObject;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                {
                    MoveComponent(prefabConsumableItem, clone);
                    
                    ResourceBank.instance.resources.Add(new ResourceBank.Resource()
                    {
                        path = path,
                        asset = clone.gameObject
                    });
                }
            }
        }
    }
    
    private static void InitializeConsumableItemDataPrefab(ConsumableItemData item)
    {
        ModelType modelType = item.GetPrefabModelType();
        if (!typeToPrefabLookup.TryGetValue(modelType, out GameObject prefab))
        {
            // No model assigned. use default model!
            prefab = defaultItemModel;
            InscryptionAPIPlugin.Logger.LogWarning($"Could not find ModelType {modelType} for ConsumableItemData {item.rulebookName}!");
        }

        GameObject gameObject = CloneAndSetupPrefab(item, prefab, item.GetComponentType(), modelType).gameObject;
        ResourceBank.instance.resources.Add(new ResourceBank.Resource()
        {
            path = "Prefabs/Items/" + item.prefabId,
            asset = gameObject
        });
    }

    private static bool CanUseBaseModel(ConsumableItemData data)
    {
        return data.rulebookSprite != null;
    }
    
    private static void MoveComponent(Component sourceComp, Component targetComp) 
    {
        FieldInfo[] sourceFields = sourceComp.GetType().GetFields(BindingFlags.Public | 
            BindingFlags.NonPublic | 
            BindingFlags.Instance);
        
        for(int i = 0; i < sourceFields.Length; i++) 
        {
            var value = sourceFields[i].GetValue(sourceComp);
            sourceFields[i].SetValue(targetComp, value);
        }
    }

    private static ConsumableItem CloneAndSetupPrefab(ConsumableItemData data, GameObject prefab, Type itemType, ModelType modelType)
    {
        GameObject clone = UnityObject.Instantiate(prefab);
        clone.name = $"Custom Item ({data.rulebookName})";
        
        // Populate icon. Only for default fallback types - If anyone wants to use this then they can add to that fallback type list i guss??
        if (defaultModelTypes.Contains(modelType))
        {
            if (data.rulebookSprite != null)
            {
                GameObject icon = clone.gameObject.FindChild("Icon");
                if (icon != null)
                {
                    Renderer iconRenderer = icon.GetComponent<Renderer>();
                    if (iconRenderer != null)
                    {
                        iconRenderer.material.mainTexture = data.rulebookSprite.texture;
                    }
                    else
                    {
                        InscryptionAPIPlugin.Logger.LogError($"Could not find Renderer on Icon GameObject to assign tribe icon!");
                    }
                }
                else
                {
                    InscryptionAPIPlugin.Logger.LogError($"Could not find Icon GameObject to assign tribe icon!");
                }
            }
            else
            {
                InscryptionAPIPlugin.Logger.LogError($"Could not change icon for {data.rulebookName}. No sprite defined!");
            }
        }

        // Add default animation if it doesn't have one
        Animator animator = clone.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Transform child = clone.transform.GetChild(0);
            if (child != null)
            {
                animator = child.gameObject.AddComponent<Animator>();
            }
            else
            {
                InscryptionAPIPlugin.Logger.LogError($"Could not add Animator. Missing a child game object!. Make sure you have a GameObject called Anim!");
            }
        }
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("animation/items/ItemAnim");
            animator.Rebind();
        }

        // Add component if it doesn't exist
        ConsumableItem consumableItem = clone.GetComponent<ConsumableItem>();
        if (consumableItem == null)
        {
            consumableItem = clone.AddComponent(itemType) as ConsumableItem;
            if (consumableItem == null)
            {
                InscryptionAPIPlugin.Logger.LogError($"Type given is not a ConsumableItem! You may encounter unexpected bugs");
            }
        }

        // Mark as dont destroy on load so it doesn't get removed between levels
        UnityObject.DontDestroyOnLoad(clone);

        return consumableItem;
    }

    public static ModelType RegisterPrefab(string pluginGUID, string rulebookName, GameObject prefab)
    {
        foreach (KeyValuePair<ModelType,GameObject> pair in typeToPrefabLookup)
        {
            if (pair.Value == prefab)
            {
                return pair.Key;
            }
        }

        ModelType type = GuidManager.GetEnumValue<ModelType>(pluginGUID, rulebookName);
        typeToPrefabLookup[type] = prefab;
        return type;
    }

    public static ConsumableItemData New(string pluginGUID,
        string rulebookName, 
        string rulebookDescription, 
        Texture2D rulebookSprite,
        Type itemType,
        GameObject prefab)
    {
        ModelType registerPrefab = RegisterPrefab(pluginGUID, rulebookName, prefab);
        return New(pluginGUID, rulebookName, rulebookDescription, rulebookSprite, itemType, registerPrefab);
    }

    public static ConsumableItemData New(string pluginGUID,
        string rulebookName, 
        string rulebookDescription, 
        Texture2D rulebookSprite,
        Type itemType,
        ModelType modelType)
    {
        ConsumableItemData data = ScriptableObject.CreateInstance<ConsumableItemData>();
        data.SetRulebookName(rulebookName);
        data.SetRulebookDescription(rulebookDescription);
        data.SetRulebookSprite(rulebookSprite.ConvertTexture());
        data.SetRegionSpecific(false);
        data.SetNotRandomlyGiven(false);
        data.SetPrefabModelType(modelType);
        data.SetPickupSoundId("stone_object_up");
        data.SetPlacedSoundId("stone_object_hit");
        data.SetExamineSoundId("stone_object_hit");
        data.SetComponentType(itemType);
        data.SetPowerLevel(1);
        
        return Add(pluginGUID, data);
    }

    public static ConsumableItemData Add(string pluginGUID, ConsumableItemData data)
    {
        string name = pluginGUID + "_" + data.rulebookName;
        data.name = name;
        data.SetPrefabID(name);
        data.SetModPrefix(pluginGUID);
        
        allNewItems.Add(data);
        return data;
    }
}