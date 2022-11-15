# Changelog

## v2.7.0
- Added support for converting audio files to AudioClip objects
- Added support for adding custom tracks to the Gramophone

## v2.6.0
- Added support for custom consumable items using a choice of a few models
- Added support for custom consumable card in a bottle items
- Added support for custom consumable items with a custom model

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
- Rewrite (Specifics to be added)

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
