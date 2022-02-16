using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace InscryptionCommunityPatch.Card;

public static class CommunityArtPatches
{
    internal static readonly List<Ability> regularIconsToPatch = new()
    {
        Ability.ActivatedDealDamage,
        Ability.ActivatedDrawSkeleton,
        Ability.ActivatedEnergyToBones,
        Ability.ActivatedHeal,
        Ability.ActivatedRandomPowerBone,
        Ability.ActivatedRandomPowerEnergy,
        Ability.ActivatedSacrificeDrawCards,
        Ability.ActivatedStatsUp,
        Ability.ActivatedStatsUpEnergy,
        Ability.BombSpawner,
        Ability.ConduitEnergy,
        Ability.ConduitFactory,
        Ability.ConduitHeal,
        Ability.DoubleDeath,
        Ability.DrawNewHand,
        Ability.GainGemTriple,
        Ability.Loot,
        Ability.SkeletonStrafe,
        Ability.SquirrelStrafe,
        Ability.SubmergeSquid
    };

    public static void PatchCommunityArt()
    {
        foreach(Ability ability in regularIconsToPatch)
            AbilityManager.BaseGameAbilities.AbilityByID(ability).SetIcon(TextureHelper.GetImageAsTexture($"{ability.ToString()}.png", typeof(CommunityArtPatches).Assembly));
    }
}