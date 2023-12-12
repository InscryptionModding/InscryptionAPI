using DiskCardGame;
using GBC;
using HarmonyLib;
using Pixelplacement;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.PixelTutor;

// fixes Tutor to work in Act 2
[HarmonyPatch]
public static class Act2TutorFix
{
    private static bool HasComponent = false;

    [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.Initialize))]
    [HarmonyPostfix]
    private static void AddPixelSelectableArray(BoardManager __instance)
    {
        if (!SaveManager.SaveFile.IsPart2)
            return;

        if (__instance.GetComponent<PixelPlayableCardArray>() == null)
        {
            __instance.gameObject.AddComponent<PixelPlayableCardArray>();
            HasComponent = true;
        }
    }
    [HarmonyPrefix, HarmonyPatch(typeof(PixelPlayableCard), nameof(PixelPlayableCard.ManagedUpdate))]
    private static bool DisableManagedUpdateForSelection(PixelPlayableCard __instance) => !HasComponent || __instance.OnBoard || __instance.InHand;

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.OnCursorSelectStart))]
    [HarmonyPrefix]
    private static bool AddSelectedCardToHand(PlayableCard __instance)
    {
        if (__instance is not PixelPlayableCard card || __instance.OnBoard || __instance.InHand || !HasComponent)
            return true;

        var component = PixelBoardManager.Instance.GetComponent<PixelPlayableCardArray>();
        if (component.displayedCards.Contains(card))
            component.selectedCard = card;

        return false;
    }
    [HarmonyPatch(typeof(Tutor), nameof(Tutor.OnResolveOnBoard))]
    [HarmonyPostfix]
    private static IEnumerator AddStartDelay(IEnumerator enumerator, Tutor __instance)
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            yield return new WaitForSeconds(0.15f);
            if (__instance.Card.OpponentCard || PixelCardDrawPiles.Instance.Deck.Cards.Count == 0)
            {
                (__instance.Card as PixelPlayableCard).Anim.NegationEffect(false);
                yield return new WaitForSeconds(0.25f);
                yield break;
            }
        }

        yield return enumerator;
    }
    [HarmonyPatch(typeof(Deck), nameof(Deck.Tutor))]
    [HarmonyPostfix]
    private static IEnumerator PixelTutorSequence(IEnumerator enumerator, Deck __instance)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            yield return enumerator;
            yield break;
        }

        if (PixelBoardManager.Instance.GetComponent<PixelPlayableCardArray>() == null)
        {
            yield return Singleton<PixelCardDrawPiles>.Instance.ChooseDraw(0);
            yield break;
        }

        CardInfo selectedCard = null;
        yield return ChoosePixelCard(__instance, delegate (CardInfo c)
        {
            selectedCard = c;
        });

        if (selectedCard == null)
            yield break;

        yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(selectedCard);
    }

    public static IEnumerator ChoosePixelCard(Deck instance, Action<CardInfo> cardSelectedCallback)
    {
        PixelPlayableCard selectedCard = null;

        yield return PixelBoardManager.Instance.GetComponent<PixelPlayableCardArray>().SelectPixelCardFrom(instance.cards, delegate (PixelPlayableCard x)
        {
            selectedCard = x;
        });

        Tween.Position(selectedCard.transform, selectedCard.transform.position + Vector3.back * 4f, 0.1f, 0f, Tween.EaseIn);
        UnityObject.Destroy(selectedCard.gameObject, 0.1f);

        cardSelectedCallback(selectedCard.Info);
    }
}