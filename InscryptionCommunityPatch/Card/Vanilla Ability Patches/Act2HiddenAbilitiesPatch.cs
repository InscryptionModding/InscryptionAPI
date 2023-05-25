using DiskCardGame;
using GBC;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class Act2HiddenAbilitiesPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(PixelCardAbilityIcons), nameof(PixelCardAbilityIcons.DisplayAbilities),
        new Type[] { typeof(List<Ability>), typeof(PlayableCard) })]
    private static void DerenderHiddenAbilities(ref List<Ability> abilities, PlayableCard card)
    {
        if (card)
        {
            for (int i = abilities.Count - 1; i >= 0; i--)
            {
                if (card.Status.hiddenAbilities.Contains(abilities[i]))
                    abilities.RemoveAt(i);
            }
            foreach (CardModificationInfo mod in card.TemporaryMods.FindAll(x => x.abilities.Count > 0 && x.singletonId != "paint"))
            {
                foreach (Ability ability in mod.abilities)
                {
                    if (!card.Status.hiddenAbilities.Contains(ability))
                        abilities.Add(ability);
                }
            }
        }
    }
    [HarmonyPrefix, HarmonyPatch(typeof(WizardDummyBattleSequencer), nameof(WizardDummyBattleSequencer.AddIconAbilityToCard))]
    private static bool AddSingletonToDummy(PlayableCard card, Ability ability, string iconPrefabName)
    {
        card.AddTemporaryMod(new CardModificationInfo(ability) { singletonId = "paint" });
        GameObject obj = UnityObject.Instantiate(Resources.Load<GameObject>("Prefabs/GBCUI/TextAdditive/" + iconPrefabName));
        obj.transform.parent = card.GetComponentInChildren<PixelCardDisplayer>().CardElements.transform;
        obj.transform.localPosition = Vector3.zero;
        return false;
    }
}