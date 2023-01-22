using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;
using System.Runtime.CompilerServices;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public abstract class ExtendedAbilityBehaviour : AbilityBehaviour, IGetOpposingSlots, IActivateWhenFacedown, IPassiveAttackBuff, IPassiveHealthBuff
{
    // This section handles attack slot management

    public virtual bool TriggerWhenFacedown => false;
    public virtual bool ShouldTriggerWhenFaceDown(Trigger trigger, object[] otherArgs) => TriggerWhenFacedown;
    public virtual bool ShouldTriggerCustomWhenFaceDown(Type customTrigger) => TriggerWhenFacedown;
    public virtual bool RespondsToGetOpposingSlots() => false;

    public virtual List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots) => new();

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

    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual bool ProvidesPassiveAttackBuff => false;

    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual bool ProvidesPassiveHealthBuff => false;

    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual int[] GetPassiveAttackBuffs() => null;

    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual int[] GetPassiveHealthBuffs() => null;

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