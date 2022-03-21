using DiskCardGame;
using InscryptionAPI.Helpers;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public class StarterDeckPaginator : MonoBehaviour
{
    public static readonly Sprite noneDeckSprite = TextureHelper.GetTextureFromResource("InscryptionAPI/starterdeck_icon_none.png").ConvertTexture();

    public void Initialize(AscensionChooseStarterDeckScreen screen)
    {
        if (pages == null)
        {
            pages = new List<List<StarterDeckInfo>>();
            pageLength = screen.deckIcons.Count;
            List<AscensionStarterDeckIcon> sorted = screen.deckIcons;
            sorted.Sort((x, x2) => Mathf.RoundToInt((Mathf.Abs(x.transform.position.y - x2.transform.position.y) < 0.1f ? x.transform.position.x - x2.transform.position.x : x2.transform.position.y - x.transform.position.y) * 100));
            List<StarterDeckInfo> page = new List<StarterDeckInfo>();
            foreach (AscensionStarterDeckIcon icon in sorted)
            {
                page.Add(icon.Info);
            }
            pages.Add(page);
        }
        this.screen = screen;
    }

    public void AddPage(List<StarterDeckInfo> page)
    {
        Initialize(GetComponent<AscensionChooseStarterDeckScreen>());
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
        foreach(AscensionStarterDeckIcon icon in screen.deckIcons)
        {
            if(icon.Info == null)
                icon.iconRenderer.sprite = noneDeckSprite;
        }
        CommandLineTextDisplayer.PlayCommandLineClickSound();
    }

    public int pageIndex;
    public List<List<StarterDeckInfo>> pages;
    public int pageLength;
    public AscensionChooseStarterDeckScreen screen;
}