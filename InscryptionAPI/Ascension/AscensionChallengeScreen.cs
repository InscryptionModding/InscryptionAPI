using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
internal static class AscensionChallengeScreenPatches
{
    private static readonly Sprite DEFAULT_ACTIVATED_SPRITE = TextureHelper.ConvertTexture(
        Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"),
        TextureHelper.SpriteType.ChallengeIcon);

    [HarmonyPatch(typeof(AscensionIconInteractable), "AssignInfo")]
    [HarmonyPostfix]
    private static void ReassignableIconFixes(ref AscensionIconInteractable __instance, AscensionChallengeInfo info)
    {
        if (info.activatedSprite == null)
            __instance.activatedRenderer.sprite = DEFAULT_ACTIVATED_SPRITE;

        if (info.pointValue > 0)
        {
            __instance.blinkEffect.blinkOffColor = GameColors.Instance.red;
            __instance.iconRenderer.color = GameColors.Instance.red;
        }
        else if (info.pointValue < 0)
        {
            __instance.blinkEffect.blinkOffColor = GameColors.Instance.darkLimeGreen;
            __instance.iconRenderer.color = GameColors.Instance.darkLimeGreen;
        } else
        {
            __instance.blinkEffect.blinkOffColor = GameColors.Instance.yellow;
            __instance.iconRenderer.color = GameColors.Instance.yellow;
        }
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "ConfigurePostGameScreens")]
    [HarmonyPostfix]
    private static void AddPaginationToChallengeScreen()
    {
        AscensionChallengePaginator paginator = AscensionMenuScreens.Instance.selectChallengesScreen.GetComponent<AscensionChallengePaginator>();
        if (paginator == null)
            paginator = AscensionMenuScreens.Instance.selectChallengesScreen.AddComponent<AscensionChallengePaginator>();

        GameObject leftPseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen.transform.Find("Unlocks/ScreenAnchor/PageLeftButton").gameObject;
        GameObject rightPseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen.transform.Find("Unlocks/ScreenAnchor/PageRightButton").gameObject;

        GameObject challengeIconGrid = AscensionMenuScreens.Instance.selectChallengesScreen.transform.Find("Icons/ChallengeIconGrid").gameObject;

        GameObject topRow = challengeIconGrid.transform.Find("TopRow").gameObject;
        GameObject bottomRow = challengeIconGrid.transform.Find("BottomRow").gameObject;

        paginator.topRow = new List<AscensionIconInteractable>();
        paginator.bottomRow = new List<AscensionIconInteractable>();
        for (int i = 1; i <= 7; i++)
        {
            paginator.topRow.Add(topRow.transform.Find($"Icon_{i}").gameObject.GetComponent<AscensionIconInteractable>());
            paginator.bottomRow.Add(bottomRow.transform.Find($"Icon_{i+7}").gameObject.GetComponent<AscensionIconInteractable>());
        }
        paginator.extraIcon = bottomRow.transform.Find($"Icon_15").gameObject.GetComponent<AscensionIconInteractable>();
        paginator.showExtraIcon = AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel);


        paginator.GeneratePages();

        var pageTuple = AscensionRunSetupScreenBase.BuildPaginators(challengeIconGrid.transform, upperPosition:true);

        AscensionMenuInteractable leftController = pageTuple.Item1;
        AscensionMenuInteractable rightController = pageTuple.Item2;

        Action<MainInputInteractable> leftClickAction = (MainInputInteractable i) => paginator.ChallengePageLeft(i);
        Action<MainInputInteractable> rightClickAction = (MainInputInteractable i) => paginator.ChallengePageRight(i);

        leftController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(leftController.CursorSelectStarted, leftClickAction);
        rightController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(rightController.CursorSelectStarted, rightClickAction);

        paginator.challengePageIndex = 0;

        if (ChallengeManager.NewInfos.Count == 0)
        {
            GameObject.Destroy(leftController.gameObject);
            GameObject.Destroy(rightController.gameObject);
        }
    }
}