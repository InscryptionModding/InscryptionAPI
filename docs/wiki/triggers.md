## Triggers and Interfaces
---
The API adds a number of interfaces you can use to add additional functionality to your ability.
It also adds a new class: `ExtendedAbilityBehaviour`, which has all the interfaces already implemented for immediate use, saving you time.

### Passive Attack and Health Buffs
To do this, you need to override GetPassiveAttackBuff(PlayableCard target) or GetPassiveAttackBuff(PlayableCard target) to calculate the appropriate buffs.
These return an int representing the buff to give to 'target'.

In battle, the game will iterate across all cards on the board and check whether they should receive the buffs; this is what 'target' refers to; the current card being checked.
You will need to write the logic for determining what cards should get the buff, as well as what buff they should receive.

Note: you need to be very careful about how complicated the logic is in GetPassiveAttackBuffs and GetPassiveHealthBuffs.
These methods will be called *every frame* for *every instance of the ability!!*
If you're not careful, you could bog the game down substantially!

### TriggerWhenFaceDown
The API allows you to add custom properties to an ability, using the same methods as for adding custom properties to cards.

The API also allows you to control whether the ability's triggers will activate when the card is facedown.
You will need to override TriggerWhenFaceDown to return true.

There are also 2 other bools you can override for more control over what triggers should activate: ShouldTriggerWhenFaceDown, which controls whether vanilla triggers will activate; and ShouldTriggerCustomWhenFaceDown, which control whether custom triggers will activate.

### Modifying What Card Slots to Attack
To do this, you need to override RespondsToGetOpposingSlots to return true (like all RespondsToXXX overrides, you can make this conditional), and then override GetOpposingSlots to return the list of card slots that your ability wants the card to attack.
If you want to override the default slot (the one directly across from the card) instead of adding an additional card slot, you will need to override RemoveDefaultAttackSlot to return true.

### Modify What Card Slots Attack
Don't let the similar names confuse you, this is in fact a different section from the above one.
Using the IGetAttackingSlots interface, you can modify what card slots will attack each turn.

For example, if you wanted to make a sigil that prevents a card from attacking at all:
```csharp
public class DontAttack : AbilityBehaviour, IGetAttackingSlots
{
    public bool RespondsToGetAttackingSlots(bool playerIsAttacker, List<CardSlot> originalSlots, List<CardSlot> currentSlots)
    {
        return true;
    }
    // returns a list of card slots TO BE ADDED to the list of attacking slots
    public List<CardSlot> GetAttackingSlots(bool playerIsAttacker, List<CardSlot> originalSlots, List<CardSlot> currentSlots)
    {
        // you can modify the currentSlots directly like this
        currentSlots.Remove(base.Card.Slot);

        // if you don't want to add new slots, return new() or null
        return new();
    }

    // used to determine when to trigger this sigil, for when multiple sigils are modifying the attacking slots (e.g., other mods)
    // triggers are sorted from highest to lowest priority
    public int TriggerPriority(bool playerIsAttacker, List<CardSlot> originalSlots)
    {
        // in this example, we don't particularly care if other sigils re-add this slot
        // if we did, we'd return a lower number like -1000 or something
        return 0;
    }
}
```
Importantly, the list of card slots is UNFILTERED, meaning that some card slots may not actually end up in the final list of slots.

The game will automatically remove card slots that are unoccupied, as well as card slots occupied by cards with 0 Power;
if you add any such slots they will be removed.

What this also means is that when checking card slots, you cannot assume that it has a card.
```csharp
public List<CardSlot> GetAttackingSlots(bool playerIsAttacker, List<CardSlot> originalSlots, List<CardSlot> currentSlots)
{
    // this will cause an error
    currentSlots.RemoveAll(slot => slot.Card.HasAbility(Ability.Sharp));

    // this will not
    currentSlots.RemoveAll(slot => slot.Card != null && slot.Card.HasAbility(Ability.Sharp));

    return new();
}

```

### Modify Damage Taken
Using IModifyDamageTaken, you can increase or reduce the damage cards take when they're attacked.
Damage cannot go below 0; if it is, the API sets it to 0 after all calculations are done.

```csharp
public class ReduceDamageByOne : AbilityBehaviour, IModifyDamageTaken
{
    public static Ability ability;
    public override Ability Ability => ability;

    public bool RespondsToModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage)
    {
        // reduce damage this card takes by 1
        if (base.Card == target && damage > 0)
            return attacker == null;

        return false;
    }

    public int OnModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage)
    {
        damage--;
        return damage; // could also return damage - 1 if you want it all on one line
    }

    public int TriggerPriority(PlayableCard target, int damage, PlayableCard attacker) => 0;
}
```

## Additional Functionality for Activated Abilities
---
The API adds a new class `ExtendedActivatedAbilityBehaviour` that adds additional functionality for use when making activated abilities.

This new class changes how activated abilities are made a bit.
As an example, if you were inheriting from the vanilla ActivatedAbilityBehaviour, you would override BonesCost to set the... Bones cost.
ExtendedActivatedAbilityBehaviour moves that functionality to the virtual int StartingBonesCost, and BonesCost is now used to keep track of the total cost, modifiders included.

Put simply, to set the starting cost(s), you override StartingBonesCost, StartingEnergyCost, or StartingHealthCost.

There is also a new override IEnumerator `PostActivate()` that triggers after the main body of code.

### Blood Cost
This behaves exactly like the Blood cost for playing cards, where you have to sacrifice other cards on your side of the board in order to trigger an effect.

The card info and card slot of sacrificed cards will be stored in the dictionary `currentSacrificedCardInfos`, with the CardInfo as the Key and the CardSlot as the Value.
Use this if you want to manipulate sacrificed cards once they're gone, but do note that the dictionary is cleared once the ability's effect has finished.

```c#
public Dictionary<CardSlot, CardInfo> currentSacrificedCardInfos = new();

public override IEnumerator Activate()
{
    // Create new copies of the sacrificed cards in their original board slots
    foreach (KeyValuePair<CardSlot, CardInfo> valuePair in currentSacrificedCardInfos)
    {
        yield return Singleton<BoardManager>.Instance.CreateCardInSlot(valuePair.Value, valuePair.Key);
    }
}
```

### Health Cost
This is easy enough to understand, it's a way of making an activated ability cost Health to use.
If the cost is equal to the card's current Health, then it will die.

You can set this by overriding StartingHealthCost.

### Dynamic Activation Costs
Using ExtendedActivatedAbilityBehaviour, you can change the cost of an activated ability during battle.

By overriding OnActivateBonesCostMod, OnActivateEnergyCostMod, OnActivateBloodCostMod, or OnActivateHealthCostMod, you can make the ability's activation cost increase after it has been activated.
```c#
public class ActivateRepulsive : ExtendedActivatedAbilityBehaviour
{
    public static Ability abiliy;
    public override Ability Ability => ability;
    
    public override int StartingBonesCost => 2;
    public override int OnActivateBonesCostMod => 1;
    
    public override IEnumerator Activate()
    {
        yield return this.Effect();
    }
}
```
You can keep changing these modifier fields as well, if you want the cost to increase exponentially.

```c#
public override IEnumerator PostActivate();
{
    OnActivateBonesCostMod += 1;
}
```
You have basically no limits on how or when you can change the activation cost.

```c#
public override IEnumerator OnUpkeep(bool playerUpkeep)
{
    bonesCostMod -= 1;
}
```
This will reduce the Bones activation cost by 1 on upkeep.

Note that `bonesCostMod` was used here instead of `OnActivateBonesCostMod`; The OnActivate... fields are used for changing the cost every activation, while the ...CostMod fields are used for everything else.
Importantly, the ...CostMod fields keep track of the current cost modifier.

```c#
bonesCostMod = 0;
```
This resets the current Bones cost modifier.

An additional note to make is that you're not limited to only modifying a single cost; you can increase any cost you want, even those that aren't initially required for activation.

#### Keeping Track of It All
With the ability to modify a card's activation cost, the question of how to keep track of the new costs comes up.
While it's entirely possible you have an incredible memory and can just hoof it from there, the API provides a simpler way for you to keep track of it all by updating the Rulebook description of your ability.

When right-clicking the ability icon of a card, the API will grab the current activation costs and display them.
This is unique to each card, so don't worry about keeping track of multiple activation costs.

To tell the API what part of the Rulebook description should be modified, you must use `[sigilcost:X]` where X is the initial activation cost.
```c#
string rulebookDescription = "Pay [sigilcost:1 Bone, 1 Energy] to do a thing, then increase its activation cost by 1 Energy."

AbilityManager.New(pluginGuid, rulebookName, rulebookDescription, typeof(T), "artpath.png");
```

#### Triggering OnResolveOnBoard
In Act 2, upon playing a card with an activated ability you will trigger a tutorial explaining how they work.
In order to ensure this code runs, you are prevented from overriding OnResolveOnBoard, both in the vanilla behaviour and in this extended version.

Worry not however, for there is a workaround: the new virtual methods RespondsToPostResolveOnBoard() and OnPostResolveOnBoard() can be overridden instead to the same effect.

```c#
public override IEnumerator RespondsToPostResolveOnBoard()
{
    return true;
}

public override IEnumerator OnPostResolveOnBoard()
{
    // put your code here
}
```