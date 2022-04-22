using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;
using Sirenix.Utilities;

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

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
    [HarmonyPostfix]
    private static void UpdateOpposingSlots(ref PlayableCard __instance, ref List<CardSlot> __result)
    {
        bool isAttackingDefaultSlot = !__instance.HasTriStrike() && !__instance.HasAbility(Ability.SplitStrike);
        CardSlot defaultslot = __instance.Slot.opposingSlot;

        List<CardSlot> alteredOpposings = new List<CardSlot>();
        bool removeDefaultAttackSlot = false;

        foreach (IGetOpposingSlots component in CustomTriggerFinder.FindTriggersOnCard<IGetOpposingSlots>(__instance))
        {
            if (component.RespondsToGetOpposingSlots())
            {
                alteredOpposings.AddRange(component.GetOpposingSlots(__result, new(alteredOpposings)));
                removeDefaultAttackSlot = removeDefaultAttackSlot || component.RemoveDefaultAttackSlot();  
            }
        }
        
        if (alteredOpposings.Count > 0) 
            __result.AddRange(alteredOpposings);

        if (isAttackingDefaultSlot && removeDefaultAttackSlot)
            __result.Remove(defaultslot);
    }

    // This section handles passive attack/health buffs

    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual bool ProvidesPassiveAttackBuff => false;

    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual bool ProvidesPassiveHealthBuff => false;

    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual int[] GetPassiveAttackBuffs() => null;

    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual int[] GetPassiveHealthBuffs() => null;

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
    [HarmonyPostfix]
    private static void AddPassiveAttackBuffs(ref PlayableCard __instance, ref int __result)
    {
        var card = __instance;
        
        if (TurnManager.Instance.GameEnding || card.SafeIsUnityNull() || card.Dead) return;

        __result += BoardManager.Instance.CardsOnBoard
            .SelectMany(CustomTriggerFinder.FindTriggersOnCard<IPassiveAttackBuff>)
            .Sum(buffer => buffer.GetPassiveAttackBuff(card));
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveHealthBuffs))]
    [HarmonyPostfix]
    private static void AddPassiveHealthBuffs(ref PlayableCard __instance, ref int __result)
    {
        var card = __instance;
        
        if (TurnManager.Instance.GameEnding || card.SafeIsUnityNull() || card.Dead) return;

        __result += BoardManager.Instance.CardsOnBoard
            .SelectMany(CustomTriggerFinder.FindTriggersOnCard<IPassiveHealthBuff>)
            .Sum(buffer => buffer.GetPassiveHealthBuff(card));
    }

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