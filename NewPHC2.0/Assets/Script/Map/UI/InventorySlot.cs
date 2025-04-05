using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image IconImage { get => iconImage; }
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    public Button OpenButton { get => openButton; }
    [SerializeField] private Button openButton;
    private CanvasGroup canvasGroup;

    public InventoryItem inventoryItem;

    private void Awake()
    {
        canvasGroup = openButton.GetComponent<CanvasGroup>();
    }

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
            iconImage.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
            return;
        }

        iconImage.sprite = inventoryItem.item?.Icon;
        iconImage.gameObject.SetActive(inventoryItem.item != null);
        if (inventoryItem.item != null && inventoryItem.item.CanStack)
        {
            countText.text = $"x{inventoryItem.count}";
            countText.gameObject.SetActive(true);
        }
        else countText.gameObject.SetActive(false);
    }

    public void SetAlpha(float alpha)
    {
        canvasGroup.DOFade(alpha, 0.3f).SetEase(Ease.OutQuad);
    }
}
