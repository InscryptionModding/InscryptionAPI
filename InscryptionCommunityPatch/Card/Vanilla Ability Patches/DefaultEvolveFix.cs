using DiskCardGame;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public class DefaultEvolveFix
{
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(EvolveParams), nameof(EvolveParams.GetDefaultEvolution))]
    private static void FixDefaultEvolveParamsIL(ILContext il)
    {
        ILCursor c = new(il);

        int modLoc = -1;

        c.GotoNext(MoveType.Before, x => x.MatchNewobj(AccessTools.DeclaredConstructor(typeof(CardModificationInfo), new[] { typeof(int), typeof(int) })));
        c.GotoNext(MoveType.After, x => x.MatchStloc(out modLoc));

        c.Emit(OpCodes.Ldloc, modLoc);
        c.EmitDelegate<Action<CardModificationInfo>>(static mod => mod.negateAbilities.Add(Ability.Evolve));

        c.GotoNext(MoveType.Before,
            x => x.MatchLdloc(out _),
            x => x.MatchLdcI4((int)Ability.Evolve),
            x => x.MatchCallOrCallvirt(AccessTools.DeclaredMethod(typeof(CardInfo), nameof(CardInfo.RemoveBaseAbility)))
        );

        c.RemoveRange(3);
    }
}
