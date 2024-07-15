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
            // account for weird gap on the right
            // try to make single gem sprites fit with number gem sprites
            List<Texture2D> gemCost = GetMoxTextures(gemsCost);
            Texture2D blankPixel = TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly);
            costTextures.Add(CombineMoxTextures(gemCost, blankPixel/*, xOffset: !RightAct2Cost ? 0 : (30 - 9 * (Mathf.Min(3, gemCost.Count))), xStep: 9*/));
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
            int amount = playableCard?.GetCustomCost(fullCost) ?? cardInfo.GetCustomCost(fullCost);
            string key = $"{fullCost.ModGUID}_{fullCost.CostName}_{amount}_part2";
            if (CardCostRender.AssembledTextures.ContainsKey(key))
            {
                if (CardCostRender.AssembledTextures[key] != null)
                    costTextures.Add(CardCostRender.AssembledTextures[key]);
                else
                    CardCostRender.AssembledTextures.Remove(key);
            }
            else
            {
                Texture2D costTex = fullCost.PixelCostTexture(amount, cardInfo, playableCard);
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
            list.Add(CardCostRender.GetTextureByName($"pixel_L_{cardCost}"));
        }

        int xOffset = !RightAct2Cost ? 0 : cardCost >= 10 ? 30 - 20 - artCost.width : cardCost <= 4 ? 30 - artCost.width * cardCost : 30 - 14 - artCost.width;
        return TextureHelper.CombineTextures(list, TextureHelper.GetImageAsTexture("pixel_blank.png", typeof(Part2CardCostRender).Assembly), xOffset: xOffset, xStep: artCost.width);
    }

    private static Texture2D CombineMoxTextures(List<Texture2D> pieces, Texture2D baseTexture)
    {
        int xOffset = RightAct2Cost ? 30 - pieces.ConvertAll(x => x.width).Sum() : 0;
        int xStep = 0;
        for (int j = 0; j < pieces.Count; j++)
        {
            if (pieces[j] != null)
            {
                baseTexture.SetPixels(xOffset + xStep, 0, pieces[j].width, pieces[j].height, pieces[j].GetPixels(), 0);
                xStep += pieces[j].width;
            }
        }
        baseTexture.Apply();

        return baseTexture;
    }

    public static List<Texture2D> GetMoxTextures(List<GemType> gemsCost) // vanilla order is OGB
    {
        List<Texture2D> gemCost = new();
        if (gemsCost.Count <= 3)
        {
            foreach (GemType type in gemsCost.Where(x => x == GemType.Orange))
                gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_orange"));

            foreach (GemType type in gemsCost.Where(x => x == GemType.Green))
                gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_green"));

            foreach (GemType type in gemsCost.Where(x => x == GemType.Blue))
                gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_blue"));
        }
        else
        {
            int orangeCount = Mathf.Min(4, gemsCost.Count(x => x == GemType.Orange));
            int greenCount = Mathf.Min(4, gemsCost.Count(x => x == GemType.Green));
            int blueCount = Mathf.Min(4, gemsCost.Count(y => y == GemType.Blue));
            if (orangeCount > 0)
            {
                if (orangeCount == 1)
                {
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_orange"));
                }
                else if (orangeCount > 2 || (greenCount + blueCount > 1))
                {
                    gemCost.Add(CardCostRender.GetTextureByName($"pixel_mox_orange_{orangeCount}"));
                    orangeCount = 1;
                }
                else
                {
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_orange"));
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_orange"));
                }
            }

            if (greenCount > 0)
            {
                if (greenCount == 1)
                {
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_green"));
                }
                else if (greenCount > 2 || (blueCount + orangeCount > 1))
                {
                    gemCost.Add(CardCostRender.GetTextureByName($"pixel_mox_green_{greenCount}"));
                    greenCount = 1; // green is only using 1 sprite 'slot'
                }
                else
                {
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_green"));
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_green"));
                }
            }

            if (blueCount > 0)
            {
                if (blueCount == 1)
                {
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_blue"));
                }
                else if (blueCount > 2 || (greenCount + orangeCount > 1))
                {
                    gemCost.Add(CardCostRender.GetTextureByName($"pixel_mox_blue_{blueCount}"));
                    //blueCount = 1;
                }
                else
                {
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_blue"));
                    gemCost.Add(CardCostRender.GetTextureByName("pixel_mox_blue"));
                }
            }
        }

        return gemCost;
    }
}