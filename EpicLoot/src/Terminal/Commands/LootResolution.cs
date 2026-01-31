using System.Collections.Generic;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void PrintLootResolution(Terminal.ConsoleEventArgs args)
    {
        string lootTable = args.GetString(2, "Greydwarf");
        int level = args.GetInt(3, 1);
        int itemIndex = args.GetInt(4);
        LootRoller.PrintLootResolutionTest(lootTable, level, itemIndex);
    }

    private static List<string> GetLootResolutionOptions(int i) => i switch
    {
        2 => GetCreatureNames(i),
        _ => [],
    };
}