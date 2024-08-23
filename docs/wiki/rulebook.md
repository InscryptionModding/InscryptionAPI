## The Rulebook
The Rulebook is a powerful tool for the player during the 3D Acts.
Every ability, stat icon, boon and item that they can encounter during a certain Act will have an entry in it, providing a name and description of what it does.

These entries are act-specific, and custom abilities and the like can be marked to only appear in a certain Act or Acts* using their specific AbilityMetaCategory field.

In most cases, this is the end of a modder's consideration Rulebook. If you're making a mod for Act 3, custom sigils should appear in the Act 3 Rulebook and nowhere else.
If you don't want a stat icon to appear in any Rulebook for one reason or another, you simply leave its AbilityMetaCategories field empty.
Custom content that appears in the Rulebook will appear in the order they were loaded into the game by the API, content that doesn't won't.

However, there may arise cases where this is insufficient for your needs.
Maybe you want an ability to always appear at the beginning of the Rulebook for some reason, or you want to add a whole new section to the Rulebook.
The former case can be easily handled with a simple patch to RuleBookInfo.ConstructPageData, but the latter can get fairly complicated.
In either case, you can use the API's RuleBookManager to make this process simpler.

\* *Items and Boons can only be marked to appear in a single Act.*

## Adding Custom Sections
Creating a custom rulebook section/range is a bit different than creating other things like abilities.
Custom sections don't require a Type reference to a custom class, though it's still good practice to create a class to keep things organised.

Simple call RuleBookManager.New() with the needed arguments and you're good to go.

```c#
RuleBookManager.New(
    MyPluginGuid, // a unique identifier for your mod
    PageRangeType.Boons, // the PageRangeType we want to inherit our page style from
    "Mod Tribes", // the subsection name that appears at the end of the header
    GetInsertPosition, // a function that determines where in the Rulebook to insert our custom section
    CreatePages, // the function to create the pages that will be in our custom section
    headerPrefix, // optional argument, if left null one will be created for you
    getStartingNumberFunc: GetStartingNumber, // optional argument, if left null the starting number will be 1
    fillPageAction: FillPage // also optional, but if you want to display custom names, descriptions, etc you will need to set this
    );
```

This will return a FullRuleBookRangeInfo object representing your custom section.

Note how you can pass in STATIC methods as arguments when creating your custom rulebook range.


```c#
// note that the return value and parameters MUST match the parameters and return value of the Func
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
    return (int)Tribe.NUM_TRIBES; // since we're doing mod Tribes only, we start after custom Tribes (pretend we have a separate section for those)
}

private static void FillPage(RuleBookPage page, string pageId, object[] otherArgs)
{
    if (page is BoonPage boonPage && int.TryParse(pageId, out int id))
    {
        TribeManager.TribeInfo tribe = TribeManager.NewTribes.FirstOrDefault(x => x.tribe == (Tribe)id);
        if (tribe != null)
        {
            boonPage.nameTextMesh.text = Localization.Translate(tribe.name.ToLowerInvariant().Replace("tribe", ""));
            boonPage.descriptionTextMesh.text = "";
            boonPage.iconRenderer.material.mainTexture = boonPage.iconRenderer2.material.mainTexture = tribe.icon.texture;
        }
    }
}
```

## Useful Information on RuleBookPages
When filling the content of your custom pages, it's important to understand the page style and what fields you'll have access to.

When your custom section's FillPage() method is called, only certain parts of the current RuleBookPageInfo will be provided to you.
If you plan on using information beyond the pageId when filling your pages, you need to know the order of objects in the array.

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
Boon texture is placed on the left and right of the page

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