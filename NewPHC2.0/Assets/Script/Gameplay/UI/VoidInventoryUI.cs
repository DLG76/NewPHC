using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoidInventoryUI : Singleton<VoidInventoryUI>
{

    [System.Serializable]
    private class VoidInventoryData
    {
        public Transform Slot { get => _slot; }
        [SerializeField] private Transform _slot;
        public VoidInventoryItem InventoryItem
        {
            get
            {
                if (_inventoryItem == null && _slot != null)
                    _inventoryItem = _slot.GetComponentInChildren<VoidInventoryItem>();

                return _inventoryItem;
            }
            set
            {
                if (value != null)
                    _inventoryItem = value;
            }
        }
        [SerializeField] private VoidInventoryItem _inventoryItem;
        public VoidItem Item { get; private set; }

        public VoidInventoryData(Transform slot, VoidItem item)
        {
            _slot = slot;

            SetItem(item);
        }

        public void SetItem(VoidItem item)
        {
            Item = item;

            InventoryItem.SetItem(item);
        }
    }

    [System.Serializable]
    private class VoidHotbarData : VoidInventoryData
    {
        public KeyCode keyCode;

        public VoidHotbarData(Transform slot, VoidItem item) : base(slot, item)
        {

        }
    }

    [SerializeField] private List<VoidHotbarData> itemHotbars = new List<VoidHotbarData>();
    private List<VoidInventoryData> itemInventories = new List<VoidInventoryData>();

    public GameObject InventoryUI { get => _inventoryUI; }
    [SerializeField] private GameObject _inventoryUI;
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private VoidItem[] starterItems;

    private VoidHotbarData itemHotbarSelected;
    private Coroutine selectCoroutine;

    private void Awake()
    {

    }

    private void Start()
    {
        VoidInventory.Clear(true); // use for clear item on start in tester

        SetupItemList();

        foreach (var starterItem in starterItems)
        {
            AddItem(starterItem);
        }

        RandomSelectItemHotbar(null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            _inventoryUI.SetActive(!_inventoryUI.activeSelf);

        foreach (var itemHotbar in itemHotbars)
            if (Input.GetKeyDown(itemHotbar.keyCode))
            {
                if (selectCoroutine != null)
                    StopCoroutine(selectCoroutine);

                selectCoroutine = StartCoroutine(SelectItemHotbarIE(itemHotbar));
            }
    }

    private void RandomSelectItemHotbar(VoidHotbarData banHotbar) => selectCoroutine = StartCoroutine(RandomSelectItemHotbarIE(banHotbar));

    private IEnumerator RandomSelectItemHotbarIE(VoidHotbarData banHotbar)
    {
        VoidHotbarData itemHotbarSelecting = null;

        foreach (var itemHotbar in itemHotbars)
            if (itemHotbar.Item != null && !itemHotbar.Equals(banHotbar))
            {
                itemHotbarSelecting = itemHotbar;
                break;
            }

        if (itemHotbarSelecting != null)
        {
            yield return SelectItemHotbarIE(itemHotbarSelecting);
        }
    }

    public void AddItem(VoidItem item)
    {
        if (item == null) return;

        if (VoidInventory.Hotbars.Where(kvp => kvp.Value != null).Count() < itemHotbars.Count)
        {
            var existingHotbarData = itemHotbars.Find(i => i.Item == null);

            AddItemHotbar(itemHotbars.IndexOf(existingHotbarData), item);
        }
        else
            AddItemInventory(item);
    }

    private void AddItemHotbar(int hotbarUIIndex, VoidItem item)
    {
        if (item == null) return;

        int hotbarIndex = VoidInventory.AddHotbar(item);

        if (hotbarIndex > -1 && hotbarIndex == hotbarUIIndex)
        {
            bool haveItemInHotbar = itemHotbars.Where(i => i.Item != null).Any();

            var hotbarData = itemHotbars[hotbarUIIndex];
            hotbarData.SetItem(item);

            if (!haveItemInHotbar || hotbarData == itemHotbarSelected)
                SelectItemHotbar(hotbarData);
        }
    }

    private void AddItemInventory(VoidItem item)
    {
        if (item == null) return;

        int inventoryIndex = VoidInventory.AddInventory(item);

        if (inventoryIndex > -1)
        {

            var button = Instantiate(Resources.Load<Button>("UI/InventorySlotUI"));
            button.name = item.Name;

            var voidInventoryData = new VoidInventoryData(button.transform, item);

            button.onClick.AddListener(() => SelectItem(voidInventoryData));
            button.transform.SetParent(inventoryPanel, false);

            itemInventories.Add(voidInventoryData);
        }
    }

    public void RemoveItem(VoidItem item, bool destroy)
    {
        if (item == null) return;

        var existHotbarData = itemHotbars.Find(i => i.Item != null && i.Item.Equals(item));
        var existInventoryData = itemInventories.Find(i => i.Item != null && i.Item.Equals(item));

        if (existHotbarData != null)
            RemoveItemHotbar(existHotbarData, destroy);
        else if (existInventoryData != null)
            RemoveItemInventory(existInventoryData, item, destroy);
    }

    private void RemoveItemHotbar(VoidHotbarData inventoryData, bool destroy)
    {
        if (inventoryData == null) return;

        if (inventoryData != null)
        {
            bool success = VoidInventory.RemoveHotbar(itemHotbars.IndexOf(inventoryData), destroy);

            if (success)
            {
                inventoryData.SetItem(null);
                RandomSelectItemHotbar(inventoryData);
            }
        }
    }

    private void RemoveItemInventory(VoidInventoryData inventoryData, VoidItem item, bool destroy)
    {
        if (item == null || inventoryData == null) return;

        if (inventoryData != null)
        {
            int inventoryIndex = VoidInventory.RemoveInventory(item, destroy);

            if (inventoryIndex > -1 && itemInventories.IndexOf(inventoryData) == inventoryIndex)
            {
                inventoryData.SetItem(null);
                itemInventories.Remove(inventoryData);
            }
        }
    }

    private void SetupItemList()
    {
        foreach (Transform slot in inventoryPanel)
            Destroy(slot.gameObject);

        foreach (var itemHotbar in itemHotbars)
            itemHotbar.SetItem(itemHotbar.Item);

        var allItems = new List<VoidItem>(VoidInventory.Hotbars.Select(kvp => kvp.Value));
        allItems.AddRange(VoidInventory.Inventories);

        VoidInventory.Clear(false);

        foreach (var item in allItems)
            AddItem(item);
    }

    private void SelectItem(VoidInventoryData inventoryData)
    {
        ShowItemData(inventoryData);
    }

    private void ShowItemData(VoidInventoryData inventoryData)
    {
        return; // remove หากมี Text GameObject
        nameText.text = inventoryData.Item.Name;
        descriptionText.text = inventoryData.Item.Description;
    }

    private bool SelectItemHotbar(VoidHotbarData itemHotbar)
    {
        if (PlayerCombat.LocalPlayerInstance == null) return false;

        bool success = false;

        if (itemHotbar != null)
        {
            if (PlayerCombat.LocalPlayerInstance.SelectVoidItem(itemHotbar.Item))
            {
                itemHotbarSelected = itemHotbar;
                success = true;
            }
        }

        LoadItemHotbar();

        return success;
    }

    private IEnumerator SelectItemHotbarIE(VoidHotbarData itemHotbar)
    {
        while (!SelectItemHotbar(itemHotbar))
        {
            yield return null;
        }
    }

    private void LoadItemHotbar()
    {
        if (PlayerCombat.LocalPlayerInstance == null) return;

        foreach (var itemHotbar in itemHotbars)
        {
            if (itemHotbarSelected == itemHotbar)
                itemHotbar.Slot.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            else
                itemHotbar.Slot.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 1);
        }
    }

    public void SwitchItem(VoidInventoryItem inventoryItem1, VoidInventoryItem inventoryItem2)
    {
        if (inventoryItem1 == null || inventoryItem2 == null) return;

        VoidHotbarData hotbarData1 = itemHotbars.Find(i => i.InventoryItem == inventoryItem1);
        VoidHotbarData hotbarData2 = itemHotbars.Find(i => i.InventoryItem == inventoryItem2);

        VoidInventoryData inventoryData1 = itemInventories.Find(i => i.Item != null && i.InventoryItem == inventoryItem1);
        VoidInventoryData inventoryData2 = itemInventories.Find(i => i.Item != null && i.InventoryItem == inventoryItem2);

        VoidItem item1 = inventoryItem1.Item;
        VoidItem item2 = inventoryItem2.Item;

        if (hotbarData1 != null && hotbarData1.Item != null && hotbarData2 != null && hotbarData2.Item != null)
        {
            VoidInventory.SwitchItem(itemHotbars.IndexOf(hotbarData1), itemHotbars.IndexOf(hotbarData2));

            hotbarData1.SetItem(item2);
            hotbarData2.SetItem(item1);
        }
        else if (inventoryData1 != null && hotbarData2 != null && hotbarData2.Item != null)
        {
            VoidInventory.SwitchItem(item1, itemHotbars.IndexOf(hotbarData2));

            inventoryData1.SetItem(item2);
            hotbarData2.SetItem(item1);
        }
        else if (hotbarData1 != null && hotbarData1.Item != null && inventoryData2 != null)
        {
            VoidInventory.SwitchItem(itemHotbars.IndexOf(hotbarData1), item2);

            hotbarData1.SetItem(item2);
            inventoryData2.SetItem(item1);
        }
        else if (inventoryData1 != null && inventoryData2 != null)
        {
            VoidInventory.SwitchItem(item1, item2);

            inventoryData1.SetItem(item2);
            inventoryData2.SetItem(item1);
        }
        else if (hotbarData1 != null && hotbarData1.Item == null && inventoryData2 != null)
        {
            RemoveItemInventory(inventoryData2, inventoryData2.Item, false);
            AddItemHotbar(itemHotbars.IndexOf(hotbarData1), inventoryData2.Item);
        }
        else if (inventoryData1 != null && hotbarData2 != null && hotbarData2.Item == null)
        {
            RemoveItemInventory(inventoryData1, inventoryData1.Item, false);
            AddItemHotbar(itemHotbars.IndexOf(hotbarData2), inventoryData1.Item);
        }
        else return;

        if (itemHotbarSelected != null && (hotbarData1 == itemHotbarSelected || hotbarData2 == itemHotbarSelected))
            SelectItemHotbar(itemHotbarSelected);
    }
}
