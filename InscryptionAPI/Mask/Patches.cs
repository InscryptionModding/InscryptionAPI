using System.Reflection;
using System.Reflection.Emit;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Items;
using UnityEngine;

namespace InscryptionAPI.Masks;

[HarmonyDebug]
[HarmonyPatch(typeof (LeshyAnimationController), nameof(LeshyAnimationController.SpawnMask), new System.Type[] {typeof (LeshyAnimationController.Mask), (typeof(bool))})]
internal static class LeshyAnimationController_SpawnMask
{
    [HarmonyPrefix]
    internal static bool Prefix(LeshyAnimationController __instance, LeshyAnimationController.Mask mask, bool justHead)
    {
        if (__instance.CurrentMask != null)
        {
            UnityObject.Destroy(__instance.CurrentMask);
            if (!justHead)
            {
                UnityObject.Destroy(__instance.currentHeldMask);
            }
        }

        GameObject prefab = GetPrefab(mask, out CustomMask customMask);
        __instance.CurrentMask = UnityObject.Instantiate<GameObject>(prefab, __instance.maskParent);
        if (__instance.CurrentMask != null)
        {
            MaskManager.InitializeMaskClone(__instance.CurrentMask, customMask);
        }
        
        if (!justHead)
        {
            __instance.currentHeldMask = UnityObject.Instantiate<GameObject>(prefab, __instance.heldMaskParent);
            if (__instance.currentHeldMask != null)
            {
                MaskManager.InitializeMaskClone(__instance.currentHeldMask, customMask);
            }

        }

        return false;
    }
    
    private static GameObject GetPrefab(LeshyAnimationController.Mask maskType, out CustomMask customMask)
    {
        InscryptionAPIPlugin.Logger.LogInfo("[LeshyAnimationController_SpawnMask] GetPrefab " + maskType + " " + maskType);
        customMask = MaskManager.GetRandomMask(maskType);
        if (customMask == null)
        {
            InscryptionAPIPlugin.Logger.LogError("[LeshyAnimationController_SpawnMask] Couldn't get mask for " + customMask.GUID + " " + customMask.Name);
            return null;
        }

        if (!MaskManager.TypeToPrefabLookup.TryGetValue(customMask.ModelType, out ResourceLookup lookup))
        {
            InscryptionAPIPlugin.Logger.LogError("[LeshyAnimationController_SpawnMask] Couldn't get resource for custom mask: " + customMask.GUID + " " + customMask.Name);
            return null;
        }

        GameObject gameObject = lookup.Get<GameObject>();
        if (gameObject == null)
        {
            InscryptionAPIPlugin.Logger.LogError("[LeshyAnimationController_SpawnMask] Got custom mask but prefab is null for: " + customMask.GUID + " " + customMask.Name);
            return MaskManager.TypeToPrefabLookup[MaskManager.ModelType.Prospector].Get<GameObject>();
        }
        
        InscryptionAPIPlugin.Logger.LogError("[LeshyAnimationController_SpawnMask] Got custom mask! " + gameObject.activeSelf);
        return gameObject;
    }
}
