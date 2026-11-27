using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using UnityEngine;
using static InscryptionAPI.Helpers.TextureHelper;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// Modifies how card costs are rendered in Act 2 to add support for mixed costs and custom costs.
/// Also reduces the size of card costs so they take up less space on the card.
/// </summary>
public static class Part2CardCostRender
{
    /// <summary>
    /// An Action of <see cref="CardInfo"/> to <see cref="List{Texture2D}"/> representing a card cost update.
    /// </summary>
    public static event Action<CardInfo, List<Texture2D>> UpdateCardCost;
    
    /// <summary>
    /// For textures that are 24 x 13 in size.
    /// </summary>
    public static event Action<CardInfo, List<Texture2D>> UpdateVanillaCardCost; // 24 x 13

    /// <summary>
    /// An Action of <see cref="CardInfo"/> to <see cref="List{Texture2D}"/> representing a playable card cost update.
    /// </summary>
    public static event Action<PlayableCard, List<Texture2D>> UpdatePlayableCardCost;
    
    /// <summary>
    /// For textures that are 24 x 13 in size.
    /// </summary>
    public static event Action<PlayableCard, List<Texture2D>> UpdateVanillaPlayableCardCost;

    /// <summary>
    /// Whether or not the Cost should be on the right in Act 2.
    /// </summary>
    public static bool RightAct2Cost => PatchPlugin.configDisplayAct2CostInTheTopRightCorner.Value;

    /// <summary>
    /// Handles getting the Final Sprite for the Vanilla Cost of an Act 2 Card.
    /// </summary>
    /// <param name="display">The <see cref="PixelCardDisplayer"/> associated with the Card.</param>
    /// <param name="cardInfo">The <see cref="CardInfo"/> we are targeting to apply the visual for..</param>
    /// <param name="playableCard">The <see cref="PlayableCard"/> representation of the <see cref="CardInfo"/>.</param>
    /// <param name="leftOriented">Whether or not to apply the Cost Left/Right oriented. True means Left, False means Right.</param>
    /// <returns>A <see cref="Sprite"/> representing the Act 2 Cost of the given card.</returns>
    public static Sprite FinalVanillaCostSprite(PixelCardDisplayer display, CardInfo cardInfo, PlayableCard playableCard, bool leftOriented)
    {
        //PatchPlugin.Logger.LogDebug($"Use vanilla style");

        Texture2D baseTexture;
        List<Texture2D> masterTextures;
        List<Texture2D> topTextures = new();
        List<Texture2D> bottomTextures = new();

        int bloodCost = playableCard?.BloodCost() ?? cardInfo.BloodCost;
        int bonesCost = playableCard?.BonesCost() ?? cardInfo.BonesCost;
        int energyCost = playableCard?.EnergyCost ?? cardInfo.EnergyCost;
        List<GemType> gemsCost = playableCard?.GemsCost() ?? cardInfo.GemsCost;

        baseTexture = TextureHelper.GetImageAsTexture("pixel_base_large.png", typeof(CardCostRender).Assembly);
        masterTextures = VanillaCostTextures(display, cardInfo, playableCard, bloodCost, bonesCost, energyCost, gemsCost);

        while (masterTextures.Count < 4)
            masterTextures.Add(null);

        if (leftOriented)
        {
            topTextures.Add(masterTextures[0]);
            topTextures.Add(masterTextures[2]);
            bottomTextures.Add(masterTextures[1]);
            bottomTextures.Add(masterTextures[3]);
        }
        else
        {
            topTextures.Add(masterTextures[2]);
            topTextures.Add(masterTextures[0]);
            bottomTextures.Add(masterTextures[3]);
            bottomTextures.Add(masterTextures[1]);
        }

        Texture2D topTexture = CombineVanillaCostTextures(topTextures, baseTexture, leftOriented, 15);
        Texture2D finalCombinedTex = CombineVanillaCostTextures(bottomTextures, topTexture, leftOriented, 1);

        return TextureHelper.ConvertTexture(finalCombinedTex, leftOriented ? SpriteType.Act2CostVanillaLeft : SpriteType.Act2CostVanillaRight);
    }

    /// <summary>
    /// A variant of <see cref="TextureHelper.CombineTextures"/> specifically designed for combining vanilla cost textures.
    /// Each cost texture is assumed to be 13 pixels in height and UP TO 24 pixels in width, and this method compensates for any textures thinner than 24 px.
    /// </summary>
    /// <param name="textures">The List of <see cref="Texture2D"/> to be combined.</param>
    /// <param name="baseTexture">The base of the Combined texture, as a <see cref="Texture2D"/>.</param>
    /// <param name="leftOriented">Whether or not to apply the Cost Left/Right oriented. True means Left, False means Right.</param>
    /// <param name="yOffset">The Y Offset to use when combining the Textures.</param>
    /// <returns>The <see cref="Texture2D"/> representing the combined texture of the Card's Cost.</returns>
    public static Texture2D CombineVanillaCostTextures(List<Texture2D> textures, Texture2D baseTexture, bool leftOriented, int yOffset)
    {
        if (textures != null)
        {
            int xStep = leftOriented ? 0 : 24;
            for (int j = 0; j < textures.Count; j++)
            {
                if (textures[j] != null)
                {
                    int xOffset = leftOriented ? 0 : 24 - textures[j].width;
                    baseTexture.SetPixels(xOffset + xStep * j, yOffset, textures[j].width, textures[j].height, textures[j].GetPixels(), 0);
                }
            }

            baseTexture.Apply();
        }

        return baseTexture;
    }

    /// <summary>
    /// This function handles getting the cost texture for the given card from the specified cost amounts that are passed into it.
    /// </summary>
    /// <param name="display">The <see cref="PixelCardDisplayer"/> representing the Card.</param>
    /// <param name="cardInfo">The <see cref="CardInfo"/> representing the Card.</param>
    /// <param name="playableCard">The <see cref="PlayableCard"/> representing the Playable Version of the Card.</param>
    /// <param name="bloodCost">The amount of blood by which the card costs.</param>
    /// <param name="bonesCost">The amount of bones by which the card costs.</param>
    /// <param name="energyCost">The amount of energy by which the card costs.</param>
    /// <param name="gemsCost">The Types of Gem's the card costs.</param>
    /// <returns>A List of <see cref="Texture2D"/> representing the Cost Textures for the card.</returns>
    public static List<Texture2D> VanillaCostTextures(PixelCardDisplayer display, CardInfo cardInfo, PlayableCard playableCard, int bloodCost, int bonesCost, int energyCost, List<GemType> gemsCost)
    {
        List<Texture2D> costTextures = new();

        if (bloodCost > 0)
        {
            costTextures.Add(CardCostRender.GetTextureByName($"pixel_blood_large_{Mathf.Min(14, bloodCost)}"));
        }

        if (bonesCost > 0)
        {
            costTextures.Add(CardCostRender.GetTextureByName($"pixel_bone_large_{Mathf.Min(16, bonesCost)}"));
        }

        if (energyCost > 0)
        {
            costTextures.Add(CardCostRender.GetTextureByName($"pixel_energy_large_{Mathf.Min(7, energyCost)}"));
        }

        if (gemsCost.Count > 0)
        {
            costTextures.AddRange(GetVanillaMoxTextures(gemsCost));
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
            string key = $"{fullCost.ModGUID}_{fullCost.CostName}_{amount}_vanilla_part2";
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
        UpdateVanillaCardCost?.Invoke(cardInfo, costTextures);
        UpdatePlayableCardCost?.Invoke(playableCard, costTextures);
        UpdateVanillaPlayableCardCost?.Invoke(playableCard, costTextures);

        return costTextures;
    }

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

    public static List<Texture2D> GetVanillaMoxTextures(List<GemType> gemsCost) // vanilla order is OGB
    {
        List<Texture2D> gemTextures = new();
        int orangeCount = Mathf.Min(4, gemsCost.Count(x => x == GemType.Orange));
        int greenCount = Mathf.Min(4, gemsCost.Count(x => x == GemType.Green));
        int blueCount = Mathf.Min(4, gemsCost.Count(y => y == GemType.Blue));

        if (orangeCount > 0)
        {
            if (orangeCount == 1)
            {
                if (gemsCost.Count == 2) // 1 orange and 1 other
                {
                    if (greenCount > 0)
                    {
                        gemTextures.Add(CardCostRender.GetTextureByName("pixel_mox_green_orange_large"));
                    }
                    else
                    {
                        gemTextures.Add(CardCostRender.GetTextureByName("pixel_mox_blue_orange_large"));
                    }
                    return gemTextures;
                }
                else if (gemsCost.Count == 3 && greenCount == orangeCount && blueCount == orangeCount) // one of each
                {
                    gemTextures.Add(CardCostRender.GetTextureByName("pixel_mox_grand_large"));
                    return gemTextures;
                }
            }

            // default behaviour, accounts for multi mono mox
            gemTextures.Add(CardCostRender.GetTextureByName($"pixel_mox_orange_large_{orangeCount}"));
        }

        if (greenCount > 0)
        {
            if (greenCount == 1 && blueCount == 1)
            {
                gemTextures.Add(CardCostRender.GetTextureByName("pixel_mox_blue_green_large"));
                return gemTextures;
            }

            // default behaviour, accounts for multi mono mox
            gemTextures.Add(CardCostRender.GetTextureByName($"pixel_mox_green_large_{greenCount}"));
        }

        if (blueCount > 0) // at this point, we have eliminated all vanilla costs and we only some combination of multi mono costs
        {
            // default behaviour, accounts for multi mono mox
            gemTextures.Add(CardCostRender.GetTextureByName($"pixel_mox_blue_large_{blueCount}"));
        }

        return gemTextures;
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