using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class GreenGemStatIconFix
{
    /// <summary>
    /// This fixes the Green Gem stat icon to appear in act 1 correctly
    /// </summary>
    [HarmonyPatch(typeof(StatIconInfo), nameof(StatIconInfo.LoadAbilityData))]
    [HarmonyPostfix]
    private static void StatIconInfo_LoadAbilityData()
    {
        StatIconInfo greenGemStatIcon = StatIconInfo.allIconInfo.Find((a) => a.name == "GreenGemsStatIcon");

        Texture2D baseTexture = TextureHelper.GetImageAsTexture("GreenGem.png", typeof(GreenGemStatIconFix).Assembly);
        greenGemStatIcon.iconGraphic = baseTexture;
        greenGemStatIcon.rulebookDescription = "The power of this card is equal to the number of Green Gems that the owner has on their side of the table.";
        greenGemStatIcon.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
    }
}