# Changelog

## v1.13.1-nonkc
- Fix for JSONLoader cards

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
