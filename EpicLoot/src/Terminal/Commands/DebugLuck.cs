namespace EpicLoot;

public static partial class MagicCommands
{
    private static void PrintPlayersLuck(Terminal.ConsoleEventArgs args)
    {
        LootRoller.DebugLuckFactor();
    }
}