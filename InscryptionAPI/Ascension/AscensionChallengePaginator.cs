using BepInEx;
using DiskCardGame;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Ascension;

/// <summary>
/// The purpose of this class is to help sync challenge icons as they appear in the selection menu.
/// 
/// </summary>
/*public class GameObject
{
    public AscensionIconInteractable IconInteractable;
    public GameObject IconObject;

    public int Index;
    public int PageNum;
    public bool Activated;
    public bool BossIcon;

    public GameObject(int pageNum, AscensionIconInteractable iconInteractable, int index = -1)
    {
        this.Activated = false;
        this.Index = index;
        this.PageNum = pageNum;
        this.IconInteractable = iconInteractable;
        this.IconObject = iconInteractable.gameObject;
        this.BossIcon = iconInteractable.challengeInfo.GetFullChallenge()?.Boss ?? false;
    }
}*/

public class AscensionChallengePaginator : MonoBehaviour
{
    public bool initialized;

    public void Initialize(AscensionChallengeScreen screen, AscensionMenuScreenTransition transition = null)
    {
        if (challengeObjectsForPages == null)
        {
            //List<GameObject> firstPageObjs = new();
            List<AscensionIconInteractable> icons;
            
            // if the screen object exists, use its list of icons; otherwise use the screenInteractables from the transition object
            if (screen != null)
            {
                icons = screen.icons;
                pageLength = screen.icons.Count;
            }
            else
            {
                icons = new(transition.screenInteractables.ConvertAll(x => x.GetComponent<AscensionIconInteractable>()));
                icons.RemoveAll(x => x == null);
                pageLength = icons.Count;
            }

            challengeObjectsForPages = new List<List<GameObject>>() { icons.ConvertAll(x => x.gameObject) };
        }

        if (screen == null)
            this.transition = transition;

        this.screen = screen;
    }
    public void InitialiseMissingChallengeInfo()
    {
        missingChallengeInfo = ScriptableObject.CreateInstance<AscensionChallengeInfo>();
        missingChallengeInfo.name = "MISSING";
        missingChallengeInfo.activatedSprite = null;
        missingChallengeInfo.iconSprite = null;
        missingChallengeInfo.challengeType = AscensionChallenge.None;
        missingChallengeInfo.description = "";
        missingChallengeInfo.title = "";
        missingChallengeInfo.pointValue = 0;
    }

    public void AddPage(List<AscensionChallengeInfo> challengeInfos)
    {
        // create the challenge info for missingChallengeInfo challenges
        if (missingChallengeInfo == null)
        {
            InitialiseMissingChallengeInfo();
        }

        // update the screen and transition
        Initialize(GetComponent<AscensionChallengeScreen>(), GetComponent<AscensionMenuScreenTransition>());
        List<GameObject> newPage = new();

        List<AscensionChallengeInfo> regularChallengeInfos = challengeInfos.FindAll(x => !x.challengeType.GetFullChallenge().Boss);
        List<AscensionChallengeInfo> bossChallengeInfos = challengeInfos.FindAll(x => x.challengeType.GetFullChallenge().Boss);

        // keep track of the number of bosses that are on this page, and account for them when determining how many icons to create        
        int numBosses = challengeInfos.Count(x => x.challengeType.GetFullChallenge().Boss);
        int numIcons = Mathf.Min(14, challengeObjectsForPages[0].Count) - numBosses;

        //int occupiedRows = (1 + regularChallengeInfos.Count) / 2 + bossChallengeInfos.Count;
        int numBossesAdded = 0;
        int bossStartingIndex = (1 + regularChallengeInfos.Count) / 2;//numRows - bossChallengeInfos.Count;

        Debug.Log($"NumIcons for Page: {numIcons}: {regularChallengeInfos.Count} {bossChallengeInfos.Count}");

        /*// Prototyping for a more complicated system of sorting boss icons according to their challenge type (reds w/ reds, greens w/ greens, etc.)
        // Current system just sticks them at the end of the page, regardless of their colour or the colour of preceding challenges
        // | | |2| |4| |6|        // | | |2| |4| | |
        // |+|+|B|B|-|x| |        // |=|=|-|B|x|x| |
        // |+|+|=|=|B| | |        // |=|B|-|x|x|x| |

        // | | | |3| | | |        // | | |2| | |5| |
        // |=|=|-|x|x|x|x|        // |+|=|-|-|B|x| |
        // |=|-|B|x|x|x| |        // |=|B|-|-|x|x| |

        int positiveChalls = regularChallengeInfos.FindAll(x => x.pointValue > 0).Count;
        int neutralChalls = regularChallengeInfos.FindAll(x => x.pointValue == 0).Count;
        int negativeChalls = regularChallengeInfos.FindAll(x => x.pointValue < 0).Count;*/

        for (int i = 0; i < Mathf.Min(14, numIcons); i++)
        {
            GameObject objectRef;

            // if there are boss icons we still need to add to the page
            if (i % 7 >= bossStartingIndex && i % 7 < bossStartingIndex + numBosses)
            {

                // if this is the correct index for adding a boss
                // but one has already been added, skip to the next non-boss index
                if (numBossesAdded < numBosses)
                {
                    numBossesAdded++;
                    objectRef = challengeObjectsForPages[0][14];
                }
                else
                {
                    i += numBosses;
                    objectRef = challengeObjectsForPages[0][i];
                }
            }
            else
            {
                objectRef = challengeObjectsForPages[0][i];
            }

            if (objectRef != null)
            {
                GameObject newIcon = CreateIconGameObject(objectRef, i);
                newPage.Add(newIcon);
            }
        }

        newPage.Sort((x, x2) => Mathf.RoundToInt(
                    (Mathf.Abs(x.transform.position.x - x2.transform.position.x) < 0.1f ? x2.transform.position.y - x.transform.position.y : x.transform.position.x - x2.transform.position.x) * 100
                    ));
        DebugChallengePageArray(challengeObjectsForPages.Count, newPage);

        // set the challenge info for each icon in the page
        for (int i = 0; i < newPage.Count; i++)
        {
            //Debug.Log($"Assign info at: {i}");
            AscensionIconInteractable interactable = newPage[i].GetComponent<AscensionIconInteractable>();
            if (i < challengeInfos.Count)
            {
                interactable.challengeInfo = challengeInfos[i];
            }
            else
            {
                interactable.challengeInfo = missingChallengeInfo;
                newPage[i].AddComponent<NoneChallengeDisplayer>();
            }
        }

        challengeObjectsForPages.Add(newPage);
    }

    private GameObject CreateIconGameObject(GameObject objectRef, int index, AscensionChallengeInfo info = null)
    {
        GameObject newIcon = Instantiate(objectRef, objectRef.transform.parent);
        newIcon.SetActive(false);
        newIcon.transform.localPosition = new Vector2(-1.65f + (index % 7) * 0.55f, objectRef.transform.localPosition.y);

        AscensionIconInteractable chall = newIcon.GetComponent<AscensionIconInteractable>();
        chall.SetEnabled(true);
        screen?.icons.Add(chall);
        if (info != null)
            chall.challengeInfo = info;
        
        return newIcon;
    }
    public void OnEnable()
    {
        // if there are still active challenges, don't rebuild the pages
        if (AscensionSaveData.Data.activeChallenges.Count > 0)
        {
            LoadPage(pageIndex = 0);
            return;
        }

        ChallengeManager.SyncChallengeList();
        Initialize(GetComponent<AscensionChallengeScreen>(), GetComponent<AscensionMenuScreenTransition>());
        
        if (leftArrow)
        {
            Destroy(leftArrow.gameObject);
        }
        if (rightArrow)
        {
            Destroy(rightArrow.gameObject);
        }

        for (int i = 1; i < challengeObjectsForPages.Count; i++)
        {
            challengeObjectsForPages[i].ForEach(DestroyImmediate);
        }

        if (AscensionSaveData.Data.activeChallenges.Count == 0)
            challengeObjectsForPages.Clear();

        List<AscensionIconInteractable> icons = new();
        if (screen != null)
        {
            screen.icons?.RemoveAll(x => x == null);
            
            icons = screen.icons;
            pageLength = screen.icons.Count;
        }
        else
        {
            icons = new(transition.screenInteractables.ConvertAll(x => x.GetComponent<AscensionIconInteractable>()));
            icons.RemoveAll(x => x == null);
            pageLength = icons.Count;
        }

        // re-order the icons so their info corresponds to their position in the array
        icons.Sort((x, x2) => Mathf.RoundToInt((Mathf.Abs(x.transform.position.x - x2.transform.position.x) < 0.1f ? x2.transform.position.y - x.transform.position.y : x.transform.position.x - x2.transform.position.x) * 100));

        challengeObjectsForPages = new List<List<GameObject>> { icons.ConvertAll(x => x.gameObject) };

        List<ChallengeManager.FullChallenge> fcs = new(ChallengeManager.AllChallenges);
        List<(ChallengeManager.FullChallenge, AscensionChallengeInfo)> challengesToAdd = new(fcs.ConvertAll(x => (x, x.Challenge).Repeat(x.AppearancesInChallengeScreen)).SelectMany(x => x));
        List<AscensionIconInteractable> sortedIcons = new(icons);

        foreach (var icon in sortedIcons)
        {
            if (challengesToAdd.Count > 0)
            {
                icon.AssignInfo(challengesToAdd[0].Item2);
                challengesToAdd.RemoveAt(0);
            }
            else
            {
                if (missingChallengeInfo == null)
                {
                    InitialiseMissingChallengeInfo();
                }
                icon.AssignInfo(missingChallengeInfo);
            }
            icon.conqueredRenderer.gameObject.SetActive(icon.Conquered && icon.showConquered);
        }

        // sort challenges in order of: positive pts, 0 pts, negatve pts, positive pts boss, 0 pts boss, negative pts boss
        challengesToAdd.Sort((x, x2) =>
        {
            if (x.Item1.SortValue != x2.Item1.SortValue)
            {
                return x.Item1.SortValue - x2.Item1.SortValue;
            }
            else
            {
                return x2.Item1.UnlockLevel - x.Item1.UnlockLevel;
            }
        });

        List<List<AscensionChallengeInfo>> pagesToAdd = new();
        while (challengesToAdd.Count > 0)
        {
            List<(ChallengeManager.FullChallenge, AscensionChallengeInfo)> page = new();
            for (int i = 0; i < 14; i++) // each page can have 14 icons max
            {
                if (challengesToAdd.Count > 0)
                {
                    page.Add(challengesToAdd.Last());
                    if (challengesToAdd.Last().Item1.Boss) // boss icons take up a whole column
                    {
                        i++;
                    }
                    challengesToAdd.RemoveAt(challengesToAdd.Count - 1);
                }
            }
            // re-sort the page so boss challenges have the correct info
            page.Sort((x, x2) =>
            {
                if (x.Item1.SortValue != x2.Item1.SortValue)
                {
                    return x2.Item1.BossSortValue - x.Item1.BossSortValue;
                }
                else
                {
                    return (x.Item1.UnlockLevel + (x2.Item1.Boss ? 0 : 0)) - (x2.Item1.UnlockLevel + (x.Item1.Boss ? 0 : 0));
                }
            });
            pagesToAdd.Add(page.ConvertAll(x => x.Item2));
            DebugChallengePageArray(pagesToAdd.Count, null, page.ConvertAll(x => x.Item2));
        }

        challengeObjectsForPages.ForEach(x => x.RemoveAll(x => x == null));
        LoadPage(pageIndex = 0);
        
        if (pagesToAdd.Count == 0)
            return;

        pagesToAdd.ForEach(AddPage);
        InstantiateArrowObjects(icons);
    }

    public void InstantiateArrowObjects(List<AscensionIconInteractable> icons)
    {
        Vector3 topRight = new(float.MinValue, float.MinValue);
        Vector3 bottomLeft = new(float.MaxValue, float.MaxValue);

        // get bounding box of challenge icons array
        foreach (AscensionIconInteractable icon in icons)
        {
            if (icon?.iconRenderer == null || !icon.gameObject.activeSelf)
                continue;

            if (icon.iconRenderer.transform.position.x < bottomLeft.x)
                bottomLeft.x = icon.iconRenderer.transform.position.x;

            if (icon.iconRenderer.transform.position.x > topRight.x)
                topRight.x = icon.iconRenderer.transform.position.x;

            if (icon.iconRenderer.transform.position.y < bottomLeft.y)
                bottomLeft.y = icon.iconRenderer.transform.position.y;

            if (icon.iconRenderer.transform.position.y > topRight.y)
                topRight.y = icon.iconRenderer.transform.position.y;
        }
        Transform screenContinue = screen?.continueButton.gameObject.transform;

        Vector3 leftArrowPos = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.left / 3f;
        Vector3 rightArrowPos = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.right / 3f;

        // if the arrows would be offscreen/clipped by the screen edge,
        // or if the arrows' positions have been overriden by the config
        if (InscryptionAPIPlugin.configOverrideArrows.Value || screenContinue.position.x < rightArrowPos.x)
        {
            rightArrowPos = screenContinue.position + Vector3.left / 2f;

            leftArrowPos = new(
                (float)(screen?.transform.position.x - screenContinue.position.x) + Vector3.right.x / 2f,
                screenContinue.position.y,
                screenContinue.position.z);
        }

        leftArrow = Instantiate((screen?.gameObject ?? transform.gameObject).GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageLeftButton.gameObject).GetComponent<AscensionMenuInteractable>();
        leftArrow.transform.SetParent(screen?.transform ?? transform.transform, worldPositionStays: false);
        leftArrow.transform.position = leftArrowPos;
        leftArrow.ClearDelegates();
        leftArrow.CursorSelectStarted += (x) => PreviousPage();

        rightArrow = Instantiate((screen?.gameObject ?? transform.gameObject).GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject).GetComponent<AscensionMenuInteractable>();
        rightArrow.transform.SetParent(screen?.transform ?? transform.transform, worldPositionStays: false);
        rightArrow.transform.position = rightArrowPos;
        rightArrow.ClearDelegates();
        rightArrow.CursorSelectStarted += (x) => NextPage();
    }
    public void NextPage()
    {
        pageIndex++;
        if (pageIndex >= challengeObjectsForPages.Count) // wrap-around
            pageIndex = 0;

        LoadPage(pageIndex);
    }
    public void PreviousPage()
    {
        pageIndex--;
        if (pageIndex < 0)
            pageIndex = challengeObjectsForPages.Count - 1;  // wrap-around

        LoadPage(pageIndex);
    }

    public void LoadPage(int page)
    {
        // if page index corresponds to an index in challengeObjects
        if (page >= 0 && page < challengeObjectsForPages.Count)
        {
            // for every page of challenges
            for (int i = 0; i < challengeObjectsForPages.Count; i++)
            {
                List<GameObject> iconsForPage = challengeObjectsForPages[i];
                iconsForPage.RemoveAll(x => x == null);

                // set all icons that aren't for the current page to inactive
                if (i != page)
                {
                    iconsForPage.ForEach(x => x.SetActive(false));
                    continue;
                }

                // for every icon on this page
                for (int j = 0; j < iconsForPage.Count; j++)
                {
                    AscensionIconInteractable interactable = iconsForPage[j].GetComponent<AscensionIconInteractable>();
                    if (interactable != null)
                    {
                        AscensionChallenge currentChallenge = interactable.Info?.challengeType ?? AscensionChallenge.FinalBoss;

                        iconsForPage[j].SetActive(currentChallenge != AscensionChallenge.FinalBoss ||
                            AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(currentChallenge, AscensionSaveData.Data.challengeLevel));
                    }
                    else
                    {
                        iconsForPage[j].SetActive(false);
                    }
                }

                //DebugChallengePageArray(page, iconsForPage);
            }
        }
        CommandLineTextDisplayer.PlayCommandLineClickSound();
    }

    /// <summary>
    /// A List representing the entire collection of challenge pages.
    /// </summary>
    public List<List<GameObject>> challengeObjectsForPages;

    public int pageIndex;
    public int pageLength;

    public AscensionChallengeScreen screen;
    
    public AscensionMenuScreenTransition transition;

    public AscensionMenuInteractable leftArrow;
    public AscensionMenuInteractable rightArrow;

    public static Sprite missingChallengeSprite;
    public static AscensionChallengeInfo missingChallengeInfo;

    private void DebugChallengePageArray(int page, List<GameObject> iconsForPage, List<AscensionChallengeInfo> infos = null)
    {
        InscryptionAPIPlugin.Logger.LogDebug($"Current Page: {page} | {iconsForPage?.Count ?? infos.Count}");
        StringBuilder topRow = new("|");

        if (iconsForPage != null)
        {
            StringBuilder bottomRow = new("|");
            for (int i = 0; i < 14; i++)
            {
                if (i % 2 == 0)
                {
                    if (i < iconsForPage.Count)
                        topRow.Append($"{DebugArrayCode(iconsForPage[i])}|");
                    else
                        topRow.Append($" |");
                }
                else
                {
                    if (i < iconsForPage.Count)
                        bottomRow.Append($"{DebugArrayCode(iconsForPage[i])}|");
                    else
                        bottomRow.Append($" |");
                }
            }
            InscryptionAPIPlugin.Logger.LogDebug(topRow.ToString());
            InscryptionAPIPlugin.Logger.LogDebug(bottomRow.ToString());
        }
        else
        {
            for (int i = 0; i < 14; i++)
            {
                if (i < infos.Count)
                    topRow.Append($"{DebugArrayCode(null, infos[i])}|");
                else
                    topRow.Append($" |");
            }
            InscryptionAPIPlugin.Logger.LogDebug(topRow.ToString());
        }
    }
    private char DebugArrayCode(GameObject icon, AscensionChallengeInfo info = null)
    {
        if (info != null)
        {
            return info.challengeType.GetFullChallenge().Boss ? 'B' : 'O';
        }

        AscensionIconInteractable interactable = icon.GetComponent<AscensionIconInteractable>();
        if (interactable.Info == missingChallengeInfo)
            return '-';

        if (interactable.Info?.challengeType.GetFullChallenge()?.Boss ?? false)
            return interactable.activatedRenderer.enabled ? 'B' : 'D';

        return interactable.activatedRenderer.enabled ? 'X' : 'O';
    }

    private class NoneChallengeDisplayer : MonoBehaviour
    {
        public void Start()
        {
            AscensionIconInteractable icon = gameObject.GetComponent<AscensionIconInteractable>();
            missingChallengeSprite ??= TextureHelper.GetImageAsTexture("ascensionicon_none.png", Assembly.GetExecutingAssembly()).ConvertTexture();
            icon.iconRenderer.sprite = missingChallengeSprite;
            icon.blinkEffect.blinkOffColor = icon.conqueredColor;
            icon.blinkEffect.SetBlinkingEnabled(false);
        }
    }
}