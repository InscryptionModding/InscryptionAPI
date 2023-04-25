using DiskCardGame;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public class AscensionChallengePaginator : MonoBehaviour
{
    public static Sprite missingChallengeSprite;
    public bool initialized;

    public void Initialize(AscensionChallengeScreen screen, AscensionMenuScreenTransition transition = null)
    {
        if (challengeObjectsForPages == null)
        {
            if (screen != null)
            {
                pageLength = screen.icons.Count;
                List<GameObject> toSort = screen.icons.ConvertAll((x) => x.gameObject);
                challengeObjectsForPages = new List<List<GameObject>>
                    {
                        toSort
                    };
            }
            else
            {
                List<AscensionIconInteractable> icons = new(transition.screenInteractables.FindAll((x) => x.GetComponent<AscensionIconInteractable>() != null).ConvertAll((x) =>
                    x.GetComponent<AscensionIconInteractable>()));
                pageLength = icons.Count;
                List<GameObject> toSort = icons.ConvertAll((x) => x.gameObject);
                challengeObjectsForPages = new List<List<GameObject>>
                    {
                        toSort
                    };
            }
        }
        this.screen = screen;
        if (screen == null)
        {
            this.transition = transition;
        }
    }

    public void AddPage(List<AscensionChallengeInfo> page)
    {
        if (missing == null)
        {
            missing = ScriptableObject.CreateInstance<AscensionChallengeInfo>();
            missing.name = "MISSING";
            missing.activatedSprite = null;
            missing.iconSprite = null;
            missing.challengeType = AscensionChallenge.None;
            missing.description = "";
            missing.title = "";
            missing.pointValue = 0;
        }
        Initialize(GetComponent<AscensionChallengeScreen>(), GetComponent<AscensionMenuScreenTransition>());
        List<GameObject> obj = new();
        for (int i = 0; i < Mathf.Min(challengeObjectsForPages[0].Count, 14); i++)
        {
            GameObject go = challengeObjectsForPages[0][i];
            if (go != null)
            {
                GameObject go2 = Instantiate(go);
                go2.transform.SetParent(go.transform.parent);
                AscensionIconInteractable chall = go2.GetComponent<AscensionIconInteractable>();
                go2.SetActive(false);
                chall.SetEnabled(true);
                go2.transform.localPosition = new Vector2(-1.65f + (i % 7) * 0.55f, go.transform.localPosition.y);
                if (screen != null)
                {
                    screen.icons.Add(chall);
                }
                obj.Add(go2);
            }
        }
        obj.Sort((x, x2) => Mathf.RoundToInt((Mathf.Abs(x.transform.position.x - x2.transform.position.x) < 0.1f ? x2.transform.position.y - x.transform.position.y : x.transform.position.x - x2.transform.position.x) * 100));
        for (int i = 0; i < obj.Count; i++)
        {
            var o = obj[i];
            o.GetComponent<AscensionIconInteractable>().challengeInfo = i < page.Count ? page[i] : missing;
            if (i >= page.Count)
            {
                o.AddComponent<NoneChallengeDisplayer>();
            }
        }
        challengeObjectsForPages.Add(obj);
    }

    public void NextPage()
    {
        pageIndex++;
        if (pageIndex >= challengeObjectsForPages.Count)
        {
            pageIndex = 0;
        }
        LoadPage(pageIndex);
    }

    public void PreviousPage()
    {
        pageIndex--;
        if (pageIndex < 0)
        {
            pageIndex = challengeObjectsForPages.Count - 1;
        }
        LoadPage(pageIndex);
    }

    public void LoadPage(int page)
    {
        if (page >= 0 && page < challengeObjectsForPages.Count)
        {
            for (int i = 0; i < challengeObjectsForPages.Count; i++)
            {
                var value = challengeObjectsForPages[i];
                if (i == page)
                {
                    value.RemoveAll(x => x == null);
                    value.ForEach((x) =>
                    {
                        if (x != null)
                        {
                            x?.SetActive(
                                x?.GetComponentInChildren<AscensionIconInteractable>()?.Info == null ||
                                x?.GetComponentInChildren<AscensionIconInteractable>().Info.challengeType != AscensionChallenge.FinalBoss ||
                                AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel));
                        }
                    });
                }
                else
                {
                    value.RemoveAll(x => x == null);
                    value.ForEach((x) => { if (x != null) { x?.SetActive(false); } });
                }
            }
        }
        CommandLineTextDisplayer.PlayCommandLineClickSound();
    }

    public void OnEnable()
    {
        Initialize(GetComponent<AscensionChallengeScreen>(), GetComponent<AscensionMenuScreenTransition>());
        ChallengeManager.SyncChallengeList();
        if (rightArrow)
            Destroy(rightArrow);
        if (leftArrow)
            Destroy(leftArrow);
        for (int i = 1; i < challengeObjectsForPages.Count; i++)
        {
            challengeObjectsForPages[i].ForEach(x => DestroyImmediate(x));
        }
        challengeObjectsForPages.Clear();
        screen?.icons?.RemoveAll(x => x == null);
        List<AscensionIconInteractable> icons = new();
        if (screen != null)
        {
            icons = screen.icons;
            pageLength = screen.icons.Count;
            List<GameObject> toSort = screen.icons.ConvertAll((x) => x.gameObject);
            challengeObjectsForPages = new List<List<GameObject>>
                    {
                        toSort
                    };
        }
        else
        {
            icons = new(transition.screenInteractables.FindAll((x) => x.GetComponent<AscensionIconInteractable>() != null).ConvertAll((x) =>
                x.GetComponent<AscensionIconInteractable>()));
            pageLength = icons.Count;
            List<GameObject> toSort = icons.ConvertAll((x) => x.gameObject);
            challengeObjectsForPages = new List<List<GameObject>>
                    {
                        toSort
                    };
        }
        List<ChallengeManager.FullChallenge> fcs = ChallengeManager.AllChallenges.ToList();
        List<(ChallengeManager.FullChallenge, AscensionChallengeInfo)> challengesToAdd = new(fcs.ConvertAll(x => (x, x.Challenge).Repeat(x.AppearancesInChallengeScreen)).SelectMany(x => x));
        List<AscensionIconInteractable> sortedicons = new(screen.icons);
        sortedicons.Sort((x, x2) => Mathf.RoundToInt((Mathf.Abs(x.transform.position.x - x2.transform.position.x) < 0.1f ? x2.transform.position.y - x.transform.position.y : x.transform.position.x - x2.transform.position.x) * 100));
        foreach (var icon in sortedicons)
        {
            if (challengesToAdd.Count > 0)
            {
                icon.AssignInfo(challengesToAdd[0].Item2);
                challengesToAdd.RemoveAt(0);
            }
            else
            {
                if (missing == null)
                {
                    missing = ScriptableObject.CreateInstance<AscensionChallengeInfo>();
                    missing.name = "MISSING";
                    missing.activatedSprite = null;
                    missing.iconSprite = null;
                    missing.challengeType = AscensionChallenge.None;
                    missing.description = "";
                    missing.title = "";
                    missing.pointValue = 0;
                }
                icon.AssignInfo(missing);
            }
            icon.conqueredRenderer.gameObject.SetActive(icon.Conquered && icon.showConquered);
        }
        challengesToAdd.Sort((x, x2) => x.Item1.SortValue != x2.Item1.SortValue ? x.Item1.SortValue - x2.Item1.SortValue : x2.Item1.UnlockLevel - x.Item1.UnlockLevel);
        List<List<AscensionChallengeInfo>> pagesToAdd = new();
        while (challengesToAdd.Count > 0)
        {
            List<AscensionChallengeInfo> page = new();
            for (int i = 0; i < 14; i++) // icons.Count
            {
                if (challengesToAdd.Count > 0)
                {
                    page.Add(challengesToAdd.Last().Item2);
                    challengesToAdd.RemoveAt(challengesToAdd.Count - 1);
                }
            }
            pagesToAdd.Add(page);
        }

        challengeObjectsForPages.ForEach(x => x.RemoveAll(x => x == null));
        pageIndex = 0;
        LoadPage(0);

        if (pagesToAdd.Count > 0)
        {
            foreach (List<AscensionChallengeInfo> page in pagesToAdd)
            {
                AddPage(page);
            }
            Vector3 topRight = new(float.MinValue, float.MinValue);
            Vector3 bottomLeft = new(float.MaxValue, float.MaxValue);

            // get bounding box of challenge icons array
            foreach (AscensionIconInteractable icon in icons)
            {
                if (icon != null && icon.iconRenderer != null && icon.gameObject.activeSelf)
                {
                    if (icon.iconRenderer.transform.position.x < bottomLeft.x)
                        bottomLeft.x = icon.iconRenderer.transform.position.x;

                    if (icon.iconRenderer.transform.position.x > topRight.x)
                        topRight.x = icon.iconRenderer.transform.position.x;

                    if (icon.iconRenderer.transform.position.y < bottomLeft.y)
                        bottomLeft.y = icon.iconRenderer.transform.position.y;

                    if (icon.iconRenderer.transform.position.y > topRight.y)
                        topRight.y = icon.iconRenderer.transform.position.y;
                }
            }
            Transform screenContinue = screen?.continueButton.gameObject.transform;

            Vector3 leftArrowPos = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.left / 3f;
            Vector3 rightArrowPos = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.right / 3f;

            // if the arrows would be offscreen/clipped by the screen edge,
            // or if the arrows' positions have been overriden by the config
            if (screenContinue.position.x < rightArrowPos.x || InscryptionAPIPlugin.configOverrideArrows.Value)
            {
                rightArrowPos = screenContinue.position + Vector3.left / 2f;

                leftArrowPos = new(
                (float)(screen?.transform.position.x - screenContinue.position.x) + Vector3.right.x / 2f,
                screenContinue.position.y,
                screenContinue.position.z);
            }

            leftArrow = Instantiate((screen?.gameObject ?? transform.gameObject).GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageLeftButton.gameObject);
            leftArrow.transform.SetParent(screen?.transform ?? transform.transform, worldPositionStays: false);
            leftArrow.transform.position = leftArrowPos;
            leftArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
            leftArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => PreviousPage();

            rightArrow = Instantiate((screen?.gameObject ?? transform.gameObject).GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject);
            rightArrow.transform.SetParent(screen?.transform ?? transform.transform, worldPositionStays: false);
            rightArrow.transform.position = rightArrowPos;
            rightArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
            rightArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => NextPage();
        }
    }

    public int pageIndex;
    public List<List<GameObject>> challengeObjectsForPages;
    public int pageLength;
    public AscensionChallengeScreen screen;
    private AscensionChallengeInfo missing;
    public AscensionMenuScreenTransition transition;
    public GameObject rightArrow;
    public GameObject leftArrow;

    private class NoneChallengeDisplayer : MonoBehaviour
    {
        public void Start()
        {
            missingChallengeSprite ??= TextureHelper.GetImageAsTexture("ascensionicon_none.png", Assembly.GetExecutingAssembly()).ConvertTexture();
            gameObject.GetComponent<AscensionIconInteractable>().iconRenderer.sprite = missingChallengeSprite;
            gameObject.GetComponent<AscensionIconInteractable>().blinkEffect.blinkOffColor = gameObject.GetComponent<AscensionIconInteractable>().conqueredColor;
            gameObject.GetComponent<AscensionIconInteractable>().blinkEffect.SetBlinkingEnabled(false);
        }
    }
}