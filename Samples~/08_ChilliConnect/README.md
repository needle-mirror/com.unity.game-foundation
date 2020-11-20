README - Game Foundation Sample Scenes - ChilliConnect

In this example, you will see how to use the ChilliConnect data access layer for Game Foundation.
It reuses the setup of the Transaction sample but instead of initializing with Memory Data Layer it uses the ChilliConnect Data Layer.
All code can be found within the "ChilliConnectSample" script.

# Setting up the ChilliConnect sample

## Import ChilliConnect Adapters for Game Foundation

1.  First you have to download [ChilliConnect SDK for Unity](https://docs.chilliconnect.com/guide/sdks/#unity) and import it to your project.
    Or you can click on the **Get the ChilliConnect Sdk Package** button in the **ChilliConnect Sample** gameobject in the scene
2.  Then you can import the **ChilliConnect Adapters for Game Foundation** package into your project.\
    Warning: If you already have a `csc.rsp` at the root of your Asset folder you will have to add the following lines to it:
    ```
    -r:System.IO.Compression.dll
    -r:System.IO.Compression.FileSystem.dll
    ```
3.  Once **ChilliConnect Adapters for Game Foundation** is imported, be sure to read its README.
    It will tell you all you need to know about setting up ChilliConnect and your game.

## Setup database & scene

1.  Set the database to the **ChilliConnectCatalog** within your **Game Foundation Catalog Settings** file.
    * To find your GameFoundationCatalogSettings go to `Window > Game Foundation > Settings > Catalog Settings`.
2.  Export it to your ChilliConnect dashboard. See the "Exporting Game Foundation catalogs to ChilliConnect dashboard" section in the **ChilliConnect Adapters for Game Foundation**'s README.
3.  In the **ChilliConnect Sample** gameobject in the scene, set the **ChilliConnect App Token** field with the App token of your game 
4.  On the same object, press the "Use ChilliConnect data access layer" and wait for the project to finish reloading.
6.  The sample is ready to use.


## Note
* Unlike the other Game Foundation Sample scenes, this one does not use a Game Foundation Init game object for initialization.
  This is because the GameFoundationInit component does not currently support initialization via the ChilliConnect Data Layer.
