using DiskCardGame;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public class StarterDeckPaginator : MonoBehaviour
{
    public const int ICONS_PER_PAGE = 8;

    public int starterDeckPageIndex = 0;

    public readonly static int CHALLENGES_PER_ROW = 7;

    public List<AscensionStarterDeckIcon> icons;

    public List<List<StarterDeckInfo>> pages = new();

    private void PageBuilder(List<StarterDeckInfo> starterDecks, int startIdx)
    {
        List<StarterDeckInfo> curPage = null;
        for (int i = startIdx; i < starterDecks.Count; i++)
        {
            if (curPage == null)
                curPage = new List<StarterDeckInfo>();

            // Check to see if we need a new page
            if (curPage.Count == ICONS_PER_PAGE)
            {
                pages.Add(curPage);
                curPage = new List<StarterDeckInfo>();
            }

            curPage.Add(starterDecks[i]);
        }

        if (curPage != null)
            pages.Add(curPage);
    }

    public void GeneratePages()
    {
        // The first page is nice and easy
        pages = new();
        List<StarterDeckInfo> pageOne = new List<StarterDeckInfo>();
        pageOne.AddRange(StarterDeckManager.AllDeckInfos.GetRange(0, ICONS_PER_PAGE));
        pages.Add(pageOne);

        // Do the starterDecks
        if (StarterDeckManager.AllDeckInfos.Count > ICONS_PER_PAGE)
            PageBuilder(StarterDeckManager.AllDeckInfos, ICONS_PER_PAGE);
    }

    public void ShowVisibleStarterDecks()
    {
        // Sort out which list of starterDecks are the visible ones
        List<StarterDeckInfo> visibleStarterDecks = pages[starterDeckPageIndex];

        // Make all starterDeck icons inactive
        foreach (var icon in icons)
            icon.gameObject.SetActive(false);

        // Start going through and setting the icons
        for (int i = 0; i < visibleStarterDecks.Count; i++)
        {
            if (visibleStarterDecks[i] == null) // this is a spacer
                continue;
            AscensionStarterDeckIcon targetIcon = this.icons[i];
            targetIcon.AssignInfo(visibleStarterDecks[i]);
            targetIcon.gameObject.SetActive(true);
        }
    }

    public void StarterDeckPageLeft(MainInputInteractable button)
    {
        if (!AscensionMenuScreens.Instance.starterDeckSelectScreen.activeSelf)
            return;

        if (starterDeckPageIndex > 0)
        {
            starterDeckPageIndex -= 1;
            ShowVisibleStarterDecks();
        }
    }

    public void StarterDeckPageRight(MainInputInteractable button)
    {
        if (!AscensionMenuScreens.Instance.starterDeckSelectScreen.activeSelf)
            return;

        if (starterDeckPageIndex < pages.Count - 1)
        {
            starterDeckPageIndex += 1;
            ShowVisibleStarterDecks();
        }
    }

    public void OnEnable()
    {
        StarterDeckManager.SyncDeckList();
        starterDeckPageIndex = 0;
        GeneratePages();
        ShowVisibleStarterDecks();
    }
}