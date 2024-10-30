### Important Note
_As of EpicLoot 0.9.16, we have fully implemented the new Enchanting Table, which going forward will be the replacement for the legacy crafting station attachments that added functionality to the Forge and Workbench crafting menus. The ``piece_enchanter`` and ``piece_augmenter`` attachments are still buildable in game but **DO NOTHING**, they are just decorations now._

# The Enchanting Table

The Enchanting Table is where all functions of magical wonder and practice take place. Like everything in the life, you must work to gain access to functions within the Enchanting Table. It starts off with learning how to build it which requires you to have touched the items that it takes to build, much like everything else in Valheim. By default to build the table you need 10 FineWood and 1 SurtlingCore.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/78e06c35-74eb-410b-bcbe-037674f5665c)

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/63e69387-9a95-4001-bc76-0def86516590)

## Functions of the Enchanting Table

There are 5 primary functions of the Enchanting Table, each capable of being unlocked and/or upgraded. When a table is first built, all functions except for Sacrifice are locked and must be unlocked before use. The behavior of the table is fully customizable, for more information see the configurations section below.

### Sacrifice

This is where you can take magical items or trophies and sacrifice them down into Enchanting Materials used for other enchanting operations. The UI allow you to multi-select items, and in the case of trophies, you can select to either sacrifice 1 or more trophies at a time from the UI. By default sacrificing regular trophies is the only way to get shards and sacrificing boss trophies is the only way to get runestones.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/f98aaaa5-f1ec-4a1c-963f-7862127e4965)

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/4fe11675-98ed-488f-949c-f8ed640625ff)

### Convert Materials

When unlocked, this function allows you to exchange various items for other various items. There are three subfunctions within Convert Materials:

#### Upgrade Material Rarity

This allows you to take multiple of one rarity magic material, and convert them to a higher tier. Example: 5 Magic Dusts can be converted to 1 Rare Dust.

#### Convert Shards

Similarly, Convert Shards allows you to convert a rarity of shards into a higher rarity of shards.

#### Salvage Junk

If any items in your inventory are considered Junk by Epic Loot standards, these can be converted to provide you with Enchanting Materials. Some items appear both here in this list as well as in the Sacrifice list.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/0c41262c-bb10-4c03-a952-40cc0cea71a6)

### Enchant

When unlocked, this function allows you to enchant mundane items with randomly selected effects. Select which rarity tier to enchant to view what it will cost. By default each enchanted item will require one runestone (acquired from sacrificing boss trophies). 

Under the enchant possibilities section you can view the list of available magical effects that COULD be rolled on as a result of your enchant. You can also view the chances for the number of total rolled effects. Each increase of rarity increases the base number of effects the item can hold. Upgrading this table section will increase the chances of rolling more effects on an item. Once an item is enchanted you cannot edit the total number of rolled effects unless you first disenchant the item and start over.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/bb530611-7506-47e4-b305-9d6f65ef2c4b)

### Augment

When unlocked, the Augment function allows you to select one current magical effect on an Enchanted Item, and pay to randomly re-roll that effect from the list of available effects shown. As more augments are performed on the same item, the cost will increase significantly. An augmented slot will appear as an empty diamond in the tooltip for your item. Upgrading this table section will increase the number of effects to choose from when re-rolling.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/840b7520-8204-406a-8202-205643321588)

### Disenchant

When unlocked, this will allow you to spend Bounty Tokens to DISENCHANT an item back to it's mundane property. No Enchanting Materials are returned as a result of the disenchanting. Bounty Tokens must be obtained from Haldor's bounty quests. If you have disabled adventure mode and wish to use this section you must reconfigure the costs of this section with file patching as explained in another wiki page.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/b945fb38-7299-442f-a956-3ffaa53cc79b)

## Upgrading the Table

The Upgrade Table tab is where you go to unlock and upgrade portions of the table. Each function has up to 5 ranks of abilities that improve the functions of the table (far and above) what the standard table provides. Some of these are mentioned in their sections above. Clicking on the various upgrade options will reveal what the benefits of the upgrade will provide as it relates to that function.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/35179d5f-89e9-4d15-a9d3-597fd7b6ba72)

### Table Decorations

As you upgrade, you'll notice that the table will start to become decorated on the spots where upgrades have happened. In the picture above, I have upgraded the Sacrifice function to 1 Star (shown in the Sacrifice function picture above). As a result, we now have the first decoration bit displayed on the table in the Sacrifice section of the table.

![image](https://github.com/RandyKnapp/ValheimMods/assets/1264136/557a727a-da3b-4242-bb93-7a47094b04d1)

## Destroying the Table

All materials used to upgrade the table will be returned upon the destruction of the Enchanting Table. If you edit the costs of building and upgrading the table through file patching or other means during a playthrough the materials returned will be that of the current configuration, not the old one.

## Multiple Tables

Each table maintains its own upgrade level allowing for multiple players on the server to manage their own tables and upgrades.

## Configurations

In the main **Bepinex\config\randyknapp.mods.epicloot.cfg** file you can turn off table upgrades with the ``Enchanting Table - Upgrades Active`` config, and choose which tabs are available with the ``Enchanting Table - Table Features Active`` config.

To customize the costs associated with all aspects of this table you can do so with config patching. See the ConfigPatching wiki page for more information.