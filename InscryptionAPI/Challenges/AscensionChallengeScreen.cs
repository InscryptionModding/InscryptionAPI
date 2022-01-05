using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Challenges
{
    [HarmonyPatch]
    public static class AscensionChallengeScreen
    {
        private static readonly Sprite DEFAULT_ACTIVATED_SPRITE = Sprite.Create(
            Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"),
            ChallengeManager.SPRITE_RECT,
            ChallengeManager.SPRITE_PIVOT
        );

        [HarmonyPatch(typeof(AscensionIconInteractable), "AssignInfo")]
        [HarmonyPostfix]
        public static void ReassignableIconFixes(ref AscensionIconInteractable __instance, AscensionChallengeInfo info)
        {
            if (info.activatedSprite == null)
                __instance.activatedRenderer.sprite = DEFAULT_ACTIVATED_SPRITE;

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
            if (ChallengeManager.newInfos.Count > 0)
            {
                InscryptionAPIPlugin.Logger.LogDebug($"Creating Paginator");

                AscensionChallengePaginator paginator = AscensionMenuScreens.Instance.selectChallengesScreen.GetComponent<AscensionChallengePaginator>();
                if (paginator == null)
                    paginator = AscensionMenuScreens.Instance.selectChallengesScreen.AddComponent<AscensionChallengePaginator>();

                InscryptionAPIPlugin.Logger.LogDebug($"Getting pseudo prefabs");

                GameObject leftPseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen.transform.Find("Unlocks/ScreenAnchor/PageLeftButton").gameObject;
                GameObject rightPseudoPrefab = AscensionMenuScreens.Instance.cardUnlockSummaryScreen.transform.Find("Unlocks/ScreenAnchor/PageRightButton").gameObject;

                InscryptionAPIPlugin.Logger.LogDebug($"Getting icon grid");

                GameObject challengeIconGrid = AscensionMenuScreens.Instance.selectChallengesScreen.transform.Find("Icons/ChallengeIconGrid").gameObject;

                GameObject topRow = challengeIconGrid.transform.Find("TopRow").gameObject;
                GameObject bottomRow = challengeIconGrid.transform.Find("BottomRow").gameObject;

                InscryptionAPIPlugin.Logger.LogDebug($"Initializing data");

                paginator.topRow = new List<AscensionIconInteractable>();
                paginator.bottomRow = new List<AscensionIconInteractable>();
                for (int i = 1; i <= 7; i++)
                {
                    paginator.topRow.Add(topRow.transform.Find($"Icon_{i}").gameObject.GetComponent<AscensionIconInteractable>());
                    paginator.bottomRow.Add(bottomRow.transform.Find($"Icon_{i+7}").gameObject.GetComponent<AscensionIconInteractable>());
                }

                InscryptionAPIPlugin.Logger.LogDebug($"Original challenge info");
                paginator.availableChallenges = new List<AscensionChallengeInfo>();
                for (int i = 0; i < 7; i++)
                {
                    paginator.availableChallenges.Add(paginator.topRow[i].challengeInfo);
                    paginator.availableChallenges.Add(paginator.bottomRow[i].challengeInfo);
                }

                InscryptionAPIPlugin.Logger.LogDebug($"Custom challenge info");
                foreach (AscensionChallengeInfo info in ChallengeManager.newInfos.Where(i => ChallengeManager.IsStackable(i.challengeType)))
                {
                    paginator.availableChallenges.Add(info); // Add stackables twice
                    paginator.availableChallenges.Add(info); // Do them first so they stack nice
                }

                foreach (AscensionChallengeInfo info in ChallengeManager.newInfos.Where(i => !ChallengeManager.IsStackable(i.challengeType)))
                {
                    paginator.availableChallenges.Add(info);
                }

                paginator.GeneratePages();

                InscryptionAPIPlugin.Logger.LogDebug($"Creating page turners");
                GameObject leftIcon = GameObject.Instantiate(leftPseudoPrefab, challengeIconGrid.transform);
                GameObject rightIcon = GameObject.Instantiate(rightPseudoPrefab, challengeIconGrid.transform);

                InscryptionAPIPlugin.Logger.LogDebug($"Positioning page turners");
                leftIcon.transform.localPosition = leftIcon.transform.localPosition + (Vector3)(new Vector2(-0.75f, 0.25f));
                rightIcon.transform.localPosition = rightIcon.transform.localPosition + (Vector3)(new Vector2(0.75f, 0.25f));;

                InscryptionAPIPlugin.Logger.LogDebug($"Getting pagination controllers");
                AscensionMenuInteractable leftController = leftIcon.GetComponent<AscensionMenuInteractable>();
                AscensionMenuInteractable rightController = rightIcon.GetComponent<AscensionMenuInteractable>();

                Action<MainInputInteractable> leftClickAction = (MainInputInteractable i) => paginator.ChallengePageLeft(i);
                Action<MainInputInteractable> rightClickAction = (MainInputInteractable i) => paginator.ChallengePageRight(i);

                InscryptionAPIPlugin.Logger.LogDebug($"Setting click actions");
                leftController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(leftController.CursorSelectStarted, leftClickAction);
                rightController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(rightController.CursorSelectStarted, rightClickAction);

                paginator.challengePageIndex = 0;
            }
        }
    }
}
