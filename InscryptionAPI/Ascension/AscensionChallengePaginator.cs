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
                challengeObjectsForPages = new Dictionary<int, List<GameObject>>
                    {
                        { 0, toSort }
                    };
            }
            else
            {
                List<AscensionIconInteractable> icons = new(transition.screenInteractables.FindAll((x) => x.GetComponent<AscensionIconInteractable>() != null).ConvertAll((x) =>
                    x.GetComponent<AscensionIconInteractable>()));
                pageLength = icons.Count;
                List<GameObject> toSort = icons.ConvertAll((x) => x.gameObject);
                challengeObjectsForPages = new Dictionary<int, List<GameObject>>
                    {
                        { 0, toSort }
                    };
            }
        }
        this.screen = screen;
        if (screen == null)
        {
            this.transition = transition;
        }
        initialized = true;
        OnEnable();
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
                go2.transform.parent = go.transform.parent;
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
        challengeObjectsForPages.Add(challengeObjectsForPages.Count, obj);
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
        if (challengeObjectsForPages.ContainsKey(page))
        {
            foreach (KeyValuePair<int, List<GameObject>> kvp in challengeObjectsForPages)
            {
                if (kvp.Key == page)
                {
                    kvp.Value.ForEach((x) => x.SetActive(
                        x.GetComponentInChildren<AscensionIconInteractable>()?.Info == null ||
                        x.GetComponentInChildren<AscensionIconInteractable>().Info.challengeType != AscensionChallenge.FinalBoss ||
                        AscensionUnlockSchedule.ChallengeIsUnlockedForLevel(AscensionChallenge.FinalBoss, AscensionSaveData.Data.challengeLevel)));
                }
                else
                {
                    kvp.Value.ForEach((x) => x.SetActive(false));
                }
            }
        }
        CommandLineTextDisplayer.PlayCommandLineClickSound();
    }

    public void OnEnable()
    {
        if (!initialized)
        {
            return;
        }
        ChallengeManager.SyncChallengeList();
        if(rightArrow)
            Destroy(rightArrow);
        if(leftArrow)
            Destroy(leftArrow);
        foreach(KeyValuePair<int, List<GameObject>> kvp in challengeObjectsForPages)
        {
            if(kvp.Key != 0)
            {
                kvp.Value.ForEach(x => Destroy(x));
            }
        }
        screen?.icons?.RemoveAll(x => x == null);
        List<ChallengeManager.FullChallenge> fcs = ChallengeManager.NewInfos.ToList();
        fcs.Sort((x, x2) => x.SortValue != x2.SortValue ? x.SortValue - x2.SortValue : x2.UnlockLevel - x.UnlockLevel);
        List<AscensionChallengeInfo> challengesToAdd = new(fcs.ConvertAll(x => x.Challenge.Repeat(x.AppearancesInChallengeScreen)).SelectMany(x => x));
        List<AscensionIconInteractable> icons = challengeObjectsForPages[0].ConvertAll(x => x.GetComponent<AscensionIconInteractable>()).FindAll(x => x != null);
        icons.ForEach(delegate (AscensionIconInteractable ic)
        {
            if (ic != null && ic.Info == null && challengesToAdd.Count > 0)
            {
                ic.challengeInfo = challengesToAdd[0];
                ic.AssignInfo(challengesToAdd[0]);
                challengesToAdd.RemoveAt(0);
            }
        });
        List<List<AscensionChallengeInfo>> pagesToAdd = new();
        while (challengesToAdd.Count > 0)
        {
            List<AscensionChallengeInfo> page = new();
            for (int i = 0; i < icons.Count; i++)
            {
                if (challengesToAdd.Count > 0)
                {
                    page.Add(challengesToAdd.Last());
                    challengesToAdd.RemoveAt(challengesToAdd.Count - 1);
                }
            }
            pagesToAdd.Add(page);
        }
        if (pagesToAdd.Count > 0)
        {
            foreach (List<AscensionChallengeInfo> page in pagesToAdd)
            {
                AddPage(page);
            }
            Vector3 topRight = new(float.MinValue, float.MinValue);
            Vector3 bottomLeft = new(float.MaxValue, float.MaxValue);
            foreach (AscensionIconInteractable icon in icons)
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
            leftArrow = UnityEngine.Object.Instantiate((screen?.gameObject ?? transform.gameObject).GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageLeftButton.gameObject);
            leftArrow.transform.parent = screen?.transform ?? transform.transform;
            leftArrow.transform.position = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.left / 3f;
            leftArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
            leftArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => PreviousPage();
            rightArrow = UnityEngine.Object.Instantiate((screen?.gameObject ?? transform.gameObject).GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject);
            rightArrow.transform.parent = screen?.transform ?? transform.transform;
            rightArrow.transform.position = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.right / 3f;
            rightArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
            rightArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => NextPage();
        }
    }

    public int pageIndex;
    public Dictionary<int, List<GameObject>> challengeObjectsForPages;
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