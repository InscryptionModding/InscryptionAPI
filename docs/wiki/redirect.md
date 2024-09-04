# Adding Text Redirects
Made of Stone is a vanilla ability that negates the effects of Stinky and Touch of Death.
This is an example of an ability whose behaviour directly relates to other abilities, and in your modding journey you may end up making something similar, be it an ability or stat icon or other.

In these situations you might want to make it easy for players to easily see what these other abilities do, rather than making them flip through the entire rulebook just to recall their effect.

Enter the API.

When making an ability, stat icon, boon, item, or custom rulebook page, you can mark certain words or phrases in their description to be turned into page redirects.

These function similarly to links you will see on the internet; the marked word(s) will be coloured and underlined, and when right-clicked they will take you to a different rulebook page.

For example, if you wanted Made of Stone to link to Stinky and Touch of Death, you would do the following:

```c#
AbilityInfo info = AbilitiesUtil.GetInfo(Ability.MadeOfStone);

// note that the provided string is CASE-SENSITIVE - if we passed in "touch of death" instead, it wouldn't function
info.SetAbilityRedirect("Touch of Death", Ability.Deathtouch, GameColors.Instance.red);
info.SetAbilityRedirect("Stinky", Ability.DebuffEnemy, GameColors.Instance.orange);
```

Now when you open the page for Made of Stone, the word 'Stinky' will be orange and redirect you to Stinky's page, and the words 'Touch of Death' will be red and redirect you to Touch of Death's page.

Redirects can be also be set for stat icons, items, and boons:

```c#
AbilityInfo info = AbilitiesUtil.GetInfo(Ability.MadeOfStone);
info.SetBoonRedirect("effects", BoonData.Type.DoubleDraw, GameColors.Instance.gray);

StatIconInfo stat = StatIconManager.AllStatIconInfos.Find(x => x.iconType == SpecialStatIcon.Ants);
stat.SetItemRedirect("represented", "Harpy Fan", GameColors.Instance.brightLimeGreen);
```

If you want a redirect to point to a custom rulebook page, you can use SetUniqueRedirect, and instead of an Ability or Boon.Type you provide a string corresponding to the custom rulebook page's `pageId`.

## Text Colours
As you likely noticed, each redirect takes a Color as an argument.
This, expectedly, changes the text's colour to help indicate that a certain text is clickable.
In the above examples, only colours that come with Inscryption are used, but you can create and pass in custom colours as well.

It's important to note that not all colours will function as expected, particularly in Act 1 due to its lighting.
The range of atypical colours hasn't been fully tested in every act, but do note that Act 1 will render blue text as green, and white and yellow text will be transparent in normal lighting.