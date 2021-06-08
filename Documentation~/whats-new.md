# What's new in version 0.9.0

Summary of changes in Game Foundation package version 0.9.0.

The main updates in this release include:

### Added

* Added support for Addressables as static properties.
* Now compatible with In App Purchasing package version 3.0 and up.
* New prefabs including:
  * Horizontal Payout Item
  * Bundle Transaction Item
* Many new features in our components:
  * Detailed Transaction Item
  * Featured Transaction Item in Store View
  * More Offers Button in Store View
  * TextMesh Pro support

### Updated

* Some usability improvements in our editor UIs.
* Non-consumable in app purchases can no longer _directly_ grant payouts.
  Ownership of a non-consumable purchase is more like a boolean, and you can
  perform special logic (including granting items or currency) based on the boolean check for ownership of a product id.
* Improvements to our prefabs:
  * Store View now auto-generates content at edit time.
  * Store View now reuses GameObjects for a performance increase.
  * No longer automatically disabling purchase buttons - it's up to the dev's custom logic.

### Removed

* Dropped support for Unity 2018.
* Support for In App Purchasing versions older than 3.0 are now deprecated and will be removed in a future release.
* Dropped support for ChilliConnect. This will be replaced soon by [Unity Gaming Services](https://unity.com/solutions/gaming-services).
* Some sample scenes and prefabs have been removed:
  * Grid Store
  * Horizontal Store
  * Vertical Transaction Item
  * Columns Transaction Item

### Fixed

* Bug fixes related to Reward expiration.
* Bug fixes in our prefabs.

## Prefab and Sample Package Note
* If the new prefab package is installed on top of a previous package, the `Vertical Payout Item` and `Transaction Item` prefabs, `01 - Store` and `02 - Manual Store` scenes will show up as newly created files.
  The old `Payout Item`, `Horizontal Transaction Item`, and `Grid Store` prefabs and `Store - Horizotal`, `Store - Vertical`, and `Store Grid` sample scenes are no longer supported, and can be deleted.
  All the current prefabs will change to point to the new prefabs.

For a full list of changes and updates in this version, see the [Game Foundation package changelog](../CHANGELOG.md)
