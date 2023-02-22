using DiskCardGame;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

public class ActivatedAbilityIconInteractable : MainInputInteractable
{
    private Material _renderMaterial;
    private Color _originalColor;
    private readonly Color _enterColor = new(.7f, .7f, .7f, .7f);
    private readonly Color _selectedColor = new(.4f, .4f, .4f, .4f);

    private PlayableCard _playableCard;

    private bool CanActivate =>
        TurnManager.Instance
        && TurnManager.Instance.IsPlayerMainPhase
        && IsValidPlayableCard
        && !BoardManager.Instance.ChoosingSacrifices
        && !BoardManager.Instance.ChoosingSlot
        && GlobalTriggerHandler.Instance.StackSize == 0;

    private void Start()
    {
        _playableCard = GetComponentInParent<PlayableCard>();
    }

    private bool IsValidPlayableCard => _playableCard && _playableCard.OnBoard && !_playableCard.OpponentCard;

    public bool AbilityAssigned => Ability > Ability.None;

    public Ability Ability { get; private set; }

    public void AssignAbility(Ability ability)
    {
        if (PatchPlugin.configFullDebug.Value)
            PatchPlugin.Logger.LogDebug($"Icon was previously {Ability} - asking to change to a {ability}");
        Ability = ability;
        _renderMaterial = GetComponent<Renderer>().material;
        _originalColor = _renderMaterial.color;
        CursorSelectEnded = (Action<MainInputInteractable>)Delegate.Combine(
            CursorSelectEnded,
            (Action<MainInputInteractable>)delegate
            {
                OnButtonPressed();
            }
        );
    }

    private void OnButtonPressed()
    {
        if (CanActivate)
        {
            CustomCoroutine.Instance.StartCoroutine(_playableCard.TriggerHandler.OnTrigger(Trigger.ActivatedAbility, Ability));
        }
    }

    public void SetColor(Color color) => _renderMaterial.color = color;

    public override void OnCursorEnter() => SetColor(_enterColor);

    public override void OnCursorExit() => SetColor(_originalColor);

    public override void OnCursorSelectStart() => SetColor(_selectedColor);

    public override void OnCursorSelectEnd() => SetColor(_originalColor);

    public override void OnCursorDrag() => SetColor(_originalColor);
}