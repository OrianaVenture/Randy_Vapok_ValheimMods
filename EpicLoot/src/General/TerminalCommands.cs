using Jotunn.Entities;
using Jotunn.Managers;


namespace EpicLoot.src.General
{
    internal class TerminalCommands
    {
        internal static void AddTerminalCommands()
        {
            CommandManager.Instance.AddConsoleCommand(new LuckTestCommand());
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
                    if (args.Length == 2)
                    {
                        lootTable = args[0];
                        luckFactor = float.Parse(args[1]);
                    } else if (args.Length == 1) {
                        luckFactor = float.Parse(args[0]);
                    } else
                    {
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
    }
}
