using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    public Button OpenButton { get => openButton; }
    [SerializeField] private Button openButton;

    public InventoryItem inventoryItem;

    public void SetItem(InventoryItem inventoryItem)
    {
        this.inventoryItem = inventoryItem;
        LoadUI();
    }

    public void LoadUI()
    {
        if (inventoryItem == null)
        {
            iconImage.sprite = null;
            countText.gameObject.SetActive(false);
            return;
        }

        iconImage.sprite = inventoryItem.item?.Icon;
        if (inventoryItem.item != null && inventoryItem.item.CanStack)
        {
            countText.text = $"x{inventoryItem.count}";
            countText.gameObject.SetActive(true);
        }
        else countText.gameObject.SetActive(false);
    }
}
