# Installing on The Steam Deck

**FOR STEAMDECK:** Ensure that the game is set to run using `Proton` in he game settings on steam. Should be a setting like this: `Change launch behaviour` -> `Proton`.

1. Download [r2modman](https://thunderstore.io/c/inscryption/p/ebkr/r2modman/) on the Steam Deck's Desktop Mode and open it from its download using its `AppImage` file.
2. Download the mods you plan on using and their dependencies.
3. Go to the setting of the profile you are using for the mods and click `Browse Profile Folder`.
4. Copy the BepInEx folder, then go to Steam and open Inscryption's Properties menu
5. Go to `Installed Files` click `Browse` to open the folder containing Inscryption's local files; paste the BepInEx folder there.
6. Enter Gaming Mode and check 'Force the use of a specific Steam Play compatibility tool' in the Properties menu under `Compatibility`.
7. Go to the launch parameters and enter `WINEDLLOVERRIDES="winhttp.dll=n,b" %command%`.
8. Open Inscryption. If everything was done correctly, you should see a console appear on your screen.