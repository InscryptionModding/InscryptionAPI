using DiskCardGame;
using Sirenix.Utilities;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// An interactable object attached to activated ability icons to make them function when clicked using main input (default left-click on keyboard).
/// </summary>
public class ActivatedAbilityIconInteractable : MainInputInteractable {
    /// <summary>
    /// The original colour of the ability icon.
    /// </summary>
    public Color _originalColor;
    /// <summary>
    /// The Color used when the interactable icon's hitbox is entered. ARGB: <c>255, 127, 127, 127</c>
    /// </summary>
    public readonly Color _enterColor = new(0.5f, 0.5f, 0.5f, 1f);
    /// <summary>
    /// The Color used when the interactable icon is selected. ARGB: <c>255, 191, 191, 191</c>
    /// </summary>
    public readonly Color _selectedColor = new(0.75f, 0.75f, 0.75f, 1f);

    /// <summary>
    /// The colour to use when overriding icon colours. If null, does not override icon colours.
    /// </summary>
    public Color? CurrentColourOverride { get; set; } = null;

    /// <summary>
    /// The Color used when the interactable icon's hitbox is entered. Lerps between <see cref="_originalColor"/> and <see cref="_enterColor"/> by default.
    /// <br/><br/>
    /// Also incorporates <see cref="ModifyOnEnterColour"/> so modders can modify this if desired.
    /// </summary>
    public Color EnterColour {
        get {
            Color enterCol = ModifyOnEnterColour?.Invoke(_enterColor, _playableCard) ?? _enterColor;
            return Color.Lerp(_originalColor, enterCol, 0.5f);
        }
    }
    /// <summary>
    /// The Color used when the interactable icon is selected. Lerps between <see cref="_originalColor"/> and <see cref="_selectedColor"/> by default.
    /// <br/><br/>
    /// Also incorporates <see cref="ModifyOnSelectColour"/> so modders can modify this if desired.
    /// </summary>
    public Color SelectedColour {
        get {
            Color selectCol = ModifyOnEnterColour?.Invoke(_selectedColor, _playableCard) ?? _selectedColor;
            return Color.Lerp(_originalColor, selectCol, 0.5f);
        }
    }

    private Material _renderMaterial;
    private PlayableCard _playableCard;
    private ActivatedAbilityIconInteractable _renderIcon;

    private bool CanActivate =>
        TurnManager.Instance
        && TurnManager.Instance.IsPlayerMainPhase
        && IsValidPlayableCard
        && !BoardManager.Instance.ChoosingSacrifices
        && !BoardManager.Instance.ChoosingSlot
        && GlobalTriggerHandler.Instance.StackSize == 0;

    private bool IsValidPlayableCard => _playableCard != null && _playableCard.OnBoard && !_playableCard.OpponentCard;

    /// <summary>
    /// Whether or not an Ability has been assigned, this is done by checking if the Ability is greater than <see cref="Ability.None"/>.
    /// </summary>
    public bool AbilityAssigned => Ability > Ability.None;

    /// <summary>
    /// The Ability associated with the Activated Ability.
    /// </summary>
    public Ability Ability { get; private set; }

    /// <summary>
    /// Event that's called whenever the game gets <see cref="EnterColour"/>. Allows modders to modify said colour.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <listheader>Arguments:</listheader>
    ///     <item><term><see cref="Color"/> defaultEnterCol</term><description>The default 'on enter' colour used by the API.</description></item>
    ///     <item><term><see cref="PlayableCard"/> card</term><description>The card this ability icon's attached to. Can be null.</description></item>
    /// </list>
    /// Returns a <see cref="Color"/>.
    /// </remarks>
    public static event Func<Color, PlayableCard, Color> ModifyOnEnterColour;

    /// <summary>
    /// Event that's called whenever the game gets <see cref="SelectedColour"/>. Allows modders to modify said colour.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <listheader>Arguments:</listheader>
    ///     <item><term><see cref="Color"/> defaultSelectCol</term><description>The default 'on select' colour used by the API.</description></item>
    ///     <item><term><see cref="PlayableCard"/> card</term><description>The card this ability icon's attached to. Can be null.</description></item>
    /// </list>
    /// Returns a <see cref="Color"/>.
    /// </remarks>
    public static event Func<Color, PlayableCard, Color> ModifyOnSelectColour;

    /// <summary>
    /// Assigns our new interactable to the passed-in Ability.
    /// </summary>
    /// <param name="ability">The ability we want to update the Interactable of.</param>
    public void AssignAbility(Ability ability) {
        if (PatchPlugin.configDisplayCompleteDebugLogsInConsole.Value) {
            PatchPlugin.Logger.LogDebug($"Icon was previously {Ability} - asking to change to a {ability}");
        }

        Ability = ability;
        _renderMaterial = GetComponent<Renderer>().material;

        CursorSelectEnded = (Action<MainInputInteractable>)Delegate.Combine(
            CursorSelectEnded,
            (Action<MainInputInteractable>)delegate
            {
                OnButtonPressed();
            }
        );
    }

    internal void SetRenderIcon(ActivatedAbilityIconInteractable icon) {
        AbilityInfo info = AbilitiesUtil.GetInfo(Ability);

        _renderIcon = icon;
        
        // get the original colour of the icon when first initialising this icon
        if (info.hasColorOverride) {
            _originalColor = info.colorOverride;
        }
        else if (_playableCard?.RenderInfo != null) {
            _originalColor = _playableCard.RenderInfo.defaultAbilityColor;
        }
        else {
            _originalColor = _renderMaterial.color;
        }
        _originalColor = info.hasColorOverride ? info.colorOverride : _renderMaterial.color;
    }
    private void OnButtonPressed()
    {
        if (CanActivate) {
            CustomCoroutine.Instance.StartCoroutine(_playableCard.TriggerHandler.OnTrigger(Trigger.ActivatedAbility, Ability));
        }
    }

    /// <summary>
    /// Sets the color of the Rendering Material related to the Activated Ability.
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color? color) {
        // don't change colours if the activated ability cannot be interacted with
        if (!CanActivate) {
            return;
        }

        //Debug.Log($"SetColour {_playableCard != null}");
        // update the colour override for this icon
        // if there is a corresponding render icon (which controls the actual colour change)
        // then update that as well
        CurrentColourOverride = color;
        if (!_renderIcon.SafeIsUnityNull()) {
            // if _renderIcon is null then the icon colour won't actually update
            _renderIcon.CurrentColourOverride = color;
        }
        
        // re-render the card so the colour updates properly
        _playableCard?.RenderCard();
    }

    /// <summary>
    /// Sets <see cref="CurrentColourOverride"/> to <see cref="EnterColour"/> when the cursor enters the icon's hitbox.
    /// </summary>
    public override void OnCursorEnter() => SetColor(EnterColour);
    /// <summary>
    /// Resets <see cref="CurrentColourOverride"/> when the cursor exits the icon's hitbox.
    /// </summary>
    public override void OnCursorExit() => SetColor(null);
    /// <summary>
    /// Sets <see cref="CurrentColourOverride"/> to <see cref="SelectedColour"/> when clicking on the icon.
    /// </summary>
    public override void OnCursorSelectStart() => SetColor(SelectedColour);
    /// <summary>
    /// Resets <see cref="CurrentColourOverride"/> when the cursor selection ends.
    /// </summary>
    public override void OnCursorSelectEnd() => SetColor(null);
    /// <summary>
    /// Resets <see cref="CurrentColourOverride"/> when the cursor drags the icon.
    /// </summary>
    public override void OnCursorDrag() => SetColor(null);

    private void Start() {
        _playableCard = GetComponentInParent<PlayableCard>();
    }
}