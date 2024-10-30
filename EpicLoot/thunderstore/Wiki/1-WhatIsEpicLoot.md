## Introduction

Epic loot is overhaul mod that adds enchanted weapons, tools, and armor ranging in rarity to Valheim. It is highly configurable, letting you take full control over your gameplay experience. Below is a brief explanation of the different features this mod adds to the game. For more detail please see each respective wiki page.

(All screenshots were taken with the UI mod Auga, by RandyKnapp. Auga is currently unavailable due to frequent game updates breaking the mod.)

![image](https://user-images.githubusercontent.com/110222875/207970751-19f5dcf6-dc7b-4571-b817-93a91ec3c06c.png)

## Why Epic Loot?

Epic Loot expands gameplay by giving you more reasons to explore and loot chests around the world, defeat bosses more than once, and encourages exploration by offering treasure chests quests and mini-boss bounties with the adventure feature. Trophies have a new use as sacrifices to obtain enchanting materials, preventing storage buildup of these otherwise decorative items. Depending on your chosen configurations the mod can reduce material grinding making it easier to obtain items, or it can inversely increase the need for you to farm creatures to obtain enchanting materials. Epic Loot is highly configurable and can be catered to anyone's desired gameplay experience with enough time and patience.

## I Played Before the 0.10.0 Update, Help!

Coming back from a break? The mod was rebalanced to be more vanilla friendly in the 0.10.0 patch. If you play with mods that boost the game difficulty or want to restore the old values install the Epic Patches [from thunderstore](https://thunderstore.io/c/valheim/p/RandyKnapp/EpicPatches_EpicLoot/versions/) or [from nexus](https://www.nexusmods.com/valheim/mods/387?tab=files).

The game on "Normal" difficulty still may be too easy for most players after these changes. It is recommended to try the "Hard" or "Very Hard" combat settings from the vanilla world modifiers, or find another mod to boost enemy difficulty. We are still trying to find a good game balance, suggestions are welcomed in the discord!

## Content

There are nearly one hundred different enchantments across 5 tiers of rarity: Magic, Rare, Epic, Legendary, and Mythic. Each rarity tier increases the strength of the enchantment. As you progress through the biomes you will encounter higher tier items and materials to aid in your journey through Valheim.

Epic Loot adds the following items to the game, listed with their prefab names:

* Enchanting Table: piece_enchantingtable
* 25 crafting materials: Runestone, Shard, Dust, Reagent, Essence of each rarity. For example: EpicShard, RareDust, MythicRunestone
* 2 decorations: piece_enchanter, piece_augmenter (Fun fact: In legacy versions these were required build pieces to add enchanting features to the forge)
* 4 utility items: LeatherBelt, SilverRing, GoldRubyRing, Andvaranaut (An item similar to the wishbone for finding treasure chests added by this mod)
* 3 tokens: ForestToken, IronBountyToken, GoldBountyToken

### Enchanting Table

Epic Loot adds a new crafting table called the "Enchanting Table" with features to **Sacrifice** trophies and enchanted items, **Convert Material** rarity, **Enchant** items, **Augment** items to replace their enchantments, and **Disenchant** items to remove their enchantments. This table is upgradable and by default requires most features to be unlocked before using. To build the table you need 10 FineWood and 1 SurtlingCore.

### Adventure Mode

This feature adds a new UI window to Haldor the trader allowing him to sell enchanting materials, enchanted items through gambling, treasure maps, and offer bounty quests. Treasure maps are the only way by default to earn **Forest Tokens**. When purchased they will spawn somewhere random in the world in the specified biome on the surface (no digging required!). Bounties are the only way to earn **Iron and Gold Bounty Tokens**. When accepted the mini-boss will spawn somewhere random in the world in their respective biome and must be slain to receive the awards. These mini-bosses can sometimes spawn with **Minions**, which must also be slain to finish the bounty. When treasure chests and bounties are found or slain you will get a message banner saying so.

To disable Adventure Mode set the ``Balance - Adventure Mode Enabled`` configuration to false in the **Bepinex\config\randyknapp.mods.epicloot.cfg** file. Please note this will make tokens unobtainable and will require file patching to remove these items from enchanting table upgrade requirements.

![image](https://user-images.githubusercontent.com/110222875/207971880-4ce02022-8e11-4aa4-b4ff-916401ac8a31.png)

## Where do I start?

Simply installing the mod and playing will give you a fun gameplay experience! This mod does not require any manual configuration to enjoy.

By default defeating any creature will have a chance to drop enchanted items. Chests around the world in locations such as abandoned meadows buildings can also have these items to loot.

After you defeat the first boss you will obtain your first boss trophy. You will now advance into the black forest and after obtaining finewood you can build your first enchanting table and access all it's features! Runestones are required to enchant gear you have crafted and can only be obtained from sacrificing boss trophies.

Finding Haldor will be your next task to have access to purchasable enchanted items and materials. Accept your first bounties to start earning some cash!

## Basic Configuration Options

Many players do not enjoy that random items drop from defeating creatures, opting instead that enchanting materials drop. In the configuration file under ``Balance - Items To Materials Drop Ratio`` this value can be set to 1 to always drop materials rather than items. Any value between 0 and 1 will give you a combination of enchanted items and crafting materials.

If the drop rates for your selected setting above are too high (or low) you can change the ``Global Drop Rate Modifier`` configuration setting as appropriate. For example: 0.5 will half the drop rate.

## Multiplayer

This mod changes typical vanilla gameplay in multiplayer. By default when defeating a boss there will be one trophy and one "extra" item (such as the swamp key) dropped per player in range when it is defeated. These settings are in the configuration file under the **Balance** section.

When installed on a dedicated server the configurations will sync automatically, ensuring all players have the same values. The exception to this is the **Item Color** settings which are client side and change be changed by anyone for their own game.

**WARNING: If you do not install this mod properly on a dedicated server your adventure mode will be broken! Bounties will not work unless the mod is installed and running on the server. For troubleshooting assistance see this guide first [Server-Troubleshooting](https://github.com/Valheim-Modding/Wiki/wiki/Server-Troubleshooting)**