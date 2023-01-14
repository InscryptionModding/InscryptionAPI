using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class Part2CardCostRender
{
    // This patches the way card costs are rendered in Act 2 (GBC)
    // It allows mixed card costs to display correctly (i.e., 2 blood, 1 bone)
    // And makes the card costs take up a smaller amount of space on the card, showing off more art.
    // Also allows for custom costs to hook in and be displayed without the creator needing to patch cost render

    public static event Action<CardInfo, List<Texture2D>> UpdateCardCost;

    //private static Dictionary<string, Texture2D> AssembledTextures = new();

    public static Texture2D CombineIconAndCount(int cardCost, Texture2D artCost)
    {
        bool left = !PatchPlugin.rightAct2Cost.Value;
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly);

        List<Texture2D> list = new List<Texture2D>();
        if (cardCost <= 4)
        {
            for (int i = 0; i < cardCost; i++)
                list.Add(artCost);
        }
        else
        {
            list.Add(artCost);
            list.Add(TextureHelper.GetImageAsTexture($"pixel_L_{cardCost}.png", typeof(Part2CardCostRender).Assembly));
        }

        int xOffset = left ? 0 : cardCost >= 10 ? 30 - 20 - artCost.width : cardCost <= 4 ? 30 - artCost.width * cardCost : 30 - 14 - artCost.width;
        return TextureHelper.CombineTextures(list, baseTexture, xOffset: xOffset, xStep: artCost.width);
    }

    public static Sprite Part2SpriteFinal(CardInfo card)
    {
        bool left = !PatchPlugin.rightAct2Cost.Value;

        // A list to hold the textures (important later, to combine them all)
        List<Texture2D> masterList = new List<Texture2D>();

        if (card.BloodCost > 0)
            masterList.Add(CombineIconAndCount(card.BloodCost, TextureHelper.GetImageAsTexture("pixel_blood.png", typeof(Part2CardCostRender).Assembly)));

        if (card.BonesCost > 0)
            masterList.Add(CombineIconAndCount(card.BonesCost, TextureHelper.GetImageAsTexture("pixel_bone.png", typeof(Part2CardCostRender).Assembly)));

        if (card.EnergyCost > 0)
            masterList.Add(CombineIconAndCount(card.EnergyCost, TextureHelper.GetImageAsTexture("pixel_energy.png", typeof(Part2CardCostRender).Assembly)));

        if (card.gemsCost.Count > 0)
        {
            List<Texture2D> gemCost = new List<Texture2D>();

            //If a card has a green mox, set the green mox
            if (card.GemsCost.Contains(GemType.Green))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_green.png", typeof(Part2CardCostRender).Assembly));

            if (card.GemsCost.Contains(GemType.Orange))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_orange.png", typeof(Part2CardCostRender).Assembly));

            if (card.GemsCost.Contains(GemType.Blue))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_blue.png", typeof(Part2CardCostRender).Assembly));

            Texture2D gemBaseTexture = TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly);

            if (!left)
                gemCost.Reverse();

            masterList.Add(TextureHelper.CombineTextures(gemCost, gemBaseTexture, xOffset: left ? 0 : 30 - 7 * gemCost.Count, xStep: 7));
        }

        // Call the event and allow others to modify the list of textures
        UpdateCardCost?.Invoke(card, masterList);

        while (masterList.Count < 4)
            masterList.Add(null);

        //Combine all the textures from the list into one texture
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("pixel_base.png", typeof(Part2CardCostRender).Assembly);
        Texture2D finalTexture = TextureHelper.CombineTextures(masterList, baseTexture, yStep: 8);

        //Convert the final texture to a sprite
        Sprite finalSprite = TextureHelper.ConvertTexture(finalTexture, left ? TextureHelper.SpriteType.Act2CostDecalLeft : TextureHelper.SpriteType.Act2CostDecalRight);
        return finalSprite;
    }

    [HarmonyPatch(typeof(CardDisplayer), nameof(CardDisplayer.GetCostSpriteForCard))]
    [HarmonyPrefix]
    private static bool Part2CardCostDisplayerPatch(ref Sprite __result, ref CardInfo card, ref CardDisplayer __instance)
    {
        //Make sure we are only modifying pixel cards
        if (__instance is PixelCardDisplayer && PatchPlugin.act2CostRender.Value)
        {
            /// Set the results as the new sprite
            __result = Part2SpriteFinal(card);
            return false;
        }

        return true;
    }
}