using System.Collections.Generic;
using UnityEngine;

namespace AdvancedPortals;

public class AdvancedPortal : TeleportWorld
{
    public static Dictionary<string, HashSet<string>> AllowedItems = new Dictionary<string, HashSet<string>>();

    private void Awake()
    {
        base.Awake();
    }

    public static void SetAllowedItems(string key, HashSet<string> items)
    {
        if (AllowedItems.ContainsKey(key))
        {
            AllowedItems[key] = items;
        }
        else
        {
            AllowedItems.Add(key, items);
        }
    }

    public static bool AllowedItem(string portalName, string item)
    {
        return AllowedItems[portalName].Contains(item);
    }
}
