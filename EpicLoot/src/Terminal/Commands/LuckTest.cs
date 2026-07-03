using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void PrintLuckTable(Terminal.ConsoleEventArgs args)
    {
        string creatureName = args.GetString(2, "Greydwarf");
        float luckFactor = args.GetFloat(3);
        args.Context.AddString($"> LuckTest: creature: {creatureName}, luck factor: {luckFactor}");
        PrintLuckTest(creatureName, luckFactor);
    }
    
    private static void PrintLuckTest(string lootTableName, float luckFactor)
    {
        KeyValuePair<string, List<LootTable>> loot_info =
            LootRoller.GetLootTableOrDefault(lootTableName);

        LootDrop lootDrop = LootRoller.GetLootForLevel(loot_info.Value[0], 1)[0];
        lootDrop = LootRoller.ResolveLootDrop(lootDrop);

        if (lootDrop.Rarity == null)
        {
            lootDrop.Rarity = [100f, 0f, 0f, 0f, 0f];
            EpicLoot.LogWarning(
                $"No rarity table was found for {loot_info.Value[0]} using default: [100, 0, 0, 0, 0]");
        }

        Dictionary<ItemRarity, float> rarityBase = LootRoller.GetRarityWeights(lootDrop.Rarity, 0);
        Dictionary<ItemRarity, float> rarityLuck = LootRoller.GetRarityWeights(lootDrop.Rarity, luckFactor);

        float rarityBaseTotal = rarityBase.Sum(x => x.Value);
        float rarityLuckTotal = rarityLuck.Sum(x => x.Value);

        StringBuilder sb = new StringBuilder();

        sb.Append("Rarity".Normalize(25));
        sb.Append("Base".Normalize(25));
        sb.Append("Base %".Normalize(25));
        sb.Append("Luck".Normalize(25));
        sb.Append("Luck %".Normalize(25));
        sb.Append("Diff".Normalize(25));
        sb.Append("Factor");
        int headerLength = sb.ToString().Length;
        sb.Append("\n");
        
        sb.AppendLine(new string('=', headerLength));

        foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
        {
            float baseWeight = rarityBase[rarity];
            float luckWeight = rarityLuck[rarity];

            float basePercent = baseWeight / rarityBaseTotal;
            float luckPercent = rarityLuckTotal > 0 ? luckWeight / rarityLuckTotal : 0f;

            string factor =
                basePercent > 0
                    ? (luckPercent / basePercent).ToString("0.##")
                    : "-";
            
            sb.Append($"{rarity}".Normalize(25));
            sb.Append($"{baseWeight:0.##}".Normalize(25));
            sb.Append($"{basePercent:0.##%}".Normalize(25));
            sb.Append($"{luckWeight:0.##}".Normalize(25));
            sb.Append($"{luckPercent:0.##%}".Normalize(25));
            sb.Append($"{luckPercent - basePercent:+0.##%;-0.##%}".Normalize(25));
            sb.Append($"{factor}".Normalize(25));
            sb.Append("\n");
        }

        Console.instance.Print(sb.ToString());
    }

    private static List<string> GetLuckTestOptions(int i) => i switch
    {
        2 => GetCreatureNames(i),
        _ => []
    };

    private static string Normalize(this string input, int desiredLength)
    {
        int difference = desiredLength - input.Length;
        if (difference <= 0) return input;
        return input.PadRight(difference);
    }
}