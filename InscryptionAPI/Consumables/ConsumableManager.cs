using System.Collections.ObjectModel;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace InscryptionAPI.Consumables;

    
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
    
    private const string CustomTotemTopID = "TotemPieces/TotemTop_Custom";
    private const string CustomTotemTopResourcePath = "Prefabs/Items/" + CustomTotemTopID;

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
            byte[] resourceBytes = TextureHelper.GetResourceBytes("customitem", typeof(InscryptionAPIPlugin).Assembly);
            AssetBundleHelper.TryGet(resourceBytes, "CustomItem", out defaultItemModel);
        }

        // Add all totem tops to the game
        foreach (CustomConsumableItem item in items)
        {
            GameObject prefab = item.Prefab;
            if (prefab == null)
            {
                InscryptionAPIPlugin.Logger.LogInfo($"{item.ConsumableItemData.rulebookName} doesn't have a custom model!");
                prefab = GameObject.Instantiate(defaultItemModel);
                
                // Populate icon
                GameObject icon = prefab.gameObject.FindChild("Icon");
                if (icon != null)
                {
                    Renderer iconRenderer = icon.GetComponent<Renderer>();
                    if (iconRenderer != null)
                    {
                        iconRenderer.material.mainTexture = item.ConsumableItemData.rulebookSprite.texture;
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
                InscryptionAPIPlugin.Logger.LogInfo($"{item.ConsumableItemData.rulebookName} is using a custom model! " + prefab.gameObject.name);
            }
            
            // Add default animation
            Animator animator = prefab.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = prefab.transform.GetChild(0).gameObject.AddComponent<Animator>();
                InscryptionAPIPlugin.Logger.LogInfo($"No Animator found. Added to {animator.gameObject.name}");
            }
            if (animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("animation/items/ItemAnim");
                animator.Rebind();
                InscryptionAPIPlugin.Logger.LogInfo($"No Animator Controller found. Added default");
            }
            
            prefab.AddComponent(item.ItemType);

            // Mark as dont destroy on load so it doesn't get removed between levels
            UnityObject.DontDestroyOnLoad(prefab);
            
            InscryptionAPIPlugin.Logger.LogInfo($"Adding resource: {item.ConsumableItemData.rulebookName}");
            ResourceBank.instance.resources.Add(new ResourceBank.Resource()
            {
                path = "Prefabs/Items/" + item.ConsumableItemData.prefabId,
                asset = prefab
            });
        }
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