README - Game Foundation Sample Scenes - Data Persistence

In this example, you will see how to save and load your data locally with Game Foundation systems.
If you save, stop playing, then start again, your changes will automatically be loaded.
All code can be found within the "DataPersistenceSample" script.

#### Initializing Game Foundation
* In this scene Game Foundation is initialized using a Game Foundation Init game object, called "Game Foundation".
  It can be added to a scene by going to `Window > Game Foundation > Create Game Foundation Init GameObject`, however there can only ever be one in any given scene.
* The GameFoundationInit component starts the process of initializing Game Foundation during Awake.
* In this scene you'll notice the "DataPersistenceCatalog" is set in the Override Catalog Asset field in the GameFoundationInit component.
  This means that during initialization that catalog will be loaded instead of whatever catalog is set in your GameFoundationCatalogSettings. 
  This allows the samples scenes to function without your having to change any settings.
* Note that the Data Layer Type field in the GameFoundationInit component is different in this sample from the previous ones.
  Previously, it was set to Memory; in this scene it is set to Local Persistence and there is a Local Persistence Filename field visible now.
  This is the file where the data saved in the scene will be stored.

#### Viewing the Sample Catalog
* Although the scenes will play successfully with only the Override Catalog Asset field set, you may want to look at or edit the catalog being used in the scene in the Game Foundation Windows.
* To do that, you will need to set your GameFoundationCatalogSettings to point to the DataPersistenceCatalog found in the 06_DataPersistence directory, where this README is located.
* To find your GameFoundationCatalogSettings go to `Window > Game Foundation > Settings > Catalog Settings`.
