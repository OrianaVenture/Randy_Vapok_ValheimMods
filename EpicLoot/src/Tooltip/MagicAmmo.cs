namespace EpicLoot;

public partial class MagicTooltip
{
    private void AmmoDamage()
    {
        text.Append(item.GetDamage(qualityLevel, Game.m_worldLevel).GetTooltipString(item.m_shared.m_skillType));
    }
}