namespace EpicLoot;

public static partial class MagicCommands
{
    public static void Init()
    {
        _ = new Command("lucktest",
            $"<color={HEX_Gray}>[CreatureID][LuckFactor]</color> Rolls an example loot table with the specified luck <color={HEX_Gray}>eg: epicloot lucktest Greydwarf 1.0</color>",
            PrintLuckTable,
            GetLuckTestOptions, 
            true);
        
        _ = new Command("printconfig",
            $"<color={HEX_Gray}>[ConfigType]</color> Prints out the Epic Loot current configuration of the specified type",
            PrintConfig, 
            GetPrintOptions);
        
        _ = new Command("magicitem", 
            $"<color={HEX_Gray}>[Rarity][ItemID]</color> Spawn a magic item", 
            SpawnMagicItem, 
            GetSpawnMagicItemOptions, 
            true);
        
        _ = new Command("mi", 
            $"<color={HEX_Gray}>[Rarity][ItemID]</color> Spawn a magic item", 
            SpawnMagicItem, 
            GetSpawnMagicItemOptions, 
            true, 
            true);
        
        _ = new Command("magicitemwitheffects", 
            $"<color={HEX_Gray}>[EffectType][ItemID]</color> Spawn a magic item with effects", 
            SpawnMagicItemWithEffects, 
            GetSpawnMagicItemWithEffectsOptions, 
            true);
        
        _ = new Command("mieffect",
            $"<color={HEX_Gray}>[EffectType][ItemID]</color> Spawn a magic item with effects", 
            SpawnMagicItemWithEffects, 
            GetSpawnMagicItemWithEffectsOptions, 
            true, true);
        
        _ = new Command("magicitemlegendary", 
            $"<color={HEX_Gray}>[LegendaryID][Rarity][ItemID]</color> Spawn a legendary item", 
            SpawnLegendaryMagicItem, 
            GetMagicItemLegendaryOptions, 
            true);
        
        _ = new Command("milegend", 
            $"<color={HEX_Gray}>[LegendaryID][Rarity][ItemID]</color> Spawn a legendary item", 
            SpawnLegendaryMagicItem, 
            GetMagicItemLegendaryOptions, 
            true, true);

        _ = new Command("magicitemset", 
            $"<color={HEX_Gray}>[SetID][ItemID]</color> Spawn a legendary set", 
            SpawnMagicItemSet, 
            GetMagicItemSetOptions, 
            true);
        
        _ = new Command("miset", 
            $"<color={HEX_Gray}>[SetID][ItemID]</color> Spawn a legendary set", 
            SpawnMagicItemSet, 
            GetMagicItemSetOptions, 
            true, 
            true);

        _ = new Command("checkstackquality",
            "Print item names with stack size and quality over 1",
            CheckStackQuality);

        _ = new Command("magicmats",
            "Spawn all magic crafting materials",
            SpawnMagicCraftingMaterials,
            adminOnly: true);
        
        _ = new Command("alwaysdrop",
            "Toggle always drop cheat",
            ToggleAlwaysDrop, 
            adminOnly: true);
        
        _ = new Command("cheatgating",
            "Toggle cheat disable gating",
            ToggleCheatGating,
            adminOnly: true);
        
        _ = new Command("testtreasuremap",
            "Test treasure map by spawning random treasure chests",
            TestTreasureMap,
            adminOnly: true);
        
        _ = new Command("testtm",
            "Test treasure map by spawning random treasure chests",
            TestTreasureMap,
            adminOnly: true, 
            isSecret: true);

        _ = new Command("resettreasuremap",
            "Clear all treasure maps",
            ResetTreasureMap);
        
        _ = new Command("resettm",
            "Clear all treasure maps",
            ResetTreasureMap, 
            isSecret: true);

        _ = new Command("debugtreasuremap",
            "Toggle treasure map debug mode",
            DebugTreasureMap);
        
        _ = new Command("debugtm",
            "Toggle treasure map debug mode",
            DebugTreasureMap, 
            isSecret: true);

        _ = new Command("resetbounties",
            "Clear all bounties",
            ResetBounties);

        _ = new Command("testbountynames",
            "Test bounty name generation",
            TestBountyNames);

        _ = new Command("resetadventure",
            "Clear all adventure save data",
            ResetAdventure);

        _ = new Command("bounties",
            "Print bounty server information",
            PrintBounties);

        _ = new Command("playerbounties",
            "Print player bounty information",
            PrintPlayerBounties);
        
        _ = new Command("gotomerchant",
            "Teleport to Haldor",
            TeleportToMerchant,
            adminOnly: true);
        
        _ = new Command("gotom",
            "Teleport to Haldor",
            TeleportToMerchant,
            adminOnly: true,
            isSecret: true);

        _ = new Command("globalkeys",
            "Print active global keys",
            PrintGlobalKeys);

        _ = new Command("lootres",
            $"<color={HEX_Gray}>[CreatureID]</color> Print loot resolution test",
            PrintLootResolution, 
            GetLootResolutionOptions);
        
        _ = new Command("resetcooldowns",
            "Reset ability cooldowns",
            ResetCooldowns,
            adminOnly: true);

        _ = new Command("debugluck",
            "Print nearby player debug luck factor",
            PrintPlayersLuck);
    }
}