using DiskCardGame;
using InscryptionAPI.Items.Extensions;
using System.Collections.ObjectModel;
using UnityEngine;
using static InscryptionAPI.Boons.BoonManager;
using static InscryptionAPI.Slots.SlotModificationManager;

namespace InscryptionAPI.RuleBook;

/// <summary>
/// Manager class for adding hyperlinks to rulebook entries that redirect to other rulebook pages.
/// </summary>
public static class RuleBookManager
{
    /// <summary>
    /// Utility class containing all the data needed to define redirects.
    /// </summary>
    public class RedirectInfo
    {
        /// <summary>
        /// The page type and rulebook section this hyperlink will redirect to.
        /// </summary>
        /// <remarks>
        /// If the hyperlink redirects to a custom section, use <see cref="PageRangeType.Unique"/>.
        /// </remarks>
        public PageRangeType redirectType;
        /// <summary>
        /// What colour hyperlinked text should be to indicate that it's interactable.
        /// </summary>
        public Color redirectTextColour;
        /// <summary>
        /// The ID of the page the hyperlink redirects to.
        /// Eg., for ability redirects this will be the ability's id as a string.
        /// </summary>
        public string redirectPageId;

        /// <summary>
        /// Creates a new instance of <see cref="RedirectInfo"/>.
        /// </summary>
        /// <param name="redirectType"></param>
        /// <param name="redirectTextColour"></param>
        /// <param name="redirectPageId"></param>
        public RedirectInfo(PageRangeType redirectType, Color redirectTextColour, string redirectPageId)
        {
            this.redirectType = redirectType;
            this.redirectTextColour = redirectTextColour;
            this.redirectPageId = redirectPageId;
        }
    }
    /// <summary>
    /// A custom class that associates a <see cref="RuleBookPageInfo"/> object with a Dictionary of <see cref="RedirectInfo"/>s. Use if 
    /// </summary>
    public class RuleBookPageInfoExt
    {
        /// <summary>
        /// The rulebook page that the redirects will be mapped to.
        /// </summary>
        public RuleBookPageInfo parentPageInfo;

        /// <summary>
        /// Tracks all rulebook redirects that this custom rulebook page's description will have.
        /// </summary>
        public Dictionary<string, RedirectInfo> RulebookDescriptionRedirects = new();

        /// <summary>
        /// Creates a new instance of <see cref="RuleBookPageInfoExt"/>.
        /// </summary>
        /// <param name="parentPageInfo">The <see cref="RuleBookPageInfo"/> that this instance will use as a prent reference.</param>
        /// <param name="redirects">All redirects for this rulebook page, mapped to the text that will be hyperlinked.</param>
        public RuleBookPageInfoExt(RuleBookPageInfo parentPageInfo, Dictionary<string, RedirectInfo> redirects)
        {
            this.parentPageInfo = parentPageInfo;
            this.RulebookDescriptionRedirects = redirects;
        }
    }

    /// <summary>
    /// All currently processed pages with redirects.
    /// </summary>
    public static readonly List<RuleBookPageInfoExt> ConstructedPagesWithRedirects = new();
    /// <summary>
    /// All currently processed custom pages with redirects.
    /// </summary>
    public static readonly List<RuleBookPageInfoExt> CustomRedirectPages = new();

    internal static void AddRedirectPage(RuleBookPageInfo pageInfo, Dictionary<string, RedirectInfo> redirects)
    {
        ConstructedPagesWithRedirects.Add(new(pageInfo, redirects));
    }

    /// <summary>
    /// Utility class containing all the data needed for a custom rulebook section.
    /// </summary>
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
        /// The function used to determine what the starting page number is; this number is added to the header.
        /// </summary>
        public Func<List<RuleBookPageInfo>, int> GetStartingNumberFunc;
        /// <summary>
        /// The function used to determine what index in the rulebook to begin inserting this rulebook range.
        /// </summary>
        public Func<PageRangeInfo, List<RuleBookPageInfo>, int> GetInsertPositionFunc;
        /// <summary>
        /// The function for filling out each RuleBookPageInfo.
        /// </summary>
        public Func<RuleBookInfo, PageRangeInfo, AbilityMetaCategory, List<RuleBookPageInfo>> CreatePagesFunc;

        /// <summary>
        /// Action that implements logic for constructing pages in this rulebook section.
        /// </summary>
        public Action<RuleBookPage, string, object[]> FillPageAction;

        /// <summary>
        /// The starting page number of this section in its header.
        /// </summary>
        /// <param name="pages"></param>
        /// <returns></returns>
        public int GetStartingNumber(List<RuleBookPageInfo> pages) => GetStartingNumberFunc(pages);
        /// <summary>
        /// Where in the rulebook this section should appear.
        /// </summary>
        /// <param name="currentPageRange">An existing rulebook section to use as a positional reference.</param>
        /// <param name="pages">All pages being added to this section.</param>
        /// <returns>The index of where in the rulebook this section is being inserted.</returns>
        public int GetInsertPosition(PageRangeInfo currentPageRange, List<RuleBookPageInfo> pages) => GetInsertPositionFunc(currentPageRange, pages);
        /// <summary>
        /// Method for creating the rulebook page objects used in this section.
        /// </summary>
        /// <param name="instance">The <see cref="RuleBookInfo"/> instance.</param>
        /// <param name="currentPageRange">The section object that will be used as a template when creating pages.</param>
        /// <param name="metaCategory">The metacategory indicating what game act this rulebook is for (Act 1, Act 3, Grimora, Magnificus).</param>
        /// <returns></returns>
        public List<RuleBookPageInfo> CreatePages(RuleBookInfo instance, PageRangeInfo currentPageRange, AbilityMetaCategory metaCategory) => CreatePagesFunc(instance, currentPageRange, metaCategory);
        /// <summary>
        /// Method for filling in information on each rulebook page in this section.
        /// </summary>
        /// <param name="instance">The current page being filled.</param>
        /// <param name="pageId">The ID of the page being filld.</param>
        /// <param name="otherArgs">Various objects used to fill this page. Varies depending on the page template used in <see cref="CreatePages"/></param>
        public void FillRuleBookPage(RuleBookPage instance, string pageId, params object[] otherArgs) => FillPageAction(instance, pageId, otherArgs);

        /// <summary>
        /// Creates an instance of <see cref="FullRuleBookRangeInfo"/>.
        /// </summary>
        /// <param name="modGuid">The GUID of the mod adding this rulebook section.</param>
        /// <param name="type">The rulebook style this section will use.</param>
        /// <param name="headerPrefix">The prefix used at the start of this section's header. If null, uses <see cref="HeaderPrefixSimple(string)"/>"with numeral "I".</param>
        /// <param name="subsectionName">The name of this rulebook section.</param>
        /// <param name="getStartingNumberFunc">The function used to determine the initial number displayed in the page corner. If null, defaults to 1.</param>
        /// <param name="getInsertPositionFunc">The function used to determine the index this section will be inserted into in the rulebook.</param>
        /// <param name="createPagesFunc">The function used to create the pages in this section.</param>
        /// <param name="fillPageAct">The action used to fill each constructed page in this section.</param>
        public FullRuleBookRangeInfo(
            string modGuid, PageRangeType type, string headerPrefix, string subsectionName,
            Func<List<RuleBookPageInfo>, int> getStartingNumberFunc,
            Func<PageRangeInfo, List<RuleBookPageInfo>, int> getInsertPositionFunc,
            Func<RuleBookInfo, PageRangeInfo, AbilityMetaCategory, List<RuleBookPageInfo>> createPagesFunc,
            Action<RuleBookPage, string, object[]> fillPageAct)
        {
            this.ModGUID = modGuid;
            this.PageTypeTemplate = type;
            this.HeaderPrefix = headerPrefix ?? HeaderPrefixSimple("I");
            this.SubSectionName = subsectionName;
            this.GetStartingNumberFunc = getStartingNumberFunc ?? ((List<RuleBookPageInfo> x) => 1);
            this.GetInsertPositionFunc = getInsertPositionFunc;
            this.CreatePagesFunc = createPagesFunc;
            this.FillPageAction = fillPageAct;
        }

        /// <summary>
        /// Creates a deep clone of this <see cref="FullRuleBookRangeInfo"/> object.
        /// </summary>
        /// <returns>The newly created deep clone.</returns>
        public FullRuleBookRangeInfo Clone()
        {
            return new(
                this.ModGUID, this.PageTypeTemplate,
                this.HeaderPrefix, this.SubSectionName,
                this.GetStartingNumberFunc, this.GetInsertPositionFunc,
                this.CreatePagesFunc,
                this.FillPageAction
                );
        }
    }

    internal readonly static ObservableCollection<FullRuleBookRangeInfo> NewRuleBookInfos = new();

    /// <summary>
    /// All currently process <see cref="FullRuleBookRangeInfo"/>s, including vanilla and mod-added.
    /// </summary>
    public static List<FullRuleBookRangeInfo> AllRuleBookInfos { get; private set; }

    /// <summary>
    /// A hook for modders to add custom code to modify the rulebook sections list dynamically.
    /// </summary>
    public static event Func<List<FullRuleBookRangeInfo>, List<FullRuleBookRangeInfo>> ModifyRuleBookInfos;

    static RuleBookManager()
    {
        NewRuleBookInfos.CollectionChanged += static (_, _) =>
        {
            SyncRuleBookList();
        };
    }

    /// <summary>
    /// Resynchronizes the custom rulebook sections list.
    /// </summary>
    public static void SyncRuleBookList()
    {
        AllRuleBookInfos = NewRuleBookInfos.Select(x => x.Clone()).ToList();
        AllRuleBookInfos = ModifyRuleBookInfos?.Invoke(AllRuleBookInfos) ?? AllRuleBookInfos;
    }

    /// <summary>
    /// Creates a custom rulebook range. Simplified version of the full New() method that only requires the minimum required parameters.
    /// </summary>
    /// <param name="modGuid">The GUID of the mod adding this <see cref="FullRuleBookRangeInfo"/>.</param>
    /// <param name="pageType">The <see cref="PageRangeType"/> we want to inherit from. This will determine the style of pages.</param>
    /// <param name="subsectionName">The name of the rulebook subsection, eg. "Abilities" or "Boons".</param>
    /// <param name="getInsertPositionFunc">The Func to determine what index in the rulebook to insert a new section.</param>
    /// <param name="createPagesFunc">The Func that is called to create the <see cref="RuleBookPage"/> objects that will be used to fill the rulebook.</param>
    /// <param name="headerPrefix">The first half of the header that will appear on rulebook pages. Leave null to use the default prefix.</param>
    /// <param name="getStartingNumberFunc">The Func to determine the first page number in this range. Leave null to use the default starting number (1).</param>
    /// <param name="fillPageAction">The Action to call when filling in the data for each rulebook page.</param>
    /// <returns>A new <see cref="FullRuleBookRangeInfo"/> containing the information to create a custom rulebook range.</returns>
    public static FullRuleBookRangeInfo New(
    string modGuid,
    PageRangeType pageType,
    string subsectionName,
    Func<PageRangeInfo, List<RuleBookPageInfo>, int> getInsertPositionFunc,
    Func<RuleBookInfo, PageRangeInfo, AbilityMetaCategory, List<RuleBookPageInfo>> createPagesFunc,
    string headerPrefix = null,
    Func<List<RuleBookPageInfo>, int> getStartingNumberFunc = null,
    Action<RuleBookPage, string, object[]> fillPageAction = null)
    {
        FullRuleBookRangeInfo info = new(
            modGuid, pageType,
            headerPrefix, subsectionName,
            getStartingNumberFunc,
            getInsertPositionFunc, createPagesFunc,
            fillPageAction);
        NewRuleBookInfos.Add(info);
        return info;
    }

    /// <summary>
    /// Creates a FullRuleBookRangeInfo representing a custom subsection of the rulebook.
    /// </summary>
    /// <param name="modGuid">The GUID of the mod calling this method.</param>
    /// <param name="pageType">The PageRangeType we want to inherit from. This will determine what page style you'll have access to.</param>
    /// <param name="headerPrefix">The first half of the header that will appear on rulebook pages.</param>
    /// <param name="subsectionName">The name of the rulebook subsection, eg. "Abilities" or "Boons".</param>
    /// <param name="getStartingNumberFunc">The </param>
    /// <param name="getInsertPositionFunc"></param>
    /// <param name="createPagesFunc"></param>
    /// <returns></returns>
    public static FullRuleBookRangeInfo New(
        string modGuid, PageRangeType pageType, string headerPrefix, string subsectionName,
        Func<List<RuleBookPageInfo>, int> getStartingNumberFunc,
        Func<PageRangeInfo, List<RuleBookPageInfo>, int> getInsertPositionFunc,
        Func<RuleBookInfo, PageRangeInfo, AbilityMetaCategory, List<RuleBookPageInfo>> createPagesFunc)
    {

        FullRuleBookRangeInfo info = new(
            modGuid, pageType,
            headerPrefix, subsectionName,
            getStartingNumberFunc,
            getInsertPositionFunc, createPagesFunc,
            null);
        NewRuleBookInfos.Add(info);
        return info;
    }
    /// <summary>
    /// Sets the FillPage Action for the given FullRuleBookRangeInfo.
    /// </summary>
    /// <param name="info">The FullRuleBookRangeInfo we want to modify.</param>
    /// <param name="action"></param>
    /// <returns>The same <see cref="FullRuleBookRangeInfo"/> so a chain can continue.</returns>
    public static FullRuleBookRangeInfo SetFillPage(this FullRuleBookRangeInfo info, Action<RuleBookPage, string, object[]> action)
    {
        info.FillPageAction = action;
        return info;
    }
    /// <summary>
    /// Sets the header prefix for the given <see cref="FullRuleBookRangeInfo"/>.
    /// </summary>
    /// <param name="info">The <see cref="FullRuleBookRangeInfo"/> we want to modify.</param>
    /// <param name="headerPrefix">The string we want our header prefix to become.</param>
    /// <remarks>
    /// If you only want to modify the subsection numeral, use <see cref="SetHeaderPrefixSimple"/> instead.
    /// </remarks>
    /// <returns>The same <see cref="FullRuleBookRangeInfo"/> so a chain can continue.</returns>
    public static FullRuleBookRangeInfo SetHeaderPrefix(this FullRuleBookRangeInfo info, string headerPrefix)
    {
        info.HeaderPrefix = headerPrefix;
        return info;
    }
    /// <summary>
    /// Sets the <see cref="FullRuleBookRangeInfo"/>'s <see cref="FullRuleBookRangeInfo.HeaderPrefix"/> using <see cref="HeaderPrefixSimple"/>.
    /// </summary>
    /// <param name="info">The <see cref="FullRuleBookRangeInfo"/> we want to modify.</param>
    /// <param name="romanNumeral">A string representing a roman numeral, used for the subsection number.</param>
    /// <returns>The same <see cref="FullRuleBookRangeInfo"/> so a chain can continue.</returns>
    public static FullRuleBookRangeInfo SetHeaderPrefixSimple(this FullRuleBookRangeInfo info, string romanNumeral)
    {
        info.HeaderPrefix = HeaderPrefixSimple(romanNumeral);
        return info;
    }
    /// <summary>
    /// Creates a header prefix with the provided string using <see cref="DEFAULT_HEADER_PREFIX"/> as a template.
    /// </summary>
    /// <param name="romanNumeral">A representing a roman numeral for the subsection number.</param>
    /// <returns>A fully formatted header in the format of <c>APPENDIX XII, SUBSECTION {0}</c>, where <c>{0}</c> is <c>romanNumeral</c>.</returns>
    public static string HeaderPrefixSimple(string romanNumeral) => string.Format(DEFAULT_HEADER_PREFIX, romanNumeral);

    private const string DEFAULT_HEADER_PREFIX = "APPENDIX XII, SUBSECTION {0}";

    /// <summary>
    /// Helper string for formatting coloured text in HTML.
    /// </summary>
    public const string HTML_REPLACE_STRING = "<color=#{0}><u>{1}</u></color>";

    /// <summary>
    /// Parses the colours of redirects for the given rulebook description and modifies said description to display coloured text.
    /// </summary>
    /// <param name="dictionary">Dictionary of strings that will be made into hyperlinks that need to be recoloured.</param>
    /// <param name="description">The rulebook description whose text will be recoloured in preparation for hyperlinks being added.</param>
    /// <returns>The modified rulebook description with parsed colours.</returns>
    public static string ParseRedirectTextColours(Dictionary<string, RedirectInfo> dictionary, string description)
    {
        string desc = description;
        if (dictionary.Count > 0)
        {
            foreach (string key in dictionary.Keys)
            {
                RedirectInfo triple = dictionary[key];
                string hexCode = ColorUtility.ToHtmlStringRGB(triple.redirectTextColour);

                desc = desc.Replace(key, string.Format(HTML_REPLACE_STRING, hexCode, key));
            }
        }
        return desc;
    }

    /// <summary>
    /// Returns a RuleBookPageInfo's pageId without the API identifier.
    /// </summary>
    public static string GetUnformattedPageId(string pageId)
    {
        int start = pageId.IndexOf(RuleBookManagerPatches.API_ID);
        if (start != -1)
        {
            int end = pageId.IndexOf(']');
            if (end != -1) return pageId.Remove(start, end - start + 1);
        }

        return pageId;
    }

    /// <summary>
    /// Opens the rulebook to the page with the given <c>pageId</c>.
    /// </summary>
    /// <param name="instance"><see cref="Singleton&lt;RuleBookController>.Instance"/>.</param>
    /// <param name="pageId">The value to compare each rulebook pages' id against. If this starts with <c>[API_</c> then it will check against a page's unformatted id.</param>
    /// <param name="offsetView">Whether to offset the camera view down when opening the rulebook.</param>
    public static void OpenToCustomPage(this RuleBookController instance, string pageId, bool offsetView = false)
    {
        int pageIndex = -1;
        instance.SetShown(shown: true, offsetView);
        if (pageId.StartsWith(RuleBookManagerPatches.API_ID))
            pageIndex = instance.PageData.IndexOf(instance.PageData.Find(x => !string.IsNullOrEmpty(x.pageId) && x.pageId == pageId));
        else
            pageIndex = instance.PageData.IndexOf(instance.PageData.Find(x => !string.IsNullOrEmpty(x.pageId) && GetUnformattedPageId(x.pageId) == pageId));

        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.flipper.FlipToPage(pageIndex, 0.2f));
    }

    /// <summary>
    /// If the given item should be added to the rulebook for the given game act.
    /// </summary>
    /// <param name="item">The <see cref="ConsumableItemData"/> to check.</param>
    /// <param name="metaCategory">The metacategory for the current act.</param>
    /// <returns>True if the item should be added to the rulebook.</returns>
    public static bool ItemShouldBeAdded(ConsumableItemData item, AbilityMetaCategory metaCategory)
    {
        return item.rulebookCategory == metaCategory || item.GetFullConsumableItemData()?.rulebookMetaCategories.Contains(metaCategory) == true;
    }
    /// <summary>
    /// If the given Boon should be added to the rulebook for the given game act.
    /// </summary>
    /// <param name="fullBoon">The Boon to check.</param>
    /// <param name="metaCategory">The metacategory for the current act.</param>
    /// <returns>True if the Boon should be added to the rulebook.</returns>
    public static bool BoonShouldBeAdded(FullBoon fullBoon, AbilityMetaCategory metaCategory)
    {
        return fullBoon?.boon?.icon != null && fullBoon.appearInRulebook && fullBoon.metaCategories.Contains(metaCategory);
    }
    /// <summary>
    /// If the given slot modification <see cref="Info"/> should be added to the rulebook for the given game act.
    /// </summary>
    /// <param name="info">The slot modification to check.</param>
    /// <param name="category">The metacategory for the current act.</param>
    /// <returns>True if the Boon should be added to the rulebook.</returns>
    public static bool SlotModShouldBeAdded(Info info, ModificationMetaCategory category) => info.RulebookName != null && info.MetaCategories.Contains(category);
}