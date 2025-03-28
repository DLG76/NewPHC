using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : Singleton<InventoryUI>
{
    [Header("Profile Panel")]
    [SerializeField] private GameObject profilePanel;
    [Header("Inventory Panel")]
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private InventorySlot inventorySlot;
    [Header("Description Panel")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Button equipItemButton;
    [SerializeField] private Button unequipItemButton;
    [Header("Equipments")]
    [SerializeField] private InventorySlot coreItemButton;
    [SerializeField] private InventorySlot weapon1ItemButton;
    [SerializeField] private InventorySlot weapon2ItemButton;
    [SerializeField] private InventorySlot weapon3ItemButton;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    private Equipment oldEquipment;
    private bool selectEquipmentMode = false;
    private InventorySlot selectingSlot;

    private void Awake()
    {
        coreItemButton.OpenButton.onClick.RemoveAllListeners();
        coreItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingSlot != null && selectingSlot.inventoryItem?.item != null)
            {
                var selectingItem = selectingSlot.inventoryItem?.item;

                if (selectingItem is CoreItem coreItem)
                {
                    selectingSlot = AddItem(User.me.equipment.core);
                    User.me.equipment.core = coreItem;
                    RemoveItem(coreItem);
                }
                selectEquipmentMode = false;
            }

            ShowItemData(coreItemButton);
        });
        weapon1ItemButton.OpenButton.onClick.RemoveAllListeners();
        weapon1ItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingSlot != null && selectingSlot.inventoryItem?.item != null)
            {
                var selectingItem = selectingSlot.inventoryItem?.item;

                if (selectingItem is VoidItem voidItem)
                {
                    selectingSlot = AddItem(User.me.equipment.weapon1);
                    User.me.equipment.weapon1 = voidItem;
                    RemoveItem(voidItem);
                }
                selectEquipmentMode = false;
            }

            ShowItemData(weapon1ItemButton);
        });
        weapon2ItemButton.OpenButton.onClick.RemoveAllListeners();
        weapon2ItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingSlot != null && selectingSlot.inventoryItem?.item != null)
            {
                var selectingItem = selectingSlot.inventoryItem?.item;

                if (selectingItem is VoidItem voidItem)
                {
                    selectingSlot = AddItem(User.me.equipment.weapon2);
                    User.me.equipment.weapon2 = voidItem;
                    RemoveItem(voidItem);
                }
                selectEquipmentMode = false;
            }

            ShowItemData(weapon2ItemButton);
        });
        weapon3ItemButton.OpenButton.onClick.RemoveAllListeners();
        weapon3ItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingSlot != null && selectingSlot.inventoryItem?.item != null)
            {
                var selectingItem = selectingSlot.inventoryItem?.item;

                if (selectingItem is VoidItem voidItem)
                {
                    selectingSlot = AddItem(User.me.equipment.weapon3);
                    User.me.equipment.weapon3 = voidItem;
                    RemoveItem(voidItem);
                }
                selectEquipmentMode = false;
            }

            ShowItemData(weapon3ItemButton);
        });
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

                slot.OpenButton.onClick.AddListener(() => ShowItemData(slot));

                inventorySlots.Add(slot);
            }

            foreach (var slot in existSlots)
                slot.SetItem(slot.inventoryItem);
        }

        coreItemButton.SetItem(new InventoryItem
        {
            item = User.me.equipment.core,
            count = 1
        });
        weapon1ItemButton.SetItem(new InventoryItem
        {
            item = User.me.equipment.weapon1,
            count = 1
        });
        weapon2ItemButton.SetItem(new InventoryItem
        {
            item = User.me.equipment.weapon2,
            count = 1
        });
        weapon3ItemButton.SetItem(new InventoryItem
        {
            item = User.me.equipment.weapon3,
            count = 1
        });
    }

    private void ShowItemData(InventorySlot inventorySlot)
    {
        if (inventorySlot == null)
            return;

        var item = inventorySlot.inventoryItem?.item;

        if (item == null)
            return;

        itemNameText.text = item.Name;
        itemIconImage.sprite = item.Icon;
        itemDescriptionText.text = item.Description;

        equipItemButton.onClick.RemoveAllListeners();
        equipItemButton.onClick.AddListener(() => EquipItem(inventorySlot));
        unequipItemButton.onClick.RemoveAllListeners();
        unequipItemButton.onClick.AddListener(() => UnequipItem(inventorySlot));

        if (item is VoidItem)
            if (User.me.equipment.weapon1 == item ||
                User.me.equipment.weapon2 == item ||
                User.me.equipment.weapon3 == item)
            {
                equipItemButton.gameObject.SetActive(false);
                unequipItemButton.gameObject.SetActive(true);
            }
            else
            {
                equipItemButton.gameObject.SetActive(true);
                unequipItemButton.gameObject.SetActive(false);
            }
        else if (item is CoreItem)
            if (User.me.equipment.core == item)
            {
                equipItemButton.gameObject.SetActive(false);
                unequipItemButton.gameObject.SetActive(true);
            }
            else
            {
                equipItemButton.gameObject.SetActive(true);
                unequipItemButton.gameObject.SetActive(false);
            }

        descriptionPanel.SetActive(true);
    }

    private void EquipItem(InventorySlot inventorySlot)
    {
        selectingSlot = inventorySlot;
        selectEquipmentMode = true;
    }

    private void UnequipItem(InventorySlot inventorySlot)
    {
        var item = inventorySlot.inventoryItem?.item;

        if (item == null)
            return;

        if (User.me.equipment.core == item)
        {
            selectingSlot = AddItem(item);
            User.me.equipment.core = null;
            coreItemButton.SetItem(null);
            ShowItemData(selectingSlot);
        }
        else if (User.me.equipment.weapon1 == item)
        {
            selectingSlot = AddItem(item);
            User.me.equipment.weapon1 = null;
            weapon1ItemButton.SetItem(null);
            ShowItemData(selectingSlot);
        }
        else if (User.me.equipment.weapon2 == item)
        {
            selectingSlot = AddItem(item);
            User.me.equipment.weapon2 = null;
            weapon2ItemButton.SetItem(null);
            ShowItemData(selectingSlot);
        }
        else if (User.me.equipment.weapon3 == item)
        {
            selectingSlot = AddItem(item);
            User.me.equipment.weapon3 = null;
            weapon3ItemButton.SetItem(null);
            ShowItemData(selectingSlot);
        }
        else return;

        equipItemButton.gameObject.SetActive(true);
        unequipItemButton.gameObject.SetActive(false);
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
        descriptionPanel.SetActive(false);
        LoadInventory();
        yield break;
    }

    public IEnumerator LeaveScreen()
    {
        if (User.me.equipment?.core == oldEquipment?.core &&
            User.me.equipment?.weapon1 == oldEquipment?.weapon1 &&
            User.me.equipment?.weapon2 == oldEquipment?.weapon2 &&
            User.me.equipment?.weapon3 == oldEquipment?.weapon3)
            yield break;

        yield return DatabaseManager.Instance.UpdateEquipment(User.me.equipment, (success, equipmentJson, inventoryJson) =>
        {
            if (success)
            {
                User.me.UpdateEquipment(equipmentJson);
                User.me.UpdateInventory(inventoryJson);
            }
        });
    }
}
