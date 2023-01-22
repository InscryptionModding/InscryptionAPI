using DiskCardGame;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class AscensionScreenManager
{
    internal static List<Type> registeredScreens = new List<Type>();

    internal static Dictionary<AscensionMenuScreens.Screen, AscensionRunSetupScreenBase> screens;

    internal static AscensionMenuScreens.Screen initialScreen = AscensionMenuScreens.Screen.JournalSummary;

    internal const int CUSTOM_SCREEN_START = 100;

    private static string challengeScreenHoverText = "START RUN";

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
            }
            catch (Exception ex)
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
    private static bool TransitionToSideDeckScreen(ref AscensionChallengeScreen __instance)
    {
        if (screens == null || screens.Count == 0)
            return true; // No custom screens; execute the original method

        AscensionMenuScreens.Instance.SwitchToScreen(initialScreen);
        return false;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "ScreenSwitchSequence")]
    [HarmonyPostfix]
    private static IEnumerator SwitchToScreen(IEnumerator sequenceEvent, AscensionMenuScreens.Screen screen)
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
    private static void InitializeScreensOnStart()
    {
        InitializeAllScreens();
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "DeactivateAllScreens")]
    [HarmonyPostfix]
    private static void DeactivateAllCustomScreens()
    {
        if (screens != null && screens.Count > 0)
            foreach (AscensionRunSetupScreenBase screenbase in screens.Values)
                if (screenbase != null && screenbase.gameObject != null)
                    screenbase.gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(AscensionChallengeScreen), "OnContinueCursorEnter")]
    [HarmonyPrefix]
    private static bool HoverTextFirstCustomScreen(ref AscensionChallengeScreen __instance)
    {
        string line = Localization.Translate(challengeScreenHoverText);
        __instance.challengeDisplayer.DisplayText("", line, "", false);
        return false;
    }
}