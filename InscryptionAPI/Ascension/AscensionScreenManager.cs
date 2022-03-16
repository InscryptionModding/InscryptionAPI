using System.Collections;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class AscensionScreenManager
{
    internal static readonly List<Type> registeredScreens = new List<Type>();

    internal static Dictionary<AscensionMenuScreens.Screen, AscensionRunSetupScreenBase> screens;

    internal static AscensionMenuScreens.Screen initialScreen = AscensionMenuScreens.Screen.JournalSummary;

    internal const int CUSTOM_SCREEN_START = 100;

    private static string _challengeScreenHoverText = "START RUN";

    public static void RegisterScreen<T>() where T : AscensionRunSetupScreenBase
    {
        registeredScreens.Add(typeof(T));
    }

    private static AscensionScreenSort.Direction GetPreferredDirection(Type t)
    {
        AscensionScreenSort sortAttr = Attribute.GetCustomAttribute(t, typeof(AscensionScreenSort)) as AscensionScreenSort;
        return sortAttr?.preferredDirection ?? AscensionScreenSort.Direction.NoPreference;
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
        _challengeScreenHoverText = screens[currentScreen].HeaderText;

        foreach (var setupScreenBase in screens)
        {
            AscensionRunSetupScreenBase cur = screens[currentScreen];

            string prevText = screens.ContainsKey(previousScreen) ? screens[previousScreen].HeaderText : "SELECT CHALLENGES";
            string nextText = screens.ContainsKey(nextScreen) ? screens[nextScreen].HeaderText : "START RUN";

            Action<MainInputInteractable> prevHoverAction = mii => cur.DisplayMessage(Localization.ToUpper(Localization.Translate(prevText)));
            Action<MainInputInteractable> nextHoverAction = mii => cur.DisplayMessage(Localization.ToUpper(Localization.Translate(nextText)));
            Action<MainInputInteractable> unHoverAction = mii => cur.ClearMessage();

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
        if (screens is { Count: > 0 })
            foreach (var screenBase in screens.Values.Where(iScreenBase => iScreenBase && iScreenBase.gameObject))
                screenBase.gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(AscensionChallengeScreen), "OnContinueCursorEnter")]
    [HarmonyPrefix]
    public static bool HoverTextFirstCustomScreen(ref AscensionChallengeScreen __instance)
    {
        string line = Localization.Translate(_challengeScreenHoverText);
        __instance.challengeDisplayer.DisplayText("", line, "");
        return false;
    }
}
