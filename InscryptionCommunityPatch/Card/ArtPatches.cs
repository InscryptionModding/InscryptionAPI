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
        Ability.SquirrelStrafe
    };

    internal static readonly List<Ability> gbcIconsToPatch = new()
    {
        Ability.ConduitSpawnGems,
        Ability.LatchBrittle,
        Ability.LatchDeathShield,
        Ability.LatchExplodeOnDeath,
        Ability.DrawRandomCardOnDeath,
        Ability.DeleteFile,
        Ability.MadeOfStone,
        Ability.PermaDeath,
        Ability.CellDrawRandomCardOnDeath,
        Ability.CellBuffSelf,
        Ability.CellTriStrike,
        Ability.GainGemTriple,
        Ability.SwapStats,
        Ability.AllStrike,
        Ability.BeesOnHit,
        Ability.BuffEnemy,
        Ability.CorpseEater,
        Ability.CreateBells,
        Ability.CreateDams,
        Ability.DrawAnt,
        Ability.ExplodeGems,
        Ability.MoveBeside,
        Ability.RandomAbility,
        Ability.RandomConsumable,
        Ability.ShieldGems,
        Ability.Sniper,
        Ability.TailOnHit,
        Ability.SquirrelOrbit,
        Ability.DrawVesselOnHit
    };

    public static void PatchCommunityArt()
    {
        foreach (Ability ability in regularIconsToPatch)
            AbilityManager.BaseGameAbilities.AbilityByID(ability).SetIcon(TextureHelper.GetImageAsTexture($"{ability.ToString()}.png", typeof(CommunityArtPatches).Assembly));

        foreach (Ability ability in gbcIconsToPatch)
            AbilityManager.BaseGameAbilities.AbilityByID(ability).Info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"Pixel{ability.ToString()}.png", typeof(CommunityArtPatches).Assembly));
    }
}