using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class AscensionChallengeScreenPatches
{
    private static readonly Sprite DefaultActivatedSprite = Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default").ConvertTexture(TextureHelper.SpriteType.ChallengeIcon);

    [HarmonyPatch(typeof(AscensionIconInteractable), "AssignInfo")]
    [HarmonyPostfix]
    public static void ReassignableIconFixes(ref AscensionIconInteractable __instance, AscensionChallengeInfo info)
    {
        if (info.activatedSprite == null)
            __instance.activatedRenderer.sprite = DefaultActivatedSprite;

        if (info.pointValue > 0)
        {
            __instance.blinkEffect.blinkOffColor = GameColors.Instance.red;
            __instance.iconRenderer.color = GameColors.Instance.red;
        }
        else
        {
            __instance.blinkEffect.blinkOffColor = GameColors.Instance.darkLimeGreen;
            __instance.iconRenderer.color = GameColors.Instance.darkLimeGreen;
        }
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "ConfigurePostGameScreens")]
    [HarmonyPostfix]
    public static void AddPaginationToChallengeScreen()
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
            paginator.bottomRow.Add(bottomRow.transform.Find($"Icon_{i + 7}").gameObject.GetComponent<AscensionIconInteractable>());
        }
        paginator.extraIcon = bottomRow.transform.Find($"Icon_15").gameObject.GetComponent<AscensionIconInteractable>();
        paginator.showExtraIcon = paginator.extraIcon.gameObject.activeSelf;


        paginator.GeneratePages();

        var pageTuple = AscensionRunSetupScreenBase.BuildPaginators(challengeIconGrid.transform, true);

        AscensionMenuInteractable leftController = pageTuple.Item1;
        AscensionMenuInteractable rightController = pageTuple.Item2;

        Action<MainInputInteractable> leftClickAction = i => paginator.ChallengePageLeft(i);
        Action<MainInputInteractable> rightClickAction = i => paginator.ChallengePageRight(i);

        leftController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(leftController.CursorSelectStarted, leftClickAction);
        rightController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(rightController.CursorSelectStarted, rightClickAction);

        paginator.challengePageIndex = 0;

        if (ChallengeManager.NewInfos.Count == 0)
        {
            UnityObject.Destroy(leftController.gameObject);
            UnityObject.Destroy(rightController.gameObject);
        }
    }
}
