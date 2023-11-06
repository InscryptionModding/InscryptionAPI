## Ability Management
---
Abilities are unfortunately a little more difficult to manage than cards.
First of all, they have an attached 'AbilityBehaviour' type which you must implement.
Second, the texture for the ability is not actually stored on the AbilityInfo object itself; it is managed separately (bizarrely, the pixel ability icon *is* on the AbilityInfo object, but we won't get into all that).

Regardless, the API will help you manage all of this with the helpful AbilityManager class. Simply create an AbilityInfo object, and then call AbilityManager.Add with that info object, the texture for the icon, and the type that implements the ability.

Abilities inherit from DiskCardGame.AbilityBehaviour

```c#
AbilityInfo myinfo = ...;
Texture2D myTexture = ...;
AbilityManager.Add(MyPlugin.guid, myInfo, typeof(MyAbilityType), myTexture);
```

You can also use the AbilityManager.New method to simplify some of this process:

```c#
AbilityInfo myInfo = AbilityManager.New(MyPlugin.guid, "Ability Name", "Ability Description", typeof(MyAbilityType), "/art/my_icon.png");
```

And there are also extension methods to help you here as well:

```c#
AbilityManager.New(MyPlugin.guid, "Ability Name", "Ability Description", typeof(MyAbilityType), "/art/my_icon.png")
    .SetDefaultPart1Ability()
    .SetPixelIcon("/art/my_pixel_icon.png");
```

### Ability Extensions
---
- **SetCustomFlippedTexture:** Use this to give the ability a custom texture when it belongs to the opponent.
- **SetPixelIcon:** Use this to set the texture used when rendered as a GBC card.
- **AddMetaCategories:** Adds any number of metacategories to the ability. Will not duplicate.
- **SetDefaultPart1Ability:** Makes this appear in the part 1 rulebook and randomly appear on mid-tier trader cards and totems.
- **SetDefaultPart3Ability:** Makes this appear in the part 3 rulebook and be valid for create-a-card (make sure your power level is accurate here!)
- **SetActivated:** Sets whether or not the ability is an activated ability. Activated abilities can be clicked to trigger an effect.
- **SetPassive:** Sets whether or not the ability is a passive ability. Passive abilities don't do anything.
- **SetOpponentUsable:** Sets whether or not the ability can be used by the opponent (such as in a Totem battle).
- **SetConduit:** Sets whether or not the ability can be used to complete Circuits.
- **SetConduitCell:** Sets whether or not the ability is a conduit cell. Unsure of what this does.
- **SetCanStack:** Sets whether multiple copies of the ability will stack, activating once per copy. Optionally controls stack behaviour should a card with the ability evolve (see below).
- **SetTriggersOncePerStack:** Sets whether the ability (if it stacks) will only ever trigger once per stack. There's a...'feature' where stackable abilities will trigger twice per stack after a card evolves.

## Programming Abilities
---
Abilities require an instance of AbilityInfo that contains the information about the ability, but they also require you to write your own class that inherits from AbilityBehaviour and describes how the ability functions.

AbilityBehaviour contains a *lot* of virtual methods. For each event that can happen during a battle, there will be a 'RespondsToXXX' and an 'OnXXX' method that you need to override. The purpose of the 'RespondsToXXX' is to indicate if your ability cares about that event - you must return True in that method for the ability to fire. Then, to actually make the ability function, you need to implement your custom behavior in the 'OnXXX' method.

See this example from the base game:

```c#
public class Sharp : AbilityBehaviour
{
    public override Ability Ability => Ability.Sharp;

    public override bool RespondsToTakeDamage(PlayableCard source) => source != null && source.Health > 0;

    public override IEnumerator OnTakeDamage(PlayableCard source)
    {
        yield return base.PreSuccessfulTriggerSequence();
        base.Card.Anim.StrongNegationEffect();
        yield return new WaitForSeconds(0.55f);
        yield return source.TakeDamage(1, base.Card);
        yield return base.LearnAbility(0.4f);
        yield break;
    }
}
```

### Stat Icons
Stat icons are unique abilities that apply specifically to a card's Health and/or Attack stats.

Stat icons require an instance of StatIconInfo that contains the information about the ability, but they also require you to write your own class that inherits from VariableStatBehaviour and describes how the ability functions.

When you implement a variable stat behavior, you need to implement the abstract method GetStateValues. This method returns an array of integers: the value at index 0 is the variable attack power, and the value at index 1 is the variable health.

Here is an example from the base game:

```c#
public class BellProximity : VariableStatBehaviour
{
    protected override SpecialStatIcon IconType => SpecialStatIcon.Bell;

    protected override int[] GetStatValues()
    {
        int num = BoardManager.Instance.PlayerSlotsCopy.Count - base.PlayableCard.Slot.Index;
        int[] array = new int[2];
        array[0] = num;
        return array;
    }
}
```

NOTE: you need to be very careful about how complicated the logic is in GetStatValues. This will be called *every frame!!* If you're not careful, you could bog the game down substantially.

### Special Triggered Abilities
Special triggered abilities are a lot like regular abilities; however, they are 'invisible' to the player (that is, they do not have icons or rulebook entries).
As such, the API for these is very simple.
You simply need to provide your plugin guid, the name of the ability, and the type implementing the ability, and you will be given back a wrapper containing the ID of your newly created special triggered ability.

Special triggered abilities inherit from DiskCardGame.SpecialCardBehaviour.

```c#
public readonly static SpecialTriggeredAbility MyAbilityID = SpecialTriggeredAbilityManager.Add(MyPlugin.guid, "Special Ability", typeof(MySpecialTriggeredAbility)).Id;
```

And now MyAbilityID can be added to CardInfo objects.

Special abilities are programmed the same as regular abilities, except they do not have a metadata object associated with them (because they are not described or documented for the player) and they inherit from SpecialCardBehaviour instead of AbilityBehaviour.

Here is an example from the base game:

```c#
public class TrapSpawner : SpecialCardBehaviour
{
    public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => base.PlayableCard.OnBoard;

    public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
    {
        yield return new WaitForSeconds(0.35f);
        yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("Trap"), base.PlayableCard.Slot, 0.1f, true);
        yield return new WaitForSeconds(0.35f);
        yield break;
    }
}
```

Note how the code uses 'base.PlayableCard' instead of 'base.Card'.
Keep this in mind when making special abilities.