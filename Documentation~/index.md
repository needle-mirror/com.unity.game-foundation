# Welcome to Game Foundation

Game Foundation delivers off-the-shelf common game systems to help you build games with Unity. Whether or not you’ve built games with Unity before, take advantage of the many flexible and extensible Game Foundation systems available to boost your game development process. Leverage these systems so you can spend less time setting up the foundations of your game, and instead focus on driving your game further in other areas. To access these features, the [GameFoundationSdk] provides managers for each game system, including:

* An **inventory** system for defining and managing game resources that are inventoriable.
* A **wallet** system for defining and managing virtual currencies.
* A **transaction** system for handling virtual purchases / crafting mechanics and IAP purchases.
* A **reward** system for periodically granting players a small gift.

[GameFoundationSdk] also maintains a Catalog of all items available, including the following item types:

* **Currencies** which track total quantities of simple items purchased or obtained by the player, for example coins, gems, etc.
* **Inventory Items** which represent objects owned by the player.  These can be complex items with static as well as mutable properties.
* **Transactions** which describe what items/currencies are consumed to grant specific items and/or currencies.
* **Rewards** can be granted to the player on an interval or through game play to engage the player and improve his or her overall experience.
* **Storefronts** hold lists of store items for sale either as virtual transactions or In App purchases.
* **Tags** are simple strings that can be added to the above items to help filter and retrieve items in the editor and at runtime.

Catalog items, except tags, can contain **static properties** for defining and managing additional information that is commonly used in gameplay.  In addition, Inventory Items also permit **mutable properties** that can be changed and retrieved at runtime.

Unity is dedicated to make your game development process as efficient as possible. That’s why Unity is working to expand the Game Foundation package with other pre-built common game systems in the future. Keep up-to-date with what’s new in the Game Foundation package by visiting the [Changelog].

To get started with Game Foundation, install and initialize the Game Foundation package in your Unity Editor. Refer to this documentation understand the architecture and other back-end specifics of the Game Foundation package, and follow along with the tutorials to learn how to set up and use the various pre-built game systems in the package.

[GameFoundationSdk]: GameFoundationSdk.md
