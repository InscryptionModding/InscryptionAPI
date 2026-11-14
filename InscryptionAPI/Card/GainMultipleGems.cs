using DiskCardGame;
using System.Collections;

namespace InscryptionAPI.Card;

/// <summary>
/// Class that allows for easily creating abilities that provide multiple gem types to their owner.
/// </summary>
public abstract class GainMultipleGems : AbilityBehaviour {
    /// <summary>
    /// Gem types that this ability should provide.
    /// </summary>
    public abstract GemType[] Gems { get; }

    /// <summary>
    /// Adds provided gems when a card with this ability is played on the board.
    /// </summary>
    public override bool RespondsToResolveOnBoard() {
        return true;
    }

    /// <summary>
    /// Add provided gems when a card with this ability is played on the board.
    /// </summary>
    public override IEnumerator OnResolveOnBoard() {
        if (!base.Card.OpponentCard) {
            foreach (GemType gem in Gems) {
                yield return ResourcesManager.Instance.AddGem(gem);
            }
        }
        else {
            foreach (GemType gem in Gems) {
                OpponentGemsManager.Instance.AddGem(gem);
            }
        }
    }

    /// <summary>
    /// Removes provided gems when a card with this ability dies.
    /// </summary>
    public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) {
        return true;
    }

    /// <summary>
    /// Remove provided gems when a card with this ability dies.
    /// </summary>
    public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer) {
        if (!base.Card.OpponentCard) {
            foreach (GemType gem in Gems) {
                yield return ResourcesManager.Instance.LoseGem(gem);
            }
        }
        else {
            foreach (GemType gem in Gems) {
                OpponentGemsManager.Instance.LoseGem(gem);
            }
        }
    }
}
