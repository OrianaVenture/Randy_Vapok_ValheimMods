using EpicLoot.Adventure;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void ResetAdventure(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        if (player == null)
        {
            args.Context.AddString("> Local Player is null");
            return;
        }
        
        AdventureComponent adventureComponent = player.GetComponent<AdventureComponent>();
        adventureComponent.SaveData = new AdventureSaveDataList();
        ResetMinimap();
    }    
}