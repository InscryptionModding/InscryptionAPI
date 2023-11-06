## DamageShieldBehaviour
---
Ever wanted to create your own version of the Armoured sigil, but were frustrated by the game's simplistic boolean logic for shields?
Worry not! Using the API, you can now easily create your own!
Simply create a class that inherits from DamageShieldBehaviour (or ActivatedDamageShieldBehaviour) and you're good to go.

Abilities using either of these classes must specify a starting number of shields the sigil will provide to a card.

A basic example can be found here:
```csharp
public class APIDeathShield : DamageShieldBehaviour
{
    public override Ability Ability => Ability.DeathShield;
    
    // for stackable sigils, you'll want to set StartingNumShields to something like this if you want the stacks to be counted
    public override int StartingNumShields => base.Card.GetAbilityStacks(Ability);

    // For non-stackable sigils (or perhaps special cases) just setting it to a number will suffice.
    // public override int StartingNumShields => 1;
}
```

You can continue to modify the shield count during battle:
```csharp
public void RegainShields
{
    // NumShield tracks the current shield amount for the ability instance; it cannot be negative
    if (NumShield == 0)
    {
        ResetShields(true); // Resets NumShield to the starting amount and updates the card display
    }
}
public void ChangeShieldCount()
{
    // to modify shield count you need to use numShield NOT NumShield
    // NumShield cannot be modified directly
    if (addShield == true)
    {
        numShield++;
    }
    else
    {
        numShield--;
    }
    base.Card.RenderCard(); // update the card display if we need to
}
```

You can check how many shields a card has using card.GetTotalShields().

For stacking shields sigils, one stack is hidden whenever the card is damaged IF the ability has been set to behave that way using SetHideSingleStacks().
Otherwise, the sigil is hidden only when that specific ability's internal shield count hits 0.