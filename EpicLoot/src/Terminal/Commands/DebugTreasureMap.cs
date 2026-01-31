using EpicLoot.Adventure;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void DebugTreasureMap(Terminal.ConsoleEventArgs args)
    {
        MinimapController.DebugMode = !MinimapController.DebugMode;
        args.Context.AddString($"> Treasure Map Debug Mode: {MinimapController.DebugMode}");
    }    
}