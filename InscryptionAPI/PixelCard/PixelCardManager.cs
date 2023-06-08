using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Resource;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InscryptionAPI.PixelCard;

[HarmonyPatch]
public static class PixelCardManager // code courtesy of Nevernamed and James/kelly
{
    public class PixelDecalData
    {
        public string PluginGUID;
        public string TextureName;
        public Texture2D DecalTexture;
    }

    public static readonly List<PixelDecalData> CustomPixelDecals = new();

    public static PixelDecalData AddGBCDecal(string pluginGUID, string textureName, Texture2D texture)
    {
        PixelDecalData result =  new()
        {
            PluginGUID = pluginGUID,
            TextureName = textureName,
            DecalTexture = texture
        };
        if (!CustomPixelDecals.Contains(result))
            CustomPixelDecals.Add(result);
        
        return result;
    }
    internal static void Initialise()
    {
        PixelGemifiedDecal = TextureHelper.GetImageAsSprite("PixelGemifiedDecal.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
        PixelGemifiedOrangeLit = TextureHelper.GetImageAsSprite("PixelGemifiedOrange.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
        PixelGemifiedGreenLit = TextureHelper.GetImageAsSprite("PixelGemifiedGreen.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
        PixelGemifiedBlueLit = TextureHelper.GetImageAsSprite("PixelGemifiedBlue.png", typeof(PixelCardManager).Assembly, TextureHelper.SpriteType.PixelDecal);
    }

    [HarmonyPatch(typeof(PixelCardDisplayer), nameof(PixelCardDisplayer.UpdateBackground))]
    [HarmonyPostfix]
    private static void PixelUpdateBackground(PixelCardDisplayer __instance, CardInfo info)
    {
        foreach (CardAppearanceBehaviour.Appearance appearance in info.appearanceBehaviour)
        {
            CardAppearanceBehaviourManager.FullCardAppearanceBehaviour fullApp = CardAppearanceBehaviourManager.AllAppearances.Find((CardAppearanceBehaviourManager.FullCardAppearanceBehaviour x) => x.Id == appearance);
            if (fullApp?.AppearanceBehaviour == null)
                continue;

            Component behav = __instance.gameObject.GetComponent(fullApp.AppearanceBehaviour);
            behav ??= __instance.gameObject.AddComponent(fullApp.AppearanceBehaviour);

            Sprite back = (behav as PixelAppearanceBehaviour)?.OverrideBackground();
            if (back != null)
            {
                SpriteRenderer component = __instance.GetComponent<SpriteRenderer>();
                if (component != null)
                    component.sprite = back;
            }
            UnityObject.Destroy(behav);
        }
    }

    [HarmonyPatch(typeof(PixelCardDisplayer), nameof(PixelCardDisplayer.DisplayInfo))]
    [HarmonyPostfix]
    private static void DecalPatches(PixelCardDisplayer __instance, PlayableCard playableCard)
    {
        if (__instance.gameObject.name == "PixelSnap" || SceneManager.GetActiveScene().name == "GBC_CardBattle" && __instance.gameObject.name != "CardPreviewPanel")
            AddDecalToCard(in __instance, playableCard);
    }

    private static void AddDecalToCard(in PixelCardDisplayer instance, PlayableCard playableCard)
    {
        Transform cardElements = instance?.gameObject?.transform?.Find("CardElements");

        if (cardElements == null)
            return;

        List<Transform> existingDecals = new();

        // clear current decals and appearances
        foreach (Transform child in cardElements.transform)
        {
            if (child?.gameObject?.GetComponent<DecalIdentifier>())
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
            if (fullApp?.AppearanceBehaviour == null)
                continue;

            Component behav = instance.gameObject.GetComponent(fullApp.AppearanceBehaviour);
            behav ??= instance.gameObject.AddComponent(fullApp.AppearanceBehaviour);

            if (behav is PixelAppearanceBehaviour)
            {
                (behav as PixelAppearanceBehaviour).OnAppearanceApplied();
                Sprite behavAppearance = (behav as PixelAppearanceBehaviour).PixelAppearance();
                Transform behavTransform = cardElements.Find(appearance.ToString() + "_Displayer");

                if (behavAppearance != null && behavTransform == null)
                    CreateDecal(in cardElements, behavAppearance, appearance.ToString() + "_Displayer");
            }
            UnityObject.Destroy(behav);
        }

        if (playableCard == null)
            return;

        List<Texture2D> decalTextures = new();
        foreach (CardModificationInfo mod in playableCard.Info.Mods)
        {
            foreach (string decalId in mod.DecalIds)
            {
                PixelDecalData data = CustomPixelDecals.Find(x => x.TextureName == decalId);

                if (data != null)
                    decalTextures.Add(data.DecalTexture);
            }
        }
        foreach (Texture2D decalTex in decalTextures)
        {
            Sprite decalSprite = TextureHelper.ConvertTexture(decalTex, TextureHelper.SpriteType.PixelDecal);
            CreateDecal(in cardElements, decalSprite, decalTex.name);
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
        SpriteRenderer sortingReference = cardElements?.Find("Portrait")?.gameObject?.GetComponent<SpriteRenderer>();
        if (sortingReference != null)
        {
            sr.sortingLayerID = sortingReference.sortingLayerID;
            sr.sortingOrder = sortingReference.sortingOrder;
        }

        return decal;
    }

    public static Sprite PixelGemifiedDecal;
    public static Sprite PixelGemifiedOrangeLit;
    public static Sprite PixelGemifiedBlueLit;
    public static Sprite PixelGemifiedGreenLit;

    private class DecalIdentifier : MonoBehaviour { }
}
