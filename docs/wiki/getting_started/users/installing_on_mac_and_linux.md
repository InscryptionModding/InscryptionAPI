# Installing on Mac & Linux

***FOR LINUX:*** Ensure that the game is set to run using `Proton` in the game settings on steam. Should be a setting like this: `Change launch behaviour` -> `Proton`.

1. Follow the steps here first: [BepInEx Installation Guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html).
2. Run the game. If everything was done correctly, you will see the BepInEx console appear on your desktop. Close the game after it finishes loading.
3. Install [MonoModLoader](https://inscryption.thunderstore.io/package/BepInEx/MonoMod_Loader_Inscryption/) and extract the contents into a folder.
4. Move the contents of the `patchers` folder into `BepInEx/patchers` (If any of the mentioned BepInEx folders don't exist, just create them).
5. Install [Inscryption API](https://inscryption.thunderstore.io/package/API_dev/API/) and extract the contents into a folder.
6. Move the contents of the `plugins` folder into `BepInEx/plugins` and the contents of the `monomod` folder into the `BepInEx/monomod` folder.
7. Run the game again. If everything runs correctly, a message will appear in the console telling you that the API was loaded.
8. For any additional mods create a new subfolder. It can be called anything. Then, extract the zips archive into it. If there is a `BepInEx` folder within the zip instead drop the contents of that folder into the `BepInEx` root for the modding instance. EX;
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
9. Run the game once more and everything should be correct and working.

If you have any issues with Mac/Linux, Steam Deck, or Manual head over to the discord for this game:

* **Inscryption Modding Discord:** [Here](https://discord.gg/ZQPvfKEpwM)

___