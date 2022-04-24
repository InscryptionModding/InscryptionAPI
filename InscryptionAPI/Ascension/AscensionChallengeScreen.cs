using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
internal static class AscensionChallengeScreenPatches
{

    [HarmonyPatch(typeof(AscensionIconInteractable), "AssignInfo")]
    [HarmonyPostfix]
    private static void ReassignableIconFixes(ref AscensionIconInteractable __instance, AscensionChallengeInfo info)
    {
        //if (info.activatedSprite == null)
        //    __instance.activatedRenderer.sprite = DEFAULT_ACTIVATED_SPRITE;
        if (info.pointValue > 0 || info.challengeType == AscensionChallenge.None)
        {
            var conquered = GameColors.Instance.darkFuschia;
            if (__instance.Conquered && __instance.showConquered)
            {
                __instance.blinkEffect.blinkOffColor = conquered;
                __instance.iconRenderer.color = conquered;
            }
            else
            {
                __instance.blinkEffect.blinkOffColor = GameColors.Instance.red;
                __instance.iconRenderer.color = GameColors.Instance.red;
            }
            __instance.conqueredColor = conquered;
        }
        else if (info.pointValue < 0)
        {
            Color32 conquered = new(0, 52, 33, 255);
            if (__instance.Conquered && __instance.showConquered)
            {
                __instance.blinkEffect.blinkOffColor = conquered;
                __instance.iconRenderer.color = conquered;
            }
            else
            {
                __instance.blinkEffect.blinkOffColor = GameColors.Instance.darkLimeGreen;
                __instance.iconRenderer.color = GameColors.Instance.darkLimeGreen;
            }
            __instance.conqueredColor = conquered;
        }
        else
        {
            var darkGold = GameColors.Instance.darkGold / 2;
            var conquered = new Color(darkGold.r, darkGold.g, darkGold.b, 1f);
            if (__instance.Conquered && __instance.showConquered)
            {
                __instance.blinkEffect.blinkOffColor = conquered;
                __instance.iconRenderer.color = conquered;
            }
            else
            {
                __instance.blinkEffect.blinkOffColor = GameColors.Instance.gold;
                __instance.iconRenderer.color = GameColors.Instance.gold;
            }
            __instance.conqueredColor = conquered;
        }
    }

    [HarmonyPatch(typeof(AscensionChallengeDisplayer), "DisplayChallenge")]
    [HarmonyPrefix]
    public static bool DisplayChallenge(AscensionChallengeDisplayer __instance, AscensionChallengeInfo challengeInfo, bool immediate)
    {
        if (challengeInfo != null && (challengeInfo.challengeType == AscensionChallenge.None || (AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(challengeInfo.challengeType, AscensionSaveData.Data.challengeLevel) &&
            challengeInfo.pointValue < 0)))
        {
            string title = "";
            string line = "";
            string line2 = "";
            if (challengeInfo.challengeType == AscensionChallenge.None)
            {
                title = " ??? ";
                line = Localization.Translate("CHALLENGE UNAVAILABLE");
                line2 = "";
            }
            else if (AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(challengeInfo.challengeType, AscensionSaveData.Data.challengeLevel) && challengeInfo.pointValue < 0)
            {
                title = " " + Localization.ToUpper(Localization.Translate(challengeInfo.title)) + " ";
                line = Localization.Translate(challengeInfo.description);
                line2 = string.Format(Localization.Translate("+{0} CHALLENGE POINTS").Substring(1), challengeInfo.pointValue.ToString()); //because not including the + would mess up the translation
            }
            ChallengeDisplayerPlus.TryAddChallengeDisplayerPlusToDisplayer(__instance).DisplayChallenge(challengeInfo, immediate);
            __instance.DisplayText(title, line, line2, immediate);
            return false;
        }
        ChallengeDisplayerPlus.TryAddChallengeDisplayerPlusToDisplayer(__instance).DisplayChallenge(challengeInfo, immediate);
        return true;
    }

    [HarmonyPatch(typeof(AscensionChallengeScreen), "UpdateFooterText")]
    [HarmonyPrefix]
    public static bool UpdateFooterText(AscensionChallengeScreen __instance, AscensionChallengeInfo challengeInfo, bool activated)
    {
        if (challengeInfo.pointValue < 0)
        {
            string arg = Localization.ToUpper(Localization.Translate(challengeInfo.title));
            string text;
            if (activated)
            {
                text = string.Format(Localization.Translate("{0} ENABLED"), arg);
            }
            else
            {
                text = string.Format(Localization.Translate("{0} DISABLED"), arg);
            }
            string text2;
            if (activated)
            {
                text2 = string.Format(Localization.Translate("{0} Challenge Points Subtracted"), (-challengeInfo.pointValue).ToString());
            }
            else
            {
                text2 = string.Format(Localization.Translate("{0} Challenge Points Added"), (-challengeInfo.pointValue).ToString());
            }
            int challengeLevel = AscensionSaveData.Data.challengeLevel;
            int activeChallengePoints = AscensionSaveData.Data.GetActiveChallengePoints();
            string text3;
            if (activeChallengePoints > AscensionSaveData.GetChallengePointsForLevel(challengeLevel))
            {
                text3 = string.Format(Localization.Translate("WARNING(!) Lvl Reqs EXCEEDED"), Array.Empty<object>());
            }
            else if (activeChallengePoints == AscensionSaveData.GetChallengePointsForLevel(challengeLevel))
            {
                text3 = string.Format(Localization.Translate("Lvl Reqs Met"), Array.Empty<object>());
            }
            else
            {
                text3 = string.Format(Localization.Translate("Lvl Reqs NOT MET"), Array.Empty<object>());
            }
            __instance.footerLines.ShowText(0.1f, new string[]
            {
                text,
                text2,
                text3
            }, false);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "Start")]
    [HarmonyPostfix]
    public static void Postfix(AscensionMenuScreens __instance)
    {
        if (__instance.challengeUnlockSummaryScreen != null && __instance.challengeUnlockSummaryScreen.GetComponent<AscensionMenuScreenTransition>() != null &&
            __instance.challengeUnlockSummaryScreen.GetComponent<AscensionChallengePaginatorSetupifier>() == null)
        {
            GameObject challengeScreen = __instance.challengeUnlockSummaryScreen;
            challengeScreen.AddComponent<AscensionChallengePaginatorSetupifier>();
        }
    }

    [HarmonyPatch(typeof(AscensionChallengeScreen), "Start")]
    [HarmonyPostfix]
    public static void Postfix(AscensionChallengeScreen __instance)
    {
        if (__instance.GetComponent<AscensionChallengePaginator>() == null)
        {
            ChallengeManager.SyncChallengeList();
            AscensionChallengePaginator manager = __instance.gameObject.AddComponent<AscensionChallengePaginator>();
            manager.Initialize(__instance);
        }
    }
}