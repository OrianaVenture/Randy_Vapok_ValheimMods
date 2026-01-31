using EpicLoot.Abilities;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void ResetCooldowns(Terminal.ConsoleEventArgs args)
    {
        Player player = Player.m_localPlayer;
        if (player == null) return;
        
        AbilityController abilityController = player.GetComponent<AbilityController>();
        if (abilityController == null) return;
        
        foreach (Ability ability in abilityController.CurrentAbilities)
        {
            ability.ResetCooldown();
        }
    }
}