## Adding Custom Sections
In some rare cases, you may want to add a whole new section of pages to the rulebook.
This can get pretty technical, so an example has been provided below on how best to set up your new rulebook section.

Once everything is set up, call RuleBookManager.New() with the required arguments and you're good to go.
```c#
RuleBookManager.New(
    MyPluginGuid, // a unique identifier for your mod
    PageRangeType.Boons, // the PageRangeType we want to inherit our page style from
    "Mod Tribes", // the subsection name that appears at the end of the header
    GetInsertPosition, // a Function that determines where in the Rulebook to insert our custom section
    CreatePages, // the Function to create the pages that will be in our custom section
    headerPrefix: null, // optional argument, if left null one will be created for you
    getStartingNumberFunc: GetStartingNumber, // optional argument, if left null the starting number will be 1
    fillPageAction: FillPage // also optional, but if you want to display custom names, descriptions, etc you will need to set this
    );
```

This will return a FullRuleBookRangeInfo object representing your custom section if you want to modify it further.

Note you can pass in STATIC methods as Func and Action arguments when creating your custom rulebook range.

```c#
// ---IMPORTANT---
// the return value and parameters MUST match the parameters and return value of the Func

// what part of the rulebook this section should be inserted into
// since we're using the BoonPage style, this will insert our pages AFTER the Boons section of the rulebook
private static int GetInsertPosition(PageRangeInfo pageRangeInfo, List<RuleBookPageInfo> pages)
{
    return pages.FindLastIndex(rbi => rbi.pagePrefab == pageRangeInfo.rangePrefab) + 1;
}

private static List<RuleBookPageInfo> CreatePages(RuleBookInfo instance, PageRangeInfo currentRange, AbilityMetaCategory metaCategory)
{
    // in this example, we're adding a rulebook section for custom Tribes
    // foreach custom tribe that exists, we create a rulebook page then set the pageId to the tribe enum so we can use it later
    List<TribeManager.TribeInfo> allTribes = TribeManager.NewTribes.ToList();
    List<RuleBookPageInfo> retval = new();
    foreach (var tribe in allTribes)
    {
        RuleBookPageInfo page = new();
        page.pageId = tribe.tribe.ToString();
        retval.Add(page);
    }
    return retval;
}

private static int GetStartingNumber(List<RuleBookPageInfo> addedPages)
{
    return (int)Tribe.NUM_TRIBES; // since we're doing mod Tribes only, our page numbers will start at 7.
}

private static void FillPage(RuleBookPage page, string pageId, object[] otherArgs)
{
    // in order to modify the page description you MUST convert it to the intended page type as shown
    if (page is BoonPage boonPage && int.TryParse(pageId, out int id))
    {
        TribeManager.TribeInfo tribe = TribeManager.NewTribes.FirstOrDefault(x => x.tribe == (Tribe)id);
        if (tribe != null)
        {
            // We use the internal name of each tribe as our page title
            // we also replace all instances of the word 'tribe' in the name if they exist so it reads more naturally
            boonPage.nameTextMesh.text = Localization.Translate(tribe.name.ToLowerInvariant().Replace("tribe", ""));
            
            // if we wanted our pages to describe each Tribe, we would set those descriptions here
            boonPage.descriptionTextMesh.text = "";

            // boon pages have 2 renderers (one on the left and one on the right side of the page)
            // in this example, we want them to both have the same texture
            boonPage.iconRenderer.material.mainTexture = boonPage.iconRenderer2.material.mainTexture = tribe.icon.texture;
        }
    }
}
```

## Useful Information on RuleBookPages
When filling the content of your custom pages, it's important to understand the page style and what fields you'll have access to.

When your custom section's FillPage() method is called, only certain parts of the current RuleBookPageInfo will be provided to you.
If you plan on using information beyond the pageId when filling your pages, you need to know the order of objects in the array so you can access them correctly.

### AbilityPage
Ability icon texture is placed on the left of the page

otherArgs: headerText, ability, fillerAbilityIds

Class Fields:
- AbilityPageContent *mainAbilityGroup*

### StatIconPage
Stat Icon texture is placed on the left of the icon's name

otherArgs: headerText

Class Fields:
- Renderer *iconRenderer*
- TextMeshPro *nameTextMesh*
- TextMeshPro *descriptionTextMesh*

### BoonPage
Boon textures are placed on the left and right of the page (right icon is flipped horizontally)

otherArgs: headerText, boon

Class Fields:
- Renderer *iconRenderer*
- Renderer *iconRenderer2*
- TextMeshPro *nameTextMesh*
- TextMeshPro *descriptionTextMesh*

### ItemPage
Item's rulebookSprite is placed on the left of the page

otherArgs: headerText

Class Fields:
- Renderer *iconRenderer*
- TextMeshPro *nameTextMesh*
- TextMeshPro *descriptionTextMesh*
- Transform *itemModelParent*
- GameObject *itemModel*