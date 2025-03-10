# Advanced Portals

Author: [RandyKnapp](https://discord.gg/ZNhYeavv3C)
Source: [Github](https://github.com/RandyKnapp/ValheimMods/tree/main/AdvancedPortals/)
Patreon: [patreon.com/randyknapp](https://www.patreon.com/randyknapp)
Discord: [RandyKnapp's Mod Community](https://discord.gg/ZNhYeavv3C)

Adds three new portals to provide a lore-friendly and balanced way to reduce the item-transport slog!

  * **Ancient Portal:** Allows teleporting Copper and Tin
    * *Requires:* 20 Ancient Bark, 5 Iron, 2 Surtling Cores
  * **Obsidian Portal:** Allows teleporting Iron
    * *Requires:* 20 Obsidian, 5 Silver, 2 Surtling Cores
  * **Black Marble Portal:** Allows teleporting anything
    * *Requires:* 20 Black Marble, BlackMetal 5, 2 Refined Eitr

## Configuration:

### Ancient Portal

  * Ancient Portal Enabled: Enable this portal. If false, the portal will not be buildable.
  * Ancient Portal Recipe: The items needed to build the Ancient portal. Must be in the following format: "ITEM1:QUANTITY,ITEM2:QUANTITY,..." where each ITEM is the item ID ([found here](https://valheim-modding.github.io/Jotunn/data/objects/item-list.html)), and QUANTITY is an integer.
  * Ancient Portal Allowed Items: The items that will be allowed to teleport through the Ancient Portal. Must be in the following format: "ITEM1,ITEM2,ITEM3,..." where ITEM is the item ID.
  * Ancient Portal Allow Everything: If set to true, the Allowed Items will be ignored and all items will be teleportable through this portal. Default=false.

### Obsidian Portal

  * Obsidian Portal Enabled: Enable this portal. If false, the portal will not be buildable.
  * Obsidian Portal Recipe: The items needed to build the Obsidian portal. (same format as Ancient Portal Recipe)
  * Obsidian Portal Allowed Items: The items that will be allowed to teleport through the Obsidian Portal. (same format as Ancient Portal Allowed Items)
  * Obsidian Portal Allow Everything: If set to true, the Allowed Items will be ignored and all items will be teleportable through this portal. Default=false.
  * Obsidian Portal Use All Previous: If set to true, automatically allow all the Allowed Items from the Ancient Portal through this portal too. Default=true.

### Black Marble Portal

  * Black Marble Portal Enabled: Enable this portal. If false, the portal will not be buildable.
  * Black Marble Portal Recipe: The items needed to build the Black Marble portal. (same format as Ancient Portal Recipe)
  * Black Marble Portal Allowed Items: The items that will be allowed to teleport through the Black Marble Portal. (same format as Ancient Portal Allowed Items)
  * Black Marble Portal Allow Everything: If set to true, the Allowed Items will be ignored and all items will be teleportable through this portal. Default=true;
  * Black Marble Portal Use All Previous: If set to true, automatically allow all the Allowed Items from the Ancient Portal and Obsidian Portal through this portal too. Default=true.

## Installation:
  * Nexus: Drop the AdvancedPortals.dll right into your BepInEx/plugins folder
  * ThunderStore: Use r2modman to install, or manually drop the dll into your BepInEx/plugins folder

This mod needs to be on both the client and server; the mod will enforce installation. Players without the mod will NOT be able to connect to the server. Config Syncing is included with Jotunn. The server with enforce the same mod configuration for all players. Live changes to the configurations will should take immediate effect.