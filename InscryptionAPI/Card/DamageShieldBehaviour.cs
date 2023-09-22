using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Card;


public class APIDeathShield : DamageShieldBehaviour
{
    public override Ability Ability => Ability.DeathShield;

    public override int StartingNumShields => 1;
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

    public void ResetShields()
    {
        numShields = StartingNumShields;
        base.Card.Status.lostShield = false;
    }
}