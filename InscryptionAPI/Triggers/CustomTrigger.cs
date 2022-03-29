using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Triggers
{
    public enum CustomTrigger
    {
        None,
        OnAddedToHand,
        OnOtherCardAddedToHand,
        OnBellRung,
        OnPreSlotAttackSequence,
        OnPostSlotAttackSequence,
        OnPostSingularSlotAttackSlot,
        OnPreScalesChanged,
        OnPostScalesChanged,
        OnUpkeepInHand,
        OnCardAssignedToSlotNoResolve,
        OnOtherCardResolveInHand,
        OnTurnEndInHand,
        OnOtherCardAssignedToSlotInHand,
        OnOtherCardPreDeathInHand,
        OnOtherCardDealtDamageInHand,
        OnOtherCardDieInHand,
        OnGetOpposingSlots,
        OnBuffOtherCardAttack,
        OnBuffOtherCardHealth
    }
}
