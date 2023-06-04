using DiskCardGame;
using GBC;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using Sirenix.Utilities;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.PixelTutor;

public class PixelPlayableCardArray : ManagedBehaviour
{
    public PixelPlayableCard selectedCard = null;

    public readonly List<PixelPlayableCard> displayedCards = new();

    private PixelPlayableCard gameObjectReference = null;
    private GameObject overlay;

    private readonly int maxRows = 6;
    private readonly int maxCardsPerRow = 7;

    private readonly float leftAnchor = -1.75f;
    private readonly float centreAnchor = -0.52f;

    private readonly float tweenDurationModifier = 0.5f;
    private Vector3 offscreenPositionOffset = new(2f, 2f, -4f);

    private GamepadGridControl gamepadGrid;

    public IEnumerator DisplayUntilCancelled(List<CardInfo> cards,
        Func<bool> cancelCondition, Action cardsPlacedCallback = null)
    {
        InitializeGamepadGrid();
        yield return SpawnAndPlaceCards(cards, GetNumRows(cards.Count), isDeckReview: true);
        cardsPlacedCallback?.Invoke();
        SetCardsEnabled(enabled: true);
        yield return new WaitUntil(() => cancelCondition());
        SetCardsEnabled(enabled: false);
        yield return CleanUpCards();
    }

    public IEnumerator SelectPixelCardFrom(
        List<CardInfo> cards, Action<PixelPlayableCard> cardSelectedCallback,
        Func<bool> cancelCondition = null, bool forPositiveEffect = true)
    {
        if (cards.Count == 0)
            yield break;

        if (gameObjectReference == null)
        {
            PixelPlayableCard[] objs = Resources.FindObjectsOfTypeAll<PixelPlayableCard>();
            objs.Sort((a, b) => a.Anim.cardRenderer.sortingGroupOrder - b.Anim.cardRenderer.sortingGroupOrder);
            gameObjectReference = Instantiate(objs[0], base.transform);
            gameObjectReference.transform.localPosition = new(-100f, -100f, 0); // go away
            gameObjectReference.transform.rotation.Set(0, 0, 0, 0);
        }

        if (overlay == null)
        {
            GameObject fade = Resources.FindObjectsOfTypeAll<PauseMenu>()[0].menuParent.FindChild("GameFade");
            overlay = Instantiate(fade, base.transform);
            overlay.name = "TutorUIFade";
            var renderer = overlay.GetComponent<SpriteRenderer>();
            renderer.sortingLayerID = gameObjectReference.Anim.cardRenderer.sortingLayerID;
            renderer.sortingLayerName = gameObjectReference.Anim.cardRenderer.sortingLayerName;
            renderer.sortingGroupID = gameObjectReference.Anim.cardRenderer.sortingGroupID;
            renderer.sortingOrder = -7000;
        }

        InitializeGamepadGrid();
        yield return SpawnAndPlaceCards(cards, GetNumRows(cards.Count), isDeckReview: false, forPositiveEffect);
        yield return new WaitForSeconds(0.15f);
        SetCardsEnabled(enabled: true);
        selectedCard = null;
        yield return new WaitUntil(() => selectedCard != null || (cancelCondition != null && cancelCondition()));
        SetCardsEnabled(enabled: false);
        if (selectedCard != null)
            displayedCards.Remove(selectedCard);

        yield return CleanUpCards();
        if (selectedCard != null)
        {
            cards.Remove(selectedCard.Info);
            cardSelectedCallback?.Invoke(selectedCard);
        }
    }

    protected Vector3 GetCardPosition(int cardIndex, int cardsToPlace, int numRows, int cardsPerRow)
    {
        int cardRowIndex = Mathf.FloorToInt(cardIndex / cardsPerRow);
        float xPos = GetXPos(cardIndex, cardRowIndex, cardsToPlace, cardsPerRow); // [-1.75, 0.7]

        float yPos = Mathf.Max(-0.801f, 0.451f - ((8.5f - numRows) * 0.1f * cardRowIndex)); // [0.451, -0.801]
        float zPos = 0f + 0.01f * cardRowIndex;
        return new Vector3(xPos, yPos, zPos);
    }
    private float GetXPos(int cardIndex, int cardRowIndex, int cardsToPlace, int cardsPerRow)
    {
        float cardGap = 0.41f;
        float anchor = leftAnchor;
        int positionInRow = cardsPerRow * cardRowIndex;
        int cardsLeftToPlace = cardsToPlace - cardsPerRow * cardRowIndex;
        float multiplier = cardIndex - positionInRow;

        // when there are <7 cards, we need to centre them
        if (PatchPlugin.act2TutorCenterRows.Value && cardsLeftToPlace > 0 && cardsLeftToPlace < cardsPerRow)
        {
            anchor = centreAnchor;
            multiplier = cardIndex - positionInRow - (cardsLeftToPlace - 1) / 2f;
        }

        return anchor + cardGap * multiplier;
    }
    protected IEnumerator CleanUpCards()
    {
        List<PixelPlayableCard> list = new(displayedCards);
        foreach (PixelPlayableCard item in list)
        {
            if (item != null)
            {
                Tween.Position(item.transform, item.transform.position + offscreenPositionOffset, 0.1f, 0f, Tween.EaseIn);
                item.Anim.PlayQuickRiffleSound();
                Destroy(item.gameObject, 0.1f);
                yield return new WaitForSeconds(CardPile.GetPauseBetweenCardTime(displayedCards.Count) * tweenDurationModifier * 0.5f);
            }
        }
        yield return new WaitForSeconds(0.1f);
        SetOverlayEnabled(false);
        SetBoardEnabled(true);
    }

    protected IEnumerator SpawnAndPlaceCards(List<CardInfo> cards, int numRows, bool isDeckReview = false, bool forPositiveEffect = true)
    {
        SetOverlayEnabled(true);
        SetBoardEnabled(false);
        displayedCards.ForEach(delegate (PixelPlayableCard x)
        {
            if (x != null)
                Destroy(x.gameObject);
        });
        displayedCards.Clear();
        int cardsPerRow = Mathf.Max(maxCardsPerRow, cards.Count / numRows);
        int cardsToPlace = Mathf.Min(maxRows * maxCardsPerRow, cards.Count); // can only show 42 cards max in the selection sequence
        for (int i = 0; i < cardsToPlace; i++)
        {
            Vector3 cardPosition = GetCardPosition(i, cardsToPlace, numRows, cardsPerRow);
            PixelPlayableCard card = CreateAndPlaceCard(cards[i], cardPosition);
            if (card == null)
                yield break;

            yield return TriggerSpecialBehaviours(card, isDeckReview, forPositiveEffect);
            yield return new WaitForSeconds(CardPile.GetPauseBetweenCardTime(cards.Count) * tweenDurationModifier);
        }
    }

    protected void TweenInCard(Transform cardTransform, Vector3 cardPos)
    {
        cardTransform.localPosition = cardPos;
        Vector3 position = cardTransform.position;
        Vector3 vector2 = cardTransform.position = position + offscreenPositionOffset;
        Tween.Position(cardTransform, position, 0.15f, 0f, Tween.EaseOut);
    }

    private void InitializeGamepadGrid()
    {
        if (gamepadGrid == null)
        {
            gamepadGrid = base.gameObject.AddComponent<GamepadGridControl>();
            gamepadGrid.Rows = new List<GamepadGridControl.Row>();
        }
        gamepadGrid.Rows.Clear();
        for (int i = 0; i < maxRows; i++)
        {
            gamepadGrid.Rows.Add(new GamepadGridControl.Row());
        }
        gamepadGrid.enabled = false;
        gamepadGrid.enabled = true;
    }

    private PixelPlayableCard CreateAndPlaceCard(CardInfo info, Vector3 cardPos)
    {
        PixelPlayableCard component = Instantiate(gameObjectReference, base.transform);

        PixelCardAnimationController controller = component.Anim as PixelCardAnimationController;
        controller.cardRenderer.sortingGroupID = gameObjectReference.Anim.cardRenderer.sortingGroupID;
        controller.cardbackRenderer.sortingGroupID = gameObjectReference.Anim.cardRenderer.sortingGroupID;
        controller.cardRenderer.sortingOrder = -9000;
        controller.cardbackRenderer.sortingOrder = -8000;

        component.SetFaceDown(false, true);
        component.SetInfo(info);
        component.SetEnabled(enabled: false);
        component.transform.position = Vector3.zeroVector;

        gamepadGrid.Rows[0].interactables.Add(component);
        displayedCards.Add(component);
        TweenInCard(component.transform, cardPos);
        component.Anim.PlayQuickRiffleSound();
        return component;
    }

    private IEnumerator TriggerSpecialBehaviours(PixelPlayableCard card, bool isDeckReview, bool forPositiveEffect)
    {
        SpecialCardBehaviour[] components = card.GetComponents<SpecialCardBehaviour>();
        foreach (SpecialCardBehaviour specialCardBehaviour in components)
        {
            if (isDeckReview)
                specialCardBehaviour.OnShownInDeckReview();
            else
                yield return specialCardBehaviour.OnShownForCardSelect(forPositiveEffect);

        }
    }

    private int GetNumRows(int numCards)
    {
        int numRows = 1;
        int counter = numCards;
        while (counter > 7)
        {
            counter -= maxCardsPerRow;
            if (counter > 0)
                numRows++;
        }
        return numRows;
    }

    private void SetOverlayEnabled(bool enabled)
    {
        bool swap = false;
        while (overlay.activeSelf != enabled)
        {
            overlay.SetActive(swap);
            swap = !swap;
        }
    }

    private void SetCardsEnabled(bool enabled)
    {
        foreach (PixelPlayableCard displayedCard in displayedCards)
        {
            displayedCard.SetEnabled(enabled);
        }
    }

    private void SetBoardEnabled(bool enabled)
    {
        IEnumerable<PixelPlayableCard> cards = PixelBoardManager.Instance.CardsOnBoard
            .Concat(TurnManager.Instance.Opponent.queuedCards)
            .Concat(PixelPlayerHand.Instance.CardsInHand)
            .Cast<PixelPlayableCard>();

        IEnumerable<HighlightedInteractable> slots = PixelBoardManager.Instance.AllSlots
            .Concat(PixelBoardManager.Instance.OpponentQueueSlots)
            .Cast<HighlightedInteractable>();

        HammerButton button = (HammerButton)Resources.FindObjectsOfTypeAll(typeof(HammerButton)).FirstOrDefault();
        PixelCombatBell bell = (PixelCombatBell)Resources.FindObjectsOfTypeAll(typeof(PixelCombatBell)).FirstOrDefault();

        bell?.SetEnabled(enabled);
        button?.gameObject.SetActive(enabled);

        foreach (HighlightedInteractable slot in slots)
        {
            slot.SetEnabled(enabled);
            slot.ShowState(enabled ? HighlightedInteractable.State.Interactable : HighlightedInteractable.State.Hidden);
        }
        foreach (PixelPlayableCard card in cards)
        {
            card.gameObject.SetActive(enabled);
        }
    }
}