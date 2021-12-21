using DiskCardGame;
using HarmonyLib;
using System;
using UnityEngine;
using System.Linq;
using APIPlugin;
using System.Collections.Generic;
using GBC;

namespace InscryptionAPI.AscensionScreens
{
    public abstract class AscensionRunSetupScreenBase : ManagedBehaviour
    {
        // This will display near the top of the screen.
        // The very top will be the challenge level text; below that will be this
        public abstract string headerText { get; }

        // If this is true, it will show the card displayer at the footer below the challenge text
        // Seting this to true will also enable the DisplayCardInfo and ClearCardInfo methods
        // If this is not true, those methods will do nothing
        public abstract bool showCardDisplayer { get; }

        // If this is true, it will put a card panel in the middle of the screen
        // The card panel will be able to display up to 7 cards
        // and will have a left/right button
        public abstract bool showCardPanel { get; }

        // This is the hook for the implementer to build their own UI elements.
        public virtual void InitializeScreen(GameObject partialScreen)
        {

        }

        public SequentialPixelTextLines cardInfoLines;

        public SequentialPixelTextLines challengeFooterLines;

        public ChallengeLevelText challengeHeaderDisplay;

        public List<PixelSelectableCard> cards;

        public GameObject cardPanel;

        public MainInputInteractable leftButton;

        public MainInputInteractable rightButton;

        public AscensionMenuInteractable continueButton;

        public AscensionMenuBackButton backButton;

        public PixelText screenTitle;

        public PixelText secondaryInfoDisplayer;

        public static AscensionRunSetupScreenBase BuildScreen(Type screenType, AscensionMenuScreens.Screen previousScreen, AscensionMenuScreens.Screen nextScreen)
        {
            // Create the new screen
            Plugin.Log.LogInfo($"Creating screen for {screenType.Name}");

            GameObject pseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen;
            GameObject screenObject = GameObject.Instantiate(pseudoPrefab, pseudoPrefab.transform.parent);
            screenObject.name = screenType.Name;

            Plugin.Log.LogInfo($"Getting old logic");
            AscensionCardsSummaryScreen oldController = screenObject.GetComponent<AscensionCardsSummaryScreen>();

            Plugin.Log.LogInfo($"Adding new logic");
            AscensionRunSetupScreenBase controller = screenObject.AddComponent(screenType) as AscensionRunSetupScreenBase;

                        // Update the title of the screen
            Plugin.Log.LogInfo($"Updating screen title");
            GameObject textHeader = screenObject.transform.Find("Header/Mid").gameObject;
            textHeader.transform.localPosition = new Vector3(0f, -0.575f, 0f);
            controller.screenTitle = textHeader.GetComponent<PixelText>();
            controller.screenTitle.SetText(Localization.ToUpper(Localization.Translate(controller.headerText)));

            // Check to see if we need the card information displayer
            GameObject footer = screenObject.transform.Find("Footer").gameObject;
            footer.SetActive(true);

            GameObject cardTextDisplayer = screenObject.transform.Find("Footer/CardTextDisplayer").gameObject;
            GameObject footerLowline = screenObject.transform.Find("Footer/PixelTextLine_DIV").gameObject;
            if (controller.showCardDisplayer)
            {
                Plugin.Log.LogInfo($"Resetting card information displayer");

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
                Plugin.Log.LogInfo($"Destroying unwanted card information displayer");
                // Destroy the card text displayer and footer low line
                GameObject.Destroy(cardTextDisplayer);
                GameObject.Destroy(footerLowline);

                Plugin.Log.LogInfo($"Creating new information displayer");
                GameObject newInfoDisplayer = GameObject.Instantiate(textHeader);
                controller.secondaryInfoDisplayer = newInfoDisplayer.GetComponent<PixelText>();
                controller.secondaryInfoDisplayer.SetColor(GameColors.Instance.nearWhite);
                newInfoDisplayer.transform.localPosition = new Vector3(0f, -2.25f, 0f);
            }

            if (controller.showCardPanel)
            {
                Plugin.Log.LogInfo($"Transferring ownership of cards");
                controller.cards = oldController.cards;
            }    

            // Destroy the old game logic
            Plugin.Log.LogInfo($"Destroying old game logic");
            Component.Destroy(oldController);

            // Sort out the unlocks block
            Plugin.Log.LogInfo($"Handling card panel");
            controller.cardPanel = screenObject.transform.Find("Unlocks").gameObject;
            if (controller.showCardDisplayer)
                controller.cardPanel.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            else
                GameObject.Destroy(controller.cardPanel);

            // Clone the challenge information from a challenge screen
            Plugin.Log.LogInfo($"Creating challenge text header");
            GameObject header = screenObject.transform.Find("Header").gameObject;
            SequentialPixelTextLines headerLines = header.AddComponent<SequentialPixelTextLines>();

            GameObject challengeScreen = AscensionMenuScreens.Instance.selectChallengesScreen;
            GameObject challengePseudoPrefab = challengeScreen.transform.Find("Header/ChallengeLevel").gameObject;
            GameObject pointsPseudoPrefab = challengeScreen.transform.Find("Header/ChallengePoints").gameObject;

            GameObject challengeLevel = GameObject.Instantiate(challengePseudoPrefab, header.transform);
            GameObject challengePoints = GameObject.Instantiate(pointsPseudoPrefab, header.transform);

            // Set the old header lines to the lines of the sequential pixel text lines
            // Fundamentally, the old header now controls these new challenge level lines
            Plugin.Log.LogInfo($"Assigning new lines of text to header");
            headerLines.lines = new List<PixelText>() {
                challengeLevel.GetComponent<PixelText>(),
                challengePoints.GetComponent<PixelText>()
            };

            // And add those lines to the new challenge level controller
            Plugin.Log.LogInfo($"Giving controller access to header");
            ChallengeLevelText challengeLevelController = screenObject.AddComponent<ChallengeLevelText>();
            challengeLevelController.headerPointsLines = headerLines;
            controller.challengeHeaderDisplay = challengeLevelController;

            Plugin.Log.LogInfo($"Creating challenge footer text");

            List<Transform> footerLinePseudos = new List<Transform>();
            GameObject challengeFooter = challengeScreen.transform.Find("Footer").gameObject; 
            foreach (Transform child in challengeFooter.transform)
                if (child.name.ToLowerInvariant() == "pixeltextline")
                    footerLinePseudos.Add(child);
            footerLinePseudos.Sort((a, b) => a.localPosition.y < b.localPosition.y ? -1 : 1);

            Plugin.Log.LogInfo($"Building footer text controller");
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
                Plugin.Log.LogInfo($"Reassigning left/right scroll buttons");
                controller.leftButton.CursorSelectStarted = controller.LeftButtonClicked;
                controller.rightButton.CursorSelectStarted = controller.RightButtonClicked;
                controller.leftButton.gameObject.transform.localPosition = controller.leftButton.gameObject.transform.localPosition - (Vector3)BETWEEN_CARD_OFFSET * 1.5f;
                controller.rightButton.gameObject.transform.localPosition = controller.rightButton.gameObject.transform.localPosition + (Vector3)BETWEEN_CARD_OFFSET * 1.5f;

                // Let's add three more cards to the panel
                Plugin.Log.LogInfo($"Expanding card panel");
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

                Plugin.Log.LogInfo($"Assigning click action to cards in card panel");
                foreach (PixelSelectableCard card in controller.cards)
                {
                    card.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(card.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
                    {
                        controller.CardClicked(card);
                    }));
                    card.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(card.CursorEntered, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
                    {
                        controller.CardCursorEntered(card);
                    }));

                    Transform lockTexture = card.gameObject.transform.Find("Locked");
                    if (lockTexture != null)
                        GameObject.Destroy(lockTexture.gameObject);
                }
            }
            else
            {
                Plugin.Log.LogInfo($"Destroying scroll buttons");
                GameObject.Destroy(controller.leftButton.gameObject);
                GameObject.Destroy(controller.rightButton.gameObject);

                controller.leftButton = null;
                controller.rightButton = null;
            }


            // Reroute the back button
            Plugin.Log.LogInfo($"Rerouting back button");
            GameObject backButton = screenObject.transform.Find("BackButton").gameObject;
            controller.backButton = backButton.GetComponent<AscensionMenuBackButton>();
            controller.backButton.screenToReturnTo = previousScreen;

            // Add a continue button
            Plugin.Log.LogInfo($"Adding continue button");
            GameObject continuePrefab = Resources.Load<GameObject>("prefabs/ui/ascension/ascensionmenucontinuebutton");
            GameObject continueButton = GameObject.Instantiate(continuePrefab, screenObject.transform);
            continueButton.transform.localPosition = new Vector3(2.08f, 1.15f, 0f);

            controller.continueButton = continueButton.GetComponent<AscensionMenuInteractable>();

            // What we do depends upon the screen we're told is next
            Action<MainInputInteractable> clickAction;
            if (nextScreen == AscensionMenuScreens.Screen.SelectChallengesConfirm)
                clickAction = (MainInputInteractable i) => TransitionToGame();
            else
                clickAction = (MainInputInteractable i) => AscensionMenuScreens.Instance.SwitchToScreen(nextScreen);
            
            controller.continueButton.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(controller.continueButton.CursorSelectStarted, clickAction);

            // Let the base class do its magic
            Plugin.Log.LogInfo($"Calling screen implementation to finish creating screen UI elements");
            controller.InitializeScreen(screenObject);

            // And we're done
            Plugin.Log.LogInfo($"Done building screen");
            return controller;
        }

        private static void TransitionToGame()
        {
            if (!AscensionSaveData.Data.ChallengeLevelIsMet() && AscensionSaveData.Data.challengeLevel <= 12)
            {
                Plugin.Log.LogInfo("Sending the player to the confirmation screen");
                AscensionMenuScreens.Instance.SwitchToScreen(AscensionMenuScreens.Screen.SelectChallengesConfirm);
            }
            else
            {
                Plugin.Log.LogInfo("Starting a new run");
                AscensionMenuScreens.Instance.TransitionToGame(true);
            }
        }

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

        public virtual void LeftButtonClicked(MainInputInteractable button)
        {

        }

        public virtual void RightButtonClicked(MainInputInteractable button)
        {
            
        }

        public virtual void CardCursorEntered(PixelSelectableCard card)
        {
            if (this.showCardDisplayer)
                this.DisplayCardInfo(card.Info);
        }

        public virtual void CardClicked(PixelSelectableCard card)
        {

        }

        private static Vector2 FIRST_CARD_OFFSET = new Vector2(-1.5f, 0f);

        private static Vector2 BETWEEN_CARD_OFFSET = new Vector2(0.5f, 0f);

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

        public const string CENTER_DASHES = "-------------------------------------------------------------------------------------------------------------------------------";
        public const string FULL_DASHES = "---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------";

        public void DisplayCardInfo(CardInfo info, string nameOverride = null, string descOverride = null, bool immediate=false)
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

        public void ClearCardInfo(bool immediate=true)
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

        public void DisplayChallengeInfo(string message, int points, bool immediate=false)
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
			} else
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

        public void DisplayChallengeInfo(AscensionChallenge challenge, bool immediate=false)
        {
            AscensionChallengeInfo info = AscensionChallengesUtil.GetInfo(challenge);
            int points = info.pointValue * (AscensionSaveData.Data.ChallengeIsActive(challenge) ? 1 : -1);
            DisplayChallengeInfo(info.title, points, immediate);
        }
    }
}