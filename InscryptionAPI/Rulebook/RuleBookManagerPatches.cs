using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items;
using InscryptionAPI.Slots;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;

using static InscryptionAPI.RuleBook.RuleBookManager;

namespace InscryptionAPI.RuleBook;

[HarmonyPatch]
public class RuleBookManagerPatches
{
    public const string API_ID = "[API_";

    [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] { typeof(AbilityMetaCategory) })]
    [HarmonyPostfix, HarmonyPriority(Priority.Low)]
    private static void AddCustomRuleBookSections(AbilityMetaCategory metaCategory, RuleBookInfo __instance, ref List<RuleBookPageInfo> __result)
    {
        if (AllRuleBookInfos.Count == 0)
            return;

        foreach (PageRangeInfo pageRangeInfo in __instance.pageRanges)
        {
            List<FullRuleBookRangeInfo> rulebooksToCreate = AllRuleBookInfos.FindAll(x => x.PageTypeTemplate == pageRangeInfo.type);
            if (rulebooksToCreate.Count == 0)
                continue;

            foreach (FullRuleBookRangeInfo rulebook in rulebooksToCreate)
            {
                int startingPageNum = rulebook.GetStartingNumber(__result);
                int insertPosition = rulebook.GetInsertPosition(pageRangeInfo, __result);

                // collect the list of RuleBookPageInfos to insert into the rulebook
                List<RuleBookPageInfo> newPages = rulebook.CreatePages(__instance, pageRangeInfo, metaCategory);
                foreach (RuleBookPageInfo page in newPages)
                {
                    if (!page.pageId.StartsWith(API_ID))
                        page.pageId = string.Format("[API_{0}]", rulebook.SubSectionName) + page.pageId; // eg: [API_Tribes]Bird

                    page.pagePrefab = pageRangeInfo.rangePrefab;
                    page.headerText = string.Format(Localization.Translate(rulebook.FullHeaderText), startingPageNum);
                    __result.Insert(insertPosition, page);
                    startingPageNum++;
                    insertPosition++;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData", new Type[] { typeof(AbilityMetaCategory) })]
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    private static void SyncRuleBookRedirectsForEachPage(List<RuleBookPageInfo> __result)
    {
        ConstructedPagesWithRedirects.Clear();
        foreach (RuleBookPageInfo info in __result)
        {
            Dictionary<string, RedirectInfo> redirectInfos = null;
            RuleBookPageInfoExt ext = CustomRedirectPages.Find(x => x.parentPageInfo == info);
            if (ext != null)
            {
                redirectInfos = ext.RulebookDescriptionRedirects;
            }
            else if (info.ability > 0)
            {
                AbilityManager.FullAbility full = AbilityManager.AllAbilities.AbilityByID(info.ability);
                redirectInfos = full?.RulebookDescriptionRedirects;
            }
            else if (info.boon > 0)
            {
                BoonManager.FullBoon boon = BoonManager.AllFullBoons.Find(x => x.boon.type == info.boon);
                redirectInfos = boon?.RulebookDescriptionRedirects;
            }
            else if (!string.IsNullOrEmpty(info.pageId) && !info.pageId.StartsWith(API_ID))
            {
                // custom rulebook pages can have their redirects created during initialisation, this only handles vanilla logic
                string pureId = GetUnformattedPageId(info.pageId);
                StatIconManager.FullStatIcon icon = StatIconManager.AllStatIcons.Find(x => x.Id.ToString() == pureId);
                if (icon != null)
                {
                    redirectInfos = icon.RulebookDescriptionRedirects;
                }
                else if (info.pageId.StartsWith(SlotModificationManager.SLOT_PAGEID))
                {
                    string slotId = info.pageId.Replace(SlotModificationManager.SLOT_PAGEID, "");
                    SlotModificationManager.Info slot = SlotModificationManager.AllModificationInfos.Find(x => x.ModificationType.ToString() == slotId);
                    redirectInfos = slot?.RulebookDescriptionRedirects;
                }
                else
                {
                    ConsumableItemManager.FullConsumableItemData fullItem = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData.name == pureId);
                    redirectInfos = fullItem?.RulebookDescriptionRedirects;
                }
            }

            if (redirectInfos != null && redirectInfos.Count > 0)
            {
                AddRedirectPage(info, new(redirectInfos));
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbilityPage), "FillPage")]
    [HarmonyPatch(typeof(StatIconPage), "FillPage")]
    [HarmonyPatch(typeof(BoonPage), "FillPage")]
    [HarmonyPatch(typeof(ItemPage), "FillPage")]
    private static bool FixFillPage(RuleBookPage __instance, string headerText, params object[] otherArgs)
    {
        if (otherArgs?.Length > 0 && otherArgs.LastOrDefault() is string pageId && pageId.StartsWith(API_ID))
        {
            string sectionId = pageId.Replace(API_ID, "");
            FullRuleBookRangeInfo fullInfo = AllRuleBookInfos.Find(x => sectionId.StartsWith(x.SubSectionName));
            if (fullInfo?.FillPageAction != null)
            {
                if (__instance.headerTextMesh != null)
                    __instance.headerTextMesh.text = headerText;

                List<object> obj = otherArgs.ToList();
                obj.PopLast();

                fullInfo.FillRuleBookPage(__instance, GetUnformattedPageId(pageId), obj.ToArray());
                return false;
            }
        }
        return true;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(PageContentLoader), nameof(PageContentLoader.LoadPage))]
    private static IEnumerable<CodeInstruction> LoadCustomPage(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        int startIndex = codes.IndexOf(codes.Find(x => x.opcode == OpCodes.Stloc_0));
        if (startIndex != -1)
        {
            startIndex += 2;
            codes.RemoveRange(startIndex, codes.Count - startIndex - 1);
            MethodInfo method = AccessTools.Method(typeof(RuleBookManagerPatches), nameof(RuleBookManagerPatches.FillPage), new Type[] { typeof(RuleBookPage), typeof(RuleBookPageInfo) });
            codes.Insert(startIndex++, new(OpCodes.Ldarg_1));
            codes.Insert(startIndex++, new(OpCodes.Call, method));
        }
        return codes;
    }

    /// <summary>
    /// Expanded version of FillPage that accounts for custom rulebook sections.
    /// </summary>
    public static void FillPage(RuleBookPage page, RuleBookPageInfo pageInfo)
    {
        TextMeshPro descriptionTextMesh = null;
        if (page is AbilityPage ab)
        {
            ab.FillPage(pageInfo.headerText, pageInfo.ability, pageInfo.fillerAbilityIds, pageInfo.pageId);
            descriptionTextMesh = ab.mainAbilityGroup.descriptionTextMesh;
        }
        else if (page is BoonPage bn)
        {
            bn.FillPage(pageInfo.headerText, pageInfo.boon, pageInfo.pageId);
            descriptionTextMesh = bn.descriptionTextMesh;
        }
        else
        {
            page.FillPage(pageInfo.headerText, pageInfo.pageId);
            if (page is ItemPage im)
            {
                descriptionTextMesh = im.descriptionTextMesh;
            }
            else if (page is StatIconPage si)
            {
                descriptionTextMesh = si.descriptionTextMesh;
            }
        }

        RuleBookPageInfoExt ext = ConstructedPagesWithRedirects.Find(x => x.parentPageInfo == pageInfo);
        if (ext != null && descriptionTextMesh != null)
        {
            descriptionTextMesh.text = ParseRedirectTextColours(ext.RulebookDescriptionRedirects, descriptionTextMesh.text);
        }
    }
}