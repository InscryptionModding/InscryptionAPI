using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Card;


public class APIDeathShield : DamageShieldBehaviour
{
    public override Ability Ability => Ability.DeathShield;

    public override int StartingNumShields => base.Card.GetAbilityStacks(Ability);
}

public abstract class DamageShieldBehaviour : AbilityBehaviour
{
    public abstract int StartingNumShields { get; } // how many shields this sigil will provide initially

    public int numShields;
    public int NumShields => Mathf.Max(numShields, 0);

    public bool HasShields() => NumShields > 0;

    private void Start()
    {
        if (base.Card == null) return;

        numShields = StartingNumShields;
    }

    public void ResetShields(bool updateDisplay)
    {
        bool depleted = !HasShields();
        numShields = StartingNumShields;
        base.Card.Status.lostShield = false;

        if (Ability.GetHideSingleStacks())
            base.Card.Status.hiddenAbilities.RemoveAll(x => x == Ability);
        else if (depleted)
            base.Card.Status.hiddenAbilities.Remove(Ability);

        if (updateDisplay)
        {
            base.Card.UpdateFaceUpOnBoardEffects();
            base.Card.RenderCard();
        }
    }
}

public abstract class ActivatedDamageShieldBehaviour : ActivatedAbilityBehaviour
{
    public abstract int StartingNumShields { get; } // how many shields this sigil will provide initially

    public int numShields;
    public int NumShields => Mathf.Max(numShields, 0);

    public bool HasShields() => NumShields > 0;

    private void Start()
    {
        if (base.Card == null) return;

        numShields = StartingNumShields;
    }

    public void ResetShields(bool updateDisplay)
    {
        bool depleted = !HasShields();
        numShields = StartingNumShields;
        base.Card.Status.lostShield = false;

        if (Ability.GetHideSingleStacks())
            base.Card.Status.hiddenAbilities.RemoveAll(x => x == Ability);
        else if (depleted)
            base.Card.Status.hiddenAbilities.Remove(Ability);

        if (updateDisplay)
        {
            base.Card.UpdateFaceUpOnBoardEffects();
            base.Card.RenderCard();
        }
    }
}