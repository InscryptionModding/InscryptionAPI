using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

public class ActivatedAbilityHandler3D : ManagedBehaviour
{
    public GameObject currentIconGroup;
    
    public List<ActivatedAbilityIconInteractable> interactables = new();

    public void AddInteractable(ActivatedAbilityIconInteractable interactable)
    {
        interactables.Add(interactable);
    }

    public void UpdateInteractableList(GameObject defaultIconGroup)
    {
        interactables.Clear();
        currentIconGroup = defaultIconGroup;
        currentIconGroup
            .GetComponentsInChildren<AbilityIconInteractable>()
            .Where(elem => AbilitiesUtil.GetInfo(elem.Ability).activated)
            .Do(abIcon =>
            {
                if (abIcon.GetComponent<ActivatedAbilityIconInteractable>())
                {
                    AddInteractable(abIcon.GetComponent<ActivatedAbilityIconInteractable>());
                }
                else
                {
                    var go = abIcon.gameObject;
                    go.layer = 0;

                    var interactable = go.AddComponent<ActivatedAbilityIconInteractable>();
                    interactable.AssignAbility(abIcon.Ability);

                    AddInteractable(interactable);
                }
            });
        PatchPlugin.Logger.LogDebug($"[Handler3D] Updated interactable list: [{interactables.Join(interactable => $"GO [{interactable.gameObject}] Ability [{interactable.Ability}]")}]");
    }

    public void OnDestroy()
    {
        foreach(var interactable in interactables)
        {
            Destroy(interactable);
        }
    }
}