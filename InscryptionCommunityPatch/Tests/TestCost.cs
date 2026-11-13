using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using InscryptionCommunityPatch.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Tests;

/// <summary>
/// This function defines our Testing Cost.
/// </summary>
public class TestCost : CustomCardCost
{
    /// <summary>
    /// The Name of the Custom Cost.
    /// </summary>
    public override string CostName => "TestCost";
    /// <summary>
    /// This function initializes our Test Cost.
    /// </summary>
    public static void Init()
    {
        PatchPlugin.Logger.LogDebug("Adding TestCost");

        CardCostManager.Register(InscryptionAPIPlugin.ModGUID, "TestCost", typeof(TestCost), Texture3D, TexturePixel)
            .SetCostTier(CostTier)
            .SetCanBePlayedByTurn2WithHand(CanBePlayed)
            .SetFoundAtChoiceNodes(true, TextureHelper.GetImageAsTexture("testCost.png", typeof(TestCost).Assembly));
    }

    /// <summary>
    /// This defines the Cost Tier associated with the Amount of the Cost.
    /// </summary>
    /// <param name="amount">The amount of the Test Cost.</param>
    /// <returns>A <see cref="Mathf.Floor"/> of <c>amount/2</c>.</returns>
    public static int CostTier(int amount)
    {
        return Mathf.FloorToInt(amount / 2f);
    }
    
    /// <summary>
    /// A bool determining whether a <see cref="CardInfo"/> can be played with the current amount of the cost.
    /// </summary>
    /// <param name="amount">The Amount of a Cost the Player currently has.</param>
    /// <param name="card">The Card the Player is trying to play.</param>
    /// <param name="hand">The Player's current Hand.</param>
    /// <returns>True if the Amount of the Cost is <c>&lt;= 2</c>.</returns>
    public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
    {
        return amount <= 2;
    }
    
    /// <summary>
    /// A Function used to get the Cost's Texture within 3D Act's.
    /// </summary>
    /// <param name="cardCost">The Cost Amount of the Card.</param>
    /// <param name="cardInfo">The <see cref="CardInfo"/> of the Card.</param>
    /// <param name="playableCard">The <see cref="PlayableCard"/> variant of the Card.</param>
    /// <returns>A <see cref="ReverseColours"/> application of the 3D Act Variant EnergyCost associated with the passed in Cost Amount.</returns>
    public static Texture2D Texture3D(int cardCost, CardInfo cardInfo, PlayableCard playableCard)
    {
        Texture2D tex = TextureHelper.GetImageAsTexture($"energy_cost_{Mathf.Min(7, cardCost)}.png", typeof(PatchPlugin).Assembly);
        return ReverseColours(tex);
    }
    
    /// <summary>
    /// A Function used to get the Cost's Texture within 2D Act's.
    /// </summary>
    /// <param name="cardCost">The Cost Amount of the Card.</param>
    /// <param name="info">The <see cref="CardInfo"/> of the Card.</param>
    /// <param name="playableCard">The <see cref="PlayableCard"/> variant of the Card.</param>
    /// <returns>A <see cref="ReverseColours"/> application of the 2D Act Variant EnergyCost associated with the passed in Cost Amount.</returns>
    /// <returns></returns>
    public static Texture2D TexturePixel(int cardCost, CardInfo info, PlayableCard playableCard)
    {
        Texture2D tex = Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture($"pixel_energy.png", typeof(PatchPlugin).Assembly));
        return ReverseColours(tex);
    }
    
    /// <summary>
    /// A function used to Reverse the Colours of a Sprite.
    /// </summary>
    /// <param name="texToReverse">The Texture in which to reverse the Colours of.</param>
    /// <returns>A Colour Reversed version of the Passed Texture.</returns>
    private static Texture2D ReverseColours(Texture2D texToReverse)
    {
        for (int x = 0; x < texToReverse.width; x++)
        {
            for (int y = 0; y < texToReverse.height; y++)
            {
                Color pixel = texToReverse.GetPixel(x, y);
                Color.RGBToHSV(pixel, out float H, out float S, out float V);
                Color final_color = Color.HSVToRGB((H + 0.5f) % 1f, (S + 0.5f) % 1f, (V + 0.5f) % 1f);
                final_color.a = pixel.a;

                texToReverse.SetPixel(x, y, final_color);
            }
        }
        texToReverse.Apply();
        return texToReverse;
    }

    /// <summary>
    /// A Bool determining whether the Card's Cost was satisfied or not.
    /// </summary>
    /// <param name="cardCost">The Card's Cost.</param>
    /// <param name="card">The Card in which is attempting to be played.</param>
    /// <returns>True if the Cost was satisified and false otherwise.</returns>
    public override bool CostSatisfied(int cardCost, PlayableCard card)
    {
        return cardCost <= (Singleton<ResourcesManager>.Instance.PlayerEnergy - card.EnergyCost);
    }
    
    /// <summary>
    /// The Hint said when the Cost was not Satisfied.
    /// </summary>
    /// <param name="cardCost">The Card's Cost.</param>
    /// <param name="card">The Card in which is attempting to be played.</param>
    /// <returns>The String of the Hint to be said when the Card's Cost is not Satisfied.</returns>
    public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
    {
        return "Eat your greens aby. " + card.Info.DisplayedNameLocalized;
    }

    /// <summary>
    /// Depletes the Card's Cost from our Custom Cost's Cost Pool.
    /// </summary>
    /// <param name="cardCost">The Card's Cost</param>
    /// <param name="card">The Card in which is attempting to be played.</param>
    /// <returns>The Card's Cost depleted from the Custom Cost's pool.</returns>
    public override IEnumerator OnPlayed(int cardCost, PlayableCard card)
    {
        if (cardCost > 0)
            yield return Singleton<ResourcesManager>.Instance.SpendEnergy(cardCost);
    }
}