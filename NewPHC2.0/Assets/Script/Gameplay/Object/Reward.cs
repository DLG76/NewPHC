using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

[Serializable]
public class Reward
{
    public ItemDropRate[] itemDrops;

    public VoidItem RandomDropItem()
    {
        int totalRate = itemDrops.Sum(i => i.rate);
        if (totalRate <= 0) return null;

        int randomRate = UnityEngine.Random.Range(0, totalRate) + 1;
        foreach (var itemDrop in itemDrops)
        {
            randomRate -= itemDrop.rate;
            if (randomRate <= 0)
                return itemDrop.GetItem();
        }
        return null;
    }
}

[Serializable]
public class ItemDropRate
{
    [Range(0, 100)] public int rate = 50;
    public string itemID;

    public VoidItem GetItem()
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        return ItemManager.Instance.GetItem(itemID);
    }
}