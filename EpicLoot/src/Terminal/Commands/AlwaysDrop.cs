namespace EpicLoot;

public static partial class MagicCommands
{
    private static void ToggleAlwaysDrop(Terminal.ConsoleEventArgs args)
    {
        Terminal context = args.Context;
        EpicLoot.AlwaysDropCheat = !EpicLoot.AlwaysDropCheat;
        context.AddString($"> Always Drop: <color={HEX_LightRed}>{EpicLoot.AlwaysDropCheat}</color>");
    }    
}