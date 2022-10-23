using System.Collections.ObjectModel;
using System.Reflection;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
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

    [HarmonyPatch(typeof(ResourceBank), "Awake", new System.Type[] { })]
    internal class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
            // The resource bank has been cleared. refill it
            Initialize();
        }
    }
    
    [HarmonyPatch(typeof(ItemsUtil), "AllConsumables", MethodType.Getter)]
    internal class ItemsUtil_AllConsumables
    {
        public static void Postfix(ref List<ConsumableItemData> __result)
        {
            foreach (CustomConsumableItem item in AllItems)
            {
                __result.Add(item.ConsumableItemData);
            }
        }
    }
    
    private static GameObject defaultItemModel = null;
    private static List<CustomConsumableItem> items = new();
    public static ReadOnlyCollection<CustomConsumableItem> AllItems = new(items);

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
            byte[] resourceBytes = TextureHelper.GetResourceBytes("customitem", typeof(InscryptionAPIPlugin).Assembly);
            AssetBundleHelper.TryGet(resourceBytes, "CustomItem", out defaultItemModel);
        }

        List<ConsumableItemData> baseConsumableItems = ItemsUtil.AllConsumables;

        // Add all totem tops to the game
        foreach (CustomConsumableItem item in items)
        {
            GameObject prefab = item.Prefab;
            if (prefab == null)
            {
                prefab = CloneBasePrefab(item.ConsumableItemData);
            }

            SetupPrefab(prefab, item.ItemType);

            ResourceBank.instance.resources.Add(new ResourceBank.Resource()
            {
                path = "Prefabs/Items/" + item.ConsumableItemData.prefabId,
                asset = prefab
            });
        }
        
        // Override vanilla items
        if (InscryptionAPIPlugin.configCustomItemTypes.Value == ConsumableState.All)
        {
            // Change all base items to use the fallback model!
            foreach (ConsumableItemData data in baseConsumableItems)
            {
                if (!CanUseBaseModel(data))
                {
                    continue;
                }
                
                string path = ("Prefabs/Items/" + data.prefabId).ToLowerInvariant();
                GameObject item = ResourceBank.Get<GameObject>(path);
                if (item == null || !item.TryGetComponent<ConsumableItem>(out ConsumableItem prefabConsumableItem))
                {
                    InscryptionAPIPlugin.Logger.LogError($"Couldn't override item {data.rulebookName}. Couldn't get prefab from ResourceBank");
                    continue;
                }

                GameObject cloneBasePrefab = CloneBasePrefab(data);
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

    private static GameObject CloneBasePrefab(ConsumableItemData data)
    {
        GameObject prefab = GameObject.Instantiate(defaultItemModel);
                
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

    public static CustomConsumableItem New(string pluginGUID,
        int powerLevel,  
        string rulebookName, 
        string rulebookDescription, 
        Texture2D rulebookSprite,
        Type itemType,
        string learnText=null,
        GameObject prefab=null,
        bool regionSpecific=false, 
        AbilityMetaCategory rulebookCategory=AbilityMetaCategory.Part1Rulebook, 
        bool notRandomlyGiven=false)
    {
        string name = pluginGUID + "_" + rulebookName;
        ConsumableItemData data = ScriptableObject.CreateInstance<ConsumableItemData>();
        data.name = name;
        data.powerLevel = powerLevel;
        data.description = learnText;
        data.rulebookCategory = rulebookCategory;
        data.rulebookName = rulebookName;
        data.rulebookDescription = rulebookDescription;
        data.rulebookSprite = TextureHelper.ConvertTexture(rulebookSprite);
        data.regionSpecific = regionSpecific;
        data.notRandomlyGiven = notRandomlyGiven;
        data.prefabId = name;
        data.pickupSoundId = "stone_object_up";
        data.placedSoundId = "stone_object_hit";
        
        return Add(pluginGUID, itemType, prefab, data);
    }

    internal static CustomConsumableItem Add(string guid, Type itemType, GameObject prefab, ConsumableItemData data)
    {
        InscryptionAPIPlugin.Logger.LogInfo($"{data.rulebookName} prefab: " + prefab);
        if (prefab != null)
        {
            GameObject.DontDestroyOnLoad(prefab);
        }
        
        CustomConsumableItem item = new CustomConsumableItem()
        {
            PluginGUID = guid,
            ItemType = itemType,
            Prefab = prefab,
            ConsumableItemData = data
        };
        items.Add(item);
        return item;
    }
    
    public class CustomConsumableItem
    {
        public string PluginGUID;
        public Type ItemType;
        public GameObject Prefab;
        public ConsumableItemData ConsumableItemData;
    }
}