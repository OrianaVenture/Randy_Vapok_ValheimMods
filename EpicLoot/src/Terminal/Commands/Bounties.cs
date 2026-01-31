using System.Collections.Generic;
using EpicLoot.Adventure;
using EpicLoot.Adventure.Feature;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void PrintBounties(Terminal.ConsoleEventArgs args)
    {
        if (!EnvMan.instance) return;
        int interval = args.GetInt(2, AdventureDataManager.Bounties.GetCurrentInterval());
        List<BountyInfo> availableBounties = AdventureDataManager.Bounties.GetAvailableBounties(interval, false);
        BountiesAdventureFeature.PrintBounties($"Bounties for Interval {interval}:", availableBounties);
    }

    private static void PrintPlayerBounties(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        List<BountyInfo> availableBounties = player.GetAdventureSaveData().Bounties;
        BountiesAdventureFeature.PrintBounties($"Player Bounties:", availableBounties);
    }
}