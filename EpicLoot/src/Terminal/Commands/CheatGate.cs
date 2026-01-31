namespace EpicLoot;

public static partial class MagicCommands
{
    private static void ToggleCheatGating(Terminal.ConsoleEventArgs args)
    {
        LootRoller.CheatDisableGating = !LootRoller.CheatDisableGating;
        args.Context.AddString($"> Disable gating for magic item drops: {LootRoller.CheatDisableGating}");
    }    
}