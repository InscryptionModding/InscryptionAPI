using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class Part1CardCostRender
{
    // This patches the way card costs are rendered in Act 1 (Leshy's cabin)
    // It allows mixed card costs to display correctly (i.e., 2 blood, 1 bone)
    // And allows gem cost and energy cost to render on the card at all.

    public static event Action<CardInfo, List<Texture2D>> UpdateCardCost;

    private static Dictionary<string, Texture2D> AssembledTextures = new();

    public const int COST_OFFSET = 28;

    public const int MOX_OFFSET = 21;

    public static Texture2D CombineCostTextures(List<Texture2D> costs)
    {
        while (costs.Count < 4)
            costs.Add(null);
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("empty_cost.png", typeof(Part1CardCostRender).Assembly);
        return TextureHelper.CombineTextures(costs, baseTexture, yStep: COST_OFFSET);
    }

    public static Texture2D CombineMoxTextures(List<Texture2D> costs)
    {
        Texture2D baseTexture = TextureHelper.GetImageAsTexture("mox_cost_empty.png", typeof(Part1CardCostRender).Assembly);
        return TextureHelper.CombineTextures(costs, baseTexture, xStep: MOX_OFFSET);
    }

    private static Texture2D GetTextureByName(string key)
    {
        if (AssembledTextures.ContainsKey(key))
        {
            if (AssembledTextures[key] != null)
                return AssembledTextures[key];

            AssembledTextures.Remove(key);
        }

        Texture2D texture = TextureHelper.GetImageAsTexture($"{key}.png", typeof(Part1CardCostRender).Assembly);
        AssembledTextures.Add(key, texture);
        return texture;
    }

    internal static string GemCost(CardInfo info)
    {
        return (info.GemsCost.Contains(GemType.Orange) ? "o" : string.Empty) +
               (info.GemsCost.Contains(GemType.Blue) ? "b" : string.Empty) +
               (info.GemsCost.Contains(GemType.Green) ? "g" : string.Empty);
    }

    public static Sprite Part1SpriteFinal(CardInfo card)
    {
        // A list to hold the textures (important later, to combine them all)
        List<Texture2D> list = new List<Texture2D>();

        //Setting mox first
        if (card.gemsCost.Count > 0)
        {
            //make a new list for the mox textures
            List<Texture2D> gemCost = new List<Texture2D>();

            //load up the mox textures as "empty"
            Texture2D orange = GetTextureByName(card.GemsCost.Contains(GemType.Orange) ? "mox_cost_o" : "mox_cost_e");
            Texture2D blue = GetTextureByName(card.GemsCost.Contains(GemType.Blue) ? "mox_cost_b" : "mox_cost_e");
            Texture2D green = GetTextureByName(card.GemsCost.Contains(GemType.Green) ? "mox_cost_g" : "mox_cost_e");

            //Add all moxes to the gemcost list
            gemCost.Add(orange);
            gemCost.Add(green);
            gemCost.Add(blue);

            //Combine the textures into one
            list.Add(CombineMoxTextures(gemCost));
        }

        if (card.EnergyCost > 0)
            list.Add(GetTextureByName($"energy_cost_{card.EnergyCost}"));

        if (card.BonesCost > 0)
            list.Add(GetTextureByName($"bone_cost_{card.BonesCost}"));

        if (card.BloodCost > 0)
            list.Add(GetTextureByName($"blood_cost_{card.BloodCost}"));

        // Call the event and allow others to modify the list of textures
        UpdateCardCost?.Invoke(card, list);

        // Combine all the textures from the list into one texture
        Texture2D finalTexture = CombineCostTextures(list);

        // Convert the final texture to a sprite
        return TextureHelper.ConvertTexture(finalTexture, TextureHelper.SpriteType.OversizedCostDecal);
    }

    [HarmonyPatch(typeof(CardDisplayer), nameof(CardDisplayer.GetCostSpriteForCard))]
    [HarmonyPrefix]
    private static bool Part1CardCostDisplayerPatch(ref Sprite __result, ref CardInfo card, ref CardDisplayer __instance)
    {
        //Make sure we are in Leshy's Cabin
        if (__instance is CardDisplayer3D && SceneLoader.ActiveSceneName.StartsWith("Part1"))
        {
            /// Set the results as the new sprite
            __result = Part1SpriteFinal(card);
            return false;
        }

        return true;
    }
}