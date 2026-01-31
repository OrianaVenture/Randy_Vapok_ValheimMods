using System.Collections.Generic;
using EpicLoot.Adventure;
using EpicLoot.Adventure.Feature;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void PrintBounties(Terminal.ConsoleEventArgs args)
    {
        if (!EnvMan.instance)
        {
            args.Context.AddString("> EnvMan is null");
            return;
        }
        int interval = args.GetInt(2, AdventureDataManager.Bounties.GetCurrentInterval());
        List<BountyInfo> availableBounties = AdventureDataManager.Bounties.GetAvailableBounties(interval, false);
        if (availableBounties.Count == 0)
        {
            args.Context.AddString("> No Available Bounties");
            return;
        }
        string result = BountiesAdventureFeature.PrintBounties($"> Bounties for Interval {interval}:", availableBounties);
        args.Context.AddString(result);
    }

    private static void PrintPlayerBounties(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        if (player == null)
        {
            args.Context.AddString("> Local Player is null");
            return;
        }
        List<BountyInfo> availableBounties = player.GetAdventureSaveData().Bounties;
        if (availableBounties.Count == 0)
        {
            args.Context.AddString("> No Active Bounties");
            return;
        }
        string result = BountiesAdventureFeature.PrintBounties("> Player Bounties:", availableBounties);
        args.Context.AddString(result);
    }
}