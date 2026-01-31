using EpicLoot.Adventure;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void ResetTreasureMap(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        if (player == null) return;
        
        AdventureSaveData saveData = player.GetAdventureSaveData();
        saveData.TreasureMaps.Clear();
        saveData.NumberOfTreasureMapsOrBountiesStarted = 0;
        ResetMinimap();
    }    
    
    private static void ResetMinimap()
    {
        PinJob pinJob = new PinJob
        {
            Task = MinimapPinQueueTask.RefreshAll
        };
        MinimapController.AddPinJobToQueue(pinJob);
    }
}