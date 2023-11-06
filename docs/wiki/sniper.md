## Custom Sniper Logic
---
Though not technically part of the API proper, the community patches still offer a number of useful options for modders to take advantage of.

InscryptionCommunityPatch.Card.SniperFix, as its name suggests, fixes the Sniper ability when used in Act 1. It does more than just that, however; it also expands the Sniper ability's logic into a number of overridable methods, allowing for further customisation to how Sniper functions in all parts of the game.

To modify these methods, you will need to patch them using Harmony.

These methods are:
- DoSniperLogic() - controls whether to use player or opponent sniper logic
- DoAttackTargetSlotsLogic() - controls attack logic for each target slot
- GetValidTargets() - returns the list of card slot the player and opponent can target
- PlayerTargetSelectedCallback() - called when the player selects a valid target
- PlayerSlotCursorEnterCallback() - called when the player's cursor enters a slot
- OpponentSelectTarget() - returns a card slot for the opponent to target and attack

For example, if you wanted to allow cards with the Sniper ability to target any card slot, including slots on the same side of the board, you would do:
```
[HarmonyPostfix, HarmonyPatch(typeof(SniperFix), nameof(SniperFix.GetValidTargets))]
private static void TargetAllSlots(ref List<CardSlot> __result, bool playerIsAttacker, CardSlot attackingSlot)
{
    __result = BoardManager.Instance.AllSlotsCopy; // override the default list of valid targets
    __result.Remove(attackingSlot); // remove the currently attacking slot as a valid target
}
```