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
            List<StarterDeckInfo> decksToAdd = new(StarterDeckManager.AllDecks.ConvertAll((x) => x.Info));
            List<AscensionStarterDeckIcon> icons = __instance.deckIcons;
            icons.ForEach(delegate (AscensionStarterDeckIcon ic)
            {
                if (ic != null && ic.Info != null && !string.IsNullOrEmpty(ic.Info.title) && decksToAdd.FindAll((StarterDeckInfo inf) => inf.title.ToLower() == ic.Info.title.ToLower()).Count > 0)
                {
                    decksToAdd.Remove(decksToAdd.Find((StarterDeckInfo inf) => inf.title.ToLower() == ic.Info.title.ToLower()));
                }
            });
            icons.ForEach(delegate (AscensionStarterDeckIcon ic)
            {
                if (ic != null && ic.Info == null && decksToAdd.Count > 0)
                {
                    ic.starterDeckInfo = decksToAdd[0];
                    ic.AssignInfo(decksToAdd[0]);
                    decksToAdd.RemoveAt(0);
                }
            });
            List<List<StarterDeckInfo>> pagesToAdd = new();
            while (decksToAdd.Count > 0)
            {
                List<StarterDeckInfo> page = new();
                for (int i = 0; i < icons.Count; i++)
                {
                    if (decksToAdd.Count > 0)
                    {
                        page.Add(decksToAdd[0]);
                        decksToAdd.RemoveAt(0);
                    }
                }
                pagesToAdd.Add(page);
            }
            if (pagesToAdd.Count > 0)
            {
                StarterDeckPaginator manager = __instance.gameObject.AddComponent<StarterDeckPaginator>();
                manager.Initialize(__instance);
                foreach (List<StarterDeckInfo> page in pagesToAdd)
                {
                    manager.AddPage(page);
                }
                Vector3 topRight = new Vector3(float.MinValue, float.MinValue);
                Vector3 bottomLeft = new Vector3(float.MaxValue, float.MaxValue);
                foreach (AscensionStarterDeckIcon icon in icons)
                {
                    if (icon != null && icon.iconRenderer != null)
                    {
                        if (icon.iconRenderer.transform.position.x < bottomLeft.x)
                        {
                            bottomLeft.x = icon.iconRenderer.transform.position.x;
                        }
                        if (icon.iconRenderer.transform.position.x > topRight.x)
                        {
                            topRight.x = icon.iconRenderer.transform.position.x;
                        }
                        if (icon.iconRenderer.transform.position.y < bottomLeft.y)
                        {
                            bottomLeft.y = icon.iconRenderer.transform.position.y;
                        }
                        if (icon.iconRenderer.transform.position.y > topRight.y)
                        {
                            topRight.y = icon.iconRenderer.transform.position.y;
                        }
                    }
                }
                GameObject leftArrow = UnityEngine.Object.Instantiate(__instance.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageLeftButton.gameObject);
                leftArrow.transform.parent = __instance.transform;
                leftArrow.transform.position = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.left / 2f;
                leftArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                leftArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.PreviousPage();
                GameObject rightArrow = UnityEngine.Object.Instantiate(__instance.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject);
                rightArrow.transform.parent = __instance.transform;
                rightArrow.transform.position = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.right / 2f;
                rightArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                rightArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.NextPage();
            }
        }
    }

    private static Sprite noneCard;

    [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), nameof(AscensionChooseStarterDeckScreen.OnCursorEnterDeckIcon))]
    [HarmonyPrefix]
    private static bool FixDeckPreview(AscensionChooseStarterDeckScreen __instance, AscensionStarterDeckIcon icon)
    {
        var screenPlus = __instance.GetComponent<StarterDeckScreenPlus>() ?? __instance.gameObject.AddComponent<StarterDeckScreenPlus>();
        screenPlus.tempCards?.ForEach(x => x.SetActive(false));
        if(icon.starterDeckInfo == null)
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
            if(icon.Unlocked && icon.starterDeckInfo.cards != null && icon.starterDeckInfo.cards.Count != __instance.cards.Count)
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
                if(firstOrDefault != null)
                {
                    screenPlus.tempCards ??= new();
                    var cards = icon.starterDeckInfo.cards;
                    for (int i = 0; i < cards.Count; i++)
                    {
                        var card = cards[i];
                        float distance = 0.5f;
                        float position = -((cards.Count - 1) / 2 * distance) + distance * i;
                        if(Math.Abs(position) < 2.6f)
                        {
                            var cloned = screenPlus.tempCards.Find(x => !x.activeSelf)?.GetComponentInChildren<PixelSelectableCard>() ?? UnityObject.Instantiate(firstOrDefault);
                            cloned.transform.parent = firstOrDefault.transform.parent;
                            cloned.transform.localPosition = new(position, firstOrDefault.transform.localPosition.y, firstOrDefault.transform.localPosition.z);
                            cloned.SetInfo(card);
                            cloned.SetEnabled(false);
                            cloned.gameObject.SetActive(true);
                            if (!screenPlus.tempCards.Contains(cloned.gameObject))
                            {
                                screenPlus.tempCards.Add(cloned.gameObject);
                            }
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
}