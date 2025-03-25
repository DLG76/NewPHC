using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : Singleton<InventoryUI>
{
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private InventorySlot inventorySlot;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    private void Awake()
    {
        LoadInventory();
    }

    public void LoadInventory()
    {
        foreach (Transform slot in inventoryPanel)
            Destroy(slot.gameObject);

        foreach (var item in User.me.inventory)
        {
            if (item == null) continue;

            InventorySlot slot = Instantiate(inventorySlot, inventoryPanel);

            slot.OpenButton.onClick.AddListener(() => ShowItemData(item));

            inventorySlots.Add(slot);
        }
    }

    private void ShowItemData(Item item)
    {
        Debug.Log("--------------------");
        Debug.Log(item.Name);
        Debug.Log(item.Description);
    }
}
