using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void CheckStackQuality(Terminal.ConsoleEventArgs args)
    {
        Terminal context = args.Context;
        context.AddString("CheckStackQuality");
        if (ObjectDB.instance == null)
        {
            context.AddString("> ObjectDB is null");
            return;
        }

        int count = 0;
        for (var i = 0; i < ObjectDB.instance.m_items.Count; ++i)
        {
            GameObject itemPrefab = ObjectDB.instance.m_items[i];
            if (!itemPrefab.TryGetComponent(out ItemDrop itemDrop))
            {
                continue;
            }

            ItemDrop.ItemData itemData = itemDrop.m_itemData;

            if (itemData.m_shared.m_maxStackSize > 1 && itemData.m_shared.m_maxQuality > 1)
            {
                count++;
                context.AddString($"> {itemDrop.name}");
            }
        }

        if (count == 0)
        {
            context.AddString("> (none)");
        }
    }
}