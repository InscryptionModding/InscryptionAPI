using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Card;
using System.Collections;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class PassiveAttackBuffPatches
{
    internal static Dictionary<Ability, bool> CanStack = new();
    internal static Dictionary<Ability, bool> IsConduit = new();

    internal static void CacheLookups()
    {
        AbilityManager.ModifyAbilityList += delegate(List<AbilityManager.FullAbility> abilities)
        {
            PassiveAttackBuffPatches.CanStack.Clear();
            PassiveAttackBuffPatches.IsConduit.Clear();
            foreach (var ab in abilities)
            {
                //PatchPlugin.Logger.LogDebug($"Ability {ab.Id} can stack {ab.Info.canStack}");
                PassiveAttackBuffPatches.CanStack[ab.Id] = ab.Info.canStack;
                PassiveAttackBuffPatches.IsConduit[ab.Id] = ab.Info.conduit;
            }
            return abilities;
        };
    }

    private static int AbilityCount (this PlayableCard card, Ability ability)
    {
        int count = StackableAbilityCount(card, ability);
        if (CanStack[ability])
            return count;
        else
            return count > 0 ? 1 : 0;
    }

    // This patch makes it so that IF passive attack buffs are made stackable, they will function correctly
    // It does NOT make them actually stackable in the metadata
    private static int StackableAbilityCount(this PlayableCard card, Ability ability)
    {
        // Here, we're just counting the excess buffs - the number beyond 1.
        // We know that the original code already counted once
        // We just need to know how many beyond 1 there was
        
        // I'm super-optimizing this:

        int count = 0;
        List<Ability> baseAbilities = card.Info.abilities;
        int abCount = baseAbilities.Count;
        for (int i = 0; i < abCount; i++)
            if (baseAbilities[i] == ability)
                count++;

        // Go through all the card's mods
        List<CardModificationInfo> mods = card.Info.mods;
        if (mods != null)
        {
            int modCount = mods.Count;
            for (int i = 0; i < modCount; i++)
            {
                List<Ability> negAbilities = mods[i].negateAbilities;
                if (negAbilities != null)
                {
                    abCount = negAbilities.Count;
                    for (int j = 0; j < abCount; j++)
                        if (negAbilities[j] == ability)
                            return 0;
                }

                List<Ability> abilities = mods[i].abilities;
                if (abilities != null)
                {
                    abCount = abilities.Count;
                    for (int j = 0; j < abCount; j++)
                        if (abilities[j] == ability)
                            count++;
                }
            }
        }

        // Go through all the card's temporary mods
        if (card.temporaryMods != null)
        {
            mods = card.temporaryMods;
            int modCount = mods.Count;
            for (int i = 0; i < modCount; i++)
            {
                List<Ability> negAbilities = mods[i].negateAbilities;
                if (negAbilities != null)
                {
                    abCount = negAbilities.Count;
                    for (int j = 0; j < abCount; j++)
                        if (negAbilities[j] == ability)
                            return 0;
                }

                List<Ability> abilities = mods[i].abilities;
                if (abilities != null)
                {
                    abCount = abilities.Count;
                    for (int j = 0; j < abCount; j++)
                        if (abilities[j] == ability)
                            count++;
                }   
            }
        }

        return count;
    }

    private static bool NegatesAbility(this PlayableCard card, Ability ability)
    {
        if (card.Info.mods != null)
        {
            int modCount = card.Info.mods.Count;
            for (int m = 0; m < modCount; m++)
            {
                CardModificationInfo mod = card.Info.mods[m];
                List<Ability> negAbs = mod.negateAbilities;
                if (negAbs != null)
                {
                    int negCount = negAbs.Count;
                    for (int n = 0; n < negCount; n++)
                    {
                        if (negAbs[n] == ability)
                            return true;
                    }
                }
            }
        }
        if (card.temporaryMods != null)
        {
            int modCount = card.temporaryMods.Count;
            for (int m = 0; m < modCount; m++)
            {
                CardModificationInfo mod = card.temporaryMods[m];
                List<Ability> negAbs = mod.negateAbilities;
                if (negAbs != null)
                {
                    int negCount = negAbs.Count;
                    for (int n = 0; n < negCount; n++)
                    {
                        if (negAbs[n] == ability)
                            return true;
                    }
                }
            }
        }
        return false;
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
    [HarmonyPostfix]
    private static IEnumerator AlwaysInstantiateConduitManager(IEnumerator sequence)
    {
        if (ConduitCircuitManager.Instance == null)
            BoardManager.Instance.gameObject.AddComponent<ConduitCircuitManager>();

        yield return sequence;
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasConduitAbility))]
    [HarmonyPrefix]
    private static bool BetterHasConduitAbility(ref bool __result, ref PlayableCard __instance)
    {
        if (__instance.Info.abilities != null)
        {
            int abCount = __instance.Info.abilities.Count;
            for (int a = 0; a < abCount; a++)
            {
                Ability ability = __instance.Info.abilities[a];
                if (IsConduit[ability] && !__instance.NegatesAbility(ability))
                {
                    __result = true;
                    return false;
                }
            }
        }
        if (__instance.Info.mods != null)
        {
            int modCount = __instance.Info.mods.Count;
            for (int m = 0; m < modCount; m++)
            {
                CardModificationInfo mod = __instance.Info.mods[m];
                if (mod.abilities != null)
                {
                    int abCount = mod.abilities.Count;
                    for (int a = 0; a < abCount; a++)
                    {
                        Ability ability = mod.abilities[a];
                        if (IsConduit[ability] && !__instance.NegatesAbility(ability))
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
            }
        }
        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
    [HarmonyPrefix]
    private static bool BetterAttackBuffs(ref int __result, ref PlayableCard __instance)
    {
        // Right now, attack buffs don't stack because this method just checks if the
        // buff exists, and doens't check how many of them you have.
        // Let's change that.
        if (__instance.OnBoard)
        {
            List<CardSlot> neighbors = BoardManager.Instance.GetAdjacentSlots(__instance.Slot);
            int neibCount = neighbors.Count;
            for (int n = 0; n < neibCount; n++)
                if (neighbors[n].Card != null)
                    __result += neighbors[n].Card.AbilityCount(Ability.BuffNeighbours);

            // Deal with buff and debuff enemy
            // We have to handle giant cards separately (i.e., the moon)
            if (__instance.Info.HasTrait(Trait.Giant) && !__instance.HasAbility(Ability.MadeOfStone))
            {    
                List<CardSlot> playerSlots = BoardManager.Instance.playerSlots;
                int playerSlotCount = playerSlots.Count;
                for (int p = 0; p < playerSlotCount; p++)
                {
                    if (playerSlots[p].Card != null)
                    {
                        __result += playerSlots[p].Card.AbilityCount(Ability.BuffEnemy);
                        __result -= playerSlots[p].Card.AbilityCount(Ability.DebuffEnemy);
                    }
                }
            }
            else if (__instance.Slot.opposingSlot.Card != null)
            {
                __result += __instance.Slot.opposingSlot.Card.AbilityCount(Ability.BuffEnemy);
                __result -= __instance.Slot.opposingSlot.Card.AbilityCount(Ability.DebuffEnemy);
            }

            if (ConduitCircuitManager.Instance != null) // No need to check save file location - this lets conduits work in all acts
            {
                List<PlayableCard> conduitsForSlot = ConduitCircuitManager.Instance.GetConduitsForSlot(__instance.Slot);
                int conduitsForSlotCount = conduitsForSlot.Count;
                for (int i = 0; i < conduitsForSlotCount; i++)
                    __result += conduitsForSlot[i].AbilityCount(Ability.ConduitBuffAttack);

                if (conduitsForSlotCount > 0)
                    __result += __instance.AbilityCount(Ability.CellBuffSelf);
            }

            if (__instance.Info.HasTrait(Trait.Gem))
            {
                List<CardSlot> slots = __instance.OpponentCard ? BoardManager.Instance.opponentSlots : BoardManager.Instance.playerSlots;
                int slotCount = slots.Count;
                for (int i = 0; i < slotCount; i++)
                    if (slots[i].Card != null)
                        __result += slots[i].Card.AbilityCount(Ability.BuffGems);
            }
        }
        return false;
    }
}