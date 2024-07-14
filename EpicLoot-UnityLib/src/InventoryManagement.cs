using System.Collections.Generic;
using UnityEngine;

namespace EpicLoot_UnityLib;

public class InventoryManagement
{
    static InventoryManagement() { }
    private InventoryManagement() { }
    private static readonly InventoryManagement _instance = new InventoryManagement();

    public static InventoryManagement Instance
    {
        get => _instance;
    }

    private void SendMessage(string message, int amount, Sprite icon)
    {
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft,
            message, amount, icon);
    }

    private Inventory GetInventory()
    {
        Player player = Player.m_localPlayer;

        if (player != null)
        {
            return player.GetInventory();
        }

        return null;
    }

    public List<ItemDrop.ItemData> GetAllItems()
    {
        Inventory inventory = GetInventory();
        if (inventory != null)
        {
            return inventory.GetAllItems();
        }

        return null;
    }

    public bool HasItem(ItemDrop.ItemData item)
    {
        Inventory inventory = GetInventory();

        if (inventory == null || inventory.CountItems(item.m_shared.m_name) < item.m_stack)
        {
            return false;
        }

        return true;
    }

    public int CountItem(ItemDrop.ItemData item)
    {
        return CountItem(item.m_shared.m_name);
    }

    public int CountItem(string item)
    {
        Inventory inventory = GetInventory();

        if (inventory == null)
        {
            return 0;
        }

        return inventory.CountItems(item);
    }

    public void GiveItem(string item, int amount)
    {
        Inventory inventory = GetInventory();
        if (inventory != null)
        {
            AddItem(ref inventory, item, amount);
        }
        else
        {
            DropItem(item, amount);
        }
    }

    public bool GiveItem(ItemDrop.ItemData item)
    {
        Inventory inventory = GetInventory();

        do
        {
            var itemToAdd = item.Clone();
            itemToAdd.m_stack = Mathf.Min(item.m_stack, item.m_shared.m_maxStackSize);
            item.m_stack -= itemToAdd.m_stack;

            if (inventory != null && inventory.CanAddItem(itemToAdd))
            {
                AddItem(ref inventory, itemToAdd);
            }
            else
            {
                DropItem(itemToAdd);
            }
        } while (item.m_stack > 0);

        return true;
    }

    private void AddItem(ref Inventory inventory, string item, int amount)
    {
        inventory.AddItem(item, amount, 1, 0, 0, string.Empty);
    }

    private void AddItem(ref Inventory inventory, ItemDrop.ItemData item)
    {
        inventory.AddItem(item);

        SendMessage($"$msg_added {item.m_shared.m_name}", item.m_stack, item.GetIcon());
    }

    private void DropItem(string item, int amount)
    {
        Player player = Player.m_localPlayer;
        var prefab = ObjectDB.instance.GetItemPrefab(item);

        if (prefab != null)
        {
            var go = GameObject.Instantiate(prefab,
                player.transform.position + player.transform.forward + player.transform.up,
                player.transform.rotation);

            var itemdrop = go.GetComponent<ItemDrop>();
            itemdrop.SetStack(amount);
            itemdrop.GetComponent<Rigidbody>().velocity = Vector3.up * 5f;

            SendMessage($"$msg_dropped {itemdrop.m_itemData.m_shared.m_name}",
                itemdrop.m_itemData.m_stack, itemdrop.m_itemData.GetIcon());
        }
    }

    private void DropItem(ItemDrop.ItemData item)
    {
        Player player = Player.m_localPlayer;
        var itemDrop = ItemDrop.DropItem(item, item.m_stack,
            player.transform.position + player.transform.forward + player.transform.up,
            player.transform.rotation);
        itemDrop.GetComponent<Rigidbody>().velocity = Vector3.up * 5f;

        SendMessage($"$msg_dropped {itemDrop.m_itemData.m_shared.m_name}",
            itemDrop.m_itemData.m_stack, itemDrop.m_itemData.GetIcon());
    }

    public void RemoveItem(ItemDrop.ItemData item)
    {
        RemoveItem(item.m_shared.m_name, item.m_stack);
    }

    public void RemoveItem(string item, int amount)
    {
        Inventory inventory = GetInventory();

        inventory.RemoveItem(item, amount);
    }
}
