using System.Collections;
using DiskCardGame;

namespace InscryptionAPI.Triggers;

public interface IAddedToHand
{
    bool RespondsToAddedToHand();
    IEnumerator OnAddedToHand();
}

public interface IOtherAddedToHand
{
    bool RespondsToOtherAddedToHand(PlayableCard card);
    IEnumerator OnOtherAddedToHand(PlayableCard card);
}

public interface IBellRung
{
    bool RespondsToBellRung(bool playerIsAttacker);
    IEnumerator OnBellRung(bool playerIsAttacker);
}

public interface IPreSlotAttackSequence
{
    bool RespondsToPreSlotAttackSequence(CardSlot slot);
    IEnumerator OnPreSlotAttackSequence(CardSlot slot);
}

public interface IPostSlotAttackSequence
{
    bool RespondsToPostSlotAttackSequence(CardSlot slot);
    IEnumerator OnPostSlotAttackSequence(CardSlot slot);
}

public interface IPostSingularSlotAttackSlot
{
    bool RespondsToPostSingularSlotAttackSlot(CardSlot attacker, CardSlot defender);
    IEnumerator OnPostSingularSlotAttackSlot(CardSlot attacker, CardSlot defender);
}

public interface IPreScalesChanged
{
    bool RespondsToPreScalesChanged(int damage, bool toPlayer);
    IEnumerator OnPreScalesChanged(int damage, bool toPlayer);
}

public interface IPostScalesChanged
{
    bool RespondsToPostScalesChanged(int damage, bool toPlayer);
    IEnumerator OnPostScalesChanged(int damage, bool toPlayer);
}

public interface IUpkeepInHand
{
    bool RespondsToUpkeepInHand(bool playerUpkeep);
    IEnumerator OnUpkeepInHand(bool playerUpkeep);
}

public interface IOtherCardResolveInHand
{
    bool RespondsToOtherCardResolveInHand(PlayableCard card);
    IEnumerator OnOtherCardResolveInHand(PlayableCard card);
}

public interface ITurnEndInHand
{
    bool RespondsToTurnEndedInHand();
    IEnumerator OnTurnEndInHand();
}

public interface IOtherCardAssignedToSlotInHand
{
    bool RespondsToOtherCardAssignedToSlotInHand(PlayableCard card);
    IEnumerator OnOtherCardAssignedToSlotInHand(PlayableCard card);
}

public interface ICardAssignedToSlotNoResolve
{
    bool RespondsToCardAssignedToSlotNoResolve(PlayableCard card);
    IEnumerator OnCardAssignedToSlotNoResolve(PlayableCard card);
}

public interface IOtherCardPreDeathInHand
{
    bool RespondsToOtherCardPreDeathInHand(CardSlot slot, bool wasSacrificed, PlayableCard killer);
    IEnumerator OnOtherCardPreDeathInHand(CardSlot slot, bool wasSacrificed, PlayableCard killer);
}

public interface IOtherCardDieInHand
{
    bool RespondsToOtherCardDieInHand(PlayableCard card, CardSlot slot, bool wasSacrificed, PlayableCard killer);
    IEnumerator OnOtherCardDieInHand(PlayableCard card, CardSlot slot, bool wasSacrificed, PlayableCard killer);
}

public interface IOtherCardDealtDamageInHand
{
    bool RespondsToOtherCardDealtDamageInHand(PlayableCard attacker, int attack, PlayableCard defender);
    IEnumerator OnOtherCardDealtDamageInHand(PlayableCard attacker, int attack, PlayableCard defender);
}
