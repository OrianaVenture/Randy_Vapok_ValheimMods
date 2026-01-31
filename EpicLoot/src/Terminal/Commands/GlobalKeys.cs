namespace EpicLoot;

public static partial class MagicCommands
{
    private static void PrintGlobalKeys(Terminal.ConsoleEventArgs args)
    {
        if (ZoneSystem.instance != null)
        {
            args.Context.AddString("> Print Global Keys:");
            foreach (string globalKey in ZoneSystem.instance.GetGlobalKeys())
            {
                args.Context.AddString("> " + globalKey);
            }
        }
    }
}