using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
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
        BasicRune = 1,
        BasicRuneWithVeins = 2,
        HoveringRune = 3,
        CardInABottle = 4,
    }

    #region Patches

    [HarmonyPatch(typeof(ResourceBank), "Awake", new System.Type[] { })]
    private class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
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

    [HarmonyPatch(typeof(ItemSlot), "CreateItem", new Type[] { typeof(ItemData), typeof(bool) })]
    private class ItemSlot_CreateItem
    {
        public static bool Prefix(ItemSlot __instance, ItemData data, bool skipDropAnimation)
        {
            if (__instance.Item != null)
            {
                UnityObject.Destroy(__instance.Item.gameObject);
            }

            string prefabId = "Prefabs/Items/" + data.PrefabId;
            GameObject gameObject = null;
            if (prefabIDToResourceLookup.TryGetValue(prefabId.ToLowerInvariant(), out ConsumableItemResource resource) && data is ConsumableItemData consumableItemData)
            {
                GameObject prefab = resource.Get<GameObject>();
                if (prefab == null)
                {
                    InscryptionAPIPlugin.Logger.LogError($"Failed to get {consumableItemData.rulebookName} model from ConsumableItemAssetGetter " + resource);
                }
                else
                {
                    gameObject = UnityObject.Instantiate<GameObject>(prefab, __instance.transform);
                }

                if (resource.PreSetupCallback != null)
                {
                    resource.PreSetupCallback(gameObject, consumableItemData);
                }
                SetupPrefab(consumableItemData, gameObject, consumableItemData.GetComponentType(), consumableItemData.GetPrefabModelType());
            }
            else
            {
                gameObject = UnityObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>(prefabId), __instance.transform);
                if (gameObject == null)
                {
                    InscryptionAPIPlugin.Logger.LogError($"Failed to get {data.name} model from ResourceBank " + prefabId);
                }
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            gameObject.transform.localPosition = Vector3.zero;
            __instance.Item = gameObject.GetComponent<Item>();
            __instance.Item.SetData(data);
            if (skipDropAnimation)
            {
                __instance.Item.PlayEnterAnimation(true);
            }


            // Setup cards
            if (__instance.Item is CardBottleItem cardBottleItem && data is ConsumableItemData consumableItemData2)
            {
                string cardWithinBottle = consumableItemData2.GetCardWithinBottle();
                if (!string.IsNullOrEmpty(cardWithinBottle))
                {
                    CardInfo cardInfo = CardLoader.GetCardByName(cardWithinBottle);
                    if (cardInfo != null)
                    {
                        cardBottleItem.cardInfo = cardInfo;
                        cardBottleItem.GetComponentInChildren<SelectableCard>().SetInfo(cardInfo);
                        cardBottleItem.gameObject.GetComponent<AssignCardOnStart>().enabled = false;
                    }
                    else
                    {
                        InscryptionAPIPlugin.Logger.LogError("Could not get card for bottled card item: " + cardWithinBottle);
                    }
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(ConsumableItem), nameof(ConsumableItem.CanActivate), new Type[] { })]
    private class ConsumableItem_CanActivate
    {
        public static bool Prefix(ConsumableItem __instance, ref bool __result)
        {
            if (__instance.Data is ConsumableItemData consumableItemData)
            {
                if (consumableItemData.CanActivateOutsideBattles())
                {
                    if (!GameFlowManager.IsCardBattle)
                    {
                        // We are not in a card battle. Unique behaviour!
                        __result = __instance.ExtraActivationPrerequisitesMet();
                        return false;
                    }
                }
            }

            // Keep original activation behaviour
            return true;
        }
    }

    [HarmonyPatch]
    private class ConsumableItemSlot_ConsumeItemEnumerator
    {
        [HarmonyPatch(typeof(ConsumableItemSlot), "ConsumeItem")]
        [HarmonyPostfix]
        private static IEnumerator Postfix(IEnumerator result, ConsumableItemSlot __instance)
        {
            ConsumableItemSlot_ConsumeItem.currentItemData = __instance.Consumable.Data;
            return result;
        }
    }

    [HarmonyPatch]
    internal class ConsumableItemSlot_ConsumeItem
    {
        static Type ConsumableItemSlot_ConsumeItem_Class = Type.GetType("DiskCardGame.ConsumableItemSlot+<ConsumeItem>d__15, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        static MethodInfo SwitchToViewMethod = AccessTools.Method(typeof(ViewManager), nameof(ViewManager.SwitchToView), new Type[] { typeof(View), typeof(bool), typeof(bool) });
        static MethodInfo CustomSwitchToViewMethod = AccessTools.Method(typeof(ConsumableItemSlot_ConsumeItem), nameof(SwitchToView), new Type[] { typeof(ViewManager), typeof(View), typeof(bool), typeof(bool) });

        internal static ItemData currentItemData = null;

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(ConsumableItemSlot_ConsumeItem_Class, "MoveNext");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // === We want to turn this

            // ConsumableItemSlot consumableItemSlot = <>4__this;
            // ...
            // Singleton<ViewManager>.Instance.SwitchToView(..., ..., ...);

            // === Into this

            // ConsumableItemSlot consumableItemSlot = <>4__this;
            // ...
            // GetNextTargetView(..., ..., ...);

            // ===

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction codeInstruction = codes[i];
                if (codeInstruction.operand == SwitchToViewMethod)
                {
                    // Call CustomSwitchToView instead of View
                    codeInstruction.operand = CustomSwitchToViewMethod;
                    i++;
                }
            }

            return codes;
        }

        public static void SwitchToView(ViewManager instance, View view, bool immediate, bool lockAfter)
        {
            if (currentItemData != null && currentItemData is ConsumableItemData itemData && itemData.CanActivateOutsideBattles())
            {
                // Do nothing!
                return;
            }

            instance.SwitchToView(view, immediate, lockAfter);
        }
    }

    #endregion

    private static Sprite cardinbottleSprite;
    private static ConsumableItemResource defaultItemModel = null;
    private static ModelType defaultItemModelType = ModelType.HoveringRune;
    private static Dictionary<string, ConsumableItemResource> prefabIDToResourceLookup = new();
    private static Dictionary<ModelType, ConsumableItemResource> typeToPrefabLookup = new();
    private static HashSet<ModelType> defaultModelTypes = new();
    private static List<ConsumableItemData> allNewItems = new();
    private static List<ConsumableItemData> baseConsumableItemsDatas = new();
    public static ReadOnlyCollection<ConsumableItemData> NewConsumableItemDatas = new(allNewItems);

    private static void InitializeDefaultModels()
    {
        LoadDefaultModelFromBundle("runeroundedbottom", "RuneRoundedBottom", ModelType.BasicRune);
        LoadDefaultModelFromBundle("customitem", "RuneRoundedBottomVeins", ModelType.BasicRuneWithVeins);
        LoadDefaultModelFromBundle("customhoveringitem", "RuneHoveringItem", ModelType.HoveringRune);
        LoadDefaultModelFromResources("prefabs/items/FrozenOpossumBottleItem", ModelType.CardInABottle);
    }

    private static void LoadDefaultModelFromBundle(string assetBundlePath, string prefabName, ModelType type)
    {
        ConsumableItemResource resource = new ConsumableItemResource();
        resource.FromAssetBundleInAssembly<InscryptionAPIPlugin>(assetBundlePath, prefabName);
        typeToPrefabLookup[type] = resource;
        defaultModelTypes.Add(type);
    }

    private static void LoadDefaultModelFromResources(string resourcePath, ModelType type)
    {
        ConsumableItemResource resource = new ConsumableItemResource();
        resource.FromResources(resourcePath);

        typeToPrefabLookup[type] = resource;
        defaultModelTypes.Add(type);
    }

    private static void Initialize()
    {
        baseConsumableItemsDatas.Clear();
        baseConsumableItemsDatas.AddRange(ItemsUtil.AllConsumables.FindAll((a) => a != null && string.IsNullOrEmpty(a.GetModPrefix())));
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
                string path = "Prefabs/Items/" + data.prefabId;
                GameObject prefab = ResourceBank.Get<GameObject>(path);
                if (prefab == null)
                {
                    InscryptionAPIPlugin.Logger.LogWarning($"Couldn't override item {data.rulebookName}. Couldn't get prefab from ResourceBank");
                    continue;
                }

                if (!CanUseBaseModel(data, prefab))
                {
                    continue;
                }

                data.SetComponentType(prefab.GetComponent<ConsumableItem>().GetType());
                data.SetPrefabModelType(defaultItemModelType);

                ConsumableItemResource resource = (ConsumableItemResource)defaultItemModel.Clone();
                resource.PreSetupCallback = static (clone, data) =>
                {
                    string path = "Prefabs/Items/" + data.prefabId;
                    GameObject defaultObject = ResourceBank.Get<GameObject>(path);

                    ConsumableItem sourceComp = defaultObject.GetComponent<ConsumableItem>();
                    ConsumableItem targetComp = clone.AddComponent<ConsumableItem>();
                    MoveComponent(sourceComp, targetComp);
                };

                prefabIDToResourceLookup[path.ToLowerInvariant()] = defaultItemModel;
            }
        }
    }

    private static void InitializeConsumableItemDataPrefab(ConsumableItemData item)
    {
        ModelType modelType = item.GetPrefabModelType();
        if (!typeToPrefabLookup.TryGetValue(modelType, out ConsumableItemResource prefab))
        {
            // No model assigned. use default model!
            prefab = defaultItemModel;
            InscryptionAPIPlugin.Logger.LogWarning($"Could not find ModelType {modelType} for ConsumableItemData {item.rulebookName}!");
        }
        if (prefab == null)
        {
            // Model assigned but model has been deleted?
            prefab = defaultItemModel;
            InscryptionAPIPlugin.Logger.LogError($"Prefab missing for ConsumableItemData {item.rulebookName}! Using default instead.");
        }

        string prefabId = "Prefabs/Items/" + item.prefabId;
        prefabIDToResourceLookup[prefabId.ToLowerInvariant()] = prefab;
    }

    private static bool CanUseBaseModel(ConsumableItemData data, GameObject gameObject)
    {
        return data.rulebookSprite != null && gameObject.GetComponent<CardBottleItem>() == null;
    }

    private static void MoveComponent(Component sourceComp, Component targetComp)
    {
        FieldInfo[] sourceFields = sourceComp.GetType().GetFields(BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance);

        for (int i = 0; i < sourceFields.Length; i++)
        {
            var value = sourceFields[i].GetValue(sourceComp);
            sourceFields[i].SetValue(targetComp, value);
        }
    }

    private static ConsumableItem SetupPrefab(ConsumableItemData data, GameObject prefab, Type itemType, ModelType modelType)
    {
        prefab.name = $"Custom Item ({data.rulebookName})";

        // Populate icon. Only for default fallback types - If anyone wants to use this then they can add to that fallback type list i guss??
        if (defaultModelTypes.Contains(modelType) && modelType != ModelType.CardInABottle)
        {
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
        }

        // Add default animation if it doesn't have one
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

        // Add component if it doesn't exist
        ConsumableItem consumableItem = prefab.GetComponent<ConsumableItem>();
        if (consumableItem == null)
        {
            consumableItem = prefab.AddComponent(itemType) as ConsumableItem;
            if (consumableItem == null)
            {
                InscryptionAPIPlugin.Logger.LogError($"Type given is not a ConsumableItem! You may encounter unexpected bugs");
            }
        }

        // Mark as dont destroy on load so it doesn't get removed between levels
        UnityObject.DontDestroyOnLoad(prefab);

        return consumableItem;
    }

    public static ModelType RegisterPrefab(string pluginGUID, string rulebookName, ConsumableItemResource resource)
    {
        ModelType type = GuidManager.GetEnumValue<ModelType>(pluginGUID, rulebookName);
        typeToPrefabLookup[type] = resource;

        return type;
    }

    public static ConsumableItemData New(string pluginGUID,
        string rulebookName,
        string rulebookDescription,
        Texture2D rulebookSprite,
        Type itemType,
        GameObject prefab)
    {
        ConsumableItemResource resource = new ConsumableItemResource();
        resource.FromPrefab(prefab);

        ModelType modelType = RegisterPrefab(pluginGUID, rulebookName, resource);

        GameObject.DontDestroyOnLoad(prefab);
        prefab.SetActive(false);

        return New(pluginGUID, rulebookName, rulebookDescription, rulebookSprite, itemType, modelType);
    }

    public static ConsumableItemData New(string pluginGUID,
        string rulebookName,
        string rulebookDescription,
        Texture2D rulebookSprite,
        Type itemType,
        ConsumableItemResource resource)
    {
        ModelType registerPrefab = RegisterPrefab(pluginGUID, rulebookName, resource);
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

    public static ConsumableItemData NewCardInABottle(string pluginGUID, string cardName, Texture2D rulebookTexture = null)
    {
        CardInfo cardInfo = CardLoader.GetCardByName(cardName);
        if (cardInfo == null)
        {
            InscryptionAPIPlugin.Logger.LogError("Could not add NewCardInABottle. Could not get card using name " + cardName);
            return null;
        }

        return NewCardInABottle(pluginGUID, cardInfo, rulebookTexture);
    }

    public static ConsumableItemData NewCardInABottle(string pluginGUID, CardInfo cardInfo, Texture2D rulebookTexture = null)
    {
        if (cardInfo == null)
        {
            InscryptionAPIPlugin.Logger.LogError("Could not add NewCardInABottle. CardInfo is null!");
            return null;
        }

        ConsumableItemData data = ScriptableObject.Instantiate(Resources.Load<ConsumableItemData>("data/consumables/FrozenOpossumBottle"));

        string rulebookName = $"{cardInfo.displayedName} Bottle";
        data.SetRulebookName(rulebookName);

        string rulebookDescription = $"A {cardInfo.displayedName} is created in your hand. [define:{cardInfo.name}]";
        data.SetRulebookDescription(rulebookDescription);

        Sprite sprite = null;
        if (rulebookTexture != null)
        {
            sprite = rulebookTexture.ConvertTexture();
        }
        else
        {
            cardinbottleSprite ??= TextureHelper.GetImageAsTexture("rulebookitemicon_cardinbottle.png", Assembly.GetExecutingAssembly()).ConvertTexture();
            sprite = cardinbottleSprite;
        }

        data.SetRulebookSprite(sprite);
        data.SetRegionSpecific(false);
        data.SetNotRandomlyGiven(false);
        data.SetLearnItemDescription("");

        data.SetPrefabModelType(ModelType.CardInABottle);
        data.SetComponentType(typeof(CardBottleItem));
        data.SetCardWithinBottle(cardInfo.name);

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