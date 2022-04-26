using DiskCardGame;
using InscryptionAPI.Helpers;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public class StarterDeckPaginator : MonoBehaviour
{
    private static Sprite noneDeckSprite;

    private void Initialize(AscensionChooseStarterDeckScreen screen)
    {
        pages = new List<List<StarterDeckInfo>>();
        pageLength = screen.deckIcons.Count;
        this.screen = screen;
    }

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

    public void NextPage()
    {
        pageIndex++;
        if (pageIndex >= pages.Count)
        {
            pageIndex = 0;
        }
        LoadPage(pages[pageIndex]);
    }

    public void PreviousPage()
    {
        pageIndex--;
        if (pageIndex < 0)
        {
            pageIndex = pages.Count - 1;
        }
        LoadPage(pages[pageIndex]);
    }

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

    public GameObject leftArrow;
    public GameObject rightArrow;
    public int pageIndex;
    public List<List<StarterDeckInfo>> pages;
    public int pageLength;
    public AscensionChooseStarterDeckScreen screen;
}