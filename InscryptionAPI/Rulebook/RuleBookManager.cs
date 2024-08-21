using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;
namespace InscryptionAPI.RuleBook;


public static class RuleBookManager
{
    public class FullRuleBookRangeInfo
    {
        /// <summary>
        /// The GUID for the mod that added this rulebook section.
        /// </summary>
        public readonly string ModGUID;
        /// <summary>
        /// What PageRangeType this rulebook section will inherit from and use as a template.
        /// </summary>
        /// <remarks>
        /// Ability pages have a small square icon on the left side of the page.
        /// Stat icon pages have a small square icon on the left side of the rulebook name.
        /// Boon pages have a large icon on both sides of the page.
        /// Item pages have a large icon on the left side of the page.
        /// Unique is unused by the game, so should be avoided here unless you plan on using a unique page template.
        /// </remarks>
        public readonly PageRangeType PageTypeTemplate;

        /// <summary>
        /// The beginning section of the header, typically containing an appendix number and subsection number.
        /// An example of what HeaderPrefix should be: "APPENDIX XII, SUBSECTION X".
        /// </summary>
        /// <remarks>
        /// Unless you plan on using a custom appendix number or some other unique header, you should use FillHeaderPrefix .
        /// </remarks>
        public string HeaderPrefix;
        /// <summary>
        /// The subsection name for this rulebook range. Examples include "ABILITIES" and "VARIABLE STATS".
        /// </summary>
        public string SubSectionName;
        /// <summary>
        /// The full text of the header, combining the prefix and subsection name.
        /// </summary>
        public string FullHeaderText => HeaderPrefix + " - " + SubSectionName + " {0}";
        /// <summary>
        /// In the event two modded rulebook ranges are inheriting from the same PageRangeInfo, this is used to determine which run is created first from highest to lowest priority.
        /// If the values are the same, they are created in order the mods were loaded.
        /// </summary>
        public int SortingPriority;
        /// <summary>
        /// The function used to determine what the starting page number is; this number is added to the header.
        /// </summary>
        public readonly Func<List<RuleBookPageInfo>, int> GetStartingNumberFunc;
        /// <summary>
        /// The function used to determine what index in the rulebook to begin inserting this rulebook range.
        /// </summary>
        public readonly Func<RuleBookInfo, List<RuleBookPageInfo>, int> GetInsertPositionFunc;
        /// <summary>
        /// The function for filling out each RuleBookPageInfo.
        /// </summary>
        public readonly Func<AbilityMetaCategory, List<RuleBookPageInfo>> FillPagesFunc;

        public int GetStartingNumber(List<RuleBookPageInfo> pages) => GetStartingNumberFunc(pages);
        public int GetInsertPosition(RuleBookInfo infoInstance, List<RuleBookPageInfo> pages) => GetInsertPositionFunc(infoInstance, pages);
        public List<RuleBookPageInfo> FillPages(AbilityMetaCategory metaCategory) => FillPagesFunc(metaCategory);


        public FullRuleBookRangeInfo(
            string modGuid, PageRangeType type, string headerPrefix, string subsectionName, int sortingPriority,
            Func<List<RuleBookPageInfo>, int> getStartingNumberFunc,
            Func<RuleBookInfo, List<RuleBookPageInfo>, int> getInsertPositionFunc,
            Func<AbilityMetaCategory, List<RuleBookPageInfo>> fillPagesFunc)
        {
            this.ModGUID = modGuid;
            this.PageTypeTemplate = type;
            this.HeaderPrefix = headerPrefix;
            this.SubSectionName = subsectionName;
            this.SortingPriority = sortingPriority;
            this.GetStartingNumberFunc = getStartingNumberFunc;
            this.GetInsertPositionFunc = getInsertPositionFunc;
            this.FillPagesFunc = fillPagesFunc;
        }

        public FullRuleBookRangeInfo Clone()
        {
            return new(
                this.ModGUID, this.PageTypeTemplate,
                this.HeaderPrefix, this.SubSectionName, this.SortingPriority,
                this.GetStartingNumberFunc, this.GetInsertPositionFunc,
                this.FillPagesFunc
                );
        }
    }

    internal readonly static ObservableCollection<FullRuleBookRangeInfo> NewRuleBookInfos;

    public static List<FullRuleBookRangeInfo> AllRuleBookInfos { get; private set; }
    public static event Func<List<FullRuleBookRangeInfo>, List<FullRuleBookRangeInfo>> ModifyRuleBookInfos;
    
    static RuleBookManager()
    {
        NewRuleBookInfos.CollectionChanged += static (_, _) =>
        {
            SyncRuleBookList();
        };
    }

    public static void SyncRuleBookList()
    {
        AllRuleBookInfos = NewRuleBookInfos.Select(x => x.Clone()).ToList();
        AllRuleBookInfos = ModifyRuleBookInfos?.Invoke(AllRuleBookInfos) ?? AllRuleBookInfos;
        AllRuleBookInfos.Sort((FullRuleBookRangeInfo a, FullRuleBookRangeInfo b) => b.SortingPriority - a.SortingPriority);
    }

    /// <summary>
    /// Simplified form of New with only the necessary parameters.
    /// </summary>
    /// <param name="modGuid">The GUID of the mod adding this FullRuleBookRangeInfo</param>
    /// <param name="pageType"></param>
    /// <param name="subsectionName"></param>
    /// <param name="getInsertPositionFunc"></param>
    /// <param name="fillPagesFunc"></param>
    /// <returns>A FullRuleBookRangeInfo with a sorting priority of 0 and a starting page number of 1.</returns>
    public static FullRuleBookRangeInfo New(
    string modGuid, PageRangeType pageType, string subsectionName,
    Func<RuleBookInfo, List<RuleBookPageInfo>, int> getInsertPositionFunc,
    Func<AbilityMetaCategory, List<RuleBookPageInfo>> fillPagesFunc)
    {
        return New(modGuid, pageType, HeaderPrefixSimple("N"), subsectionName, 0, (x) => 1, getInsertPositionFunc, fillPagesFunc);
    }

    /// <summary>
    /// Creates a FullRuleBookRangeInfo representing a custom subsection of the rulebook.
    /// </summary>
    /// <param name="modGuid">The GUID of the mod calling this method.</param>
    /// <param name="pageType"></param>
    /// <param name="headerPrefix"></param>
    /// <param name="subsectionName"></param>
    /// <param name="sortingPriority"></param>
    /// <param name="getStartingNumberFunc"></param>
    /// <param name="getInsertPositionFunc"></param>
    /// <param name="fillPagesFunc"></param>
    /// <returns></returns>
    public static FullRuleBookRangeInfo New(
        string modGuid, PageRangeType pageType, string headerPrefix, string subsectionName,
        int sortingPriority,
        Func<List<RuleBookPageInfo>, int> getStartingNumberFunc,
        Func<RuleBookInfo, List<RuleBookPageInfo>, int> getInsertPositionFunc,
        Func<AbilityMetaCategory, List<RuleBookPageInfo>> fillPagesFunc)
    {
        
        FullRuleBookRangeInfo info = new(
            modGuid, pageType,
            headerPrefix, subsectionName,
            sortingPriority,
            getStartingNumberFunc,
            getInsertPositionFunc, fillPagesFunc);
        NewRuleBookInfos.Add(info);
        return info;
    }

    public static string HeaderPrefixSimple(string romanNumeral) => string.Format(DEFAULT_HEADER_PREFIX, romanNumeral);
    public static FullRuleBookRangeInfo SetHeaderPrefixSimple(this FullRuleBookRangeInfo info, string romanNumeral)
    {
        info.HeaderPrefix = HeaderPrefixSimple(romanNumeral);
        return info;
    }
    public static FullRuleBookRangeInfo SetSortingPriority(this FullRuleBookRangeInfo info, int priority)
    {
        info.SortingPriority = priority;
        return info;
    }

    public static string ParseCustomRuleBookId(string id)
    {
        return id.Replace("API_", "");
    }
    public const string DEFAULT_HEADER_PREFIX = "APPENDIX XII, SUBSECTION {0}";
}