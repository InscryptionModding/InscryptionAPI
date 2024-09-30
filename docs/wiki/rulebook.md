## The Rulebook
The Rulebook is a vital tool for the player while playing outside of Act 2.

Every ability, stat icon, boon, and item that can appear in the current Act will have an entry in the book, providing information on what each one does.
Custom abilities and such will appear in the rulebook if marked to do so in their `metaCategories` field*.

For most situations, this will be the extent of your modding experience with the rulebook; if you want an ability to appear in Act 1, you give it the appropriate AbilityMetaCategoy.
If you don't want a stat icon to appear in the rulebook at all, you leave `metaCategories` empty.

However, there are some instances where you'll want to modify the rulebook even further, such as adding custom pages or even a whole new section to the rulebook.
In these cases, the API has you covered.

However, there may arise cases where this is insufficient for your needs.
Maybe you want an ability to always appear at the beginning of the Rulebook for some reason, or you want to add a whole new section to the Rulebook.
The former case can be easily handled with a simple patch to RuleBookInfo.ConstructPageData, but the latter can get fairly complicated.
In either case, you can use the API's RuleBookManager to make this process simpler.

\* *Items and Boons can be made to appear in multiple Acts or outside Act 1 by modifying their associated Full(...)Info.*