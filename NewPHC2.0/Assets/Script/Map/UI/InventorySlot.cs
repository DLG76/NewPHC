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

    private Item item;

    public void SetItem(Item item)
    {
        this.item = item;
        LoadUI();
    }

    public void LoadUI()
    {
        if (item == null)
        {
            iconImage.sprite = null;
            countText.gameObject.SetActive(false);
            return;
        }

        iconImage.sprite = item.Icon;
        if (item.CanStack)
        {
            countText.text = $"x{item.Count}";
            countText.gameObject.SetActive(true);
        }
        else countText.gameObject.SetActive(false);
    }
}
