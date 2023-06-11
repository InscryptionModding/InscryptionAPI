using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

public class ActivatedAbilityHandler3D : ManagedBehaviour
{
    public List<AbilityIconInteractable> currentIconGroup;

    public List<ActivatedAbilityIconInteractable> interactables = new();

    public void AddInteractable(ActivatedAbilityIconInteractable interactable)
    {
        interactables.Add(interactable);
    }

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

        if (PatchPlugin.configFullDebug.Value)
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