using System.Collections.Generic;
using System.Text;
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
        string result = PrintBounties($"> Bounties for Interval {interval}:", availableBounties);
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
        string result = PrintBounties("> Player Bounties:", availableBounties);
        args.Context.AddString(result);
    }

    private static string PrintBounties(string label, List<BountyInfo> results)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(label);
        for (int index = 0; index < results.Count; ++index)
        {
            BountyInfo bountyInfo = results[index];

            sb.Append($"{index} - ");
            sb.Append($"interval: <color=orange>{bountyInfo.Interval}</color>");
            sb.Append($", biome: <color=orange>{bountyInfo.Biome}</color>");
            sb.Append($", name: <color=orange>{bountyInfo.TargetName}</color>");
            sb.Append($", ID: <color=orange>{bountyInfo.ID}</color>");
            sb.Append($", state: <color=orange>{bountyInfo.State}</color>\n");
        }
        return sb.ToString();
    }
}