using DiskCardGame;
using InscryptionAPI.Helpers;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Ascension;

/// <summary>
/// Class that handles adding displaying starter decks and allows for multiple pages of starter decks.
/// </summary>
public class StarterDeckPaginator : MonoBehaviour
{
    private static Sprite noneDeckSprite;

    private void Initialize(AscensionChooseStarterDeckScreen screen)
    {
        pages = new List<List<StarterDeckInfo>>();
        pageLength = screen.deckIcons.Count;
        this.screen = screen;
    }
    /// <summary>
    /// Adds a new page of starter decks to the list of pages.
    /// Pages contain (by default) up to 8 starter deck. Excess decks overflow onto new pages.
    /// </summary>
    /// <param name="page"><see cref="StarterDeckInfo"/>s that will be displayed on this page.</param>
    public void AddPage(List<StarterDeckInfo> page)
    {
        if (page.Count < pageLength)
        {
            while (page.Count < pageLength)
            {
                page.Add(null);
            }
        }
        pages.Add(page);
    }
    /// <summary>
    /// Loads the next page. Wraps around to the first page if currently on the last page.
    /// </summary>
    public void NextPage()
    {
        pageIndex++;
        if (pageIndex >= pages.Count)
        {
            pageIndex = 0;
        }
        LoadPage(pages[pageIndex]);
    }
    /// <summary>
    /// Loads the previous page. Wraps around to the last page if currently on the first page.
    /// </summary>
    public void PreviousPage()
    {
        pageIndex--;
        if (pageIndex < 0)
        {
            pageIndex = pages.Count - 1;
        }
        LoadPage(pages[pageIndex]);
    }

    /// <summary>
    /// Loads the challenge page at the given index.
    /// </summary>
    /// <param name="page">Index of the page to load.</param>
    public void LoadPage(List<StarterDeckInfo> page)
    {
        List<AscensionStarterDeckIcon> sorted = new List<AscensionStarterDeckIcon>(screen.deckIcons);
        sorted.Sort((x, x2) => Mathf.RoundToInt((Mathf.Abs(x.transform.position.y - x2.transform.position.y) < 0.1f ? x.transform.position.x - x2.transform.position.x : x2.transform.position.y - x.transform.position.y) * 100));
        for (int i = 0; i < pageLength; i++)
        {
            if (i < sorted.Count)
            {
                if (i < page.Count)
                {
                    sorted[i].starterDeckInfo = page[i];
                    sorted[i].AssignInfo(page[i]);
                    if (!sorted[i].Unlocked)
                    {
                        sorted[i].conqueredRenderer.enabled = false;
                    }
                }
                else
                {
                    sorted[i].starterDeckInfo = null;
                    sorted[i].AssignInfo(null);
                    sorted[i].conqueredRenderer.enabled = false;
                    sorted[i].iconRenderer.sprite = noneDeckSprite;
                }
            }
        }
        noneDeckSprite ??= TextureHelper.GetImageAsTexture("starterdeck_icon_none.png", Assembly.GetExecutingAssembly()).ConvertTexture();
        foreach (AscensionStarterDeckIcon icon in screen.deckIcons)
        {
            if (icon.Info == null)
                icon.iconRenderer.sprite = noneDeckSprite;
        }
        CommandLineTextDisplayer.PlayCommandLineClickSound();
    }
    /// <summary>
    /// Initialise the starter deck screen and pages whenever the starter deck screen is enabled.
    /// </summary>
    public void OnEnable()
    {
        StarterDeckManager.SyncDeckList();
        Initialize(GetComponent<AscensionChooseStarterDeckScreen>());
        if (rightArrow)
        {
            Destroy(rightArrow);
        }
        if (leftArrow)
        {
            Destroy(leftArrow);
        }
        List<StarterDeckInfo> decksToAdd = new(StarterDeckManager.AllDecks.ConvertAll((x) => x.Info));
        List<AscensionStarterDeckIcon> icons = screen.deckIcons;
        List<List<StarterDeckInfo>> pagesToAdd = new();
        while (decksToAdd.Count > 0)
        {
            List<StarterDeckInfo> page = new();
            for (int i = 0; i < icons.Count; i++)
            {
                if (decksToAdd.Count > 0)
                {
                    page.Add(decksToAdd[0]);
                    decksToAdd.RemoveAt(0);
                }
            }
            pagesToAdd.Add(page);
        }
        if (pagesToAdd.Count > 0)
        {
            foreach (List<StarterDeckInfo> page in pagesToAdd)
            {
                AddPage(page);
            }
            Vector3 topRight = new Vector3(float.MinValue, float.MinValue);
            Vector3 bottomLeft = new Vector3(float.MaxValue, float.MaxValue);
            foreach (AscensionStarterDeckIcon icon in icons)
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
            leftArrow = UnityEngine.Object.Instantiate(screen.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageLeftButton.gameObject);
            leftArrow.transform.parent = screen.transform;
            leftArrow.transform.position = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.left / 2f;
            leftArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
            leftArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => PreviousPage();
            rightArrow = UnityEngine.Object.Instantiate(screen.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject);
            rightArrow.transform.parent = screen.transform;
            rightArrow.transform.position = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.right / 2f;
            rightArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
            rightArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => NextPage();
        }
        pageIndex = 0;
        LoadPage(pages[0]);
    }

    /// <summary>
    /// The UI interactable for moving to the previous page.
    /// </summary>
    public GameObject leftArrow;
    /// <summary>
    /// The UI interactable for moving to the next page.
    /// </summary>
    public GameObject rightArrow;
    /// <summary>
    /// The current page index. Determines what starter decks are currently displayed.
    /// </summary>
    public int pageIndex;
    /// <summary>
    /// A List representing the entire collection of pages.
    /// </summary>
    public List<List<StarterDeckInfo>> pages;
    /// <summary>
    /// How many decks are displayed on each page.
    /// </summary>
    public int pageLength;

    /// <summary>
    /// Object that handles displaying starter decks.
    /// </summary>
    public AscensionChooseStarterDeckScreen screen;
}