using DiskCardGame;

namespace InscryptionCommunityPatch.Card;

public class ActivatedAbilityHandler3D : ManagedBehaviour
{
    private readonly List<ActivatedAbilityIconInteractable> _interactables = new();

    private PlayableCard _playableCard;

    private bool CanPress => TurnManager.Instance != null
        && this._playableCard != null
        && this._playableCard.OnBoard
        && this._playableCard.slot.IsPlayerSlot
        && !BoardManager.Instance.ChoosingSacrifices
        && !BoardManager.Instance.ChoosingSlot
        && TurnManager.Instance.IsPlayerMainPhase
        && GlobalTriggerHandler.Instance.StackSize == 0;

    //TODO: fix this to work with multiple
    public void AddInteractable(ActivatedAbilityIconInteractable interactable)
    {
        this._interactables.Add(interactable);

        interactable.CursorSelectEnded = (Action<MainInputInteractable>)Delegate.Combine(
            interactable.CursorSelectEnded,
            new Action<MainInputInteractable>(
                delegate(MainInputInteractable i)
                {
                    if (this.CanPress && i is ActivatedAbilityIconInteractable icon)
                        this.OnButtonPressed(icon.Ability);
                }
            )
        );
    }

    public void SetCard(PlayableCard card)
    {
        this._playableCard = card;
    }

    public void OnDestroy()
    {
        foreach (var interactable in this._interactables)
        {
            Destroy(interactable);
        }
    }

    private void OnButtonPressed(Ability ability)
    {
        CustomCoroutine.Instance.StartCoroutine(
            this._playableCard.TriggerHandler.OnTrigger(
                Trigger.ActivatedAbility,
                ability
            )
        );
    }
}
