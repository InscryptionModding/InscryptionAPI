using DiskCardGame;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public class AscensionChallengePaginator : MonoBehaviour
{
    public int challengePageIndex = 0;

    public readonly static int CHALLENGES_PER_ROW = 7;

    public List<AscensionChallengeInfo> availableChallenges;

    public List<AscensionIconInteractable> topRow;

    public List<AscensionIconInteractable> bottomRow;

    public List<List<AscensionChallengeInfo>> pages = new();

    public List<List<bool>> pageStates = new();

    private void PageBuilder(List<AscensionChallengeInfo> challenges, int startIdx)
    {
        List<AscensionChallengeInfo> curPage = new List<AscensionChallengeInfo>();
        for (int i = startIdx; i < challenges.Count; i++)
        {
            // Check to see if we need a new page
            if (curPage.Count == 14)
            {
                pages.Add(curPage);
                curPage = new List<AscensionChallengeInfo>();
            }

            // Check to see if we need a blank buffer
            // This happens if the next icon is the same as the current, and the
            // current icon would be on the bottom row
            if (i < challenges.Count - 1 && challenges[i + 1].challengeType == challenges[i].challengeType && curPage.Count % 2 == 1)
                curPage.Add(null);

            curPage.Add(challenges[i]);
        }
        pages.Add(curPage);
    }

    public void GeneratePages()
    {
        // The first page is nice and easy
        List<AscensionChallengeInfo> pageOne = new List<AscensionChallengeInfo>();
        pageOne.AddRange(availableChallenges.GetRange(0, 14));
        pages.Add(pageOne);

        // Do the challenges first:
        List<AscensionChallengeInfo> challenges = availableChallenges.Where(i => i.pointValue > 0).ToList();
        List<AscensionChallengeInfo> assists = availableChallenges.Where(i => i.pointValue < 0).ToList();

        // Do the challenges
        if (challenges.Count > 14)
            PageBuilder(challenges, 14);

        if (assists.Count > 0)
            PageBuilder(assists, 0);

        while (pageStates.Count < pages.Count)
        {
            pageStates.Add(new List<bool>());
            for (int i = 0; i < 14; i++)
                pageStates[pageStates.Count - 1].Add(false);
        }
    }

    private void SavePageState()
    {
        for (int i = 0; i < 7; i++)
        {
            pageStates[challengePageIndex][i * 2] = topRow[i].activatedRenderer.enabled;
            pageStates[challengePageIndex][i * 2 + 1] = bottomRow[i].activatedRenderer.enabled;
        }
    }

    public void ShowVisibleChallenges()
    {
        // Sort out which list of challenges are the visible ones
        List<AscensionChallengeInfo> visibleChallenges = pages[challengePageIndex];
        List<bool> selectedChallenges = pageStates[challengePageIndex];

        // Make all challenge icons inactive
        foreach (AscensionIconInteractable icon in topRow.Concat(bottomRow))
            icon.gameObject.SetActive(false);

        // Start going through and setting the icons
        for (int i = 0; i < visibleChallenges.Count; i++)
        {
            if (visibleChallenges[i] == null) // this is a spacer
                continue;
            AscensionIconInteractable targetIcon = (i % 2 == 0) ? this.topRow[i / 2] : this.bottomRow[i / 2];
            targetIcon.AssignInfo(visibleChallenges[i]);
            targetIcon.activatedRenderer.enabled = selectedChallenges[i];
            targetIcon.gameObject.SetActive(true);
        }
    }

    public void ChallengePageLeft(MainInputInteractable button)
    {
        if (!AscensionMenuScreens.Instance.selectChallengesScreen.activeSelf)
            return;

        if (challengePageIndex > 0)
        {
            SavePageState();
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
            SavePageState();
            challengePageIndex += 1;
            ShowVisibleChallenges();
        }
    }
}