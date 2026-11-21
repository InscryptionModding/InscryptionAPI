using DiskCardGame;
using HarmonyLib;
using Sirenix.Utilities;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// Handles activated abilities outside Act 2 to make them function correctly.
/// </summary>
public class ActivatedAbilityHandler3D : ManagedBehaviour {
    /// <summary>
    /// The current Icon Group of <see cref="AbilityIconInteractable"/>.
    /// </summary>
    public List<AbilityIconInteractable> currentIconGroup;

    /// <summary>
    /// The current Interactable Group of <see cref="ActivatedAbilityIconInteractable"/>.
    /// </summary>
    public List<ActivatedAbilityIconInteractable> interactables = new();

    /// <summary>
    /// A function used to add an Interactable to <see cref="interactables"/>.
    /// </summary>
    /// <param name="interactable">The Interactable that we want to add.</param>
    public void AddInteractable(ActivatedAbilityIconInteractable interactable)
    {
        interactables.Add(interactable);
    }

    /// <summary>
    /// Updates the List of <see cref="interactables"/> to the passed in List.
    /// </summary>
    /// <param name="activeIconsCard">The list of ability interactables to replace the current <see cref="interactables"/> list.</param>
    /// /// <param name="activeIconsRender">The list of ability interactables used by the game to render ability icons.</param>
    public void UpdateInteractableList(List<AbilityIconInteractable> activeIconsCard, List<AbilityIconInteractable> activeIconsRender) {
        interactables.Clear();
        currentIconGroup = activeIconsCard;
        //PatchPlugin.Logger.LogInfo($"{activeIconsCard.Count} {activeIconsRender.Count}");
        for (int i = 0; i < activeIconsCard.Count; i++) {
            // if this active icon is for an activated sigil
            if (AbilitiesUtil.GetInfo(activeIconsCard[0].Ability).activated) {
                ActivatedAbilityIconInteractable currentIcon = activeIconsCard[0].gameObject.GetComponent<ActivatedAbilityIconInteractable>();
                // if there is no activated interactable, add one
                if (currentIcon.SafeIsUnityNull()) {
                    GameObject obj = activeIconsCard[0].gameObject;
                    obj.layer = 0;

                    currentIcon = obj.AddComponent<ActivatedAbilityIconInteractable>();
                }

                currentIcon.AssignAbility(activeIconsCard[0].Ability);

                // if there is a corresponding render interactable object, connect it to the active interactable object
                if (i >= 0 && i < activeIconsRender.Count && activeIconsRender.Count > 0) {
                    ActivatedAbilityIconInteractable renderIcon = activeIconsRender[i].gameObject.GetComponent<ActivatedAbilityIconInteractable>();
                    if (renderIcon.SafeIsUnityNull()) {
                        renderIcon = activeIconsRender[i].gameObject.AddComponent<ActivatedAbilityIconInteractable>();
                    }
                    renderIcon.AssignAbility(currentIcon.Ability);
                    
                    currentIcon.SetRenderIcon(renderIcon);
                }
                AddInteractable(currentIcon);
            }
        }

        if (PatchPlugin.configDisplayCompleteDebugLogsInConsole.Value) {
            PatchPlugin.Logger.LogDebug($"[Handler3D] Updated interactable list: [{interactables.Join(interactable => $"GO [{interactable.gameObject}] Ability [{interactable.Ability}]")}]");
        }
    }

    /// <summary>
    /// Updates the List of <see cref="interactables"/> to the passed in List.
    /// </summary>
    /// <param name="controllerActiveIcons">The list of Interactable used to replace the current <see cref="interactables"/> list.</param>
    [Obsolete("Does not update icon rendering. Use UpdateInteractableList(List, List) instead.")]
    public void UpdateInteractableList(List<AbilityIconInteractable> controllerActiveIcons) {
        UpdateInteractableList(controllerActiveIcons, new(1));
    }

    private void OnDestroy() {
        foreach (var interactable in interactables) {
            Destroy(interactable);
        }
    }
}