using DiskCardGame;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// Fixes related to how Activated Ability Interactable's are rendered on cards.
/// </summary>
public class ActivatedAbilityIconInteractable : MainInputInteractable
{
    private Material _renderMaterial;
    /// <summary>
    /// The Original Interactable Color.
    /// </summary>
    public Color _originalColor;
    /// <summary>
    /// The Color used when the region is entered: ARGB: <c>178, 178, 178, 178</c>
    /// </summary>
    public readonly Color _enterColor = new(.7f, .7f, .7f, .7f);
    /// <summary>
    /// The Color used when the region is selected: ARGB: <c>102, 102, 102, 102</c>
    /// </summary>
    public readonly Color _selectedColor = new(.4f, .4f, .4f, .4f);

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

    /// <summary>
    /// Whether or not an Ability has been assigned, this is done by checking if the Ability is greater than <see cref="Ability.None"/>.
    /// </summary>
    public bool AbilityAssigned => Ability > Ability.None;

    /// <summary>
    /// The Ability associated with the Activated Ability.
    /// </summary>
    public Ability Ability { get; private set; }

    /// <summary>
    /// Assigns our new interactable to the passed in Ability.
    /// </summary>
    /// <param name="ability">The ability we want to update the Interactable of.</param>
    public void AssignAbility(Ability ability)
    {
        if (PatchPlugin.configDisplayCompleteDebugLogsInConsole.Value)
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

    /// <summary>
    /// Set's the color of the Rendering Material related to the Activated Ability.
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color) => _renderMaterial.color = color;

    /// <summary>
    /// Set's the color to <see cref="_enterColor"/> using <see cref="SetColor"/> when the cursor enters the region.
    /// </summary>
    public override void OnCursorEnter() => SetColor(_enterColor);
    /// <summary>
    /// Set's the color to <see cref="_originalColor"/> using <see cref="SetColor"/> when the cursor exits the region.
    /// </summary>
    public override void OnCursorExit() => SetColor(_originalColor);
    /// <summary>
    /// Set's the color to <see cref="_selectedColor"/> using <see cref="SetColor"/> when the cursor selects the region.
    /// </summary>
    public override void OnCursorSelectStart() => SetColor(_selectedColor);
    /// <summary>
    /// Set's the color to <see cref="_originalColor"/> using <see cref="SetColor"/> when the cursor unselects the region.
    /// </summary>
    public override void OnCursorSelectEnd() => SetColor(_originalColor);
    /// <summary>
    /// Set's the color to <see cref="_originalColor"/> using <see cref="SetColor"/> when the cursor drags the region.
    /// </summary>
    public override void OnCursorDrag() => SetColor(_originalColor);
}