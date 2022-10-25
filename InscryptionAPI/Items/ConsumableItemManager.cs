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
        Unknown,
        Basic,
        BasicVeins,
        Hover
    }
    
#region Patches

    [HarmonyPatch(typeof(ResourceBank), "Awake", new System.Type[] { })]
    internal class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
            // The resource bank has been cleared. refill it
            if (defaultItemModel == null)
            {
                Initialize();
            }
        }
    }
    
    [HarmonyPatch(typeof(ItemsUtil), "AllConsumables", MethodType.Getter)]
    internal class ItemsUtil_AllConsumables
    {
        public static void Postfix(ref List<ConsumableItemData> __result)
        {
            __result.AddRange(NewConsumableItems);
        }
    }
    
#endregion
    
    private static Dictionary<ModelType, GameObject> typeToPrefabLookup = new();
    private static GameObject defaultItemModel = null;
    private static List<ConsumableItemData> items = new();
    public static ReadOnlyCollection<ConsumableItemData> NewConsumableItems = new(items);

    private static void InitializeDefaultModels()
    {
        LoadModel("runeroundedbottom", "RuneRoundedBottom", ModelType.Basic);
        LoadModel("customitem", "RuneRoundedBottomVeins", ModelType.BasicVeins);
        LoadModel("customhoveringitem", "RuneHoveringItem", ModelType.Hover);
    }

    private static void LoadModel(string assetBundlePath, string prefabName, ModelType type)
    {
        byte[] resourceBytes = TextureHelper.GetResourceBytes(assetBundlePath, typeof(InscryptionAPIPlugin).Assembly);
        if (AssetBundleHelper.TryGet(resourceBytes, prefabName, out GameObject prefab))
        {
            typeToPrefabLookup[type] = prefab;
        }
    }

    private static void Initialize()
    {
        if (InscryptionAPIPlugin.configCustomItemTypes.Value == ConsumableState.Vanilla)
        {
            // Don't change any items!
            return;
        }
        
        if (defaultItemModel == null)
        {
            // TODO: Get the actual custom item model
            // NOTE: Do not add a ConsumableItem component here so we can override default models with this!
            InitializeDefaultModels();
            defaultItemModel = typeToPrefabLookup[ModelType.Basic];
        }

        List<ConsumableItemData> baseConsumableItems = ItemsUtil.AllConsumables;

        // Add all totem tops to the game
        foreach (ConsumableItemData item in items)
        {
            ModelType modelType = item.GetPrefabModelType();
            if (!typeToPrefabLookup.TryGetValue(modelType, out GameObject prefab))
            {
                // No model assigned. use default model!
                prefab = defaultItemModel;
            }
            
            // TODO: Do not error if this is a custom model!
            GameObject gameObject = CloneBasePrefab(item, prefab);

            SetupPrefab(gameObject, item.GetComponentType());

            ResourceBank.instance.resources.Add(new ResourceBank.Resource()
            {
                path = "Prefabs/Items/" + item.prefabId,
                asset = gameObject
            });
        }
        
        // Override vanilla items
        if (InscryptionAPIPlugin.configCustomItemTypes.Value == ConsumableState.All)
        {
            // Change all base items to use the fallback model!
            foreach (ConsumableItemData data in baseConsumableItems)
            {
                if (NewConsumableItems.Contains(data) || !CanUseBaseModel(data))
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

                GameObject cloneBasePrefab = CloneBasePrefab(data, defaultItemModel);
                ConsumableItem cloneConsumableItem = SetupPrefab(cloneBasePrefab, prefabConsumableItem.GetType());

                bool replaced = false;
                for (int i = 0; i < ResourceBank.instance.resources.Count; i++)
                {
                    ResourceBank.Resource resource = ResourceBank.instance.resources[i];
                    if (resource.path.ToLowerInvariant() == path)
                    {
                        resource.asset = cloneBasePrefab;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                {
                    MoveComponent(prefabConsumableItem, cloneConsumableItem);
                    
                    ResourceBank.instance.resources.Add(new ResourceBank.Resource()
                    {
                        path = path,
                        asset = cloneBasePrefab
                    });
                }
            }
        }
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

    private static GameObject CloneBasePrefab(ConsumableItemData data, GameObject gameObject)
    {
        GameObject prefab = GameObject.Instantiate(gameObject);
        prefab.name = $"Custom Item ({data.rulebookName})";
                
        // Populate icon
        if (data.rulebookSprite != null)
        {
            GameObject icon = prefab.gameObject.FindChild("Icon");
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

        return prefab;
    }

    private static ConsumableItem SetupPrefab(GameObject prefab, Type itemType)
    {
        // Add default animation
        Animator animator = prefab.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Transform child = prefab.transform.GetChild(0);
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

        ConsumableItem consumableItem = prefab.GetComponent<ConsumableItem>();
        if (consumableItem == null)
        {
            consumableItem = (ConsumableItem)prefab.AddComponent(itemType);
        }

        // Mark as dont destroy on load so it doesn't get removed between levels
        UnityObject.DontDestroyOnLoad(prefab);

        return consumableItem;
    }

    private static ModelType RegisterPrefab(string guid, string name, GameObject o)
    {
        foreach (KeyValuePair<ModelType,GameObject> pair in typeToPrefabLookup)
        {
            if (pair.Value == o)
            {
                return pair.Key;
            }
        }

        ModelType type = GuidManager.GetEnumValue<ModelType>(guid, name);
        typeToPrefabLookup[type] = o;
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
        string name = pluginGUID + "_" + rulebookName;
        ConsumableItemData data = ScriptableObject.CreateInstance<ConsumableItemData>();
        data.name = name;
        data.SetRulebookName(rulebookName);
        data.SetRulebookDescription(rulebookDescription);
        data.SetRulebookSprite(rulebookSprite.ConvertTexture());
        data.SetRegionSpecific(false);
        data.SetNotRandomlyGiven(false);
        data.SetPrefabModelType(modelType);
        data.SetPrefabID(name);
        data.SetPickupSoundId("stone_object_up");
        data.SetPlacedSoundId("stone_object_hit");
        data.SetExamineSoundId("stone_object_hit");
        
        data.SetModPrefix(pluginGUID);
        data.SetComponentType(itemType);
        data.SetPowerLevel(1);
        
        items.Add(data);
        return data;
    }
}