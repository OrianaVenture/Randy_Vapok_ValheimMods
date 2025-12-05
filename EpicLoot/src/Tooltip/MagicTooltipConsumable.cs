namespace EpicLoot;

public partial class MagicTooltip
{
    private void AddFoodHealth()
    {
        text.AppendFormat("\n$item_food_health: <color=orange>{0}</color>", item.m_shared.m_food);
    }

    private void AddFoodStamina()
    {
        text.AppendFormat("\n$item_food_stamina: <color=orange>{0}</color>", item.m_shared.m_foodStamina);
    }

    private void AddFoodBurn()
    {
        text.AppendFormat("\n$item_food_duration: <color=orange>{0}s</color>", item.m_shared.m_foodBurnTime);
    }

    private void AddFoodRegen()
    {
        text.AppendFormat("\n$item_food_regen: <color=orange>{0} hp/tick</color>", item.m_shared.m_foodRegen);
    }
}