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

    private bool inventoryActive = false;
    private bool selectEquipmentMode = false;
    private Item selectingItem;

    private void Awake()
    {
        coreItemButton.OpenButton.onClick.RemoveAllListeners();
        coreItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingItem != null)
            {
                if (selectingItem is CoreItem coreItem)
                {
                    AddItem(User.me.equipment.core);
                    User.me.equipment.core = coreItem;
                    coreItemButton.SetItem(new InventoryItem
                    {
                        item = selectingItem,
                        count = 1
                    });
                }
                selectEquipmentMode = false;
            }

            ShowItemData(User.me.equipment.core);
        });
        weapon1ItemButton.OpenButton.onClick.RemoveAllListeners();
        weapon1ItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingItem != null)
            {
                if (selectingItem is VoidItem voidItem)
                {
                    AddItem(User.me.equipment.weapon1);
                    User.me.equipment.weapon1 = voidItem;
                    weapon1ItemButton.SetItem(new InventoryItem
                    {
                        item = selectingItem,
                        count = 1
                    });
                }
                selectEquipmentMode = false;
            }

            ShowItemData(User.me.equipment.weapon1);
        });
        weapon2ItemButton.OpenButton.onClick.RemoveAllListeners();
        weapon2ItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingItem != null)
            {
                if (selectingItem is VoidItem voidItem)
                {
                    AddItem(User.me.equipment.weapon2);
                    User.me.equipment.weapon2 = voidItem;
                    weapon2ItemButton.SetItem(new InventoryItem
                    {
                        item = selectingItem,
                        count = 1
                    });
                }
                selectEquipmentMode = false;
            }

            ShowItemData(User.me.equipment.weapon2);
        });
        weapon3ItemButton.OpenButton.onClick.RemoveAllListeners();
        weapon3ItemButton.OpenButton.onClick.AddListener(() =>
        {
            if (selectEquipmentMode && selectingItem != null)
            {
                if (selectingItem is VoidItem voidItem)
                {
                    AddItem(User.me.equipment.weapon3);
                    User.me.equipment.weapon3 = voidItem;
                    weapon3ItemButton.SetItem(new InventoryItem
                    {
                        item = selectingItem,
                        count = 1
                    });
                }
                selectEquipmentMode = false;
            }

            ShowItemData(User.me.equipment.weapon3);
        });
    }

    private void Update()
    {
        if (profilePanel.activeSelf && !inventoryActive)
        {
            inventoryActive = true;
            descriptionPanel.SetActive(false);
            LoadInventory();
        }
        else if (!profilePanel.activeSelf && inventoryActive)
        {
            inventoryActive = false;
            SaveData();
        }
    }

    public void LoadInventory()
    {
        Debug.Log("Load Data");

        if (!profilePanel.activeSelf)
            return;

        foreach (Transform slot in inventoryPanel)
            Destroy(slot.gameObject);

        foreach (var inventoryItem in User.me.inventory)
        {
            if (inventoryItem == null) continue;

            InventorySlot slot = Instantiate(inventorySlot, inventoryPanel);

            slot.SetItem(inventoryItem);

            slot.OpenButton.onClick.AddListener(() => ShowItemData(inventoryItem.item));

            inventorySlots.Add(slot);
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

    private void ShowItemData(Item item)
    {
        if (item == null)
            return;

        itemNameText.text = item.Name;
        itemIconImage.sprite = item.Icon;
        itemDescriptionText.text = item.Description;

        equipItemButton.onClick.RemoveAllListeners();
        equipItemButton.onClick.AddListener(() => EquipItem(item));
        unequipItemButton.onClick.RemoveAllListeners();
        unequipItemButton.onClick.AddListener(() => UnequipItem(item));

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

    private void EquipItem(Item item)
    {
        selectingItem = item;
        selectEquipmentMode = true;
    }

    private void UnequipItem(Item item)
    {
        if (User.me.equipment.core == item)
        {
            User.me.inventory.Add(new InventoryItem
            {
                item = User.me.equipment.core,
                count = 1
            });
            User.me.equipment.core = null;
            coreItemButton.SetItem(null);
        }
        else if (User.me.equipment.weapon1 == item)
        {
            User.me.inventory.Add(new InventoryItem
            {
                item = User.me.equipment.weapon1,
                count = 1
            });
            User.me.equipment.weapon1 = null;
            weapon1ItemButton.SetItem(null);
        }
        else if (User.me.equipment.weapon2 == item)
        {
            User.me.inventory.Add(new InventoryItem
            {
                item = User.me.equipment.weapon2,
                count = 1
            });
            User.me.equipment.weapon2 = null;
            weapon2ItemButton.SetItem(null);
        }
        else if (User.me.equipment.weapon3 == item)
        {
            User.me.inventory.Add(new InventoryItem
            {
                item = User.me.equipment.weapon3,
                count = 1
            });
            User.me.equipment.weapon3 = null;
            weapon3ItemButton.SetItem(null);
        }
        else return;

        equipItemButton.gameObject.SetActive(true);
        unequipItemButton.gameObject.SetActive(false);
    }

    private void AddItem(Item item)
    {
        if (item == null)
            return;

        bool haveData = false;

        foreach (var slot in inventorySlots)
            if (slot.inventoryItem.item == item)
            {
                slot.inventoryItem.count++;
                haveData = true;
                break;
            }

        if (!haveData)
        {
            var inventoryItem = new InventoryItem
            {
                item = item,
                count = 1
            };

            InventorySlot slot = Instantiate(inventorySlot, inventoryPanel);

            slot.SetItem(inventoryItem);

            slot.OpenButton.onClick.AddListener(() => ShowItemData(inventoryItem.item));

            inventorySlots.Add(slot);
        }
    }

    private void SaveData()
    {
        Debug.Log("Save Data");
        StartCoroutine(DatabaseManager.Instance.UpdateEquipment(User.me.equipment, (success, equipmentJson, inventoryJson) =>
        {
            if (success)
            {
                User.me.UpdateEquipment(equipmentJson);
                User.me.UpdateInventory(inventoryJson);
            }
        }));
    }
}
