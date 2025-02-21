using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public enum VoidInventoryChange
{
    Hotbar,
    Inventory
}

public class VoidInventory
{
    public static Dictionary<int, VoidItem> Hotbars { get => _hotbars; }
    private static Dictionary<int, VoidItem> _hotbars = new Dictionary<int, VoidItem>();

    public static List<VoidItem> Inventories { get => _inventories; }
    private static List<VoidItem> _inventories = new List<VoidItem>();

    private static VoidItem Create(VoidItem item)
    {
        if (item == null) return null;

        if (item.IsRealInstance)
            item = VoidItem.CreateNewItem(item);

        return item;
    }

    public static int AddHotbar(VoidItem item)
    {
        item = Create(item);

        foreach (var kvp in _hotbars)
            if (kvp.Value == null)
            {
                _hotbars[kvp.Key] = item;
                return kvp.Key;
            }

        _hotbars.Add(_hotbars.Count, item);

        return _hotbars.Count - 1;
    }

    public static int AddInventory(VoidItem item)
    {
        item = Create(item);

        _inventories.Add(item);

        return _inventories.Count - 1;
    }

    public static bool RemoveHotbar(int index, bool destroy)
    {
        if (index > -1 && index < _hotbars.Count)
        {
            var voidItem = _hotbars[index];

            _hotbars[index] = null;

            if (destroy)
                Object.Destroy(voidItem);

            return true;
        }

        return false;
    }

    public static int RemoveInventory(VoidItem item, bool destroy)
    {
        VoidItem existingInventoryItem = _inventories.Find(i => i.Equals(item));

        if (existingInventoryItem != null)
        {
            int index = _inventories.IndexOf(existingInventoryItem);

            _inventories.Remove(existingInventoryItem);

            if (destroy)
                Object.Destroy(existingInventoryItem);

            return index;
        }

        return -1;
    }

    public static void Clear(bool destroy)
    {
        var hotbars = new Dictionary<int, VoidItem>(_hotbars);
        var inventories = new List<VoidItem>(_inventories);

        foreach (var kvp in hotbars)
            RemoveHotbar(kvp.Key, destroy);

        foreach (var item in inventories)
            RemoveInventory(item, destroy);

        _hotbars.Clear();
        _inventories.Clear();
    }

    public static bool SwitchItem(VoidItem item1, VoidItem item2)
    {
        int index1InItems = _inventories.IndexOf(item1);
        int index2InItems = _inventories.IndexOf(item2);

        if (index1InItems != -1 && index2InItems != -1)
        {
            _inventories[index1InItems] = item2;
            _inventories[index2InItems] = item1;
            return true;
        }

        return false;
    }

    public static bool SwitchItem(VoidItem item1, int index2InHotbar)
    {
        int index1InInventory = _inventories.IndexOf(item1);
        VoidItem item2 = _hotbars[index2InHotbar];

        if (index1InInventory != -1 && index2InHotbar != -1)
        {
            _inventories[index1InInventory] = item2;
            _hotbars[index2InHotbar] = item1;
            return true;
        }

        return false;
    }

    public static bool SwitchItem(int index1InHotbar, VoidItem item2)
    {
        VoidItem item1 = _hotbars[index1InHotbar];
        int index2InInventory = _inventories.IndexOf(item2);

        if (index1InHotbar != -1 && index2InInventory != -1)
        {
            _hotbars[index1InHotbar] = item2;
            _inventories[index2InInventory] = item1;
            return true;
        }

        return false;
    }

    public static bool SwitchItem(int index1InHotbar, int index2InHotbar)
    {
        VoidItem item1 = _hotbars[index1InHotbar];
        VoidItem item2 = _hotbars[index2InHotbar];

        if (index1InHotbar != -1 && index2InHotbar != -1)
        {
            _hotbars[index1InHotbar] = item2;
            _hotbars[index2InHotbar] = item1;
            return true;
        }

        return false;
    }

    public static VoidItem GetItemByID(string id)
    {
        var voidItems = Resources.LoadAll<VoidItem>("Item/Void");

        return voidItems.FirstOrDefault(v => v.ID == id);
    }
}