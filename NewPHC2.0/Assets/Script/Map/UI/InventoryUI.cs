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
    [Header("Description Panel")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Button equipItemButton;
    [SerializeField] private Button unequipItemButton;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    private void Awake()
    {
        LoadInventory();
    }

    private void OnEnable()
    {
        descriptionPanel.SetActive(false);
    }

    public void LoadInventory()
    {
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
    }

    private void ShowItemData(Item item)
    {
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
        if (User.me.equipment.weapon1 == item)
        {
            User.me.inventory.Add(new InventoryItem
            {
                item = User.me.equipment.weapon1,
                count = 1
            });
            User.me.equipment.weapon1 = null;
        }
        if (User.me.equipment.weapon2 == item)
        {
            User.me.inventory.Add(new InventoryItem
            {
                item = User.me.equipment.weapon2,
                count = 1
            });
            User.me.equipment.weapon2 = null;
        }
        if (User.me.equipment.weapon3 == item)
        {
            User.me.inventory.Add(new InventoryItem
            {
                item = User.me.equipment.weapon3,
                count = 1
            });
            User.me.equipment.weapon3 = null;
        }
    }

    private void UnequipItem(Item item)
    {

    }

    private void OnDisable() => SaveData();

    private void SaveData()
    {

    }
}
