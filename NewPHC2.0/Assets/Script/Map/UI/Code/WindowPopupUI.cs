using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowPopupUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private static List<WindowPopupUI> Instances = new List<WindowPopupUI>();

    [SerializeField] private RectTransform headerRect;
    [SerializeField] private Button quitWindowButton;
    [SerializeField] private float borderThickness = 15;
    [SerializeField] private float transitionTime = 0.5f;
    [SerializeField] private Vector2 minSize;
    private CanvasGroup backgroundCanvasGroup;
    private RectTransform windowRect;

    private bool mouseInWindow = false;
    private bool isActived = false;
    private bool isResizing = false;
    private bool isMoving = false;
    private Vector2 resizeDirection;
    private Vector2 lastMousePosition;

    static WindowPopupUI()
    {
        Instances.Clear();
    }

    private void Awake()
    {
        Instances.Add(this);

        quitWindowButton.onClick.RemoveAllListeners();
        quitWindowButton.onClick.AddListener(() => StartCoroutine(QuitWindow()));

        backgroundCanvasGroup = GetComponent<CanvasGroup>();
        windowRect = GetComponent<RectTransform>();

        backgroundCanvasGroup.interactable = false;
        backgroundCanvasGroup.alpha = 0;
        transform.localScale = Vector2.zero;

        // เก็บ world position เดิมไว้ก่อน
        Vector3 worldPos = windowRect.position;
        Vector2 size = windowRect.rect.size;

        // เปลี่ยน anchor และ pivot
        windowRect.anchorMin = new Vector2(0, 1);
        windowRect.anchorMax = new Vector2(0, 1);

        var oldPivot = windowRect.pivot;
        windowRect.pivot = new Vector2(0, 1);

        // เซ็ตตำแหน่ง world กลับให้เหมือนเดิม (ใช้หลังเปลี่ยน anchor/pivot)
        windowRect.position = worldPos;
        windowRect.anchoredPosition -= new Vector2(windowRect.rect.width * oldPivot.x, windowRect.rect.height * (oldPivot.y - 1));

        // คงขนาดไว้
        windowRect.sizeDelta = size;
    }

    private void Update()
    {
        if (mouseInWindow && Input.GetMouseButtonDown(0))
            transform.SetAsLastSibling();
    }

    public void ToggleWindow()
    {
        if (!isActived)
            StartCoroutine(ShowWindow());
        else
            StartCoroutine(QuitWindow());
    }

    private IEnumerator ShowWindow()
    {
        transform.localScale = Vector2.zero;
        backgroundCanvasGroup.alpha = 0;
        backgroundCanvasGroup.DOFade(1, transitionTime);
        transform.DOScale(Vector2.one, transitionTime);
        yield return new WaitForSecondsRealtime(transitionTime);
        backgroundCanvasGroup.interactable = true;
        isActived = true;
    }

    private IEnumerator QuitWindow()
    {
        backgroundCanvasGroup.interactable = false;
        backgroundCanvasGroup.DOFade(0, transitionTime);
        transform.DOScale(Vector2.zero, transitionTime);
        yield return new WaitForSecondsRealtime(transitionTime);
        isActived = false;
    }

    public void QuitWindowNow()
    {
        Awake();

        backgroundCanvasGroup.interactable = false;
        backgroundCanvasGroup.alpha = 0;
        transform.localScale = Vector2.zero;
        isActived = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localMousePos = GetLocalMousePosition(eventData);

        if (IsOnBorder(localMousePos))
        {
            isResizing = true;
            lastMousePosition = eventData.position;
        }
        else if (IsOnHeader(localMousePos))
        {
            isMoving = true;
            lastMousePosition = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isResizing = false;
        isMoving = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isResizing)
        {
            Vector2 delta = eventData.position - lastMousePosition;
            ResizeWindow(delta);
            lastMousePosition = eventData.position;
        }
        else if (isMoving)
        {
            Vector2 delta = eventData.position - lastMousePosition;
            MoveWindow(delta);
            lastMousePosition = eventData.position;
        }
    }

    private Vector2 GetLocalMousePosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(windowRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        return localPoint;
    }

    private bool IsOnBorder(Vector2 localMousePos)
    {
        float width = windowRect.rect.width;
        float height = windowRect.rect.height;

        bool left = localMousePos.x <= borderThickness;
        bool right = Mathf.Abs(width - localMousePos.x) <= borderThickness;
        bool top = Mathf.Abs(localMousePos.y) <= borderThickness;
        bool bottom = Mathf.Abs(height - Mathf.Abs(localMousePos.y)) <= borderThickness;

        resizeDirection = new Vector2(right ? 1 : left ? -1 : 0, top ? 1 : bottom ? -1 : 0);
        return left || right || top || bottom;
    }

    private bool IsOnHeader(Vector2 localMousePos)
    {
        float top = headerRect.rect.yMax;
        float left = headerRect.rect.xMin;
        float right = headerRect.rect.xMax;
        float bottom = headerRect.rect.yMin;

        return localMousePos.x >= left &&
            localMousePos.x <= right &&
            localMousePos.y <= top &&
            localMousePos.y >= bottom;
    }

    private void MoveWindow(Vector2 delta)
    {
        Vector2 newPos = windowRect.anchoredPosition + delta;

        windowRect.anchoredPosition = newPos;
    }

    private void ResizeWindow(Vector2 delta)
    {
        Vector2 newPos = windowRect.anchoredPosition + new Vector2(delta.x * resizeDirection.x * (resizeDirection.x == -1 ? -1 : 0), delta.y * resizeDirection.y * (resizeDirection.y == 1 ? 1 : 0));
        Vector2 newSize = windowRect.sizeDelta + new Vector2(delta.x * resizeDirection.x, delta.y * resizeDirection.y);
        newSize.x = Mathf.Max(minSize.x, newSize.x);
        newSize.y = Mathf.Max(minSize.y, newSize.y);

        if ((newSize.x != minSize.x && resizeDirection.x != 0) || (newSize.y != minSize.y && resizeDirection.y != 0))
        {
            windowRect.anchoredPosition = newPos;
            windowRect.sizeDelta = newSize;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseInWindow = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseInWindow = false;
    }
}
