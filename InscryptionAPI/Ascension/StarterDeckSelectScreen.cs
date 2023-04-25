using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Helpers;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
internal static class StarterDeckSelectscreenPatches
{
    [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), "Start")]
    [HarmonyPostfix]
    private static void Postfix(AscensionChooseStarterDeckScreen __instance)
    {
        if (__instance.GetComponent<StarterDeckPaginator>() == null)
        {
            StarterDeckPaginator manager = __instance.gameObject.AddComponent<StarterDeckPaginator>();
        }
    }

    private static Sprite noneCard;

    [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), nameof(AscensionChooseStarterDeckScreen.OnCursorEnterDeckIcon))]
    [HarmonyPrefix]
    private static bool FixDeckPreview(AscensionChooseStarterDeckScreen __instance, AscensionStarterDeckIcon icon)
    {
        var screenPlus = __instance.GetComponent<StarterDeckScreenPlus>() ?? __instance.gameObject.AddComponent<StarterDeckScreenPlus>();
        screenPlus.tempCards?.ForEach(x => x.SetActive(false));
        if (icon.starterDeckInfo == null)
        {
            screenPlus.noneCardObjects ??= new();
            if (screenPlus.noneCardObjects.Count <= 0)
            {
                foreach (var card in __instance.cardLockedSprites)
                {
                    var cloned = UnityObject.Instantiate(card.gameObject);
                    cloned.transform.parent = card.transform.parent;
                    cloned.transform.localPosition = card.transform.localPosition;
                    cloned.GetComponent<SpriteRenderer>().sprite = noneCard ??= TextureHelper.GetImageAsTexture("ascension_card_none_darker.png", Assembly.GetExecutingAssembly()).ConvertTexture();
                    cloned.GetComponent<SpriteRenderer>().enabled = true;
                    screenPlus.noneCardObjects.Add(cloned);
                }
            }
            screenPlus.noneCardObjects.ForEach(x => x.SetActive(true));
            __instance.cardLockedSprites.ForEach(x => x.gameObject.SetActive(false));
        }
        else
        {
            screenPlus?.noneCardObjects.ForEach(x => x.SetActive(false));
            __instance.cardLockedSprites.ForEach(x => x.gameObject.SetActive(true));
            if (icon.Unlocked && icon.starterDeckInfo.cards != null && icon.starterDeckInfo.cards.Count != __instance.cards.Count)
            {
                __instance.cardLockedSprites.ForEach(delegate (SpriteRenderer x)
                {
                    x.enabled = false;
                });
                __instance.cards.ForEach(delegate (PixelSelectableCard x)
                {
                    x.gameObject.SetActive(false);
                });
                var firstOrDefault = __instance.cards.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    screenPlus.tempCards ??= new();
                    var cards = icon.starterDeckInfo.cards;
                    for (int i = 0; i < cards.Count; i++)
                    {
                        var card = cards[i];
                        float distance = cards.Count > 8 ? 0.425f : 0.5f;
                        float position = distance * (i - (cards.Count - 1) / 2f);
                        if (Math.Abs(position) <= 2.5f)
                        {
                            var cloned = screenPlus.tempCards.Find(x => !x.activeSelf)?.GetComponentInChildren<PixelSelectableCard>() ?? UnityObject.Instantiate(firstOrDefault);
                            cloned.transform.parent = firstOrDefault.transform.parent;
                            cloned.transform.localPosition = new(position, firstOrDefault.transform.localPosition.y, firstOrDefault.transform.localPosition.z);
                            cloned.SetInfo(card);
                            cloned.SetEnabled(false);
                            cloned.gameObject.SetActive(true);
                            if (!screenPlus.tempCards.Contains(cloned.gameObject))
                                screenPlus.tempCards.Add(cloned.gameObject);
                        }
                    }
                    __instance.StopAllCoroutines();
                    __instance.StartCoroutine(__instance.ShowCardsSequence());
                    return false;
                }
            }
        }
        __instance.cards.ForEach(delegate (PixelSelectableCard x)
        {
            x.gameObject.SetActive(true);
        });
        return true;
    }

    [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), nameof(AscensionChooseStarterDeckScreen.OnRandomSelected))]
    [HarmonyPrefix]
    public static bool TrueRandom(AscensionChooseStarterDeckScreen __instance)
    {
        var paginator = __instance.GetComponent<StarterDeckPaginator>();
        if (paginator != null)
        {
            var decks = paginator.pages.SelectMany(x => x).Where(x => x != null && AscensionUnlockSchedule.StarterDeckIsUnlockedForLevel(x.name, AscensionSaveData.Data.challengeLevel)).ToArray();
            var deck = decks[UnityEngine.Random.Range(0, decks.Length)];
            var index = paginator.pages.FindIndex(x => x.Contains(deck));
            if (index >= 0 && index < paginator.pages.Count)
            {
                paginator.LoadPage(paginator.pages[index]);
                var icon = __instance.deckIcons.Find(x => x.starterDeckInfo.name == deck.name);
                if (icon != null)
                {
                    __instance.OnSelectStarterDeck(icon, false);
                    __instance.randomButton.GetComponent<AscensionMenuBlinkEffect>().SetBlinkingEnabled(false);
                }
                return false;
            }
        }
        return true;
    }
}
