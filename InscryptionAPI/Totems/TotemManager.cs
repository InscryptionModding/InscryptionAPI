using System.Reflection;
using System.Reflection.Emit;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace InscryptionAPI.Totems;

public static class TotemManager
{
    [HarmonyPatch(typeof(BuildTotemSequencer), "GenerateTotemChoices", new System.Type[] {typeof(BuildTotemNodeData), typeof(int)})]
    public class ItemsUtil_AllConsumables
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // === We want to turn this
            
            // List<Tribe> list = new()
            // {
            //     Tribe.Bird,
            //     Tribe.Canine,
            //     Tribe.Hooved,
            //     Tribe.Insect,
            //     Tribe.Reptile
            // };
            
            // === Into this
            
            // List<Tribe> list = new()
            // {
            //     Tribe.Bird,
            //     Tribe.Canine,
            //     Tribe.Hooved,
            //     Tribe.Insect,
            //     Tribe.Reptile
            // };
            // ItemsUtil_AllConsumables.AddCustomTribesToList(list);
            
            // ===
            
            
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Newobj)
                {
                    // Make a new list
                    for (int j = i+1; i < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Stloc_0)
                        {
                            MethodInfo customMethod = AccessTools.Method(typeof(ItemsUtil_AllConsumables), "AddCustomTribesToList", new Type[] { typeof(List<Tribe>)});
                            
                            // Stored the list
                            codes.Insert(j+1, new CodeInstruction(OpCodes.Ldloc_0));
                            codes.Insert(j+2, new CodeInstruction(OpCodes.Call, customMethod));
                            return codes;
                        }
                    }
                }
            }

            return codes;
        }

        public static void AddCustomTribesToList(List<Tribe> list)
        {
            List<CardInfo> cards = CardManager.AllCardsCopy;
            foreach (TribeManager.TribeInfo tribeInfo in TribeManager.NewTribes)
            {
                // Only add 
                foreach (CardInfo info in cards)
                {
                    if (info.IsOfTribe(tribeInfo.tribe))
                    {
                        list.Add(tribeInfo.tribe);
                        break;
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(CompositeTotemPiece), "Start", new Type[]{})]
    public class CompositeTotemPiece_Start
    {
        public static bool Prefix(CompositeTotemPiece __instance)
        {
            if (__instance.emissiveRenderer == null)
            {
                GameObject icon = __instance.gameObject.FindChild("Icon");
                if (icon != null)
                {       
                    __instance.emissiveRenderer = icon.GetComponent<Renderer>();
                }
                else
                {
                    InscryptionAPIPlugin.Logger.LogError($"Could not find Icon Gameobject t oassign emissiveRenderer!");
                }
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(CompositeTotemPiece), "SetData", new Type[]{typeof(ItemData)})]
    public class CompositeTotemPiece_SetData
    {
        public static bool Prefix(CompositeTotemPiece __instance, ItemData data)
        {
            if (__instance.emissiveRenderer == null)
            {
                if (data is TotemTopData topData)
                {
                    foreach (TribeManager.TribeInfo tribeInfo in TribeManager.NewTribes)
                    {
                        if (tribeInfo.tribe == topData.prerequisites.tribe)
                        {
                            GameObject icon = __instance.gameObject.FindChild("Icon");
                            if (icon != null)
                            {
                                Renderer iconRenderer = icon?.GetComponent<Renderer>();
                                if (iconRenderer != null)
                                {
                                    iconRenderer.material.mainTexture = tribeInfo.icon.texture;
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
                            break;
                        }
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ResourceBank), "Awake", new System.Type[] { })]
    public class ResourceBank_Awake
    {
        public static void Postfix(ResourceBank __instance)
        {
            InscryptionAPIPlugin.Logger.LogError($"[ResourceBank_Awake] Starting");
            if (ResourceBank.Get<GameObject>(CustomTotemTopResourcePath) == null)
            {
                InscryptionAPIPlugin.Logger.LogError($"[ResourceBank_Awake] Initializing");
                Initialize();
            }
        }
    }
    
    [HarmonyPatch(typeof(Totem), "GetTopPiecePrefab", new Type[]{typeof(TotemTopData)})]
    public class Totem_GetTopPiecePrefab
    {
        public static bool Prefix(Totem __instance, TotemTopData data, ref GameObject __result)
        {
            if (TribeManager.IsCustomTribe(data.prerequisites.tribe))
            {
                CustomTotemTop customTribeTotem = totemTops.Find((a) => a.Tribe == data.prerequisites.tribe);
                if (customTribeTotem == null)
                {
                    // Get custom totem model
                    __result = ResourceBank.Get<GameObject>(CustomTotemTopResourcePath);
                }
                else
                {
                    // No custom totem model - use default model
                    __result = defaultTotemTop.Prefab;
                }
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(Totem), "SetData", new Type[]{typeof(ItemData)})]
    public class Totem_SetData
    {
        public static void Postfix(Totem __instance, ItemData data)
        {
            __instance.topPieceParent.GetComponentInChildren<CompositeTotemPiece>().SetData(__instance.TotemItemData.top);
        }
    }
    
    [HarmonyPatch(typeof(TotemTopData), "PrefabId", MethodType.Getter)]
    public class TotemTopData_PrefabId
    {
        public static bool Prefix(TotemTopData __instance, ref string __result)
        {
            if (TribeManager.IsCustomTribe(__instance.prerequisites.tribe))
            {
                CustomTotemTop customTribeTotem = totemTops.Find((a) => a.Tribe == __instance.prerequisites.tribe);
                if (customTribeTotem == null)
                {
                    __result = CustomTotemTopID;
                    return false;
                }
            }
            
            return true;
        }
    }

    public static CustomTotemTop NewTopPiece(string name, string guid, GameObject prefab, Tribe tribe)
    {
        return Add(new CustomTotemTop()
        {
            Name = name,
            GUID = guid,
            Prefab = prefab,
            Tribe = tribe
        });
    }

    private static CustomTotemTop Add(CustomTotemTop totem)
    {
        totemTops.Add(totem);
        return totem;
    }
    
    private const string CustomTotemTopID = "TotemPieces/TotemTop_Custom";
    private const string CustomTotemTopResourcePath = "Prefabs/Items/" + CustomTotemTopID;

    private static CustomTotemTop defaultTotemTop = null;
    private static List<CustomTotemTop> totemTops = new();
    
    private static void Initialize()
    {
        if (defaultTotemTop == null)
        {
            byte[] resourceBytes = TextureHelper.GetResourceBytes("customtotemtop", typeof(InscryptionAPIPlugin).Assembly);
            if (AssetBundleHelper.TryGet(resourceBytes, "CustomTotemTop", out GameObject go))
            {
                defaultTotemTop = NewTopPiece("DefaultTotemTop",
                    InscryptionAPIPlugin.ModGUID,
                    go,
                    Tribe.None
                );
            }
        }

        // Add all totem tops to the game
        foreach (CustomTotemTop totem in totemTops)
        {
            string path = "Prefabs/Items/TotemPieces/TotemTop_" + totem.Tribe;
            if (totem == defaultTotemTop)
            {
                path = CustomTotemTopResourcePath;
            }
            
            GameObject prefab = totem.Prefab;
            
            // Add require components in case the prefab doesn't have them
            if (prefab.GetComponent<CompositeTotemPiece>() == null)
            {
                prefab.AddComponent<CompositeTotemPiece>();
            }
            if (prefab.GetComponent<Animator>() == null)
            {
                Animator addComponent = prefab.AddComponent<Animator>();
                addComponent.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("animation/items/ItemAnim");
                addComponent.Rebind();
            }

            // Mark as dont destroy on load so it doesn't get removed between levels
            UnityObject.DontDestroyOnLoad(prefab);
            
            ResourceBank.instance.resources.Add(new ResourceBank.Resource()
            {
                path = path,
                asset = prefab
            });
        }

    }

    public class CustomTotemTop
    {
        public string Name;
        public string GUID;
        public GameObject Prefab;
        public Tribe Tribe;
    }
}
