# Environment Setup

First off we need to get some initial setup for the JSONLoader environment out of the way. The sections on this page, shown under `IN THIS ARTICLE` on the right hand side of the screen, will be focused on various aspects of setting up the environment. Why? We want to reduce the frictions you may run into when working with JSONLoader, before you run into them yourself. If you already have the environment you will be using setup, than feel free to skip ahead to the next article.

___

## What do I need?

First off you will need an Text Editor of some variety. A basic notepad can do the trick. However, if you want an easy to use system, which has syntax highlighting for both JSON and CSV, we reccomend grabbing either [Visual Studio Code](https://code.visualstudio.com/download) or [Visual Studio Code Insiders](https://code.visualstudio.com/insiders). There is also a plethora of other editors that could work too, such as [Notepad++](https://notepad-plus-plus.org/downloads/), or [Atom](https://atom-editor.cc/). Once you've got the given editor you want to use installed/setup, we can proceed to the next section.

___

## File Explorer Setup for Windows

If your not a `Windows` user, skip this section. But if you are a `Windows` user, you'll want to follow it, that way we can ensure the file extensions, of the files your making will be the correct ones. The following steps will help you to enable File Explorer to show extensions of your files.

1. First things first, you'll want to open your File Explorer, you can do this by pressing the `Windows` key, than searching for `File Explorer`, and lastly pressing `Open`.
2. Below the `Navbar` at the top of the window, press the button that looks like `…` or `three dots vertically centered and in a row`.
3. After having pressed the button, click on `Options` from the popup menu.
4. Once your in the menu, three tabs will be present at the top of the pop up, you will want to press the one labeled `View`.
5. Under `Advanced Settings` toggle the following setting off, `Hide extensions for known file types`, followed by toggling on `Show hidden files, folders, and drives`, this one is less needed, but it may come in handy when looking for your `AppData` folder if it ever becomes needed.
6. Now press `Apply to Folders` followed by `OK`.

With that we are now set to proceed to the next section! If you ever find yourself unable to view your file extensions, just refollow the above steps to fix it.

___

## Fetching the Relavent Paths

Next up we'll want to grab a few paths to use as a quick reference. If you ever need them you can also come back here for where they are as well. Namely we'll need to navigate to your game or profile's `plugins` folder alot during development of your JSON/CSV Packs.

### Mod Manager Installations

For Mod Managers we need to navigate through the UI of the Mod Manager to navigate to the folder we'll need. Each of the following Sub Sections corrolates with a given Mod Manager that can be used with the game, that we are officially supported through.

#### R2ModMan

For R2ModMan the process for getting there is as follows:

1. Navigate to `Settings`. 
2. Under `Settings` find `Directories` in the Left-Hand Table of Contents, than click it.
3. Find `Profile Folder` than press `Browse`, File Explorer (or an equivalent) should now be open.
4. For the `plugins` folder, navigate to `BepInEx` followed by `plugins`.
5. Now store the full path to this folder somewhere you can easily reference.

#### Gale Mod Manager

For Gale the process is as follows:

1. Click `File` in the `Navbar`, than click on `Open Profile Folder`, File Explorer (or an equivalent) should be open.
2. For the `plugins` folder, navigate to `BepInEx` followed by `plugins`.
3. Now store the full path to this folder somewhere you can easily reference.

#### Thunderstore Mod Manager

For Thunderstore Mod Manager the process is pretty much the same as it would be for R2ModMan:

1. Navigate to `Settings`. 
2. Under `Settings` find `Directories` in the Left-Hand Table of Contents, than click it.
3. Find `Profile Folder` than press `Browse`, File Explorer (or an equivalent) should now be open.
4. For the `plugins` folder, navigate to `BepInEx` followed by `plugins`.
5. Now store the full path to this folder somewhere you can easily reference.

### Manual Installations

If your modding the game manually, first off refer to [Getting Started (Users)](../../users/index.md) for how to setup the basics. Make sure you have it setup correct otherwise the following steps won't align the greatest.

1. Navigate to Inscryption's local install folder
    * For XBOX Gamepass the path is roughly `C:\XboxGames\Inscryption\Content`, the drive may differ based on where you installed it to.
    * For Steam first navigate to where your Steam is installed to, than follow this path `\steamapps\common\Inscryption`.
2. For the `plugins` folder, navigate to `BepInEx` followed by `plugins`.
3. Now store the full path to this folder somewhere you can easily reference.

___

## Adding the JSON/CSV Loader File Extensions to the Context Menu on Windows 11

If your not on `Windows 11`, unfortunatly this won't be able to apply to you, but if you find anything similar for your OS we could include, feel free to let us know that way we can add it. Anyways, the following is a `Registry File` that you can use to add the File Type's needed for JSON/CSV Loader directly in your `New File` under the `Windows Context Menu`.

So first of all your going to want to create a new file, namely a text file. You can use Notepad or one of the aforementioned text editors for JSON Editing to do this. Once your in the new file, you'll want to paste the following into it. You can copy it by pressing the `Clipboard` icon in the top right corner of the code box. Note you don't need to have [Visual Studio Code](https://code.visualstudio.com/download) to use this, that's namely there for the purpose of giving you pretty icons for the file types.

```ini
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\.md]
@="markdownfile"

[HKEY_CLASSES_ROOT\.md\ShellNew]
"NullFile"=""

[HKEY_CLASSES_ROOT\markdownfile]
@="Markdown Document"

[HKEY_CLASSES_ROOT\markdownfile\DefaultIcon]
@="\"C:\\Users\\Chaos\\AppData\\Local\\Programs\\Microsoft VSCode\\a44adf7f53\\resources\\app\\resources\\win32\\markdown.ico\""

[HKEY_CLASSES_ROOT\.json]
@="jsonfile"

[HKEY_CLASSES_ROOT\.json\ShellNew]
"NullFile"=""

[HKEY_CLASSES_ROOT\jsonfile]
@="JSON File"

[HKEY_CLASSES_ROOT\jsonfile\DefaultIcon]
@="\"C:\\Users\\Chaos\\AppData\\Local\\Programs\\Microsoft VSCode\\a44adf7f53\\resources\\app\\resources\\win32\\json.ico\""

[HKEY_CLASSES_ROOT\.jldr]
@="jldrfile"

[HKEY_CLASSES_ROOT\.jldr\ShellNew]
"NullFile"=""

[HKEY_CLASSES_ROOT\jldrfile]
@="JSONLoader File"

[HKEY_CLASSES_ROOT\jldrfile\DefaultIcon]
@="\"C:\\Users\\Chaos\\AppData\\Local\\Programs\\Microsoft VSCode\\a44adf7f53\\resources\\app\\resources\\win32\\json.ico\""

[HKEY_CLASSES_ROOT\.jldr2]
@="jldr2file"

[HKEY_CLASSES_ROOT\.jldr2\ShellNew]
"NullFile"=""

[HKEY_CLASSES_ROOT\jldr2file]
@="JSONLoader2 File"

[HKEY_CLASSES_ROOT\jldr2file\DefaultIcon]
@="\"C:\\Users\\Chaos\\AppData\\Local\\Programs\\Microsoft VSCode\\a44adf7f53\\resources\\app\\resources\\win32\\json.ico\""

[HKEY_CLASSES_ROOT\.jldr3]
@="jldr3file"

[HKEY_CLASSES_ROOT\.jldr3\ShellNew]
"NullFile"=""

[HKEY_CLASSES_ROOT\jldr3file]
@="JSONLoader3 File"

[HKEY_CLASSES_ROOT\jldr3file\DefaultIcon]
@="\"C:\\Users\\Chaos\\AppData\\Local\\Programs\\Microsoft VSCode\\a44adf7f53\\resources\\app\\resources\\win32\\json.ico\""

[HKEY_CLASSES_ROOT\.csv]
@="csvfile"

[HKEY_CLASSES_ROOT\.csv\ShellNew]
"NullFile"=""

[HKEY_CLASSES_ROOT\csvfile]
@="CSV File"

[HKEY_CLASSES_ROOT\csvfile\DefaultIcon]
@="\"C:\\Users\\Chaos\\AppData\\Local\\Programs\\Microsoft VSCode\\a44adf7f53\\resources\\app\\resources\\win32\\html.ico\""
```

Once you have that pasted into the new file, you'll want to save it as a `.reg` file, you can also rename it to hav the `.reg` extension as well. The files name itself does not matter. Once you have it saved as a `.reg` file all you need to do now is run it. Which you can either do via right click, or by double clicking the file. For it to apply you'll want to restart your Windows Explorer via Task Manager.

Now you should see the following File Type's appear when you go to make a new file: `Markdown File`, `CSV File`, `JSONLoader File`, `JSONLoader2 File`, `JSONLoader3 File`, and `JSON File`. The `JSONLoader(X) File` will be what you care about for current JSONLoader, namely the `JSONLoader2 File` for Stable, the rest are applicable for Nightly (including CSV). 

The `Markdown File` is included in this for the purposes of making your packages `README`, `CHANGELOG` and any other documentation you'd want to include. Your Package will be addressed in a later section of the Wiki. But for now, just note that you have it now.

___

Alright now that we have the environment setup, press `Next` to proceed into the [Basics of JSON](basics_of_json.md).

___