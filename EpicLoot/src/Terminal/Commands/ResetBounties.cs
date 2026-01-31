using EpicLoot.Adventure;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void ResetBounties(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        if (player == null) return;
        
        AdventureSaveData saveData = player.GetAdventureSaveData();
        saveData.Bounties.Clear();
        ResetMinimap();
    }    
}