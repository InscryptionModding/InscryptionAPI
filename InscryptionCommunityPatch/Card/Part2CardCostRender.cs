using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

//[HarmonyPatch]
/// <summary>
/// Modifies how card costs are rendered in Act 2 to add support for mixed costs and custom costs.
/// Also reduces the size of card costs so they take up less space on the card.
/// </summary>
public static class Part2CardCostRender
{
    public static event Action<CardInfo, List<Texture2D>> UpdateCardCost;
    public static bool RightAct2Cost => PatchPlugin.rightAct2Cost.Value;

    public static List<Texture2D> CostTextures(CardInfo cardInfo, PlayableCard playableCard, int bloodCost, int bonesCost, int energyCost, List<GemType> gemsCost)
    {
        List<Texture2D> costTextures = new();

        if (bloodCost > 0)
            costTextures.Add(CombineIconAndCount(bloodCost, CardCostRender.GetTextureByName("pixel_blood")));

        if (bonesCost > 0)
            costTextures.Add(CombineIconAndCount(bonesCost, CardCostRender.GetTextureByName("pixel_bone")));

        if (energyCost > 0)
            costTextures.Add(CombineIconAndCount(energyCost, CardCostRender.GetTextureByName("pixel_energy")));

        if (gemsCost.Count > 0)
        {
            List<Texture2D> gemCost = new();

            // Order of: green, orange, blue
            if (gemsCost.Contains(GemType.Green))
                gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_green"));

            if (gemsCost.Contains(GemType.Orange))
                gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_orange"));

            if (gemsCost.Contains(GemType.Blue))
                gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_blue"));

            if (RightAct2Cost)
                gemCost.Reverse();

            Texture2D blankPixel = TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly);
            costTextures.Add(TextureHelper.CombineTextures(gemCost, blankPixel, xOffset: !RightAct2Cost ? 0 : (30 - 7 * gemCost.Count), xStep: 7));
        }

        // get a list of the custom costs we need textures for
        // check for PlayableCard to account for possible dynamic costs (no API support but who knows what modders do)
        List<CardCostManager.FullCardCost> customCosts;
        if (playableCard != null)
            customCosts = playableCard.GetCustomCardCosts().Select(x => CardCostManager.AllCustomCosts.Find(c => c.CostName == x.CostName)).ToList();
        else
            customCosts = cardInfo.GetCustomCosts();

        foreach (CardCostManager.FullCardCost fullCost in customCosts)
        {
            string key = fullCost.CostName + cardInfo.GetCustomCost(fullCost.CostName);
            if (CardCostRender.AssembledTextures.ContainsKey(key))
            {
                if (CardCostRender.AssembledTextures[key] != null)
                    costTextures.Add(CardCostRender.AssembledTextures[key]);
                else
                    CardCostRender.AssembledTextures.Remove(key);
            }
            else
            {
                Texture2D costTex = fullCost.GetPixelCostTexture?.Invoke(cardInfo.GetCustomCost(fullCost.CostName), cardInfo, playableCard);
                if (costTex != null)
                {
                    costTextures.Add(costTex);
                    CardCostRender.AssembledTextures.Add(key, costTex);
                }
            }
        }

        // Call the event and allow others to modify the list of textures
        UpdateCardCost?.Invoke(cardInfo, costTextures);
        return costTextures;
    }

    public static Texture2D CombineIconAndCount(int cardCost, Texture2D artCost)
    {
        //string key = 
        List<Texture2D> list = new();
        if (cardCost <= 4)
        {
            for (int i = 0; i < cardCost; i++)
                list.Add(artCost);
        }
        else
        {
            list.Add(artCost);
            list.Add(CardCostRender.GetTextureByName("pixel_L_{cardCost}"));
        }

        int xOffset = !RightAct2Cost ? 0 : cardCost >= 10 ? 30 - 20 - artCost.width : cardCost <= 4 ? 30 - artCost.width * cardCost : 30 - 14 - artCost.width;
        return TextureHelper.CombineTextures(list, TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly), xOffset: xOffset, xStep: artCost.width);
    }

    /*#region old
    public static Texture2D BlankPixelCost() => TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly);

    public static Texture2D CombineIconAndCount(int cardCost, Texture2D artCost)
    {
        List<Texture2D> list = new();
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

        int xOffset = !RightAct2Cost ? 0 : cardCost >= 10 ? 30 - 20 - artCost.width : cardCost <= 4 ? 30 - artCost.width * cardCost : 30 - 14 - artCost.width;
        return TextureHelper.CombineTextures(list, BlankPixelCost(), xOffset: xOffset, xStep: artCost.width);
    }

    public static Sprite Part2SpriteFinal(CardInfo card)
    {
        // A list to hold the textures (important later, to combine them all)
        List<Texture2D> masterList = new();
        PlayableCard playableCard = card.GetPlayableCard();

        int bloodCost = playableCard?.BloodCost() ?? card.BloodCost;
        if (bloodCost > 0)
            masterList.Add(CombineIconAndCount(bloodCost, TextureHelper.GetImageAsTexture("pixel_blood.png", typeof(Part2CardCostRender).Assembly)));

        int bonesCost = playableCard?.BonesCost() ?? card.BonesCost;
        if (bonesCost > 0)
            masterList.Add(CombineIconAndCount(bonesCost, TextureHelper.GetImageAsTexture("pixel_bone.png", typeof(Part2CardCostRender).Assembly)));

        int energyCost = playableCard?.EnergyCost ?? card.EnergyCost;
        if (energyCost > 0)
            masterList.Add(CombineIconAndCount(energyCost, TextureHelper.GetImageAsTexture("pixel_energy.png", typeof(Part2CardCostRender).Assembly)));

        List<GemType> gemsCost = playableCard?.GemsCost() ?? card.GemsCost;
        if (gemsCost.Count > 0)
        {
            List<Texture2D> gemCost = new();

            // Order of: green, orange, blue
            if (gemsCost.Contains(GemType.Green))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_green.png", typeof(Part2CardCostRender).Assembly));

            if (gemsCost.Contains(GemType.Orange))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_orange.png", typeof(Part2CardCostRender).Assembly));

            if (gemsCost.Contains(GemType.Blue))
                gemCost.Add(TextureHelper.GetImageAsTexture("pixel_mox_blue.png", typeof(Part2CardCostRender).Assembly));

            if (RightAct2Cost)
                gemCost.Reverse();

            masterList.Add(TextureHelper.CombineTextures(gemCost, BlankPixelCost(),
                xOffset: !RightAct2Cost ? 0 : (30 - 7 * gemCost.Count), xStep: 7));
        }

        if (playableCard != null)
        {
            foreach (CustomCardCost customCost in playableCard.GetCustomCardCosts())
            {
                Texture2D costTex = customCost.GetPixelCostTexture(playableCard.GetCustomCost(customCost.CostName), card, playableCard, false);
                if (costTex != null)
                    masterList.Add(costTex);
            }
        }
        else
        {
            foreach (CardCostManager.FullCardCost fullCost in card.GetCustomCosts())
            {
                Texture2D costTex = fullCost.GetPixelCostTextureFallback?.Invoke(card.GetCustomCost(fullCost.CostName), card);
                if (costTex != null)
                    masterList.Add(costTex);
            }
        }

        // Call the event and allow others to modify the list of textures
        UpdateCardCost?.Invoke(card, masterList);

        while (masterList.Count < 4)
            masterList.Add(null);

        //Combine all the textures from the list into one texture
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("pixel_base.png", typeof(Part2CardCostRender).Assembly);
        Texture2D finalTexture = TextureHelper.CombineTextures(masterList, baseTexture, yStep: 8);

        //Convert the final texture to a sprite
        return TextureHelper.ConvertTexture(finalTexture, !RightAct2Cost ? TextureHelper.SpriteType.Act2CostDecalLeft : TextureHelper.SpriteType.Act2CostDecalRight);
    }
    #endregion*/
}