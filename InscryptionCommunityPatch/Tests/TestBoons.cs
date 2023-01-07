using DiskCardGame;
using InscryptionAPI.Boons;
using System.Collections;

namespace InscryptionCommunityPatch.Tests;

public class SacrificeSquirrels : BoonBehaviour
{
    public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
    {
        return otherCard.Info.name.ToLowerInvariant().Equals("squirrel");
    }

    public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
    {
        yield return this.PlayBoonAnimation();
        otherCard.TemporaryMods.Add(new(Ability.DrawCopyOnDeath));
        otherCard.RenderCard();
        yield break;
    }
}