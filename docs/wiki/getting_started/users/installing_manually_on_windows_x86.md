# Installing Manually on Windows (x86)

1. Install [BepInEx](https://thunderstore.io/package/download/BepInEx/BepInExPack_Inscryption/5.4.2305/) by pressing `Manual Download` and extract the contents into a folder. **Do not extract into the game folder!**
2. Move the contents of the `BepInExPack_Inscryption` folder into the game folder (where the game executable is; usually found here: `C:\Program Files (x86)\Steam\steamapps\common\Inscryption`).
3. Run the game. If everything was done correctly, you will see the BepInEx console appear on your desktop. Close the game after it finishes loading.
4. Install [MonoModLoader](https://inscryption.thunderstore.io/package/BepInEx/MonoMod_Loader_Inscryption/) and extract the contents into a folder.
5. Move the contents of the `patchers` folder into `BepInEx/patchers` (If any of the mentioned BepInEx folders don't exist, just create them).
6. Install [Inscryption API](https://inscryption.thunderstore.io/package/API_dev/API/) and extract the contents into a folder.
7. Move the contents of the `plugins` folder into `BepInEx/plugins` and the contents of the `monomod` folder into the `BepInEx/monomod` folder.
8. Run the game again. If everything runs correctly, a message will appear in the console telling you that the API was loaded.
9. For any additional mods create a new subfolder, it can be called anything and extract the zips archive into it and if there is a `BepInEx` folder within the zip instead drop the contents of that folder into the `BepInEx` root for the modding instance. EX;
    ```
    BepInEx // These go within the BepInEx root folder
    |-- config
    |-- patchers
    |-- plugins
    |-- monomod
    |-- core
    plugins // Files within go into the created plugin subfolder that was created for the mod
    |-- Art
    |-- Scripts
    |-- MyMod.dll
    manifest.json // Ignorable, but if kept, goes in plugin subfolder
    README.md // Ignorable, but if kept, goes in plugin subfolder
    CHANGELOG.md // Ignorable, but if kept, goes in plugin subfolder
    icon.png // Ignorable, but if kept, goes in plugin subfolder
    ```
10. Run the game once more and everything should be correct and working.

___