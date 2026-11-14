using DiskCardGame;
using GBC;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Ascension;

/// <summary>
/// Abstract class for creating custom menu screens to be displayed in Kaycee's Mod.
/// </summary>
public abstract class AscensionRunSetupScreenBase : ManagedBehaviour
{
    /// <summary>
    /// Header name of this screen.
    /// </summary>
    /// <remarks>
    /// This will display near the top of the screen.
    /// The very top will be the challenge level text; below that will be this.
    /// </remarks>
    public abstract string headerText { get; }

    /// <summary>
    /// Whether the card displayer should be shown and usable on this screen.
    /// </summary>
    /// <remarks>
    /// If this is true, it will show the card displayer at the footer below the challenge text
    /// Seting this to true will also enable the <see cref="DisplayCardInfo"/> and <see cref="ClearCardInfo"/> methods
    /// If this is not true, those methods will do nothing
    /// </remarks>
    public abstract bool showCardDisplayer { get; }

    /// <summary>
    /// Whether the card panel should be shown on this screen.
    /// </summary>
    /// <remarks>
    /// If this is true, it will put a card panel in the middle of the screen
    /// The card panel will be able to display up to 7 cards and will have a left/right button
    /// </remarks>
    public abstract bool showCardPanel { get; }

    // This is the hook for the implementer to build their own UI elements.
    /// <summary>
    /// Hook for building this screen's UI elements.
    /// </summary>
    /// <param name="partialScreen"></param>
    public virtual void InitializeScreen(GameObject partialScreen)
    {

    }

    /// <summary>
    /// Objects for handling the text lines that appear in the card displayer when a card is selected/hovered over.
    /// </summary>
    public SequentialPixelTextLines cardInfoLines;

    /// <summary>
    /// Object for handling the footer text lines.
    /// </summary>
    public SequentialPixelTextLines challengeFooterLines;

    /// <summary>
    /// Object for storing the header text.
    /// </summary>
    public ChallengeLevelText challengeHeaderDisplay;

    /// <summary>
    /// All cards currently displayed in the card displayer.
    /// </summary>
    public List<PixelSelectableCard> cards;

    /// <summary>
    /// Object for the card panel.
    /// </summary>
    public GameObject cardPanel;

    /// <summary>
    /// UI interactable for going to the previous page (if applicable).
    /// </summary>
    public MainInputInteractable leftButton;

    /// <summary>
    /// UI interactable for going to the next page (if applicable).
    /// </summary>
    public MainInputInteractable rightButton;

    /// <summary>
    /// UI interactble for continuing to the next screen..
    /// </summary>
    public AscensionMenuInteractable continueButton;

    /// <summary>
    /// UI interactable for backing out of the current screen.
    /// </summary>
    public AscensionMenuBackButton backButton;

    /// <summary>
    /// Text object for displaying the screen's title.
    /// </summary>
    public PixelText screenTitle;

    /// <summary>
    /// Text object for displaying information without using the card displayer.
    /// </summary>
    public PixelText secondaryInfoDisplayer;
    /// <summary>
    /// Object that handles transitions to and from this screen.
    /// </summary>

    public AscensionMenuScreenTransition transitionController;

    private static void CleanupGameObject(GameObject obj, AscensionMenuScreenTransition transition, bool destroy = true)
    {
        MainInputInteractable intact = obj.GetComponent<MainInputInteractable>();
        if (intact != null)
            transition.screenInteractables.Remove(intact);
        transition.onEnableRevealedObjects.Remove(obj);

        foreach (Transform child in obj.transform)
            CleanupGameObject(child.gameObject, transition, destroy: false);

        if (destroy)
            GameObject.Destroy(obj);
    }

    /// <summary>
    /// Constructs this screen's paginator buttons. Used to transition to the previous and next screens.
    /// </summary>
    /// <param name="parent">The object the paginators will be parented to.</param>
    /// <param name="upperPosition">If the paginator should be anchored to the top of the screen.</param>
    /// <returns></returns>
    public static (AscensionMenuInteractable, AscensionMenuInteractable) BuildPaginators(Transform parent, bool upperPosition = false)
    {
        GameObject leftPseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen.transform.Find("Unlocks/ScreenAnchor/PageLeftButton").gameObject;
        GameObject rightPseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen.transform.Find("Unlocks/ScreenAnchor/PageRightButton").gameObject;

        GameObject leftIcon = GameObject.Instantiate(leftPseudoPrefab, parent);
        GameObject rightIcon = GameObject.Instantiate(rightPseudoPrefab, parent);

        leftIcon.transform.localPosition = Vector3.zero;
        rightIcon.transform.localPosition = Vector3.zero;

        ViewportRelativePosition leftPos = leftIcon.AddComponent<ViewportRelativePosition>();
        ViewportRelativePosition rightPos = rightIcon.AddComponent<ViewportRelativePosition>();

        leftPos.viewportCam = Camera.main;
        rightPos.viewportCam = Camera.main;

        if (!upperPosition)
        {

            // Find the leftmost and rightmost x values
            // We'll put the arrows halfway between the edge of the screen and the leftmost/rightmost objects
            float leftX = 1f;
            float rightX = 0f;

            foreach (Renderer renderer in parent.gameObject.GetComponentsInChildren<Renderer>())
            {
                Vector3 min = Camera.main.WorldToViewportPoint(renderer.bounds.min);
                Vector3 max = Camera.main.WorldToViewportPoint(renderer.bounds.max);

                if (min.x < leftX)
                    leftX = min.x;

                if (max.x > rightX)
                    rightX = max.x;
            }

            leftX = leftX / 2f;
            rightX = 1f - ((1f - rightX) / 2f);

            leftPos.viewportAnchor = new Vector2(leftX, 0.565f);
            rightPos.viewportAnchor = new Vector2(rightX, 0.565f);
        }
        else
        {
            leftPos.viewportAnchor = new Vector2(0.25f, 0.8f);
            rightPos.viewportAnchor = new Vector2(0.75f, 0.8f);
        }

        leftPos.offset = new(0f, 0f);
        rightPos.offset = new(0f, 0f);

        return (leftIcon.GetComponent<AscensionMenuInteractable>(), rightIcon.GetComponent<AscensionMenuInteractable>());
    }

    /// <summary>
    /// Constructs this screen and adds it to the game.
    /// </summary>
    /// <param name="screenType">The <see cref="AscensionRunSetupScreenBase"/> to build.</param>
    /// <param name="previousScreen">The ID of the screen that comes before this screen.</param>
    /// <param name="nextScreen">The ID of the screen that comes after this screen.</param>
    /// <returns></returns>
    public static AscensionRunSetupScreenBase BuildScreen(Type screenType, AscensionMenuScreens.Screen previousScreen, AscensionMenuScreens.Screen nextScreen)
    {
        // Create the new screen
        GameObject pseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen;
        GameObject screenObject = GameObject.Instantiate(pseudoPrefab, pseudoPrefab.transform.parent);
        screenObject.name = screenType.Name;

        AscensionCardsSummaryScreen oldController = screenObject.GetComponent<AscensionCardsSummaryScreen>();

        AscensionRunSetupScreenBase controller = screenObject.AddComponent(screenType) as AscensionRunSetupScreenBase;

        // Update the title of the screen
        GameObject textHeader = screenObject.transform.Find("Header/Mid").gameObject;
        textHeader.transform.localPosition = new Vector3(0f, -0.575f, 0f);
        controller.screenTitle = textHeader.GetComponent<PixelText>();
        controller.screenTitle.SetText(Localization.ToUpper(Localization.Translate(controller.headerText)));

        controller.transitionController = screenObject.GetComponent<AscensionMenuScreenTransition>();

        // Check to see if we need the card information displayer
        GameObject footer = screenObject.transform.Find("Footer").gameObject;
        footer.SetActive(true);

        GameObject cardTextDisplayer = screenObject.transform.Find("Footer/CardTextDisplayer").gameObject;
        GameObject footerLowline = screenObject.transform.Find("Footer/PixelTextLine_DIV").gameObject;
        if (controller.showCardDisplayer)
        {
            // Move the stuff
            cardTextDisplayer.transform.localPosition = new Vector3(2.38f, 0.27f, 0f);
            footerLowline.transform.localPosition = new Vector3(0f, 0.33f, 0f);
            footerLowline.transform.SetParent(cardTextDisplayer.transform, true);

            // Copy over a reference to the card info lines
            Component.Destroy(footer.GetComponent<SequentialPixelTextLines>());
            controller.cardInfoLines = cardTextDisplayer.AddComponent<SequentialPixelTextLines>();
            controller.cardInfoLines.lines = new List<PixelText>() {
                footerLowline.GetComponent<PixelText>(),
                cardTextDisplayer.transform.Find("PixelTextLine_DESC1").gameObject.GetComponent<PixelText>(),
                cardTextDisplayer.transform.Find("PixelTextLine_NAME").gameObject.GetComponent<PixelText>()
            };
        }
        else
        {
            // Destroy the card text displayer and footer low line
            CleanupGameObject(cardTextDisplayer, controller.transitionController);
            CleanupGameObject(footerLowline, controller.transitionController);

            GameObject newInfoDisplayer = GameObject.Instantiate(textHeader);
            controller.secondaryInfoDisplayer = newInfoDisplayer.GetComponent<PixelText>();
            controller.secondaryInfoDisplayer.SetColor(GameColors.Instance.nearWhite);
            newInfoDisplayer.transform.localPosition = new Vector3(0f, -2.25f, 0f);
        }

        if (controller.showCardPanel)
        {
            controller.cards = oldController.cards;
        }

        // Destroy the old game logic
        Component.Destroy(oldController);

        // Sort out the unlocks block
        controller.cardPanel = screenObject.transform.Find("Unlocks").gameObject;

        // Clone the challenge information from a challenge screen
        GameObject header = screenObject.transform.Find("Header").gameObject;
        SequentialPixelTextLines headerLines = header.AddComponent<SequentialPixelTextLines>();

        GameObject challengeScreen = AscensionMenuScreens.Instance.selectChallengesScreen;
        GameObject challengePseudoPrefab = challengeScreen.transform.Find("Header/ChallengeLevel").gameObject;
        GameObject pointsPseudoPrefab = challengeScreen.transform.Find("Header/ChallengePoints").gameObject;

        GameObject challengeLevel = GameObject.Instantiate(challengePseudoPrefab, header.transform);
        GameObject challengePoints = GameObject.Instantiate(pointsPseudoPrefab, header.transform);

        // Set the old header lines to the lines of the sequential pixel text lines
        // Fundamentally, the old header now controls these new challenge level lines
        headerLines.lines = new List<PixelText>() {
            challengeLevel.GetComponent<PixelText>(),
            challengePoints.GetComponent<PixelText>()
        };

        // And add those lines to the new challenge level controller
        ChallengeLevelText challengeLevelController = screenObject.AddComponent<ChallengeLevelText>();
        challengeLevelController.headerPointsLines = headerLines;
        controller.challengeHeaderDisplay = challengeLevelController;

        List<Transform> footerLinePseudos = new List<Transform>();
        GameObject challengeFooter = challengeScreen.transform.Find("Footer").gameObject;
        foreach (Transform child in challengeFooter.transform)
            if (child.name.ToLowerInvariant() == "pixeltextline")
                footerLinePseudos.Add(child);
        footerLinePseudos.Sort((a, b) => a.localPosition.y < b.localPosition.y ? -1 : 1);

        List<GameObject> newFooterLines = footerLinePseudos.Select(t => GameObject.Instantiate(t.gameObject, footer.transform)).ToList();
        SequentialPixelTextLines footerLines = footer.AddComponent<SequentialPixelTextLines>();
        footerLines.lines = newFooterLines.Select(o => o.GetComponent<PixelText>()).ToList();
        footerLines.linePrefix = challengeFooter.GetComponent<SequentialPixelTextLines>().linePrefix;
        controller.challengeFooterLines = footerLines;

        // Let's reroute the left and right buttons
        controller.leftButton = screenObject.transform.Find("Unlocks/ScreenAnchor/PageLeftButton").gameObject.GetComponent<MainInputInteractable>();
        controller.rightButton = screenObject.transform.Find("Unlocks/ScreenAnchor/PageRightButton").gameObject.GetComponent<MainInputInteractable>();
        if (controller.showCardPanel)
        {
            controller.cardPanel.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            controller.leftButton.CursorSelectStarted = controller.LeftButtonClicked;
            controller.rightButton.CursorSelectStarted = controller.RightButtonClicked;
            controller.leftButton.gameObject.transform.localPosition = controller.leftButton.gameObject.transform.localPosition - (Vector3)BETWEEN_CARD_OFFSET * 1.5f;
            controller.rightButton.gameObject.transform.localPosition = controller.rightButton.gameObject.transform.localPosition + (Vector3)BETWEEN_CARD_OFFSET * 1.5f;

            // Let's add three more cards to the panel
            GameObject cardPrefab = Resources.Load<GameObject>("prefabs/gbccardbattle/pixelselectablecard");
            Transform cardsParent = screenObject.transform.Find("Unlocks/ScreenAnchor/Cards");
            for (int i = 0; i < 3; i++)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, cardsParent);
                PixelSelectableCard newPixelComponent = newCard.GetComponent<PixelSelectableCard>();
                controller.cards.Add(newPixelComponent);

                // I have to manually connect the pixel border to the component for some reason
                GameObject pixelBorder = newCard.transform.Find("Base/PixelSnap/CardElements/PixelSelectionBorder").gameObject;
                pixelBorder.GetComponent<SpriteRenderer>().color = new Color(.619f, .149f, .188f); // Got this color off of the unityexplorer
                Traverse.Create(newPixelComponent).Field("pixelBorder").SetValue(pixelBorder);
            }

            foreach (PixelSelectableCard card in controller.cards)
            {
                card.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(card.CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
                {
                    controller.CardClicked(card);
                }));
                card.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(card.CursorEntered, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
                {
                    controller.CardCursorEntered(card);
                }));

                Transform lockTexture = card.gameObject.transform.Find("Locked");
                if (lockTexture != null)
                    CleanupGameObject(lockTexture.gameObject, controller.transitionController);
            }
        }
        else
        {
            CleanupGameObject(controller.cardPanel, controller.transitionController);
            CleanupGameObject(controller.leftButton.gameObject, controller.transitionController);
            CleanupGameObject(controller.rightButton.gameObject, controller.transitionController);

            controller.leftButton = null;
            controller.rightButton = null;
        }


        // Reroute the back button
        GameObject backButton = screenObject.transform.Find("BackButton").gameObject;
        controller.backButton = backButton.GetComponent<AscensionMenuBackButton>();
        controller.backButton.screenToReturnTo = previousScreen;

        // Add a continue button
        GameObject continuePrefab = Resources.Load<GameObject>("prefabs/ui/ascension/ascensionmenucontinuebutton");
        GameObject continueButton = GameObject.Instantiate(continuePrefab, screenObject.transform);
        //continueButton.transform.localPosition = new Vector3(2.15f, 1.13f, 0f);

        controller.continueButton = continueButton.GetComponent<AscensionMenuInteractable>();

        // What we do depends upon the screen we're told is next
        Action<MainInputInteractable> clickAction;
        if (nextScreen == AscensionMenuScreens.Screen.SelectChallengesConfirm)
            clickAction = (MainInputInteractable i) => TransitionToGame();
        else
            clickAction = (MainInputInteractable i) => AscensionMenuScreens.Instance.SwitchToScreen(nextScreen);

        controller.continueButton.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(controller.continueButton.CursorSelectStarted, clickAction);

        // Let the base class do its magic
        controller.InitializeScreen(screenObject);

        // And we're done
        return controller;
    }

    private static void TransitionToGame()
    {
        if (!AscensionSaveData.Data.ChallengeLevelIsMet() && AscensionSaveData.Data.challengeLevel <= 12)
        {
            AscensionMenuScreens.Instance.SwitchToScreen(AscensionMenuScreens.Screen.SelectChallengesConfirm);
        }
        else
        {
            AscensionMenuScreens.Instance.TransitionToGame(true);
        }
    }

    /// <summary>
    /// Clears any message currently being displayed.
    /// If <see cref="showCardDisplayer"/> is true, clears the message using <see cref="DisplayCardInfo(CardInfo, string, string, bool)"/>.
    /// Otherwise, sets the <see cref="secondaryInfoDisplayer"/> to inactive.
    /// </summary>
    public virtual void ClearMessage()
    {
        if (this.gameObject.activeSelf)
        {
            if (this.showCardDisplayer)
            {
                this.DisplayCardInfo(null, " ", " ");
            }
            else
            {
                this.secondaryInfoDisplayer.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Displays the given message.
    /// If <see cref="showCardDisplayer"/> is true, displays the message using <see cref="DisplayCardInfo(CardInfo, string, string, bool)"/>.
    /// Otherwise, uses the <see cref="secondaryInfoDisplayer"/> to display the message.
    /// </summary>
    /// <param name="message">String to show.</param>
    public virtual void DisplayMessage(string message)
    {
        if (this.showCardDisplayer)
        {
            this.DisplayCardInfo(null, message, " ");
        }
        else
        {
            this.secondaryInfoDisplayer.SetText(message);
            this.secondaryInfoDisplayer.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hook for when the left button is clicked.
    /// </summary>
    /// <param name="button">Interactable corresponding to the left button.</param>
    public virtual void LeftButtonClicked(MainInputInteractable button)
    {

    }

    /// <summary>
    /// Hook for when the right button is clicked.
    /// </summary>
    /// <param name="button">Interactable corresponding to the right button.</param>
    public virtual void RightButtonClicked(MainInputInteractable button)
    {

    }

    /// <summary>
    /// Hook for when a card is moused over in the card displayer.
    /// </summary>
    /// <param name="card">The card that was moused over.</param>
    /// <remarks>
    /// If <see cref="showCardDisplayer"/> is true, will call <see cref="DisplayCardInfo(CardInfo, string, string, bool)"/> by default.
    /// </remarks>
    public virtual void CardCursorEntered(PixelSelectableCard card)
    {
        if (this.showCardDisplayer)
            this.DisplayCardInfo(card.Info);
    }

    /// <summary>
    /// Hook for when a card is selected in the card panel displayer.
    /// </summary>
    /// <param name="card">The card that was selected.</param>
    public virtual void CardClicked(PixelSelectableCard card)
    {

    }

    private static Vector2 FIRST_CARD_OFFSET = new Vector2(-1.5f, 0f);

    private static Vector2 BETWEEN_CARD_OFFSET = new Vector2(0.5f, 0f);

    /// <summary>
    /// Displays the given cards in the card displayer.
    /// </summary>
    /// <param name="cardsToDisplay">The <see cref="CardInfo"/>s to display.</param>
    /// <remarks>
    /// Only functions if <see cref="showCardDisplayer"/> is true.
    /// </remarks>
    public void ShowCards(List<CardInfo> cardsToDisplay)
    {
        if (!this.showCardDisplayer)
            return;

        foreach (PixelSelectableCard card in this.cards)
            card.gameObject.SetActive(false);

        int numToShow = Math.Min(this.cards.Count, cardsToDisplay.Count);

        float gaps = ((float)(this.cards.Count - numToShow)) / 2f;
        Vector2 startPos = FIRST_CARD_OFFSET + BETWEEN_CARD_OFFSET * gaps;

        for (int i = 0; i < numToShow; i++)
        {
            this.cards[i].SetInfo(cardsToDisplay[i]);
            this.cards[i].gameObject.transform.localPosition = startPos + (float)i * BETWEEN_CARD_OFFSET;
            this.cards[i].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// String comprised entirely of dashes.
    /// Used for displaying the centre delimiters.
    /// </summary>
    public const string CENTER_DASHES = "-------------------------------------------------------------------------------------------------------------------------------";
    /// <summary>
    /// String comprised entirely of dashes that covers the entire screen horizontal.
    /// </summary>
    public const string FULL_DASHES = "---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------";

    /// <summary>
    /// Displays the given <see cref="CardInfo"/> information in the card displayer.
    /// </summary>
    /// <param name="info">The <see cref="CardInfo"/> to display.</param>
    /// <param name="nameOverride">String to display instead of the default displayed name. Use null to use the default.</param>
    /// <param name="descOverride">String to display instead of the default description. Leave null to use the default.</param>
    /// <param name="immediate">If the text should be immediately updated or play a transition.</param>
    /// <remarks>
    /// Only functions if <see cref="showCardDisplayer"/> is true.
    /// </remarks>
    public void DisplayCardInfo(CardInfo info, string nameOverride = null, string descOverride = null, bool immediate = false)
    {
        if (!showCardDisplayer)
            return;

        string lineOne = $"{CENTER_DASHES} <color=#eef4c6>{nameOverride ?? info.DisplayedNameLocalized}</color> {CENTER_DASHES}";
        string lineTwo = descOverride ?? info.GetGBCDescriptionLocalized(info.Abilities);
        string lineThree = FULL_DASHES;

        this.cardInfoLines.ShowText(0.1f, new string[]
        {
            lineThree,
            lineTwo,
            lineOne
        }, immediate);
    }

    /// <summary>
    /// Clears the displayed <see cref="CardInfo"/> information in the card displayer.
    /// </summary>
    /// <param name="immediate">If the text should be immediately updated or play a transition.</param>
    /// <remarks>
    /// Only functions if <see cref="showCardDisplayer"/> is true.
    /// </remarks>
    public void ClearCardInfo(bool immediate = true)
    {
        if (!showCardDisplayer)
            return;

        this.cardInfoLines.ShowText(0.1f, new string[]
        {
            FULL_DASHES,
            string.Empty,
            FULL_DASHES
        }, immediate);
    }

    /// <summary>
    /// Displays the given information in the same format as challenges.
    /// </summary>
    /// <param name="message">The message to display on line 1.</param>
    /// <param name="points">The points amount to be displayed on line 2.</param>
    /// <param name="immediate">If a transition should play before displaying the challenge.</param>
    public void DisplayChallengeInfo(string message, int points, bool immediate = false)
    {
        string lineOne = Localization.Translate(message);

        string lineTwo;
        if (points == 0)
        {
            lineTwo = string.Format(Localization.Translate("{0} Challenge Points"), 0);
        }
        else if (points < 0)
        {
            lineTwo = string.Format(Localization.Translate("{0} Challenge Points Subtracted"), -points);
        }
        else
        {
            lineTwo = string.Format(Localization.Translate("{0} Challenge Points Added"), points);
        }

        int challengeLevel = AscensionSaveData.Data.challengeLevel;
        int activeChallengePoints = AscensionSaveData.Data.GetActiveChallengePoints();
        string lineThree;
        if (activeChallengePoints > challengeLevel * 10)
        {
            lineThree = string.Format(Localization.Translate("WARNING(!) Lvl Reqs EXCEEDED"), Array.Empty<object>());
        }
        else
        {
            if (activeChallengePoints == challengeLevel * 10)
            {
                lineThree = string.Format(Localization.Translate("Lvl Reqs Met"), Array.Empty<object>());
            }
            else
            {
                lineThree = string.Format(Localization.Translate("Lvl Reqs NOT MET"), Array.Empty<object>());
            }
        }

        this.challengeFooterLines.ShowText(0.1f, new string[]
        {
            lineOne,
            lineTwo,
            lineThree
        }, immediate);

        challengeHeaderDisplay.UpdateText();
    }

    /// <summary>
    /// Displays information on the given challenge.
    /// </summary>
    /// <param name="challenge">The challenge to display.</param>
    /// <param name="immediate">If a transition should play before displaying the challenge.</param>
    public void DisplayChallengeInfo(AscensionChallenge challenge, bool immediate = false) {
        AscensionChallengeInfo info = AscensionChallengesUtil.GetInfo(challenge);
        int points = info.pointValue * (AscensionSaveData.Data.ChallengeIsActive(challenge) ? 1 : -1);
        DisplayChallengeInfo(info.title, points, immediate);
    }

    /// <summary>
    /// Corrects the viewport cameras in this screen's children's components.
    /// </summary>
    public override void OnEnable()
    {
        // Set all the viewport camera stuff
        foreach (var vrp in this.gameObject.GetComponentsInChildren<ViewportRelativePosition>())
            vrp.viewportCam = Camera.main;

        base.OnEnable();
    }
}