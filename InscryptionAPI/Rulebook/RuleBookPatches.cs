using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static InscryptionAPI.RuleBook.RuleBookManager;
using static InscryptionAPI.Slots.SlotModificationManager;

namespace InscryptionAPI.RuleBook;


public class RuleBookPatches
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
            if (rulebooksToCreate.Count == 0)
                continue;

            foreach (FullRuleBookRangeInfo rulebook in rulebooksToCreate)
            {
                int startingPageNum = rulebook.GetStartingNumber(__result);
                int insertPosition = rulebook.GetInsertPosition(__instance, __result);

                List<RuleBookPageInfo> newPages = rulebook.FillPages(metaCategory);
                foreach (RuleBookPageInfo page in newPages)
                {
                    if (!page.pageId.StartsWith("API_"))
                        page.pageId = "API_" + page.pageId;
                    
                    page.pagePrefab = pageRangeInfo.rangePrefab;
                    page.headerText = string.Format(Localization.Translate(rulebook.FullHeaderText), startingPageNum);
                    __result.Insert(insertPosition, page);
                    startingPageNum++;
                    insertPosition++;
                }
            }
        }
    }
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(PageContentLoader), nameof(PageContentLoader.LoadPage))]
    private static void LoadCustomPage()
    {
        // This inner transpiler will be applied to the original and
        // the result will replace this method
        //
        // That will allow this method to have a different signature
        // than the original and it must match the transpiled result
        //
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);
            int index = codes.FindIndex(0, x => x.opcode == OpCodes.Stloc_0) + 1;
            codes.RemoveRange(index, codes.Count - index);
            var method = SymbolExtensions.GetMethodInfo(() => RuleBookPatches.FillPage(null, null));
            codes.Insert(index++, new(OpCodes.Ldloc_0));
            codes.Insert(index++, new(OpCodes.Ldarg_1));
            codes.Insert(index++, new(OpCodes.Call, method));
            return codes.AsEnumerable();
        }

        // make compiler happy
        _ = Transpiler(null);
    }

    public static void FillPage(RuleBookPage page, RuleBookPageInfo pageInfo)
    {
        if (page is AbilityPage)
        {
            page.FillPage(pageInfo.headerText, pageInfo.ability, pageInfo.fillerAbilityIds, pageInfo.pageId);
        }
        else if (page is StatIconPage)
        {
            page.FillPage(pageInfo.headerText, pageInfo.pageId);
        } else if (page is BoonPage)
        {
            page.FillPage(pageInfo.headerText, pageInfo.boon, pageInfo.pageId);
        }
        else if (page is ItemPage)
        {
            page.FillPage(pageInfo.headerText, pageInfo.pageId);
        }
        else
        {

        }
    }
}