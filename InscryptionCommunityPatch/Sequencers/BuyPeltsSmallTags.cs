using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using Pixelplacement;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Sequencers;

[HarmonyPatch]
internal class BuyPeltsSmallTags
{
    [HarmonyPostfix, HarmonyPatch(typeof(BuyPeltsSequencer), nameof(BuyPeltsSequencer.AddPricetagToCard))]
    private static void ReducePricetagSize(SelectableCard card)
    {
        if (!PatchPlugin.configSmallPricetags.Value)
            return;

        Transform t = card.transform.Find("pricetag");
        if (t != null)
        {
            t.localScale = new(0.75f, 1f, 0.75f);
        }
    }
}