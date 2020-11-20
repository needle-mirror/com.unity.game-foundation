README - Game Foundation Sample Scenes - Inventory With Quantity

Most of the time when you want to keep a quantity of items, you will want to use the Stackable Inventory Item discussed in 02a_StackableInventoryItem sample.
However there may be times when you want a different way to represent quantities. This example provides an alternative path.
In this sample, you will see how to group items by their InventoryItemDefinition to get a representation of quantity, while adding and removing items from the Inventory Manager.
All code can be found within the "InventoryWithQuantitySample" script.

#### Initializing Game Foundation
* In this scene Game Foundation is initialized using a Game Foundation Init game object, called "Game Foundation".
  It can be added to a scene by going to `Window > Game Foundation > Create Game Foundation Init GameObject`, however there can only ever be one in any given scene.
* The GameFoundationInit component starts the process of initializing Game Foundation during Awake.
* In this scene you'll notice the "InventoryWithQuantityCatalog" is set in the Override Catalog Asset field in the GameFoundationInit component.
  This means that during initialization that catalog will be loaded instead of whatever catalog is set in your GameFoundationCatalogSettings. 
  This allows the samples scenes to function without your having to change any settings.

#### Viewing the Sample Catalog
* Although the scenes will play successfully with only the Override Catalog Asset field set, you may want to look at or edit the catalog being used in the scene in the Game Foundation Windows.
* To do that, you will need to set your GameFoundationCatalogSettings to point to the InventoryWithQuantityCatalog found in the 02b_InventoryWithQuantity directory, where this README is located.
* To find your GameFoundationCatalogSettings go to `Window > Game Foundation > Settings > Catalog Settings`.
