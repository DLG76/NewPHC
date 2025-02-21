using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class VoidInventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static VoidItem itemMoving;

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text nameText;
    private Transform parentAfterDrag;

    public VoidItem Item { get => _item; }
    private VoidItem _item;

    private bool canUpdate = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        nameText = transform.parent.parent.GetComponentInChildren<TMP_Text>();
    }

    private void FixedUpdate()
    {
        if (canUpdate)
        {
            UpdateUI();
            canUpdate = false;
        }
    }

    public void SetItem(VoidItem item)
    {
        _item = item;

        if (image == null || nameText == null)
            canUpdate = true;
        else
            UpdateUI();
    }

    private void UpdateUI()
    {
        if (_item != null)
        {
            image.sprite = _item.Icon;
            nameText.text = _item.Name;
        }
        else
        {
            if (image != null)
                image.sprite = null;

            if (nameText != null)
                nameText.text = string.Empty;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null && (VoidInventoryUI.Instance == null || !VoidInventoryUI.Instance.InventoryUI.activeSelf)) return;

        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);

        itemMoving = _item;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_item != null && VoidInventoryUI.Instance != null && VoidInventoryUI.Instance.InventoryUI.activeSelf)
        {
            var mousePos = Input.mousePosition;
            (transform as RectTransform).anchoredPosition = new Vector2(mousePos.x, mousePos.y - Screen.height);
        }
        else
            OnEndDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);

        if (itemMoving != null)
        {
            var slotUnderMouse = GetSlotUnderMouse(eventData);
            if (slotUnderMouse != null && slotUnderMouse != parentAfterDrag
                && slotUnderMouse.TryGetComponent(out VoidInventoryItem inventoryItemPressing)
                && inventoryItemPressing != null && this != inventoryItemPressing)
            {
                VoidInventoryUI.Instance?.SwitchItem(this, inventoryItemPressing);
            }
            itemMoving = null;
        }
    }

    private Transform GetSlotUnderMouse(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null)
        {
            return eventData.pointerEnter.transform;
        }
        return null;
    }
}
