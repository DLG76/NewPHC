using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class FuseUI : MonoBehaviour
{
    [Header("Profile Panel")]
    [SerializeField] private GameObject profilePanel;
    [Header("Fuse Panel")]
    [SerializeField] private GameObject fusePanel;
    [SerializeField] private InventorySlot item1Button;
    [SerializeField] private InventorySlot item2Button;
    [SerializeField] private InventorySlot resultItemButton;
    [SerializeField] private Button fuseButton;
    [Header("Inventory Panel")]
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private InventorySlot inventorySlot;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    private bool fuseActive = false;

    private void Awake()
    {
        item1Button.OpenButton.onClick.RemoveAllListeners();
        item1Button.OpenButton.onClick.AddListener(() =>
        {
            var item = item1Button.inventoryItem?.item;

            if (item != null)
            {
                AddItem(item);
                item1Button.SetItem(null);
                fuseButton.interactable = false;
            }
        });
        item2Button.OpenButton.onClick.RemoveAllListeners();
        item2Button.OpenButton.onClick.AddListener(() =>
        {
            var item = item2Button.inventoryItem?.item;

            if (item != null)
            {
                AddItem(item);
                item2Button.SetItem(null);
                fuseButton.interactable = false;
            }
        });
        resultItemButton.OpenButton.onClick.RemoveAllListeners();
        resultItemButton.OpenButton.onClick.AddListener(() =>
        {
            var item = resultItemButton.inventoryItem?.item;

            if (item != null)
            {
                AddItem(item);
                resultItemButton.SetItem(null);
                fuseButton.interactable = false;
            }
        });

        fuseButton.onClick.RemoveAllListeners();
        fuseButton.onClick.AddListener(Fuse);
    }

    private void ClearItemToFuse()
    {
        AddItem(item1Button.inventoryItem?.item);
        item1Button.SetItem(null);
        AddItem(item2Button.inventoryItem?.item);
        item2Button.SetItem(null);
        AddItem(resultItemButton.inventoryItem?.item);
        resultItemButton.SetItem(null);

        fuseButton.interactable = false;
    }

    private void LoadInventory()
    {
        if (!profilePanel.activeSelf)
            return;

        var nowInventory = User.me.inventory;

        if (nowInventory.Equals(inventorySlots))
            return;

        if (nowInventory.Count == inventorySlots.Count && nowInventory.Count > 0)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                var inventoryItem = nowInventory[i];
                var inventorySlot = inventorySlots[i];

                inventorySlot.SetItem(inventoryItem);
            }
        }
        else
        {
            List<InventorySlot> existSlots = inventorySlots.Where(s => nowInventory.Contains(s.inventoryItem)).ToList();

            foreach (Transform slotObj in inventoryPanel)
                if (slotObj.TryGetComponent(out InventorySlot slot))
                    if (!existSlots.Contains(slot))
                    {
                        inventorySlots.Remove(slot);
                        Destroy(slot.gameObject);
                    }

            foreach (var inventoryItem in nowInventory.Where(i => !existSlots.Select(s => s.inventoryItem).Contains(i)))
            {
                if (inventoryItem == null) continue;

                InventorySlot slot = Instantiate(inventorySlot, inventoryPanel);

                slot.SetItem(inventoryItem);

                slot.OpenButton.onClick.AddListener(() => AddItemToFuse(slot));

                inventorySlots.Add(slot);
            }

            foreach (var slot in existSlots)
                slot.SetItem(slot.inventoryItem);
        }
    }

    private void AddItemToFuse(InventorySlot inventorySlot)
    {
        var item = inventorySlot.inventoryItem?.item;

        if (item == null)
            return;

        if (!(item is VoidItem) && !(item is CoreItem))
            return;

        if (item1Button.inventoryItem?.item == null)
        {
            item1Button.SetItem(new InventoryItem
            {
                item = item,
                count = 1,
            });
            RemoveItem(item);
        }
        else if (item2Button.inventoryItem?.item == null)
        {
            item2Button.SetItem(new InventoryItem
            {
                item = item,
                count = 1,
            });
            RemoveItem(item);
        }

        if (item1Button.inventoryItem?.item != null && item2Button.inventoryItem?.item != null)
            fuseButton.interactable = true;
    }

    private void Fuse()
    {
        var item1 = item1Button.inventoryItem?.item;
        var item2 = item2Button.inventoryItem?.item;
        if (item1 == null || item2 == null)
            return;

        fuseButton.interactable = false;

        StartCoroutine(DatabaseManager.Instance.FuseVoid(item1, item2, (success, resultItem, newInventory) =>
        {
            if (success)
            {
                ClearItemToFuse();
                resultItemButton.SetItem(new InventoryItem
                {
                    item = resultItem,
                    count = 1,
                });

                User.me.UpdateInventory(newInventory);
                RemoveItem(resultItem);
                LoadInventory();

                fuseButton.interactable = false;
            }
            else fuseButton.interactable = true;
        }));
    }

    private InventorySlot AddItem(Item item)
    {
        if (item == null)
            return null;

        foreach (var slot in inventorySlots)
            if (slot.inventoryItem.item?.id == item.id && item.CanStack)
            {
                slot.inventoryItem.count++;
                LoadInventory();
                return slot;
            }

        var inventoryItem = new InventoryItem
        {
            item = item,
            count = 1
        };

        User.me.inventory.Add(inventoryItem);

        LoadInventory();

        return inventorySlots.FirstOrDefault(s => s.inventoryItem == inventoryItem);
    }

    private void RemoveItem(Item item)
    {
        if (item == null)
            return;

        InventoryItem inventoryItem = null;

        foreach (var _inventoryItem in User.me.inventory)
            if (_inventoryItem.item?.id == item.id)
            {
                inventoryItem = _inventoryItem;
                break;
            }

        if (inventoryItem != null)
        {
            inventoryItem.count--;

            if (inventoryItem.count <= 0)
                User.me.inventory.Remove(inventoryItem);

            LoadInventory();
        }
    }

    public IEnumerator LoadScreen()
    {
        ClearItemToFuse();
        LoadInventory();
        yield break;
    }

    public IEnumerator LeaveScreen()
    {
        ClearItemToFuse();
        yield break;
    }
}
