using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;
using static InscryptionAPI.RuleBook.RuleBookManager;

namespace InscryptionAPI.RuleBook;

[HarmonyPatch]
public class RuleBookManagerPatches
{
    [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] { typeof(AbilityMetaCategory) })]
    [HarmonyPostfix, HarmonyPriority(-100)]
    private static void AddCustomRuleBookSections(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
    {
        if (AllRuleBookInfos.Count == 0)
            return;

        foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
        {
            List<FullRuleBookRangeInfo> rulebooksToCreate = AllRuleBookInfos.FindAll(x => x.PageTypeTemplate == pageRangeInfo.type);
            //InscryptionAPIPlugin.Logger.LogDebug($"New infos for {pageRangeInfo.type}: {rulebooksToCreate.Count}");
            if (rulebooksToCreate.Count == 0)
                continue;

            foreach (FullRuleBookRangeInfo rulebook in rulebooksToCreate)
            {
                //InscryptionAPIPlugin.Logger.LogDebug($"Create new rulebook subsection: {rulebook.SubSectionName}");
                int startingPageNum = rulebook.GetStartingNumber(__result);
                int insertPosition = rulebook.GetInsertPosition(pageRangeInfo, __result);

                // collect the list of RuleBookPageInfos to insert into the rulebook
                List<RuleBookPageInfo> newPages = rulebook.CreatePages(__instance, pageRangeInfo, metaCategory);
                foreach (RuleBookPageInfo page in newPages)
                {
                    if (!page.pageId.StartsWith(API_ID))
                        page.pageId = API_ID + $"{rulebook.SubSectionName}_" + page.pageId;
                    
                    page.pagePrefab = pageRangeInfo.rangePrefab;
                    page.headerText = string.Format(Localization.Translate(rulebook.FullHeaderText), startingPageNum);
                    __result.Insert(insertPosition, page);
                    startingPageNum++;
                    insertPosition++;
                }
            }
        }
    }

    public static void FillPage(RuleBookPage page, RuleBookPageInfo pageInfo)
    {
        if (page is AbilityPage)
        {
            page.FillPage(pageInfo.headerText, pageInfo.ability, pageInfo.fillerAbilityIds, pageInfo.pageId);
        }
        else if (page is BoonPage)
        {
            page.FillPage(pageInfo.headerText, pageInfo.boon, pageInfo.pageId);
        }
        else
        {
            page.FillPage(pageInfo.headerText, pageInfo.pageId);
        }
    }

    private const string API_ID = "API_";

    [HarmonyPrefix, HarmonyPatch(typeof(PageContentLoader), nameof(PageContentLoader.LoadPage))]
    private static bool LoadCustomPage(PageContentLoader __instance, RuleBookPageInfo pageInfo)
    {
        if (__instance.currentPagePrefab != pageInfo.pagePrefab)
        {
            if (__instance.currentPageObj != null)
                UnityObject.Destroy(__instance.currentPageObj);

            __instance.currentPageObj = UnityObject.Instantiate(pageInfo.pagePrefab, __instance.transform.position, __instance.transform.rotation, __instance.transform);
        }
        __instance.currentPagePrefab = pageInfo.pagePrefab;
        foreach (GameObject currentAdditiveObject in __instance.currentAdditiveObjects)
        {
            UnityObject.Destroy(currentAdditiveObject);
        }
        __instance.currentAdditiveObjects.Clear();
        foreach (GameObject additivePrefab in pageInfo.additivePrefabs)
        {
            if (additivePrefab != null)
            {
                GameObject gameObject = UnityObject.Instantiate(additivePrefab, __instance.transform.position, __instance.transform.rotation, __instance.transform);
                gameObject.SetActive(value: true);
                __instance.currentAdditiveObjects.Add(gameObject);
            }
        }
        RuleBookPage component = __instance.currentPageObj.GetComponent<RuleBookPage>();
        FillPage(component, pageInfo);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbilityPage), "FillPage")]
    [HarmonyPatch(typeof(StatIconPage), "FillPage")]
    [HarmonyPatch(typeof(BoonPage), "FillPage")]
    [HarmonyPatch(typeof(ItemPage), "FillPage")]
    private static bool FixFillAbilityPage(RuleBookPage __instance, string headerText, params object[] otherArgs)
    {
        if (otherArgs != null && otherArgs.Last() is string pageId && pageId.StartsWith(API_ID))
        {
            pageId = pageId.Replace(API_ID, "");
            FullRuleBookRangeInfo fullInfo = AllRuleBookInfos.Find(x => pageId.StartsWith(x.SubSectionName));
            if (fullInfo != null && fullInfo.FillPageAction != null)
            {
                if (__instance.headerTextMesh != null)
                    __instance.headerTextMesh.text = headerText;

                List<object> obj = otherArgs.ToList();
                obj.PopLast();

                fullInfo.FillRuleBookPage(__instance, pageId.Replace(fullInfo.SubSectionName + "_", ""), obj.ToArray());
                return false;
            }
        }
        return true;
    }
}