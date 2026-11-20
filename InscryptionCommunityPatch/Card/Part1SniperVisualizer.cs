using DiskCardGame;
using Pixelplacement;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// Adds a Sniper Visualizer to Act 1.
/// </summary>
public class Part1SniperVisualizer : ManagedBehaviour
{
    /// <summary>
    /// This function handles visualizing the Start of the Sniper Ability.
    /// </summary>
    /// <param name="sniperSlot">The <see cref="CardSlot"/> in which the Sniper is in.</param>
    public void VisualizeStartSniperAbility(CardSlot sniperSlot)
    {
    }

    /// <summary>
    /// This function handles visualizing the AIm of the Sniper Ability.
    /// </summary>
    /// <param name="sniperSlot">The <see cref="CardSlot"/> in which the Sniper is in.</param>
    /// <param name="targetSlot">The <see cref="CardSlot"/> in which we are trying to target.</param>
    public void VisualizeAimSniperAbility(CardSlot sniperSlot, CardSlot targetSlot)
    {
        if (tempSniperIcon != null)
        {
            CleanUpTargetIcon(tempSniperIcon);
            tempSniperIcon = null;
        }
        if (sniperIconPrefab == null)
            sniperIconPrefab = ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CannonTargetIcon");

        GameObject gameObject = Instantiate(sniperIconPrefab, targetSlot.transform);
        gameObject.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        gameObject.transform.localRotation = Quaternion.identity;
        tempSniperIcon = gameObject;
    }

    /// <summary>
    /// This function handles cleaning up the Target Icon.
    /// </summary>
    /// <param name="icon">The <see cref="GameObject"/> representing the Target Icon.</param>
    public void CleanUpTargetIcon(GameObject icon)
    {
        Tween.LocalScale(icon.transform, Vector3.zero, 0.1f, 0f, Tween.EaseIn, Tween.LoopType.None, null, delegate ()
        {
            Destroy(icon);
        }, true);
    }

    /// <summary>
    /// Handles the Visualization of the Confirmation of the Sniper Ability.
    /// </summary>
    /// <param name="targetSlot">The <see cref="CardSlot"/> in which we are trying to target.</param>
    public void VisualizeConfirmSniperAbility(CardSlot targetSlot)
    {
        if (sniperIconPrefab == null)
            sniperIconPrefab = ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CannonTargetIcon");

        GameObject gameObject = Instantiate(sniperIconPrefab, targetSlot.transform);
        gameObject.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        gameObject.transform.localRotation = Quaternion.identity;
        sniperIcons.Add(gameObject);
        if (tempSniperIcon != null)
        {
            CleanUpTargetIcon(tempSniperIcon);
            tempSniperIcon = null;
        }
    }

    /// <summary>
    /// Handles the Visualization of the Clearing of the Sniper Ability
    /// </summary>
    public void VisualizeClearSniperAbility()
    {
        sniperIcons.ForEach(delegate (GameObject x)
        {
            if (x != null)
            {
                CleanUpTargetIcon(x);
            }
        });
        sniperIcons.Clear();
        if (tempSniperIcon != null)
        {
            CleanUpTargetIcon(tempSniperIcon);
            tempSniperIcon = null;
        }
    }
    
    /// <summary>
    /// A List <see cref="GameObject"/> representing the Sniper Icons.
    /// </summary>
    public readonly List<GameObject> sniperIcons = new();
    private GameObject sniperIconPrefab;
    private GameObject tempSniperIcon;
}
