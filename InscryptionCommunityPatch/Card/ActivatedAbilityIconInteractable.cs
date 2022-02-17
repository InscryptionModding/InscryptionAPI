using UnityEngine;
using DiskCardGame;

namespace InscryptionCommunityPatch.Card;

public class ActivatedAbilityIconInteractable : MainInputInteractable
{
    private Color originalColor;
    private Color enterColor = new(.7f, .7f, .7f, .7f);
    private Color selectedColor = new(.4f, .4f, .4f, .4f);

    public bool AbilityAssigned { get => this.Ability > Ability.None; }

    public Ability Ability { get; private set; }

    public void AssigneAbility(Ability ability)
    {
        this.Ability = ability;
        this.originalColor = base.GetComponent<Renderer>().material.color;
    }

    public void SetColor(Color color)
    {
        base.GetComponent<Renderer>().material.color = color;
    }

    public override void OnCursorEnter() => this.SetColor(enterColor);

    public override void OnCursorExit() => this.SetColor(originalColor);

    public override void OnCursorSelectStart() => this.SetColor(selectedColor);

    public override void OnCursorSelectEnd() => this.SetColor(originalColor);

    public override void OnCursorDrag() => this.SetColor(originalColor);
}