using DiskCardGame;
using InscryptionAPI.Boons;
using System.Collections;

namespace InscryptionCommunityPatch.Tests;

/// <summary>
/// This is a test related to Boons.
/// </summary>
public class SacrificeSquirrels : BoonBehaviour
{
    /// <summary>
    /// This makes the Boon respond specifically when resolving a card other than itself being a Squirrel.
    /// </summary>
    /// <param name="otherCard">The <see cref="PlayableCard"/> being resolved.</param>
    /// <returns>True when the Card Being Resolved other than itself is a Squirrel.</returns>
    public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
    {
        return otherCard.Info.name.ToLowerInvariant().Equals("squirrel");
    }

    /// <summary>
    /// This triggers when <see cref="RespondsToOtherCardResolve"/> returns True. When it triggers it gives the cards the Unkillable Sigil after playing the Boon's Animation, then Render's the card.
    /// </summary>
    /// <param name="otherCard">The <see cref="PlayableCard"/> being resolved.</param>
    /// <returns>The <see cref="PlayableCard"/> updated and rerendered with the Unkillable Sigil.</returns>
    public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
    {
        yield return this.PlayBoonAnimation();
        otherCard.TemporaryMods.Add(new(Ability.DrawCopyOnDeath));
        otherCard.RenderCard();
        yield break;
    }
}