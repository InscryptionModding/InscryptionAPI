using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InscryptionAPI.PixelCard;

[HarmonyPatch]
public static class PixelCardManager // code courtesy of Nevernamed and James/kelly
{
    internal static void Initialise()
    {
        PixelGemifiedDecal = TextureHelper.GetImageAsSprite("PixelGemifiedDecal.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
        PixelGemifiedOrangeLit = TextureHelper.GetImageAsSprite("PixelGemifiedOrange.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
        PixelGemifiedGreenLit = TextureHelper.GetImageAsSprite("PixelGemifiedGreen.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
        PixelGemifiedBlueLit = TextureHelper.GetImageAsSprite("PixelGemifiedBlue.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
    }

    [HarmonyPatch(typeof(PixelCardDisplayer), nameof(PixelCardDisplayer.DisplayInfo))]
    [HarmonyPostfix]
    private static void DecalPatches(PixelCardDisplayer __instance)
    {
        if (__instance.gameObject.name == "PixelSnap" || SceneManager.GetActiveScene().name == "GBC_CardBattle" && __instance.gameObject.name != "CardPreviewPanel")
            AddDecalToCard(in __instance);
    }

    private static void AddDecalToCard(in PixelCardDisplayer instance)
    {
        if (instance && instance.gameObject && instance.gameObject.transform && instance.gameObject.transform.Find("CardElements"))
        {
            Transform cardElements = instance.gameObject.transform.Find("CardElements");

            List<Transform> existingDecals = new();
            foreach (Transform child in cardElements.transform)
            {
                if (child && child.gameObject && child.gameObject.GetComponent<DecalIdentifier>())
                    existingDecals.Add(child);
            }
            for (int i = existingDecals.Count - 1; i >= 0; i--)
            {
                existingDecals[i].parent = null;
                UnityObject.Destroy(existingDecals[i].gameObject);
            }

            if (instance.info.Gemified && cardElements.Find("PixelGemifiedBorder") == null)
            {
                GameObject border = CreateDecal(in cardElements, PixelGemifiedDecal, "PixelGemifiedBorder");
                PixelGemificationBorder gemBorder = border.AddComponent<PixelGemificationBorder>();
                gemBorder.BlueGemLit = CreateDecal(in cardElements, PixelGemifiedBlueLit, "PixelGemifiedBlue");
                gemBorder.GreenGemLit = CreateDecal(in cardElements, PixelGemifiedGreenLit, "PixelGemifiedGreen");
                gemBorder.OrangeGemLit = CreateDecal(in cardElements, PixelGemifiedOrangeLit, "PixelGemifiedOrange");
            }

            foreach (CardAppearanceBehaviour.Appearance appearance in instance.info.appearanceBehaviour)
            {
                CardAppearanceBehaviourManager.FullCardAppearanceBehaviour fullApp = CardAppearanceBehaviourManager.AllAppearances.Find((x) => x.Id == appearance);
                if (fullApp != null && fullApp.AppearanceBehaviour != null)
                {
                    Component behav = instance.gameObject.GetComponent(fullApp.AppearanceBehaviour);
                    if (behav == null) behav = instance.gameObject.AddComponent(fullApp.AppearanceBehaviour);
                    if (behav is PixelAppearanceBehaviour)
                    {
                        (behav as PixelAppearanceBehaviour).OnAppearanceApplied();
                        if ((behav as PixelAppearanceBehaviour).PixelAppearance() != null && cardElements.Find(appearance.ToString() + "_Displayer") == null)
                            CreateDecal(in cardElements, (behav as PixelAppearanceBehaviour).PixelAppearance(), appearance.ToString() + "_Displayer");
                    }
                    UnityObject.Destroy(behav);
                }
            }
        }
    }
    private static GameObject CreateDecal(in Transform cardElements, Sprite sprite, string name)
    {
        GameObject decal = new(name);
        decal.transform.SetParent(cardElements, false);
        decal.layer = LayerMask.NameToLayer("GBCUI");

        decal.AddComponent<DecalIdentifier>();

        SpriteRenderer sr = decal.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // Find sorting group values
        if (cardElements.Find("Portrait") != null && cardElements.Find("Portrait").gameObject && cardElements.Find("Portrait").gameObject.GetComponent<SpriteRenderer>())
        {
            SpriteRenderer sortingReference = cardElements.Find("Portrait").gameObject.GetComponent<SpriteRenderer>();
            sr.sortingLayerID = sortingReference?.sortingLayerID ?? 0;
            sr.sortingOrder = sortingReference?.sortingOrder + 100 ?? 0;
        }

        return decal;
    }

    public static Sprite PixelGemifiedDecal;
    public static Sprite PixelGemifiedOrangeLit;
    public static Sprite PixelGemifiedBlueLit;
    public static Sprite PixelGemifiedGreenLit;

    private class DecalIdentifier : MonoBehaviour { }
}
