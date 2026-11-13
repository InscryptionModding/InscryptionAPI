using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;
using System.Runtime.CompilerServices;

namespace InscryptionAPI.Card;

/// <summary>
/// Extended subclass of AbilityBehaviour that implements several custom API triggers.
/// </summary>
[HarmonyPatch]
public abstract class ExtendedAbilityBehaviour : AbilityBehaviour, IGetOpposingSlots, IActivateWhenFacedown, IPassiveAttackBuff, IPassiveHealthBuff
{
    // This section handles attack slot management

    /// <summary>
    /// Whether or not this ability should activate certain triggers while facedown.
    /// </summary>
    public virtual bool TriggerWhenFacedown => false;
    /// <summary>
    /// Whether or not the given trigger should be called on this ability when its card is face down.
    /// </summary>
    /// <param name="trigger">The trigger being checked.</param>
    /// <param name="otherArgs">Array of objects relating to the trigger being checked.</param>
    /// <returns>True if the given trigger should be activated while this ability's card is face down.</returns>
    public virtual bool ShouldTriggerWhenFaceDown(Trigger trigger, object[] otherArgs) => TriggerWhenFacedown;
    /// <summary>
    /// Whether or not the given trigger should be called on this ability when its card is face down.
    /// </summary>
    /// <param name="customTrigger">The Type of the custom API trigger being checked.</param>
    /// <returns>True if the given trigger should be activated while this ability's card is face down.</returns>
    public virtual bool ShouldTriggerCustomWhenFaceDown(Type customTrigger) => TriggerWhenFacedown;

    /// <summary>
    /// Whether or not this ability should modify what CardSlots cards with this ability will target when attacking.
    /// </summary>
    /// <returns>True if this ability should modify the list of cards being added.</returns>
    public virtual bool RespondsToGetOpposingSlots() => false;
    /// <summary>
    /// Gets an additional list of CardSlots that cards with this ability should target when attacking.
    /// </summary>
    /// <param name="originalSlots">List of CardSlots that were originally being targeted before custom abilities were applied.</param>
    /// <param name="otherAddedSlots">Other CardSlots added by other abilities to the list of targeted slots.</param>
    /// <returns>List of CardSlots that should be added to the list of targeted slots.</returns>
    public virtual List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots) => new();

    /// <summary>
    /// Whether or not the default attack slot should be removed (eg., Bifurcated Strike).
    /// </summary>
    /// <returns>True if cards with this ability should not strike the opposing slot by default.</returns>
    public virtual bool RemoveDefaultAttackSlot() => false;

    // This section handles passive attack/health buffs

    private static ConditionalWeakTable<PlayableCard, List<ExtendedAbilityBehaviour>> AttackBuffAbilities = new();
    private static ConditionalWeakTable<PlayableCard, List<ExtendedAbilityBehaviour>> HealthBuffAbilities = new();

    private static List<ExtendedAbilityBehaviour> GetAttackBuffs(PlayableCard card)
    {
        List<ExtendedAbilityBehaviour> retval;
        if (AttackBuffAbilities.TryGetValue(card, out retval))
            return retval;

        retval = card.GetComponents<ExtendedAbilityBehaviour>().Where(x => x.ProvidesPassiveAttackBuff).ToList();
        AttackBuffAbilities.Add(card, retval);
        return retval;
    }

    private static List<ExtendedAbilityBehaviour> GetHealthBuffs(PlayableCard card)
    {
        List<ExtendedAbilityBehaviour> retval;
        if (HealthBuffAbilities.TryGetValue(card, out retval))
            return retval;

        retval = card.GetComponents<ExtendedAbilityBehaviour>().Where(x => x.ProvidesPassiveHealthBuff).ToList();
        HealthBuffAbilities.Add(card, retval);
        return retval;
    }

    /// <summary>
    /// If this ability provides a passive attack buff.
    /// </summary>
    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual bool ProvidesPassiveAttackBuff => false;

    /// <summary>
    /// If this ability provides a passive health buff.
    /// </summary>
    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual bool ProvidesPassiveHealthBuff => false;

    /// <summary>
    /// Array representing the attack buff given by this ability.
    /// </summary>
    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual int[] GetPassiveAttackBuffs() => null;

    /// <summary>
    /// Array representing the health buff given by this ability.
    /// </summary>
    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual int[] GetPassiveHealthBuffs() => null;

    /// <summary>
    /// Determines the passive attack buff given by this ability if one is given.
    /// </summary>
    /// <param name="target">The PlayableCard current being checked.</param>
    /// <returns>The attack buff to give to the target card.</returns>
    public virtual int GetPassiveAttackBuff(PlayableCard target)
    {
        if (ProvidesPassiveAttackBuff)
        {
            if (target.OpponentCard == this.Card.OpponentCard)
            {
                int[] result = GetPassiveAttackBuffs();
                if (result != null && target.Slot.Index < result.Length)
                {
                    return result[target.Slot.Index];
                }
            }
        }
        return 0;
    }

    /// <summary>
    /// Determines the passive health buff given by this ability if one is given.
    /// </summary>
    /// <param name="target">The PlayableCard current being checked.</param>
    /// <returns>The health buff to give to the target card.</returns>
    public virtual int GetPassiveHealthBuff(PlayableCard target)
    {
        if (ProvidesPassiveHealthBuff)
        {
            if (target.OpponentCard == this.Card.OpponentCard)
            {
                int[] result = GetPassiveHealthBuffs();
                if (result != null && target.Slot.Index < result.Length)
                {
                    return result[target.Slot.Index];
                }
            }
        }
        return 0;
    }
}