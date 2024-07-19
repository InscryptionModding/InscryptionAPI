## Slot Modifications
---
The API now supports adding abilities and behaviors to card slots! These "slot modifications" are implemented very similarly to how you would implement sigils; you just need to code your modification logic as a subclass of `SlotModificationBehaviour` and make some custom artwork for what the new slot should look like.

### Creating Slot Textures and Sprites

Slot textures in 3D game zones are 154x226 pixels. You can either create a single slot texture for all zones, or create a separate texture for Leshy, P03, Grimora, and Magnificus.

Slot textures in 2D game zones are 44x58 pixels. There are five different themes for 2D battles (Nature, Tech, Undead, Wizard, and Finale), and slots change their appearance when you hover over them. Additionally, you may wish to have a different texture for the opponent than for the player (this is most likely to happen in the Nature theme as the claw icons faces down for the opponent and up for the player). In order to accomodate all of these different combinations, you are given a number of options when supplying textures to the `SlotModificationManager`:

- If you provide a 44x58 texture, the manager will create 10 variations of it automatically, recoloring it to fit the color (standard and highlighted/mouse-over) of each theme. All black and transparent pixels will be left as-is; all remaining pixels will be replaced with the appropriate color for the theme.
- If you provide a 220x116 texture, the manager will slice this into 10 sprites - two rows, five columns. The first row will be the standard slot, and the second row will be the slot when hovering. In order, the columns will be Nature, Undead, Tech, Wizard, Finale.
- If you provide a 220x232 texture, it behaves the same as above, except rows 3 and 4 are used for the opponent slots.

### Creating a Slot Modification Behaviour

You need to create a subclass of `SlotModificationBehaviour` to implement your slot's logic. You can also add on API interface triggers as part of this.

For example, this slot deals one damage to the card in it every end step:

```csharp
class SharpSlotBehaviour : SlotModificationBehaviour
{
    public override bool RespondsToTurnEnd(bool playerTurnEnd) => playerTurnEnd == Slot.IsPlayerSlot;

    public override IEnumerator OnTurnEnd(bool playerTurnEnd)
    {
        if (Slot.Card != null)
            yield return Slot.Card.TakeDamage(1, null);
    }
}
```

...and this slot adds 1 passive attack to the card in it...

```csharp
public class BuffSlot : SlotModificationBehaviour, IPassiveAttackBuff
{
    public int GetPassiveAttackBuff(PlayableCard target)
    {
        return Slot.Card == target ? 1 : 0;
    }
}
```

If you want your slot to grant another ability to the card in it, there's a helper for that: `SlotModificationGainAbilityBehaviour`:

```csharp
public class SharpQuillsSlot : SlotModificationGainAbilityBehaviour
{ 
    protected override Ability AbilityToGain => Ability.Sharp;
}
```

If you want your slot to take an action when it is first created or when it is removed, override `Setup` and `Cleanup` respectively. Often these could be used to add additional visual flair to your slot:

```csharp
public class AwesomeLookingSlot : SlotModificationBehaviour
{
    public override IEnumerator Setup()
    {
        yield return ShowSomeAwesomeVisuals();
    }

    public override IEnumerator Cleanup(SlotModificationManager.ModificationType replacement)
    {
        yield return DestroyMyAwesomeVisuals();
    }
}
```

### Registering A Slot Modification

The pattern is very similar to creating a new sigil; you need to call the `SlotModificationManager` to register your slot mod and then store the result you get back:

```csharp
public static readonly SlotModificationManager.ModificationType SharpQuillsSlot = SlotModificationManager.New(
    "MyPluginGuid",
    "SharpQuillsSlot",
    typeof(SharpQuillsSlot),
    TextureHelper.GetImageAsTexture("my_3d_card_slot.png", typeof(SharpQuillsSlot).Assembly),
    TextureHelper.GetImageAsTexture("my_2d_card_slot.png", typeof(SharpQuillsSlot).Assembly)
)
```

### Activating A Slot Modification

You'll almost always end up creating a slot modification as part of another sigil. Here, we have an example custom sigil that activates the slot when the card dies. You do this by using the extension method `SetSlotModification`.

```csharp
public class LeaveSharpBehindBehaviour : AbilityBehaviour
{
    public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Card.OnBoard;

    public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
    {
        // SharpQuillSlot is the ID that was returned by the SlotModificationManager
        yield return Card.Slot.SetSlotModification(SharpQuillsSlot);
    }
}
```