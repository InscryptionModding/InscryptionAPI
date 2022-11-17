using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using InscryptionAPI.Helpers;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class GreenGemStatIconFix
{
    /// <summary>
    /// This fixes the Green Gem stat icon from appearing as a black square but instead as a gem
    /// </summary>
    [HarmonyPatch(typeof(StatIconInfo), nameof(StatIconInfo.LoadAbilityData))]
    [HarmonyPostfix]
    private static void StatIconInfo_LoadAbilityData()
    {
        StatIconInfo greenGemStatIcon = StatIconInfo.allIconInfo.Find((a) => a.name == "GreenGemsStatIcon");
        
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("GreenGem.png", typeof(GreenGemStatIconFix).Assembly);
        greenGemStatIcon.iconGraphic = baseTexture;
        greenGemStatIcon.rulebookDescription = greenGemStatIcon.gbcDescription;
    }
}