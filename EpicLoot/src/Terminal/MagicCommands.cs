namespace EpicLoot;

public static partial class MagicCommands
{
    public static void Init()
    {
        new Command("lucktest",
            "Rolls an example loot table with the specified luck eg: epicloot lucktest Greydwarf 1.0",
            PrintLuckTable,
            i => i switch
            {
                2 => GetCreatureNames(i),
                _ => []
            }, 
            true);
        
        new Command("printconfig",
            "Prints out the Epic Loot current configuration of the specified type",
            PrintConfig, 
            GetPrintOptions);
        
        new Command("magicitem", 
            "Spawn a magic item", 
            SpawnMagicItem, 
            GetSpawnMagicItemOptions, 
            true);
        
        new Command("mi", 
            "Spawn a magic item", 
            SpawnMagicItem, 
            GetSpawnMagicItemOptions, 
            true);
        
        new Command("magicitemwitheffects", 
            "Spawn a magic item with effects", 
            SpawnMagicItemWithEffects, 
            GetSpawnMagicItemWithEffectsOptions, 
            true);
        
        new Command("mieffect",
            "Spawn a magic item with effects", 
            SpawnMagicItemWithEffects, 
            GetSpawnMagicItemWithEffectsOptions, 
            true);
        
        new Command("magicitemlegendary", 
            "Spawn a legendary item", 
            SpawnLegendaryMagicItem, 
            GetMagicItemLegendaryOptions, 
            true);
        
        new Command("milegend", 
            "Spawn a legendary item", 
            SpawnLegendaryMagicItem, 
            GetMagicItemLegendaryOptions, 
            true);

        new Command("magicitemset", 
            "Spawn a legendary set", 
            SpawnMagicItemSet, 
            GetMagicItemSetOptions, 
            true);
        
        new Command("miset", 
            "Spawn a legendary set", 
            SpawnMagicItemSet, 
            GetMagicItemSetOptions, 
            true);

        new Command("checkstackquality",
            "Print item names with stack size and quality over 1",
            CheckStackQuality);

        new Command("magicmats",
            "Spawn all magic crafting materials",
            SpawnMagicCraftingMaterials,
            adminOnly: true);
        
        new Command("alwaysdrop",
            "Toggle always drop cheat",
            ToggleAlwaysDrop, 
            adminOnly: true);
        
        new Command("cheatgating",
            "Toggle cheat disable gating",
            ToggleCheatGating,
            adminOnly: true);
        
        new Command("testtreasuremap",
            "Test treasure map by spawning random treasure chests",
            TestTreasureMap,
            adminOnly: true);
        
        new Command("testtm",
            "Test treasure map by spawning random treasure chests",
            TestTreasureMap,
            adminOnly: true);

        new Command("resettreasuremap",
            "Clear all treasure maps",
            ResetTreasureMap);
        
        new Command("resettm",
            "Clear all treasure maps",
            ResetTreasureMap);

        new Command("debugtreasuremap",
            "Toggle treasure map debug mode",
            DebugTreasureMap);
        
        new Command("debugtm",
            "Toggle treasure map debug mode",
            DebugTreasureMap);

        new Command("resetbounties",
            "Clear all bounties",
            ResetBounties);

        new Command("testbountynames",
            "Test bounty name generation",
            TestBountyNames);

        new Command("resetadventure",
            "Clear all adventure save data",
            ResetAdventure);

        new Command("bounties",
            "Print bounty server information",
            PrintBounties);

        new Command("playerbounties",
            "Print player bounty information",
            PrintPlayerBounties);
        
        new Command("gotomerchant",
            "Teleport to Haldor",
            TeleportToMerchant,
            adminOnly: true);
        
        new Command("gotom",
            "Teleport to Haldor",
            TeleportToMerchant,
            adminOnly: true);

        new Command("globalkeys",
            "Print active global keys",
            PrintGlobalKeys);

        new Command("lootres",
            "Print loot resolution test",
            PrintLootResolution, i => i switch
            {
                2 => GetCreatureNames(i),
                _ => [],
            });
        
        new Command("resetcooldowns",
            "Reset ability cooldowns",
            ResetCooldowns,
            adminOnly: true);

        new Command("debugluck",
            "Print nearby player debug luck factor",
            PrintPlayersLuck);
    }
}