using System.Collections;
using DiskCardGame;

namespace InscryptionAPI.Triggers;

public interface IAddedToHand
{
    IEnumerator OnAddedToHand();
}

public interface IOtherAddedToHand
{
    IEnumerator OnOtherAddedToHand(PlayableCard card);
}

public interface IBellRung
{
    IEnumerator OnBellRung(bool playerIsAttacker);
}

public interface IPreSlotAttackSequence
{
    IEnumerator OnPreSlotAttackSequence(CardSlot slot);
}

public interface IPostSlotAttackSequence
{
    IEnumerator OnPostSlotAttackSequence(CardSlot slot);
}

public interface IPostSingularSlotAttackSlot
{
    IEnumerator OnPostSingularSlotAttackSlot(CardSlot attacker, CardSlot defender);
}

public interface IPreScalesChanged
{
    IEnumerator OnPreScalesChanged(int damage, bool toPlayer);
}

public interface IPostScalesChanged
{
    IEnumerator OnPostScalesChanged(int damage, bool toPlayer);
}

public interface IUpkeepInHand
{
    IEnumerator OnUpkeepInHand(bool playerUpkeep);
}
