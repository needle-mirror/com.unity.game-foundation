# Game Foundation: Known issues/limitations

## Unreleased

* Subscription IAP type not yet explicitly supported.
* IAP refunds/returns not yet explicitly supported.
* When using the LocalPersistence data layer, if the player has an existing save file, initial allocation of newly created currencies are not granted.
* Addressable image assets are not loaded at editor time when using them through the provided Game Foundation prefabs (e.g. Transaction Item prefab). This produces a blank image in place of the Addressable image asset in the editor scene view. A warning is also logged for every blank image as a result of Addressables not being loaded at editor time.
* In StoreView, if a prefab with a DetailedTransactionItemView component is used in the `Featured Item Prefab` field (as opposed to the `Featured Bundle Item Prefab` field) payout item images will not be shown.
* If a Promotion Popup prefab is used to complete an IAP Purchase, that instance of the Promotion Popup will not be used to make a purchase again, neither for the same nor a different Transaction Id. The instance should be destroyed and a new instance of the prefab instantiated.
* When using Auto Generate Promo Image on the Promotion Popup prefab, the payout items aren't being laid out properly. At editor time, clicking on any game object other than the top level Promotion Popup game object will show the correct layout. At runtime, if the popup is exhibiting the same layout behavior, the popup needs to be opened, closed, and reopened to show the correct layout. An alternative workaround is to not auto generate promo image, and provide a signle promotion image.
* Claim button on Reward Popup prefab jumps around a bit when the coutdown field changes from shown to hidden or vice versa.
* Sometimes, deleting a catalog item will not delete the item from the catalog asset. The item will still show up as a child of the catalog asset when the catalog asset is expanded/unfolded. Upon restarting the editor, the catalog item sometimes gets deleted and sometimes does not.

## 0.8.0 - (2020-11-20)

* The ChilliConnect Adapters for Game Foundation do not support the Reward system.
* If using Unity IAP version 2.2.1, processing IAP transactions on Android may initially fail, triggering a OnPurchaseFailed callback, however the purchase should get processed again immediately and that one should complete and items should still get paid out. No workaround should be necessary.
* With IAP, background purchases processed automatically by Game Foundation will currently not be fulfilled in the data layer (the purchased items or currencies will not be granted). To handle things like automatic purchase restoration, for now you should uncheck "Process Background Purchases" in Game Foundation's runtime settings and process background purchases yourself.
* When using the ChilliConnect data layer, the player's inventory is sometimes not synced to the game on startup. Workaround: Relaunch the app or reinitialize Game Foundation.
* While creating a new item in a Game Foundation editor window, if you click on an existing item in the left column, GUI errors will show in the console.
* While creating a new item in a Game Foundation editor window, if you enter an invalid key in the key field, future changes to the display name field will no longer auto-generate a new key.
* Initial Allocation can be set to a greater amount than Max Allocation. If it is, it will cause Transactions to fail if that item is either a cost or a payout. Ensure that initial allocation is never set higher than max allocation.
* In Reward editor, if "Reset if Expired" is checked, and then Expiration is set to 0, the Reset if Expired checkbox will be unchecked and disabled, but the reward may still have unexpected resetting behavior. Workaround is to uncheck Reset if Expired before setting Expiration to 0.

## 0.7.0 - (2020-10-13)

* The ChilliConnect Adapters for Game Foundation do not support the Reward system.
* If you get exception: "InvalidOperationException: UnityPurchasingAdapter Error: GameFoundationSdk.Initialize() must be called before FindProduct is used". There are two possible causes:
  1) Game Foundation Sdk is trying to initialize in the same scene as UnityPurchasingAdapter is used (used by IAP Transactions). Workaround is to initialize Game Foundation Sdk in a scene that loads before the one where UnityPurchasingAdapter is used and make that object Do Not Destroy.
  1) IAP product IDs aren't being successfully loaded by Unity IAP when testing IAP purchases on device. Workaround: check device logs at the beginning of app load to see if products are being loaded correctly, if not, fix those issues.
* Game Foundation Sdk will throw an exception during initialization if building for iOS or Android and there is an IAP Transaction that doesn't have the product id for that platform's store filled out.
* Resources Asset Property selector shows duplicates for prefabs that are stored in Resources folders.
* May see a "Module 'Unity.Promise' should be referenced" error in Rider for DebugEditorWindow.cs. It does not effect compilation in Unity or functionality however.
* Chilliconnect breaks linkage to IAP product keys that have lower case letters in them, by converting them to uppercase. Workaround: If using Chilliconnect data layer, use product IDs that are all uppercase.
* Debug window may not show changes made to mutable properties during runtime until you have moused over or clicked on the property in the debug window.
* Null reference to the parent Reward of a Reward Item when the Reward and RewardItem are newly created. Workaround: Restart the editor and the parent reward value for the Reward Item will get set.
* In reward prefab, Sample 03_RewardPopup, when you miss 1-2 reward cycles (ie days 2 & 3) then claim the next day (ie day 4), the 'missed' indication on previous days sometimes changes to green while waiting, then changes back to gray (missed) when rewards are claimed. This is just a UI artifact that does not appear to effect underlying integrity of GF data.
* In reward prefab, Sample 03_RewardPopup, once all rewards have been claimed and the popup resets, you sometimes cannot immediately claim the 1st reward despite the button appearing active and being interactable.  Once the button has been pressed early, the dialog 'locks' so you can never claim until you restart the sample.

## 0.6.0 - (2020-06-30)

* The editor window can get really slow when a catalog item has a `JsonDetail` containing a lot of data.
  This won't be fixed since this detail is `Obsolete` and will be replaced by Static Properties in the future.
* In some cases, some fields on the Store prefabs will only mark the scene as dirty the first time they are changed and saved; subsequent changes will not get marked as dirty and therefore won't be saved on their own.
  Workaround: changing something else in the scene will mark the scene as dirty, and then the editor will allow saving of the scene, including the field with the bug.
* When playing a scene that has a store prefab with IAP transactions in the scene, the editor may log a "GameFoundation.Initialize() must be called before the TransactionManager is used" error, and the store will not be displayed in the scene.
  Workaround: This error only occurs when playing that scene directly, if a previous scene is played, and then navigated to the scene with the store prefab, the error will not appear and the store will display properly.
  Alternatively, while in the editor, changing something about the store prefab will trigger a redraw and will display the store properly.

## 0.5.0-preview.1 (2020-06-04)

* When changing tag on Store prefabs, if the selected tag doesn't have any transaction items associated with it, it will not change the items displayed, and will continue to display whatever items were associated with the previously selected tag.

## 0.4.0-preview.4 (2020-04-24)

* Old catalogs must be deleted and recreated due to new Store System and the removal of hashes.
Simply delete your Assets/GameFoundation folder from Project window and reopen Window->Game Foundation->Inventory (or Store, Stat or Game Item) to reinitialize catalogs, then reenter your desired inventories, stats and game items.
* The IAP integration is limited to consumable products.
* When checking the Purchasing Enabled box in GameFoundationSettings, you may see an error in the console: `PlayerSettings Validation: Requested build target group (##) doesn't exist; #define symbols for scripting won't be added.` This error can be cleared and should not cause any problems.
* Inventory Item prefab needs to be toggled away from default selection before it starts working.
* When running in the Editor with an iOS build target, TransactionManager always returns the Google Store price instead of iOS (this is a known issue with the Unity Purchasing SDK).
* Editor doesn't block duplicate ids created in different windows/catalogs, but Game Foundation will throw error during initialization.
* Mixing prefabs and components from Codeless IAP with Game Foundation is untested and not recommended at this time.
* When running in the editor, all IAP transactions are sent to the data layer as iOS because there is not yet a platform-independent test store in the data layers.
* The Game Foundation editor window UI is not yet updated to look better in Unity 2019.3.x.
* After entering play mode and creating persistent data files, any new default inventories and default inventory items will no longer be automatically created at runtime.
To work around this, you can delete your local persistent runtime data.
* Debug Window: Removing an inventory in the middle of the inventories list causes new inventories to add themselves in the middle.
* Debug Window: Adding or Removing Items may change the foldouts state of other items that should be unaffected.

