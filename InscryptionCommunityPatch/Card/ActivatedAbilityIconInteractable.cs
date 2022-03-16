using UnityEngine;
using DiskCardGame;

namespace InscryptionCommunityPatch.Card;

public class ActivatedAbilityIconInteractable : MainInputInteractable
{
    private Color _originalColor;
    private readonly Color _enterColor = new(.7f, .7f, .7f, .7f);
    private readonly Color _selectedColor = new(.4f, .4f, .4f, .4f);

    public bool AbilityAssigned => this.Ability > Ability.None;

    public Ability Ability { get; private set; }

    public void AssignAbility(Ability ability)
    {
        this.Ability = ability;
        this._originalColor = base.GetComponent<Renderer>().material.color;
    }

    public void SetColor(Color color)
    {
        base.GetComponent<Renderer>().material.color = color;
    }

    public override void OnCursorEnter() => this.SetColor(_enterColor);

    public override void OnCursorExit() => this.SetColor(_originalColor);

    public override void OnCursorSelectStart() => this.SetColor(_selectedColor);

    public override void OnCursorSelectEnd() => this.SetColor(_originalColor);

    public override void OnCursorDrag() => this.SetColor(_originalColor);
}