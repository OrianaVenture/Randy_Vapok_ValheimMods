using EpicLoot.Abilities;
using EpicLoot.Adventure;
using EpicLoot.Crafting;
using EpicLoot.CraftingV2;
using EpicLoot.GatedItemType;
using EpicLoot.LegendarySystem;
using EpicLoot_UnityLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace EpicLoot.src.General
{
    internal class TerminalCommands
    {
        internal static void AddTerminalCommands()
        {
            CommandManager.Instance.AddConsoleCommand(new LuckTestCommand());
            CommandManager.Instance.AddConsoleCommand(new PrintConfig());
        }
        


        internal class LuckTestCommand : ConsoleCommand
        {
            public override string Name => "lucktest";
            public override string Help => "Rolls an example loot table with the specified luck eg: lucktest Greydwarf 1.0";
            public override bool IsCheat => true;
            public override void Run(string[] args)
            {
                string lootTable = "Greydwarf";
                float luckFactor = 0f;
                try {
                    if (args.Length == 2) {
                        lootTable = args[0];
                        luckFactor = float.Parse(args[1]);
                    } else if (args.Length == 1) {
                        luckFactor = float.Parse(args[0]);
                    } else {
                        Console.instance.Print($"Using Lucktest Defaults: lucktest {lootTable} {luckFactor}");
                    }
                }
                catch
                {
                    Console.instance.Print($"lucktest invalid arguments, was 'lucktest {string.Join(" ", args)}' using the default: 'lucktest {lootTable} {luckFactor}' \n Supported formats are:\n  lucktest\n  lucktest 1.2\n  lucktest Neck 1.2");
                }
                LootRoller.PrintLuckTest(lootTable, luckFactor);
            }
        }

        internal class PrintConfig : ConsoleCommand
        {
            public override string Name => "printconfig";
            public override string Help => "Prints out the Epic Loot current configuration of the specified type";
            public override bool IsCheat => false;
            public override void Run(string[] args)
            {
                List<string> configs = new List<string>() { "loottable", "abilities", "adventuredata", "enchantcosts", "enchantingupgrades", "iteminfo", "itemnames", "legendaries", "magiceffects", "materialconversion", "recipes" };
                string patchType = "loottable";
                try
                {
                    if (args.Length == 1) {
                        string type = args[0].Trim();
                        if (!configs.Contains(type)) {
                            Console.instance.Print($"printconfig argument must be one of [{string.Join(", ", configs)}]");
                            return;
                        } else { patchType = type; }
                    } else {
                        Console.instance.Print($"Using printconfig Defaults: printconfig {patchType}");
                    }
                } catch {
                    Console.instance.Print($"printconfig invalid arguments, was 'lucktest {args}' using: printconfig {patchType}");
                }
                switch(patchType.ToLower())
                {
                    case "loottable":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(LootRoller.Config, Formatting.Indented));
                        break;
                    case "abilities":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(AbilityDefinitions.Config, Formatting.Indented));
                        break;
                    case "adventuredata":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(AdventureDataManager.Config, Formatting.Indented));
                        break;
                    case "enchantcosts":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(EnchantCostsHelper.Config, Formatting.Indented));
                        break;
                    case "enchantingupgrades":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(EnchantingTableUpgrades.Config, Formatting.Indented));
                        break;
                    case "iteminfo":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(GatedItemTypeHelper.gatedConfig, Formatting.Indented));
                        break;
                    case "itemnames":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(MagicItemNames.Config, Formatting.Indented));
                        break;
                    case "legendaries":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(UniqueLegendaryHelper.Config, Formatting.Indented));
                        break;
                    case "magiceffects":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(MagicItemEffectDefinitions.AllDefinitions, Formatting.Indented));
                        break;
                    case "materialconversion":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(MaterialConversions.Config, Formatting.Indented));
                        break;
                    case "recipes":
                        EpicLoot.LogWarning(JsonConvert.SerializeObject(RecipesHelper.Config, Formatting.Indented));
                        break;
                }
            }
        }
    }
}
