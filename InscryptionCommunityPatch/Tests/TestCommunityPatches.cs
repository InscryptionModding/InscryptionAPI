using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using UnityEngine;

namespace InscryptionCommunityPatch.Tests;

[HarmonyPatch]
public static class ExecuteCommunityPatchTests
{
    public static BoonData.Type TestBoon;

    public static void PrepareForTests()
    {
        AbilityManager.BaseGameAbilities.AbilityByID(Ability.Sharp).Info.canStack = true;
        AbilityManager.BaseGameAbilities.AbilityByID(Ability.BuffNeighbours).Info.canStack = true;
        AbilityManager.BaseGameAbilities.AbilityByID(Ability.RandomConsumable).Info.canStack = true;

        // Creates a boon
        TestBoon = BoonManager.New(PatchPlugin.ModGUID, "Sacrificial Squirrels", typeof(SacrificeSquirrels),
        "Squirrels are super OP now", Resources.Load<Texture2D>("art/cards/abilityicons/ability_drawcopyondeath"),
        Resources.Load<Texture2D>("art/cards/boons/boonportraits/boon_startinggoat"), false, true, true);
    }

    [HarmonyPatch(typeof(AscensionSaveData), nameof(AscensionSaveData.NewRun))]
    [HarmonyPostfix]
    private static void StartRunInTestMode(ref AscensionSaveData __instance)
    {
        if (PatchPlugin.configTestState.Value)
        {
            List<Ability> testAbilityList = new() { Ability.Sharp, Ability.DebuffEnemy, Ability.CreateDams, Ability.DrawRabbits, Ability.Strafe, Ability.Deathtouch, Ability.DoubleStrike, Ability.Reach, Ability.BeesOnHit };

            // This tests how cardmerge abilities work and tests abilities beyond 2
            for (int i = 2; i < 4; i++)
            {
                CardInfo cardToAdd = CardLoader.GetCardByName("Rabbit");
                CardModificationInfo mod = new();
                mod.abilities = new(testAbilityList.Take(i));
                cardToAdd.mods.Add(mod);

                CardModificationInfo mod2 = new();
                mod2.abilities = new() { testAbilityList[i - 1] };
                mod2.fromCardMerge = true;
                cardToAdd.mods.Add(mod2);

                __instance.currentRun.playerDeck.AddCard(cardToAdd);
            }

            // This tests stackable icons
            // We need to make these abilities stackable to make it work of course
            for (int i = 1; i <= 2; i++)
            {
                CardInfo cardToAdd = CardLoader.GetCardByName("Rabbit");
                CardModificationInfo mod = new();
                mod.abilities = new();
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

            __instance.currentRun.playerDeck.AddBoon(TestBoon);
        }
    }
}