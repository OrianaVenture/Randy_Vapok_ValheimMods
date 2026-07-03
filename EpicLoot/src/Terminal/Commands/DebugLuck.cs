namespace EpicLoot;

public static partial class MagicCommands
{
    private static void PrintPlayersLuck(Terminal.ConsoleEventArgs args)
    {
        string result = LootRoller.DebugLuckFactor();
        args.Context.AddString(result);
    }
}