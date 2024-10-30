# How Loot Rolling Works

One of the most common questions we get on Discord is how the loot rolling and item drops work in Epic Loot, and how to read the configuration JSON's that configure it.  In fact, so much so that we have finally put together a wiki to fully explain, hopefully in a manner that is easily understood.

## Weight
Before we dive in, let's first talk about WEIGHT.  In many areas of EpicLoot configs, there is a weight value that is assigned.  If all weights, in a given roll add up to 100, you can very easily think of it as a percentage.  However, that's not always the case, and in fact, weight is a little bit different.

But to explain weight, I like to think of it as a Raffle Drawing.  Every person has a number of tickets.  All tickets are put into a barrel, rolled, and 1 is randomly picked.

So, let's say we have 3 people holding tickets:

Bob has 5 tickets.
Jane has 3 tickets.
Ronald has 40 tickets.

There are a total of 48 tickets in play.  All tickets are put into a barrel and rolled.  Bob and Jane still have a chance to get picked, but Ronald is definitely the favorite.

EpicLoot is no different. Anytime you see weight specified (either directly or indirectly), think about Bob, Jane, and Ronald.

## Loot Rolling Process

Let's first just talk about how the Loot Roller works.  If you've ever played D&D, it works very closely as if a DM was rolling against a set of tables to come up with loot.  In EpicLoot, there are several tables that get rolled against, and several rolls that happen in a sequence very quickly.

In this example, we JUST killed a 1-Star Greydwarf Brute.

### Figuring Out the Number of Drops

```
    // Tier 2 Mobs (Greydwarf Brute, Greydwarf Shaman, Poison Skeleton)
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
      "Object": "Tier2Mob",
      "LeveledLoot": [
        {
          "Level": 1,
          "Drops": [ [0, 90], [1, 10] ],
          "Loot": [
            { "Item": "Tier1Everything", "Weight": 1, "Rarity": [ 50, 47, 2, 1 ] },
            { "Item": "Tier0Shields",    "Weight": 1, "Rarity": [ 50, 47, 2, 1 ] }
          ]
        },
        {
          "Level": 2,
          "Drops": [  [0, 65], [1, 34], [2, 1] ],
          "Loot": [
            { "Item": "Tier1Everything", "Weight": 5, "Rarity": [ 40, 53, 5, 2 ] },
            { "Item": "Tier0Shields",    "Weight": 5, "Rarity": [ 40, 53, 5, 2 ] },
            { "Item": "TrollArmor",      "Weight": 1, "Rarity": [ 40, 53, 5, 2 ] }
          ]
        },
```

1. Killing a Mob or Spawning a Chest
   1. When you kill a mob, or the system spawns a chest nearby, the system looks at the `loottables.json` file to determine which table to use.
1. Identify the Level of the Mob (aka number of stars).
   1. A 0 Star Mob will use the `Level: 1` loot table.  
   1. A 1-Star Mob will use the 'Level: 2' loot table. So on and So forth.
1. Roll to Determine **Number of items that will Drop**
   1. The table will have a Drops array that provides the associated chance for the number of drops.
   1. It'll look like this:  `"Drops": [ [0, 65], [1, 34], [2, 1] ],`
      1. In the example above, each array item breaks down into two parts `[0,65]` or `[Number of Drops, Weight of Drops]`
      1. The weight here adds up to 100, so it's easy to think of the **Ticket Drawing** as a percentage:
         1. 65% of the time, 0 Drops will drop. (meaning nothing). `[0,65]`.
            1. Said differently, the chance for 0 Drops to be picked has 65 Tickets in the Drawing.
         1. 34% of the time, 1 Drop will drop.  `[1,34]`
            1. Or 34 Tickets in the Drawing.
         1. 1% of the time, 2 Drops will drop.  `[2,1]`
            1. Or 1 Ticket in the drawing.
1. Once we have identified the **NUMBER** of Drops, then the process for rolling EACH drop is the same.

### Rolling Each Drop from the Loot Table

1. Roll what TYPE of Item to Drop
   1. At this point, for each drop that we are dropping, we look at the associated `Loot: []` on the Mob or Chest loottables.json.
      1. Each Item in the List will have a number of Tickets in the Drawing in order to be picked.
      1. The `Item:` can either be a specific Prefab Name, or an ItemSet defined in the loottables.json.
      1. Each item has a `Weight` or the number of tickets.
      1. Each item has a `Rarity` to roll.
   1. Based on the Weight (or the number of tickets) that each item has, a ROLL is performed to pick the TYPE of Item.
      1. ```
            { "Item": "Tier1Everything", "Weight": 5, "Rarity": [ 40, 53, 5, 2 ] },
            { "Item": "Tier0Shields",    "Weight": 5, "Rarity": [ 40, 53, 5, 2 ] },
            { "Item": "TrollArmor",      "Weight": 1, "Rarity": [ 40, 53, 5, 2 ] }
         ```
      1. In the loot table above the Weights are distributed:
         1. **Tier1Everything** has 5 tickets.
         1. **Tier0Sheilds** has 5 tickets.
         1. **TrollArmor** has 1 ticket.
   1. The Loot Roller rolls, and pickets a **Tier1Everything** item.
      1. Because this is an Item Set, it will require another set of rolls.
1. Determine **RARITY**
   1. At this point, IF there is a **Rarity** table next to the associated Item that was rolled, a Rarity roll is performed to determine whether the item will be Magic, Rare, Epic, Legendary, or Mythic.
   1. In this case **Tier1Everything** has a Rarity table:  `"Rarity": [ 40, 53, 5, 2 ] }`
      1. The Rarity Table defines how many tickets each rarity has:
         1. [ Magic, Rare, Epic, Legendary, (Mythic when implemented)]
         1. Because of Rarity tickets add up to 100, you can read it like this:
            1. 40% chance to drop a Magic Item
            1. 53% chance to drop a Rare Item
            1. 5% chance to drop an Epic Item
            1. 2% chance to drop a Legendary Item
   1. The Loot Roller rolls for rarity, pulls a ticket out of the barrel, and it's a RARE item.

So now we know for our first drop, we are dropping a **Rare** quality **Tier1Everything** item.  Now it's time to resolve what **Tier1Everything** means.

If we look higher up in the `loottables.json` file, we'll find this table under the **ItemSets**:

```
    {
      "Name": "Tier1Everything",
      "Loot": [
        { "Item": "Tier1Weapons" },
        { "Item": "Tier1Shields" },
        { "Item": "Tier1Armor"   },
        { "Item": "Tier1Tools"   }
      ]
    },
```
This tells us that **Tier1Everything** will roll from the following Items (which happen to also be Item Sets).

A roll against the table above will be performed to determine WHICH ItemSet to roll below.   You'll also note that Weight is not listed. In cases where Weight is NOT listed, Weight will = 1 as a default value.  So, all of the items listed above are eligible to be picked in the drawing, and because Weight isn't specified, they each hold 1 ticket (said differently, have a Weight of 1).

Once this roll is performed, then it will roll the next loot table's set of items.

In our example, **Tier1Armor** is rolled.  So now we know we are rolling a **Rare Tier1Armor**

```
    {
      "Name": "Tier1Armor",
      "Loot": [
        { "Item": "ArmorLeatherLegs",  "Rarity": [ 94, 3, 2, 1 ] },
        { "Item": "ArmorLeatherChest", "Rarity": [ 94, 3, 2, 1 ] },
        { "Item": "HelmetLeather",     "Rarity": [ 94, 3, 2, 1 ] },
        { "Item": "CapeDeerHide",      "Rarity": [ 94, 3, 2, 1 ] }
      ]
    },
```

At this point, we are starting to see actual Prefab names of items.  You'll also note, again, that Weight is not listed, giving each of these a weight of 1.

Because we ALREADY know what the Rarity of the item is going to be from the earlier rarity roll, the **Rarity** table listed here is NOT used.  However, if none of the previous winning Item Rolls had a rarity table associated with it, then that roll would be delayed until it rolled an item that DID have a rarity table.

So all the tickets for the items above are put into the barrel, and we get **HelmetLeather**!

So, for this first drop, we have rolled a **Rare quality Leather Helmet**.

The process is now REPEATED for the number of drops previously rolled.

## In Summary, and in Short.... the Rolling Process is:

1. Locate Mob or Chest
1. Locate the level of Mob (0 stars, 1 star)
1. Roll for the number of DROPS
1. For each Drop:
   1. Roll Item Type
   1. Roll Item Rarity (if available).
   1. For The Item Type from ItemSets
      1. Roll for specific item Prefab
      1. Roll Rarity (if not previously available).