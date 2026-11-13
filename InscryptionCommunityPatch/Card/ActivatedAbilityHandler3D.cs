using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// Fixes the Activated Ability handler in 3D Environments.
/// </summary>
public class ActivatedAbilityHandler3D : ManagedBehaviour
{
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
    /// <param name="controllerActiveIcons">The list of Interactable used to replace the current <see cref="interactables"/> list.</param>
    public void UpdateInteractableList(List<AbilityIconInteractable> controllerActiveIcons)
    {
        interactables.Clear();
        currentIconGroup = controllerActiveIcons;
        controllerActiveIcons
            .Where(elem => AbilitiesUtil.GetInfo(elem.Ability).activated)
            .Do(abIcon =>
            {
                ActivatedAbilityIconInteractable currentIcon = abIcon.gameObject.GetComponent<ActivatedAbilityIconInteractable>();
                if (currentIcon)
                {
                    currentIcon.AssignAbility(abIcon.Ability);
                    AddInteractable(currentIcon);
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

        if (PatchPlugin.configDisplayCompleteDebugLogsInConsole.Value)
            PatchPlugin.Logger.LogDebug($"[Handler3D] Updated interactable list: [{interactables.Join(interactable => $"GO [{interactable.gameObject}] Ability [{interactable.Ability}]")}]");
    }

    private void OnDestroy()
    {
        foreach (var interactable in interactables)
        {
            Destroy(interactable);
        }
    }
}