using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void TeleportToMerchant(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        if (player == null) return;
        
        if (ZoneSystem.instance.FindClosestLocation("Vendor_BlackForest", player.transform.position, out ZoneSystem.LocationInstance location))
        {
            Console.instance.AddString(location.m_position.ToString());
            player.TeleportTo(location.m_position + Vector3.right * 5, player.transform.rotation, true);
        }
    }
}