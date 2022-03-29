using DiskCardGame;

namespace InscryptionAPI.Triggers;

/// <summary>
/// Contains information on how to modify a card's attack sequence during a given attack phase.
/// </summary>
public class AttackModifier
{
    /// <summary>
    /// Indicates how many additional sniper hits the card should make if it has the Sniper ability
    /// </summary>
    public int additionalSniperAttacks = 0;

    /// <summary>
    /// Indicates if the attacking slots in **slotsToAttack** are intended to override the original attack slot or not.
    /// </summary>
    /// <remarks>Consider the difference between SplitStrike and TriStrike. SplitTrike replaces the original target slot
    /// with the two on either side of it, while TriStrike retains the original target slot and simply adds two more. 
    /// You can imagine that if these sigils were implemented using this system, SplitStrike would return **true** here
    /// and TriStrike would return **false**.
    public bool shouldRemoveOriginalAttackSlot = false;

    /// <summary>
    /// Indicates which card slots the card should attack
    /// </summary>
    public List<CardSlot> slotsToAttack = null;
}