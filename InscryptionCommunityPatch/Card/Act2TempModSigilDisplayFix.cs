using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Lets you add and remove decals via temporary mods
[HarmonyPatch]
internal class Act2TempModSigilDisplayFix
{
    [HarmonyPatch(typeof(PixelCardAbilityIcons), nameof(PixelCardAbilityIcons.DisplayAbilities),
        new Type[] { typeof(List<Ability>), typeof(PlayableCard) })]
    [HarmonyPrefix]
    private static bool ConcatModAbilities(ref List<Ability> abilities, PlayableCard card)
    {
        if (card != null) // code copied from CardAbilityIcons
        {
            List<Ability> tempAbilities = AbilitiesUtil.GetAbilitiesFromMods(card.RenderInfo.temporaryMods);
            abilities.AddRange(tempAbilities);
            abilities = AbilitiesUtil.RemoveNonDistinctNonStacking(abilities);
            abilities.RemoveAll((Ability x) => card.RenderInfo.temporaryMods.Exists((CardModificationInfo m) => m.negateAbilities.Contains(x)));
            if (card.Status.hiddenAbilities != null)
                abilities.RemoveAll((Ability x) => card.Status.hiddenAbilities.Contains(x));
        }
        return true;
    }
}