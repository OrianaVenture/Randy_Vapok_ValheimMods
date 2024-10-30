# Adventure Mode

This feature adds a new UI window to Haldor the trader allowing him to sell enchanting materials, enchanted items through gambling, treasure maps, and offer bounty quests. 

**WARNING: If you do not install this mod properly on a dedicated server your adventure mode will be broken! Bounties will not work unless the mod is installed and running on the server. For troubleshooting assistance see this guide first [Server-Troubleshooting](https://github.com/Valheim-Modding/Wiki/wiki/Server-Troubleshooting)**

(All screenshots were taken with the UI mod Auga, by RandyKnapp)

![Trader window](https://user-images.githubusercontent.com/110222875/207977161-9303c0ec-bb85-40ab-9129-6c1fb97c951f.png)

### Secret Stash

The secret stash is the the tab where you will be able to purchase enchanting materials and an Andvaranaut. Similar to the Wishbone, the Andvaranaut is a powerful utility ring that helps the player locate treasure chests after purchasing a treasure map. Under the main configuration file you can change the range of this item with the ``Balance - Andvaranaut Range`` setting.

![image](https://user-images.githubusercontent.com/110222875/207977507-f75bc2b5-5e35-4736-91d4-317e31c0f143.png)

### Gamble

The gamble tab is where you can spend your hard earned gold for an item with a random rarity. You can also buy rare, epic, or legendary items for gold and one of three special currency, the Iron token, the Gold token, or the forest token.

![image](https://user-images.githubusercontent.com/110222875/207995897-9cbbb138-78c4-4429-a7e2-84d7e54c2a56.png)
![image](https://user-images.githubusercontent.com/110222875/207995956-43a58196-3626-4f0e-ac00-41f925441dba.png)

### Treasure Maps

Treasure maps are purchasable quests that give you an area on the map to go and find a Treasure Chest. In those treasure chests you can find gear ranging in quality and forest tokens. Treasure maps are the only way by default to earn **Forest Tokens**. When purchased they will spawn somewhere random in the world in the specified biome on the surface (no digging required!).

![image](https://user-images.githubusercontent.com/110222875/207996367-d7096467-17d6-41b7-9776-55472078c1ae.png)

### Bounties

Bounties are quests to do the obvious, you go and slay the bounty that you have accepted for gold, iron tokens, gold tokens, AND GLORY! Bounties are the only way to earn **Iron and Gold Bounty Tokens**. When accepted the mini-boss will spawn somewhere random in the world in their respective biome and must be slain to receive the awards. These mini-bosses can sometimes spawn with **Minions**, which must also be slain to finish the bounty. When fully completed a banner message will be displayed declaring victory, and returning to Haldor you will be able to claim your reward.

In the main configuration file there are several options to edit this feature. Under ``Bounty Management`` there are two configurations to set a limit on the number of active bounties a player can have. Under ``Balance - Gated Bounty Mode`` you can select how bounties become available based on boss kills (or unlimited to raise restrictions).

![image](https://user-images.githubusercontent.com/110222875/207997398-17532fed-3112-438c-9845-15babcdc03e3.png)

## Configurations

Similar to the enchanting table, all features of adventure mode can be configured with file patching. See the ConfigPatching wiki page for more information.

## Disable this feature

To disable Adventure Mode set the ``Balance - Adventure Mode Enabled`` configuration to false in the **Bepinex\config\randyknapp.mods.epicloot.cfg** file. Please note this will make tokens unobtainable and will require file patching to remove these items from enchanting table upgrade requirements.
