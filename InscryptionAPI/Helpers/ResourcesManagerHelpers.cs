using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Helpers;

/// <summary>
///  A Helper Set related to the <see cref="ResourcesManager"/>.
/// </summary>
public static class ResourcesManagerHelpers
{
    /// <summary>
    /// Removes a given amount of energy cells, which determines how much energy a player has available at the start of a turn.
    /// Affected by 'ResourcesManager.preventNextEnergyLoss'.
    /// </summary>
    /// <param name="instance">The current <see cref="ResourcesManager"/>.</param>
    /// <param name="amount">How many energy cells to close. Gets capped to the current number of open energy cells.</param>
    public static IEnumerator RemoveMaxEnergy(this ResourcesManager instance, int amount)
    {
        yield return instance.RemoveMaxEnergy(amount, true);
    }

    /// <summary>
    /// A variant of RemoveMaxEnergy that can bypass ResourcesManager.PreventNextEnergyLoss.
    /// Affected by 'ResourcesManager.preventNextEnergyLoss'.
    /// </summary>
    /// <param name="instance">The current <see cref="ResourcesManager"/>.</param>
    /// <param name="amount">How many energy cells to close. Gets capped to the current number of open energy cells.</param>
    public static IEnumerator RemoveMaxEnergy(this ResourcesManager instance, int amount, bool preventable)
    {
        if (preventable && instance.preventNextEnergyLoss)
        {
            instance.preventNextEnergyLoss = false;
            yield break;
        }
        int numToClose = Mathf.Min(instance.PlayerMaxEnergy, amount);

        instance.PlayerMaxEnergy -= numToClose;
        if (instance.PlayerEnergy > instance.PlayerMaxEnergy)
        {
            instance.PlayerEnergy -= numToClose;
            yield return instance.ShowSpendEnergy(numToClose);
        }

        yield return instance.ShowRemoveMaxEnergy(numToClose);
    }
    /// <summary>
    /// A <see cref="IEnumerator"/> used to show removal of Max Energy.
    /// </summary>
    /// <param name="instance">The current <see cref="ResourcesManager"/>.</param>
    /// <param name="amount">The amount of lost Max Energy.</param>
    /// <returns>The Depleted Energy Counter from removing the specified amount of max energy.</returns>
    public static IEnumerator ShowRemoveMaxEnergy(this ResourcesManager instance, int amount)
    {
        PixelResourcesManager pixelManager = instance as PixelResourcesManager;
        for (int i = instance.PlayerMaxEnergy + amount - 1; i >= instance.PlayerMaxEnergy; i--)
        {
            if (pixelManager != null)
            {
                AudioController.Instance.PlaySound2D("crushBlip3", MixerGroup.None, 0.4f, 0f, new AudioParams.Pitch(0.9f + (instance.PlayerMaxEnergy + i) * 0.05f));
                pixelManager.energyRenderers[i].sprite = GetEmptyBatterySprite.emptyBatterySprite;
                pixelManager.BounceRenderer((instance as PixelResourcesManager).energyRenderers[i].transform);
            }
            else if (ResourceDrone.m_Instance != null)
            {
                AudioController.Instance.PlaySound3D("crushBlip3", MixerGroup.TableObjectsSFX, instance.transform.position, 0.4f, 0f, new AudioParams.Pitch(0.9f + (instance.PlayerMaxEnergy + i) * 0.05f));
                ResourceDrone.Instance.cellRenderers[i].material.DisableKeyword("_EMISSION");
                ResourceDrone.Instance.CloseCell(i, false);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    /// <summary>
    /// Gets the Amount of Gems of the passed <see cref="GemType"/>.
    /// </summary>
    /// <param name="instance">The current <see cref="ResourcesManager"/>.</param>
    /// <param name="gem">The <see cref="GemType"/> we want to get the count of.</param>
    /// <returns>The Count of Gems associated with the passed <see cref="GemType"/>.</returns>
    public static int GemsOfType(this ResourcesManager instance, GemType gem)
    {
        return instance.gems.Count(x => x == gem);
    }

    /// <summary>
    /// Counts how many gems of the given type are owned by the specified player.
    /// </summary>
    /// <param name="playerGems">True to check the player's gems or false to check the opponent's gems.</param>
    /// <param name="gemToCheck">GemType to get the count of.</param>
    /// <returns>The number of gems of the given type that are owned by the player/opponent.</returns>
    public static int GemCount(bool playerGems, GemType gemToCheck)
    {
        if (playerGems)
            return ResourcesManager.Instance.GemsOfType(gemToCheck);
        else
            return OpponentGemsManager.Instance.GemsOfType(gemToCheck);
    }
    
    /// <summary>
    /// A bool used to determine whether the Owner has Gems of the specified <see cref="GemType"/>'s.
    /// </summary>
    /// <param name="playerGems">Whether the Check should be based on the Opponent or Player.</param>
    /// <param name="gems">The <see cref="GemType"/>'s in which to check if the Opponent has.</param>
    /// <returns>The result of <see cref="PlayerHasGems"/> if the Check is for the Player, otherwise the result of <see cref="OpponentHasGems"/>.</returns>
    public static bool OwnerHasGems(bool playerGems, params GemType[] gems)
    {
        if (playerGems)
            return PlayerHasGems(gems);
        else
            return OpponentHasGems(gems);
    }
    
    /// <summary>
    /// A bool used to determine whether the Opponent has Gems of the specified <see cref="GemType"/>'s.
    /// </summary>
    /// <param name="gems">The <see cref="GemType"/>'s in which to check if the Opponent has.</param>
    /// <returns>A true if the Opponent has all specified <see cref="GemType"/>'s, a false if not.</returns>
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
    
    /// <summary>
    /// A bool used to determine whether the Player has Gems of the specified <see cref="GemType"/>'s.
    /// </summary>
    /// <param name="gems">The <see cref="GemType"/>'s in which to check if the Player  has.</param>
    /// <returns>A true if the Player has all specified <see cref="GemType"/>'s, a false if not.</returns>
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

/// <summary>
/// A Patch Class used to Get the Empty Battery Sprite in the GBC section.
/// </summary>
[HarmonyPatch(typeof(PixelResourcesManager), nameof(PixelResourcesManager.Start))]
public static class GetEmptyBatterySprite
{
    /// <summary>
    /// The Empty Battery Sprite.
    /// </summary>
    public static Sprite emptyBatterySprite = null;

    /// <summary>
    /// A function used to Get and set the <see cref="emptyBatterySprite"/>.
    /// </summary>
    /// <param name="__instance">The Instance of the Object, in this case <see cref="PixelResourcesManager"/>.</param>
    [HarmonyPostfix]
    private static void GetSprite(PixelResourcesManager __instance)
    {
        if (emptyBatterySprite == null)
            emptyBatterySprite = __instance.energyRenderers[0].sprite;
    }
}