using System;
using EpicLoot.Adventure.Feature;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void TestBountyNames(Terminal.ConsoleEventArgs args)
    {
        Random random = new Random();
        int count = args.GetInt(2, 10);
        for (int i = 0; i < count; ++i)
        {
            string name = BountiesAdventureFeature.GenerateTargetName(random);
            args.Context.AddString(name);
        }
    }    
}