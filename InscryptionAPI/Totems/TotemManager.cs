using System.Reflection;
using System.Reflection.Emit;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace InscryptionAPI.Totems;

[HarmonyPatch]
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
            foreach (Tribe tribe in TribeManager.NewTribesTypes)
            {
                // Only add 
                foreach (CardInfo info in cards)
                {
                    if (info.IsOfTribe(tribe))
                    {
                        list.Add(tribe);
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
            InscryptionAPIPlugin.Logger.LogInfo($"[CompositeTotemPiece_PrefabId] Start");
            if (__instance.emissiveRenderer == null)
            {
                GameObject icon = __instance.gameObject.FindChild("Icon");
                InscryptionAPIPlugin.Logger.LogInfo($"[CompositeTotemPiece_PrefabId] icon: " + icon);
                __instance.emissiveRenderer = icon.GetComponent<Renderer>();
                InscryptionAPIPlugin.Logger.LogInfo($"[CompositeTotemPiece_PrefabId] Assigned new renderer: " + __instance.emissiveRenderer);
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(CompositeTotemPiece), "SetData", new Type[]{typeof(ItemData)})]
    public class CompositeTotemPiece_SetData
    {
        public static bool Prefix(CompositeTotemPiece __instance, ItemData data)
        {
            InscryptionAPIPlugin.Logger.LogInfo($"[CompositeTotemPiece_SetData] " + data);
            if (__instance.emissiveRenderer == null)
            {
                if (data is TotemTopData topData)
                {
                    foreach (TribeManager.TribeInfo tribeInfo in TribeManager.NewTribes)
                    {
                        if (tribeInfo.tribe == topData.prerequisites.tribe)
                        {
                            InscryptionAPIPlugin.Logger.LogInfo($"[CompositeTotemPiece_SetData] tribeInfo");
                            InscryptionAPIPlugin.Logger.LogInfo($"[CompositeTotemPiece_SetData] tribeInfo " + tribeInfo.tribe);
                
                            Renderer Icon = __instance.gameObject.FindChild("Icon")?.GetComponent<Renderer>();
                            InscryptionAPIPlugin.Logger.LogInfo($"[CompositeTotemPiece_SetData] Icon " + Icon);
                            Icon.material.mainTexture = tribeInfo.icon.texture;
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
            InscryptionAPIPlugin.Logger.LogInfo("[ResourceBank_Awake] Postfix");
            if (defaultTotemTop == null)
            {
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
                    __result = ResourceBank.Get<GameObject>(CustomTotemTopResourcePath);
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
                }
                return false;
            }

            return true;
        }
    }

    public static void NewTopPiece(GameObject prefab, Tribe tribe)
    {
        Add(new CustomTotemTop()
        {
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
        foreach (CustomTotemTop totem in totemTops)
        {
            ResourceBank.instance.resources.Add(new ResourceBank.Resource()
            {
                path = "TotemPieces/TotemTop_" + totem.Tribe,
                asset = totem.Prefab
            });
        }
        
        if (AssetBundleHelper.TryGet("customtotemtop", "CustomTotemTop", out GameObject go))
        {
            defaultTotemTop = Add(new CustomTotemTop()
            {
                Prefab = go,
                Tribe = Tribe.None,
            });
            
            ResourceBank.instance.resources.Add(new ResourceBank.Resource()
            {
                path = CustomTotemTopID,
                asset = go
            });
        }
    }
    
    private class CustomTotemTop
    {
        public GameObject Prefab;
        public Tribe Tribe;
    }
}
