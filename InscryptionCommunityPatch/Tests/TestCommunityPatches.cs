using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionCommunityPatch.Card;

namespace InscryptionCommunityPatch.Tests;

[HarmonyPatch]
public static class ExecuteCommunityPatchTests
{
    public static void PrepareForTests()
    {
        AbilityManager.BaseGameAbilities.AbilityByID(Ability.Sharp).Info.canStack = true;
        AbilityManager.BaseGameAbilities.AbilityByID(Ability.BuffNeighbours).Info.canStack = true;
        AbilityManager.BaseGameAbilities.AbilityByID(Ability.RandomConsumable).Info.canStack = true;
    }

    [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.NewRun))]
    [HarmonyPostfix]
    public static void StartRunInTestMode(ref AscensionSaveData __instance)
    {
        if (PatchPlugin.configTestState.Value)
        {
            List<Ability> testAbilityList = new()
                { Ability.Sharp, Ability.DebuffEnemy, Ability.CreateDams, Ability.DrawRabbits, Ability.Strafe, Ability.Deathtouch, Ability.DoubleStrike, Ability.Reach, Ability.BeesOnHit };

            // This tests how cardmerge abilities work and tests abilities beyond 2
            for (int i = 1; i < 9; i++)
            {
                CardInfo cardToAdd = CardLoader.GetCardByName("Rabbit");
                CardModificationInfo mod = new()
                {
                    abilities = new(testAbilityList.Take(i))
                };
                cardToAdd.mods.Add(mod);

                CardModificationInfo mod2 = new()
                {
                    abilities = new() { testAbilityList[i - 1] },
                    fromCardMerge = true
                };
                cardToAdd.mods.Add(mod2);

                __instance.currentRun.playerDeck.AddCard(cardToAdd);
            }

            // This tests stackable icons
            // We need to make these abilities stackable to make it work of course
            for (int i = 1; i <= 9; i++)
            {
                CardInfo cardToAdd = CardLoader.GetCardByName("Rabbit");
                CardModificationInfo mod = new()
                {
                    abilities = new()
                };
                for (int j = 0; j < i; j++)
                {
                    mod.abilities.Add(Ability.Sharp);
                    mod.abilities.Add(Ability.BuffNeighbours);
                    mod.abilities.Add(Ability.RandomConsumable);
                    mod.abilities.Add(Ability.BeesOnHit);
                }
                cardToAdd.mods.Add(mod);
                __instance.currentRun.playerDeck.AddCard(cardToAdd);
            }

            // Test all of the new icon artworks
            List<Ability> iconsToTest = new(CommunityArtPatches.regularIconsToPatch);
            while (iconsToTest.Count > 0)
            {
                CardInfo cardToAdd = CardLoader.GetCardByName("Rabbit");
                CardModificationInfo mod = new()
                {
                    abilities = new()
                };
                while (mod.abilities.Count < 6 && iconsToTest.Count > 0)
                {
                    mod.abilities.Add(iconsToTest[0]);
                    iconsToTest.RemoveAt(0);
                }
                cardToAdd.mods.Add(mod);
                __instance.currentRun.playerDeck.AddCard(cardToAdd);
            }
        }
    }
}
