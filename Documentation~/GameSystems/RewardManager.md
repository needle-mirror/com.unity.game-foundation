# The Reward Manager

## Overview

The __Reward Manager__, accessible through [GameFoundationSdk](../GameFoundationSdk.md), provides access to in-game [Rewards], which can promote retention, improve a player's game experience, and incentivize players to continue playing.  A Reward defines a schedule for making Reward Items available to players to claim when certain conditions are met.

## Managing Reward instances

Reward instances are managed at runtime by the Reward Manager. Every runtime reward instance has a list of the states of each reward item. The state of a reward item can be: Claimed, Claimable, Missed, or Locked.  You can also check the reward instance to get its countdown state, and whether it's currently in cooldown or not.

## Processing Rewards

Processing a reward is done by sending a reward item into the __Reward Manager__ and then monitoring the progress from there, while dealing with any successes or failures as necessary.

## Incompatibility with the ChilliConnect adapter

If you use the ChilliConnect adapter as your data layer, the Reward system won't work because ChilliConnect doesn't currently have a reward system.


[Rewards]: ../CatalogItems/RewardDefinition.md

[catalog]: ../Catalog.md
