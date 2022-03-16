using DiskCardGame;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public class AscensionChallengePaginator : MonoBehaviour
{
    public int challengePageIndex = 0;

    public readonly static int CHALLENGES_PER_ROW = 7;

    public List<AscensionIconInteractable> topRow;

    public List<AscensionIconInteractable> bottomRow;

    public AscensionIconInteractable extraIcon;

    public bool showExtraIcon;

    public List<List<AscensionChallenge>> pages = new();

    private void PageBuilder(List<AscensionChallenge> challenges, int startIdx)
    {
        List<AscensionChallenge> curPage = new List<AscensionChallenge>();
        for (int i = startIdx; i < challenges.Count; i++)
        {
            // Check to see if we need a new page
            if (curPage.Count == 14)
            {
                pages.Add(curPage);
                curPage = new List<AscensionChallenge>();
            }

            // Check to see if we need a blank buffer
            // This happens if the next icon is the same as the current, and the
            // current icon would be on the bottom row
            if (i < challenges.Count - 1 && challenges[i + 1] == challenges[i] && curPage.Count % 2 == 1)
                curPage.Add(AscensionChallenge.None);

            curPage.Add(challenges[i]);
        }
        pages.Add(curPage);
    }

    public void GeneratePages()
    {
        // The first page is nice and easy
        pages = new();

        // Do the challenges first:
        List<AscensionChallenge> challenges = new();
        List<AscensionChallenge> assists = new();

        foreach (AscensionChallenge ch in ChallengeManager.AllInfo.Where(i => i.pointValue >= 0 && i.challengeType != extraIcon.Info.challengeType).Select(sci => sci.challengeType))
        {
            challenges.Add(ch);
            if (ChallengeManager.IsStackable(ch))
                challenges.Add(ch);
        }

        foreach (AscensionChallenge ch in ChallengeManager.AllInfo.Where(i => i.pointValue < 0).Select(sci => sci.challengeType))
        {
            assists.Add(ch);
            if (ChallengeManager.IsStackable(ch))
                assists.Add(ch);
        }

        // Do the challenges
        if (challenges.Count > 0)
            PageBuilder(challenges, 0);

        if (assists.Count > 0)
            PageBuilder(assists, 0);
    }

    public void ShowVisibleChallenges()
    {
        // Sort out which list of challenges are the visible ones
        List<AscensionChallenge> visibleChallenges = pages[challengePageIndex];

        // Make all challenge icons inactive
        foreach (AscensionIconInteractable icon in topRow.Concat(bottomRow))
            icon.gameObject.SetActive(false);

        // Start going through and setting the icons
        for (int i = 0; i < visibleChallenges.Count; i++)
        {
            if (visibleChallenges[i] == AscensionChallenge.None) // this is a spacer
                continue;
            AscensionIconInteractable targetIcon = (i % 2 == 0) ? this.topRow[i / 2] : this.bottomRow[i / 2];
            targetIcon.AssignInfo(ChallengeManager.AllInfo.First(sci => sci.challengeType == visibleChallenges[i]));

            int numActive = AscensionSaveData.Data.GetNumChallengesOfTypeActive(visibleChallenges[i]);
            int target = 0;
            if (i > 0 && visibleChallenges[i] == visibleChallenges[i - 1])
                target = 1;

            targetIcon.activatedRenderer.enabled = numActive > target;
            targetIcon.gameObject.SetActive(true);
        }

        if (challengePageIndex == 0 && showExtraIcon)
            this.extraIcon.gameObject.SetActive(true);
        else
            this.extraIcon.gameObject.SetActive(false);
    }

    public void ChallengePageLeft(MainInputInteractable button)
    {
        if (!AscensionMenuScreens.Instance.selectChallengesScreen.activeSelf)
            return;

        if (challengePageIndex > 0)
        {
            challengePageIndex -= 1;
            ShowVisibleChallenges();
        }
    }

    public void ChallengePageRight(MainInputInteractable button)
    {
        if (!AscensionMenuScreens.Instance.selectChallengesScreen.activeSelf)
            return;

        if (challengePageIndex < pages.Count - 1)
        {
            challengePageIndex += 1;
            ShowVisibleChallenges();
        }
    }

    public void OnEnable()
    {
        ChallengeManager.SyncChallengeList();
        challengePageIndex = 0;
        GeneratePages();
        ShowVisibleChallenges();
    }
}
