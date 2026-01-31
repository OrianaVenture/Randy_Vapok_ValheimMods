using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private const string HaldorLocationName = "Vendor_BlackForest";
    private static void TeleportToMerchant(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        if (player == null)
        {
            args.Context.AddString("> Local Player is null");
            return;
        }
        
        if (ZoneSystem.instance.FindClosestLocation(HaldorLocationName, player.transform.position, out ZoneSystem.LocationInstance location))
        {
            args.Context.AddString(location.m_position.ToString());
            player.TeleportTo(location.m_position + Vector3.right * 5, player.transform.rotation, true);
        }
    }
}