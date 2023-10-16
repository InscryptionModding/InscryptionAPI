using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Helpers;

public static class ResourcesManagerHelpers
{
    public static IEnumerator RemoveMaxEnergy(this ResourcesManager instance, int amount)
    {
        int num = Mathf.Max(instance.PlayerMaxEnergy - amount, 0);
        int numToClose = instance.PlayerMaxEnergy - num;

        instance.PlayerMaxEnergy = num;
        if (instance.PlayerEnergy > instance.PlayerMaxEnergy)
            instance.PlayerEnergy = instance.PlayerMaxEnergy;

        yield return instance.ShowRemoveMaxEnergy(numToClose);
    }
    public static IEnumerator ShowRemoveMaxEnergy(this ResourcesManager instance, int amount)
    {
        if (instance is PixelResourcesManager)
        {
            for (int i = instance.PlayerMaxEnergy + amount - 1; i >= instance.PlayerMaxEnergy; i--)
            {
                AudioController.Instance.PlaySound2D("crushBlip3", MixerGroup.None, 0.4f, 0f, new AudioParams.Pitch(0.9f + (instance.PlayerMaxEnergy + i) * 0.05f));
                (instance as PixelResourcesManager).energyRenderers[i].sprite = GetEmptyBatterySprite.emptyBatterySprite;
                (instance as PixelResourcesManager).BounceRenderer((instance as PixelResourcesManager).energyRenderers[i].transform);
                yield return new WaitForSeconds(0.05f);
            }
        }
        else if (ResourceDrone.m_Instance != null)
        {
            for (int i = instance.PlayerMaxEnergy + amount - 1; i >= instance.PlayerMaxEnergy; i--)
            {
                AudioController.Instance.PlaySound3D("crushBlip3", MixerGroup.TableObjectsSFX, instance.transform.position, 0.4f, 0f, new AudioParams.Pitch(0.9f + (instance.PlayerMaxEnergy + i) * 0.05f));
                ResourceDrone.Instance.cellRenderers[i].material.DisableKeyword("_EMISSION");
                ResourceDrone.Instance.CloseCell(i, false);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
    public static int GemsOfType(this ResourcesManager instance, GemType gem)
    {
        return instance.gems.Count(x => x == gem);
    }
    public static int GemCount(bool playerGems, GemType gemToCheck)
    {
        if (playerGems)
            return ResourcesManager.Instance.GemsOfType(gemToCheck);
        else
            return OpponentGemsManager.Instance.GemsOfType(gemToCheck);
    }
    public static bool OwnerHasGems(bool playerGems, params GemType[] gems)
    {
        if (playerGems)
            return PlayerHasGems(gems);
        else
            return OpponentHasGems(gems);
    }
    public static bool OpponentHasGems(params GemType[] gems)
    {
        if (OpponentGemsManager.Instance == null)
            return false;

        foreach (GemType gem in gems)
        {
            if (!OpponentGemsManager.Instance.HasGem(gem))
                return false;
        }
        return true;
    }
    public static bool PlayerHasGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
        {
            if (!ResourcesManager.Instance.HasGem(gem))
                return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(PixelResourcesManager), nameof(PixelResourcesManager.Start))]
public static class GetEmptyBatterySprite
{
    public static Sprite emptyBatterySprite = null;
    
    [HarmonyPostfix]
    private static void GetSprite(PixelResourcesManager __instance)
    {
        if (emptyBatterySprite == null)
            emptyBatterySprite = __instance.energyRenderers[0].sprite;
    }
}