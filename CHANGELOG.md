# Changelog

## 2.14.0
- Fixed Sniper duplicating attacks from sigils like Double Strike
- Fixed interaction between Waterborne and Fledgling in Act 2
- Fixed Cuckoo sigil softlocking in Act 2 when making a Raven Egg
- Fixed sigils added via temporary mods not displaying in Act 2
- Fixed hiddenAbilities not affecting sigil display in Act 2
- Fixed Handy sigil visual bug outside of Act 2
- Fixed Shapeshifter special ability in Act 2
- Added pixel sprites for Raven Egg and Cuckoo/Broken Egg
- Added ResourceBankManager.AddDecal(), PlayableCard.AddTemporaryMods(), CardModificationInfo.AddDecalIds
- Added AbilityInfo.SetPixelIcon(string pathToArt), CardInfo.RemoveAppearances(), CardInfo.SetDefaultEvolutionName()
- Added DialogueManager.PlayDialogueEventSafe - combines TextDisplayer.PlayDialogueEvent and DialogueHandler.PlayDialogueHandler for multi-act support
- Added support for directly loading AudioClips via the GramophoneManager
- Added support for adding decals to pixel cards via DecalIds
- Added pixel portrait for Ijiraq
- Added support for changing costs midbattle using CardModificationInfos or a HarmonyPatch
- Changed TranspilerHelpers.LogCodeInscryptions to also function as an extension method for List<CodeInstruction>
- FullSpecialTriggeredAbility now stores the ability name and mod GUID
- Temporary mods can now be used to add decals to a card
- CardRenderInfo.OverrideAbilityIcon now works for Act 2 sigils
- CardInfo.SetTerrain() now has optional parameter 'useTerrainLayout', defaulting to true
- Made method used to add stacks to pixel sigils public
- Updated the wiki

## 2.13.3
- Fixed null error when opening card packs in Act 2
- Fixed pixel cards with activated sigils showing the activated sigil icon twice (does not fix the button obscuring sigils)
- Added new helper class GemsManagerHelpers with helper methods: OpponentHasGems, PlayerHasGems
- Changed how Act 2 descriptions are altered to prevent conflicts
- True Scholar now correctly requires a Blue Gem to be owned prior to use

## 2.13.2
- Fixed Hoarder sigil breaking when used by opponents in Act 2
- Fixed Hodag special ability not working in Act 2
- Fixed cards marked as AffectedByTidalLock not being killed by Tidal Lock when it's on a giant card
- Added card extension methods SetAffectedByTidalLock and HasAlternatePortrait
- Added ability extension method SetPart2Ability
- Added AbilityCardMetaCategory AbilityManager.Part2Modular
- Added pixel portraits for Empty Vessel and its Gemified variants, Ant, Bee, Dam, Chime, and the Tail cards
- Amorphous sigil now works in Act 2
- Vessel Printer sigil now works in Act 2
- Trinket Bearer sigil is now disabled in Act 2
- Hidden abilities are now properly hidden in Act 2
- Fledgling sigil now properly shows the required (up to the number 3) in Act 2
- Fledgling sigil's rulebook description now updates to show the selected card's actual number of required turns
- Squirrel, Aqua Squirrel, and Rabbit are now marked as AffectedByTidalLock
- SteelTrap sigil no longer changes a card's portrait to the closed trap; will now switch to an alternate portrait if it exists

## 2.13.1
- Fixed custom items falling through reality
- Added card extension method IsAffectedByTidalLock
- Mental Gemnastics sigil now works in Act 1
- Tidal Lock sigil now works for non-Moon cards

## 2.13.0
- Fixed DontDestroyOnLoad warnings when using custom items
- Fixed weird spacing for Mox cost textures in Act 1
- Fixed player death cards not inheriting Energy, Mox, or custom costs
- Fixed the hint dialogue for insufficient Energy in Act 1 being the wrong colour
- Fixed ExtendedActivatedAbilityBehaviour discarding negative activation cost modifiers
- Fixed Sniper not accounting for custom sigils that modify attack slots
- Fixed Tutor not working in Act 2
- Added more extension methods to BoardManager
- Added new card extensions SetGemify and SetGemsCost(params GemType[])
- Added catch-all cost textures for when Blood or Bones go above 13
- Added CardModificationInfoManager and DeathCardManager
- Added extended property support and extensions for CardModificationInfo
- Added Blood activation cost support to ExtendedActivatedAbilityBehaviour
- ExtendedActivatedAbilityBehaviour now calls PostActivate() if a card dies from paying the Health cost
- Leshy now recognises death cards with multiple costs in his dialogue
- Leshy will now let you create death cards with up to 8 sigils
- Minor adjustments to some cost textures
- Rearranged order of Mox cost textures to align with order of Mox on the Gem Module
- Removed empty cost textures for Blood, Bones, Energy, Mox from the community patches
- Sniper patch's methods are now public

## 2.12.0
- Fixed ExtendedActivatedAbilityBehaviour's Health cost not subtracting Health correctly
- Fixed softlock in Act 1 during death card creation
- Fixed custom cards that start Gemified not working as intended when obtained in-game
- Potentially fixed softlock when making terrain for a region
- Added further checks to challenge icon-related patch to prevent softlocks
- Added decal, appearance behaviour, and Gemified card support for Act 2 cards
- Added Singleton<OpponentGemsManager> for keeping track of opponent gems
- Added new helper method for creating Sprites from resource files in an assembly
- Added new SpriteType for creating pixel card decals
- Gemified visuals now work correctly for Act 3 opponents
- Cost choice node now offers each Mox colour individually
- Added new config "Default Drone" to change the model and position of the Energy Drone
- Amorphous sigil now activates when used by opponents or obtained via evolution/temp mod
- Owned Mox in Act 1 now updates when a card is hooked by the Angler or via the Hook item

## 2.11.2
- Fixed starter deck custom unlocks not working
- Fixed card icons not being properly centred for starter decks with 4+ cards
- Cards in Acts 2 and 3 can now display up to 8 sigils
- Blood tokens in Act 3 now appear to the side of the board instead of on it
- Blood tokens now stack on each other when there are more than 4

## 2.11.1
- Fixed regions in Act 1 being out of order
- Fixed the console message concerning custom dialogue events not giving the right amount

## 2.11.0
- Refactored how regions are handled by the API to prevent duplicate bosses
- Refactored how bosses are selected to prevent duplicates being encountered
- Changed when modded Ascension data is cleared to allow for editing it post-clear
- Added more descriptive error logs for some commonly encountered errors
- Added config option to reduce the amount of debug info shown in the console
- Added methods to aid in creating encounter turn plans
- Added more methods for interacting with lists, new debug method to aid in making transpilers
- Added ExtendedActivatedAbilityBehaviour class; allows for dynamic costs and Health costs
- Fixed SetOnePerDeck() and SetHideStats() being inaccessible
- Fixed AddCardBlueprint() not setting the replacement card correctly

## 2.10.0
- Completely revamped PeltManager to be more user friendly (Mod breaking)
- Added LocalizationManager for more language support with mods
- Added helper method for custom pelts to change cards trader
- Pelts offered by Trapper capped at 8.
- Pelts offered by Trapper are now randomized if more than 8
- Fixed soft lock at trader when having more pelts than cards to offer 
- Fixed the campfire fix breaking the normal sequence
- Fixed HasCardMetaCategory returning the inverse of its intended value
- Fixed stackable sigils not showing numbers above 9

## 2.9.1
- Fixed the campfire fix breaking the normal sequence

## 2.9.0
- Added talking card support!
- Moved the "CustomLine" struct outside of the Dialogue.Helpers class.
- Fixed tribe choice node being able to offer vanilla tribes with no cards
- Fixed totem choice node being able to offer tops for vanilla tribes with no cards
- Added fallbacks for tribal choice node if there are less than 3 chooseable tribes
- Added fallback to campfire node if you don't have any cards that can be buffed 
- Fixed 'outdated plugins' warning showing up when it shouldn't, tweaked message slightly

## 2.8.1
- Added CardInfo extensions for checking CardMetaCategories, cause why not
- Added DialogueManager for custom dialogue for regions and Custom Color support
- Added ResourceBankManager for custom resources. Avoids doing this for every mod
- Deprecated DialogueEventGenerator (Moved to Dialogue Manager)
- Fixed repeating bosses on regions that have multiple boss possibilities
- Fixed custom props not having a renderer on the top parent and breaking loading regions
- Fixed arrows on the challenges select screen being offscreen at certain resolutions
- Fixed tribe choice node being able to offer custom tribes with no cards
- Fixed being able to get custom totem tops for tribes with no cards

## 2.8.0
- Added support for custom masks
- Fixed sometimes items use the wrong behaviour
- Added more resource and asset bundle helpers

## 2.7.4
- Fixed latch fix modifying the base info
- Fixed stackable abilities activating twice when they shouldn't

## 2.7.3
- Fixed sniper fix not accounting for cards with Repulsive ability
- Fixed latch abilities not working in Act 2
- Added ExtendedProperties for abilities
- Added new ability setter SetTriggersOncePerStack for controlling the behaviour of stackable abilities after a card evolves
- Added new helper methods for creating cards: SetOnePerDeck, SetHideStats
- Added new helper methods for abilities: SetCanStack, SetTriggersOncePerStack, SetActivated, SetPassive, SetConduit, SetConduitCell
- Added new remover methods for cards: RemoveAbilities, RemoveAbilitiesSingle, RemoveTraits, RemoveTribes

## v2.7.2
- Added `CanActivateOutsideBattles` extension method to ConsumableItemData so they can be used outside of battles.
- Added Missing Tribe Icon fallback texture for totem tops when a tribe has no icon
- Changed TotemManager to accept a `CompositeTotemPiece` type for custom behaviour other than always a custom icon
- Fixed lag when entering gain consumable item map node
- Fixed crash when using custom consumable items
- Fixed hard lock when getting totem top that doesn't have an icon
- Fixed Pack Rat card object not having the correct background during the item node sequence
- Fixed Latch abilities removing stat boosts when latching a card
- Fixed latched abilities not properly rendering in some acts

## v2.7.1
- Changed Pelt Manager to no longer have an interface for future safety! (NOTE This will break all mods with custom pelts!)
- Added Squirrel tribe art (Thanks Drift!)
- Fixed Green Gem stat icon showing as a black square in act 1
- Fixed Green Gem stat icon not appearing in rulebook
- Fixed Squirrel totem top causing NMA when using custom totem tops
- Fixed being unable to play cards with a Blood cost above 4 via sacrifices

## v2.7.0
- Added support for custom pelts
- Added support for converting audio files to AudioClip objects
- Added support for adding custom tracks to the Gramophone
- Added support for adding custom audio files
- Warning message for outdated plugins now lists the outdated plugins
- Energy Drone now tweens with the scales, kinda
- Fixed visual bug where energy cells didn't start closed in successive battles

## v2.6.0
- Added support for custom consumable items using a choice of a few models
- Added support for custom consumable card in a bottle items
- Added support for custom consumable items with a custom model
- Added more helper extensions for checking abilities, traits, special abilities
- Fixed null instances in Act 2 spamming the console with warnings

## v2.5.3
- Added support for custom card unlock requirements
- Fixed non-giant cards with Omni Strike not directly attacking their opposing slot when there are no opposing cards
- Fixed cards attacking their own side of the board during combat not adding damage to the correct side of the scale
- Fixed an issue where a challenge would go missing if you had more than 14 installed

## v2.5.2
- Fixed the sentry fix overriding patches to SlotAttackSlot

## v2.5.1
- Reverted part of the sentry fix that was causing problems
- Made it easier to override the default totem head

## v2.5.0
- Added support for custom totem heads
- Custom Tribes now appear as a totem in the Wood Carver nodes
- Fixes for Sentry ability in Act 1 relating to PackMule, Loose Tail, and enemy totems
- Fixed stacked ability icons causing issues when trying to render numbers on some sigil icons
- Fixed Latches not working in Act 1

## v2.4.2
- Switched to debug version

## v2.4.1
- Fixed Sentry ability not working properly in Act for players or opponents

## v2.4.0
- Reworked challenges
- Fixed gemified opponent cards not working properly
- Fixed stat icons in Act 3

## v2.3.0
- Fixed orange gem not counting towards passive attack
- Fixed PackMule special ability not working on the player's side
- Fixed Mox cost choice node not working
- Fixed boon rulebook and removal
- Fixed tribe choice nodes
- Fixed error caused by passing null when assigning a custom tail portrait
- Improved activated ability fix
- Improved some extensions and attack buffs
- Fixes for starter decks
- Fixes for custom regions, more customisation when creating one
- Added more extensions
- Fixed stat icon rendering for Act 3

## v2.2.0
- Added an interface that triggers when cards are facedown
- Updated custom artwork for GBC numbers
- Fixed flipped icons spamming the log with warnings
- Fixed Tribe API breaking mods that use CardbackTexture
- Added custom combat triggers
- Added more custom extensions
- Fixed Latch abilities in Act 1
- Fixed extension methods for setting custom flipped portrait affecting the wrong card
- Fixed optimisation issues caused by passive attack bufs
- Fixed activated sigils
- Added node manager for custom nodes
- Fixed cards getting buffs after the game ends

## v2.1.0
- Fixed blurry portraits when playing on low graphics settings

## v2.0.3
- Added support for custom tribes and boons
- Added config option to opt of custom cost renders for Act 2 cards
- Refactored and added documentation for CardExtensions

## v2.0.2
- Improved the process of creating stat icons to automatically register and add the corresponding special ability
- Added log warnings for improperly registered cards

## v2.0.1
- Bugfix for SaveData

## v2.0
- Rewritten to use base game objects

## v1.13.0
- Added support for custom card backgrounds, dialogs, encounters and talking cards
- Fixes to abilities loading and stackable custom abilities

## v1.12.1
- Bugfix so CustomCard doesn't wipe ability information.

## v1.12
- Fixes params.
- Adds feature for special abilities and special stat icons.
- Added support for emissions.

## v1.11
- Added support for more identifiers

## v1.10.1
- Fix for abilities which do not have identifier.

## v1.10
- Added ability identifiers.

## v1.9.1
- Added support for mox.
- Forced ability texture to point filter.

## v1.9
- Added config options for energy.

## v1.8.2
- Fixed appearanceBehaviour (again).

## v1.8.1
- Fix pixelTex dimensions.

## v1.8
### Not compatible with v1.7.2
- Changes to using TypeMapper.

## v1.7.2
- Fixed error when not adding any abilities.

## v1.7.1
- Fixed appearance behaviours not loading properly.

## v1.7
- Added support for custom abilities!

## v1.6
- Changed textures to point filter to reduce blur.

## v1.5.2
- Enabled fix for evolveParams and some other disabled options.

## v1.5.1
- Fix to accessing private instance for regions.

## v1.5
### Not compatible with v1.4
- Changed all references to API including guid.

## v1.4
- Set up support for customising and adding regions.

## v1.3
- Set up project to work as a library for other plugins to use.

## v1.2.1.1
- Fixed previous patch.

## v1.2.1
- Fixed cards not being inserted into the card pool on chapter select.

## v1.2
### Not compatible with v1.1
- Added customising default cards through CustomCard.
- Custom cards are added via the **CustomCard** constructor rather than through the **AddCard** method.

## v1.1
- Hooked into a much more sensible method to load the cards into the card pool.
