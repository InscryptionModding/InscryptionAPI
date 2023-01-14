using DiskCardGame;
using Pixelplacement;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;
internal class Part1SniperVisualizer : ManagedBehaviour
{
    public void VisualizeStartSniperAbility(CardSlot sniperSlot)
    {
    }

    public void VisualizeAimSniperAbility(CardSlot sniperSlot, CardSlot targetSlot)
    {
        if (tempSniperIcon != null)
        {
            CleanUpTargetIcon(tempSniperIcon);
            tempSniperIcon = null;
        }
        if (sniperIconPrefab == null)
        {
            sniperIconPrefab = ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CannonTargetIcon");
        }
        GameObject gameObject = Instantiate(sniperIconPrefab, targetSlot.transform);
        gameObject.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        gameObject.transform.localRotation = Quaternion.identity;
        tempSniperIcon = gameObject;
    }

    public void CleanUpTargetIcon(GameObject icon)
    {
        Tween.LocalScale(icon.transform, Vector3.zero, 0.1f, 0f, Tween.EaseIn, Tween.LoopType.None, null, delegate ()
        {
            Destroy(icon);
        }, true);
    }

    public void VisualizeConfirmSniperAbility(CardSlot targetSlot)
    {
        if (sniperIconPrefab == null)
        {
            sniperIconPrefab = ResourceBank.Get<GameObject>("Prefabs/Cards/SpecificCardModels/CannonTargetIcon");
        }
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


    private List<GameObject> sniperIcons = new();
    private GameObject sniperIconPrefab;
    private GameObject tempSniperIcon;
}
