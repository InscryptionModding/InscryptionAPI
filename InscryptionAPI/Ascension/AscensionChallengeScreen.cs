using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class AscensionChallengeScreenPatches
{
    [HarmonyPatch(typeof(AscensionIconInteractable), "AssignInfo")]
    [HarmonyPostfix]
    public static void ReassignableIconFixes(ref AscensionIconInteractable __instance, AscensionChallengeInfo info)
    {
        if (info.pointValue < 0)
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
    }

    [HarmonyPatch(typeof(AscensionChallengeDisplayer), "DisplayChallenge")]
    [HarmonyPrefix]
    public static bool DisplayChallenge(AscensionChallengeDisplayer __instance, AscensionChallengeInfo challengeInfo, bool immediate = false)
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
            __instance.DisplayText(title, line, line2, immediate);
            return false;
        }
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
            List<T> Repeat<T>(T h, int h2)
            {
                List<T> ret = new();
                for(int i = 0; i < h2; i++)
                {
                    ret.Add(h);
                }
                return ret;
            }
            AscensionChallengePaginator manager = __instance.gameObject.AddComponent<AscensionChallengePaginator>();
            manager.Initialize(__instance);
            List<ChallengeManager.FullChallenge> fullchallengesToAdd = new(ChallengeManager.NewInfos);
            fullchallengesToAdd.Sort((x, x2) => Math.Sign(x.Info.pointValue) == Math.Sign(x2.Info.pointValue) ? x.UnlockLevel - x2.UnlockLevel : Math.Sign(x2.Info.pointValue) - Math.Sign(x.Info.pointValue));
            List<AscensionChallengeInfo> challengesToAdd = fullchallengesToAdd.ConvertAll(x => Repeat(x.Info, x.AppearancesInChallengeScreen)).SelectMany(x => x).ToList();
            List<AscensionIconInteractable> icons = __instance.icons;
            icons.ForEach(delegate (AscensionIconInteractable ic)
            {
                if (ic != null && ic.Info == null && challengesToAdd.Count > 0)
                {
                    ic.challengeInfo = challengesToAdd[0];
                    ic.AssignInfo(challengesToAdd[0]);
                    challengesToAdd.RemoveAt(0);
                }
            });
            List<List<AscensionChallengeInfo>> pagesToAdd = new();
            while (challengesToAdd.Count > 0)
            {
                List<AscensionChallengeInfo> page = new();
                for (int i = 0; i < icons.Count; i++)
                {
                    if (challengesToAdd.Count > 0)
                    {
                        page.Add(challengesToAdd[0]);
                        challengesToAdd.RemoveAt(0);
                    }
                }
                pagesToAdd.Add(page);
            }
            if (pagesToAdd.Count > 0)
            {
                foreach (List<AscensionChallengeInfo> page in pagesToAdd)
                {
                    manager.AddPage(page);
                }
                Vector3 topRight = new(float.MinValue, float.MinValue);
                Vector3 bottomLeft = new(float.MaxValue, float.MaxValue);
                foreach (AscensionIconInteractable icon in icons)
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
                leftArrow.transform.position = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.left / 3f;
                leftArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                leftArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.PreviousPage();
                GameObject rightArrow = UnityEngine.Object.Instantiate(__instance.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject);
                rightArrow.transform.parent = __instance.transform;
                rightArrow.transform.position = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.right / 3f;
                rightArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                rightArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.NextPage();
            }
        }
    }
}