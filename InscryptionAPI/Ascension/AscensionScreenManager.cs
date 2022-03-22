using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using GBC;
using InscryptionAPI.Helpers;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class AscensionScreenManager
{
    internal static List<Type> registeredScreens = new();

    internal static Dictionary<AscensionMenuScreens.Screen, AscensionRunSetupScreenBase> screens;

    internal static AscensionMenuScreens.Screen initialScreen = AscensionMenuScreens.Screen.JournalSummary;

    internal const int CUSTOM_SCREEN_START = 100;

    private static string challengeScreenHoverText = "START RUN";

    public static Sprite noneCardSprite = TextureHelper.GetTextureFromResource("InscryptionAPI/ascension_card_none_darker.png").ConvertTexture();

    public static void RegisterScreen<T>() where T : AscensionRunSetupScreenBase
    {
        registeredScreens.Add(typeof(T));
    }

    private static AscensionScreenSort.Direction GetPreferredDirection(Type t)
    {
        AscensionScreenSort sortAttr = Attribute.GetCustomAttribute(t, typeof(AscensionScreenSort)) as AscensionScreenSort;
        if (sortAttr == null)
            return AscensionScreenSort.Direction.NoPreference;
        else
            return sortAttr.preferredDirection;
    }

    public static void InitializeAllScreens()
    {
        // Sort the screens
        registeredScreens.Sort((a, b) => (int)GetPreferredDirection(a) - (int)GetPreferredDirection(b));

        // Build screens
        screens = new Dictionary<AscensionMenuScreens.Screen, AscensionRunSetupScreenBase>();

        AscensionMenuScreens.Screen previousScreen = AscensionMenuScreens.Screen.SelectChallenges;
        AscensionMenuScreens.Screen currentScreen = (AscensionMenuScreens.Screen)CUSTOM_SCREEN_START;
        AscensionMenuScreens.Screen nextScreen = (AscensionMenuScreens.Screen)(CUSTOM_SCREEN_START + 1);
        initialScreen = currentScreen;
        for (int i = 0; i < registeredScreens.Count; i++)
        {
            Type screenType = registeredScreens[i];

            if (i == registeredScreens.Count - 1) // the last one
                nextScreen = AscensionMenuScreens.Screen.SelectChallengesConfirm;

            try
            {
                AscensionRunSetupScreenBase screen = AscensionRunSetupScreenBase.BuildScreen(screenType, previousScreen, nextScreen);

                screens.Add(currentScreen, screen);

                previousScreen = currentScreen;
                currentScreen = nextScreen;
                nextScreen = (AscensionMenuScreens.Screen)((int)nextScreen + 1);
            } catch (Exception ex)
            {
                InscryptionAPIPlugin.Logger.LogError(ex);
            }
        }

        if (screens.Count == 0)
            return;

        // Now make another pass through the screens and set the behavior of hovering over the continue and back buttons

        previousScreen = AscensionMenuScreens.Screen.SelectChallenges;
        currentScreen = (AscensionMenuScreens.Screen)CUSTOM_SCREEN_START;
        nextScreen = (AscensionMenuScreens.Screen)(CUSTOM_SCREEN_START + 1);
            
        // Set the hover text of the challenge screen to be the title of the first custom screen
        challengeScreenHoverText = screens[currentScreen].headerText;

        for (int i = 0; i < screens.Count; i++)
        {
            AscensionRunSetupScreenBase cur = screens[currentScreen];

            string prevText = screens.ContainsKey(previousScreen) ? screens[previousScreen].headerText : "SELECT CHALLENGES";
            string nextText = screens.ContainsKey(nextScreen) ? screens[nextScreen].headerText : "START RUN";

            Action<MainInputInteractable> prevHoverAction = (MainInputInteractable i) => cur.DisplayMessage(Localization.ToUpper(Localization.Translate(prevText)));
            Action<MainInputInteractable> nextHoverAction = (MainInputInteractable i) => cur.DisplayMessage(Localization.ToUpper(Localization.Translate(nextText)));
            Action<MainInputInteractable> unHoverAction = (MainInputInteractable i) => cur.ClearMessage();

            cur.backButton.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(cur.backButton.CursorEntered, prevHoverAction);
            cur.backButton.CursorExited = (Action<MainInputInteractable>)Delegate.Combine(cur.backButton.CursorExited, unHoverAction);
            cur.continueButton.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(cur.continueButton.CursorEntered, nextHoverAction);
            cur.continueButton.CursorExited = (Action<MainInputInteractable>)Delegate.Combine(cur.continueButton.CursorExited, unHoverAction);

            previousScreen = currentScreen;
            currentScreen = nextScreen;
            nextScreen = (AscensionMenuScreens.Screen)((int)nextScreen + 1);
        }
    }

    // This patches the confirmation button of the challenge screen to ensure it starts the queue
    [HarmonyPatch(typeof(AscensionChallengeScreen), "OnContinuePressed")]
    [HarmonyPrefix]
    public static bool TransitionToSideDeckScreen(ref AscensionChallengeScreen __instance)
    {
        if (screens == null || screens.Count == 0)
            return true; // No custom screens; execute the original method

        AscensionMenuScreens.Instance.SwitchToScreen(initialScreen);
        return false;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "ScreenSwitchSequence")]
    [HarmonyPostfix]
    public static IEnumerator SwitchToScreen(IEnumerator sequenceEvent, AscensionMenuScreens.Screen screen)
    {
        while (sequenceEvent.MoveNext())
            yield return sequenceEvent.Current;

        yield return new WaitForSeconds(0.05f);

        if (AscensionScreenManager.screens.ContainsKey(screen))
            AscensionScreenManager.screens[screen].gameObject.SetActive(true);

        yield break;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "ConfigurePostGameScreens")]
    [HarmonyPostfix]
    public static void InitializeScreensOnStart()
    {
        InitializeAllScreens();
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "DeactivateAllScreens")]
    [HarmonyPostfix]
    public static void DeactivateAllCustomScreens()
    {
        if (screens != null && screens.Count > 0)
            foreach (AscensionRunSetupScreenBase screenbase in screens.Values)
                if (screenbase != null && screenbase.gameObject != null)
                    screenbase.gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(AscensionChallengeScreen), "OnContinueCursorEnter")]
    [HarmonyPrefix]
    public static bool HoverTextFirstCustomScreen(ref AscensionChallengeScreen __instance)
    {
        string line = Localization.Translate(challengeScreenHoverText);
        __instance.challengeDisplayer.DisplayText("", line, "", false);
        return false;
    }

    [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), "OnCursorEnterDeckIcon")]
    [HarmonyPrefix]
    public static bool AddTempCards(AscensionChooseStarterDeckScreen __instance, AscensionStarterDeckIcon icon)
    {
        if (__instance.GetComponent<AscensionDeckSelectTempCardManager>() != null)
        {
            __instance.GetComponent<AscensionDeckSelectTempCardManager>().tempCards = __instance.GetComponent<AscensionDeckSelectTempCardManager>().tempCards ?? new List<GameObject>();
            __instance.GetComponent<AscensionDeckSelectTempCardManager>().tempCards?.ForEach(delegate (GameObject x) { if (x != null) { UnityEngine.Object.Destroy(x); } });
            __instance.GetComponent<AscensionDeckSelectTempCardManager>().tempCards?.Clear();
        }
        if (__instance.cards != null && icon != null && icon.Info != null && icon.Info.cards != null && icon.Info.cards.Count != __instance.cards.Count && icon.Unlocked)
        {
            __instance.cards?.ForEach(delegate (PixelSelectableCard x)
            {
                x?.gameObject?.SetActive(false);
            });
            List<GameObject> tempCards = new List<GameObject>();
            for (int i = 0; i < icon.Info.cards.Count; i++)
            {
                CardInfo ci = icon.Info.cards[i];
                if (ci != null)
                {
                    float distance = __instance.cards[1].transform.position.x - __instance.cards[0].transform.position.x;
                    GameObject tempCard = UnityEngine.Object.Instantiate(__instance.cards[0].gameObject, __instance.cards[1].transform.position + (Vector3.right * ((icon.Info.cards.Count / 2 - 0.5f) * -distance + i * distance)), Quaternion.identity);
                    PixelSelectableCard sc = tempCard.GetComponent<PixelSelectableCard>();
                    sc.SetInfo(ci);
                    sc.SetEnabled(false);
                    tempCard.SetActive(true);
                    tempCard.transform.parent = __instance.cards[0].transform.parent;
                    tempCards.Add(tempCard);
                }
            }
            __instance.StopAllCoroutines();
            __instance.StartCoroutine(__instance.ShowCardsSequence());
            AscensionDeckSelectTempCardManager manager = __instance.GetComponent<AscensionDeckSelectTempCardManager>() ?? __instance.gameObject.AddComponent<AscensionDeckSelectTempCardManager>();
            manager.tempCards ??= new List<GameObject>();
            manager.tempCards?.AddRange(tempCards);
            return false;
        }
        else
        {
            __instance.cards?.ForEach(delegate (PixelSelectableCard x)
            {
                x?.gameObject?.SetActive(true);
            });
        }
        return true;
    }

    [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), "OnCursorEnterDeckIcon")]
    [HarmonyPostfix]
    public static void AddNoneCardSprites(AscensionChooseStarterDeckScreen __instance, AscensionStarterDeckIcon icon)
    {
        if (icon != null && icon.Info == null && __instance.cardLockedSprites != null)
        {
            __instance.cardLockedSprites.ForEach(delegate (SpriteRenderer x)
            {
                x.enabled = false;
            });
            AscensionDeckSelectTempCardManager manager = __instance.GetComponent<AscensionDeckSelectTempCardManager>() ?? __instance.gameObject.AddComponent<AscensionDeckSelectTempCardManager>();
            if (manager.noneCards == null || manager.noneCards.Count <= 0)
            {
                manager.noneCards ??= new();
                foreach (SpriteRenderer sr in __instance.cardLockedSprites)
                {
                    GameObject noneCard = UnityEngine.Object.Instantiate(sr.gameObject);
                    noneCard.transform.parent = sr.transform.parent;
                    noneCard.transform.position = sr.transform.position;
                    SpriteRenderer rend = noneCard.GetComponent<SpriteRenderer>();
                    rend.sprite = noneCardSprite;
                    rend.enabled = true;
                    manager.noneCards.Add(rend);
                }
            }
            manager.noneCards.ForEach(delegate (SpriteRenderer x)
            {
                x.enabled = true;
            });
        }
        else
        {
            AscensionDeckSelectTempCardManager manager = __instance.GetComponent<AscensionDeckSelectTempCardManager>() ?? __instance.gameObject.AddComponent<AscensionDeckSelectTempCardManager>();
            manager.noneCards.ForEach(delegate (SpriteRenderer x)
            {
                x.enabled = false;
            });
        }
    }

    public class AscensionDeckSelectTempCardManager : ManagedBehaviour
    {
        public IEnumerator SequentiallyShowTempCards(List<GameObject> cards)
        {
            cards.ForEach(delegate (GameObject x)
            {
                x?.SetActive(false);
            });
            foreach (GameObject obj in cards)
            {
                yield return new WaitForSeconds(0.1f);
                obj?.SetActive(true);
                CommandLineTextDisplayer.PlayCommandLineClickSound();
            }
            yield break;
        }

        public List<GameObject> tempCards = new();
        public List<SpriteRenderer> noneCards = new();
    }
}