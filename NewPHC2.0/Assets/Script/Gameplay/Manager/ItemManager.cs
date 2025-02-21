using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemManager : SingletonPersistent<ItemManager>
{
    public VoidItem[] allItems;

    private void Start()
    {
        allItems = Resources.LoadAll<VoidItem>("Item/Items");
    }
    public VoidItem GetItem(string id) => allItems.FirstOrDefault(i => i.id == id);
}
