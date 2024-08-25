using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace InscryptionCommunityPatch.Card;

internal static class CommunityArtPatches
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
        Ability.Transformer,
        Ability.SquirrelOrbit,
        Ability.DrawVesselOnHit
    };
    internal static readonly List<string> regularCardsToPatch = new()
    {
        "Ant",
        "AquaSquirrel",
        "Bee",
        "BrokenEgg",
        "Dam",
        "DausBell",
        "DefaultTail",
        "EmptyVessel",
        "EmptyVessel_GreenGem",
        "EmptyVessel_OrangeGem",
        "EmptyVessel_BlueGem",
        "Ijiraq",
        "RavenEgg",
        "SkinkTail",
        "Tail_Bird",
        "Tail_Furry",
        "Tail_Insect",
    };

    internal static void PatchCommunityArt()
    {
        System.Reflection.Assembly ass = typeof(CommunityArtPatches).Assembly;
        foreach (Ability ability in regularIconsToPatch)
            AbilityManager.BaseGameAbilities.AbilityByID(ability).SetIcon(TextureHelper.GetImageAsTexture($"{ability}.png", ass));

        foreach (Ability ability in gbcIconsToPatch)
            AbilityManager.BaseGameAbilities.AbilityByID(ability).Info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"Pixel{ability}.png", ass));

        foreach (string name in regularCardsToPatch)
            CardManager.BaseGameCards.CardByName(name).SetPixelPortrait(TextureHelper.GetImageAsTexture($"{name}_pixel.png", ass));

        if (PatchPlugin.undeadCatEmission.Value)
        {
            CardManager.BaseGameCards.CardByName("CatUndead")
            .SetEmissivePortrait(TextureHelper.GetImageAsTexture("CatUndead_emission.png", ass))
            .AddAppearances(CardAppearanceBehaviour.Appearance.RedEmission);
        }
    }
}