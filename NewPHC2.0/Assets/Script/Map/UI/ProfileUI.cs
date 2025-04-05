using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup profilePanel;
    [SerializeField] private RectTransform selection;
    [Header("Selections")]
    [SerializeField] private RectTransform infoButton;
    [SerializeField] private RectTransform fuseButton;
    [Header("Panels")]
    [SerializeField] private CanvasGroup infoPanel;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private CanvasGroup fusePanel;
    [SerializeField] private FuseUI fuseUI;
    [Header("Settings")]
    [SerializeField] private float duration = 0.3f;

    private bool loadingUI = false;

    private void Awake()
    {
        (profilePanel.transform as RectTransform).anchorMax = Vector2.one * 2;
        (profilePanel.transform as RectTransform).anchorMin = Vector2.one;
    }

    public void BackToOverworld() => StartCoroutine(BackToOverworldIE());

    private IEnumerator BackToOverworldIE()
    {
        profilePanel.DOFade(0, duration).SetEase(Ease.OutQuad);
        (profilePanel.transform as RectTransform).DOAnchorMax(Vector2.one * 2, duration).SetEase(Ease.OutQuad);
        (profilePanel.transform as RectTransform).DOAnchorMin(Vector2.one, duration).SetEase(Ease.OutQuad);
        yield return inventoryUI.LeaveScreen();
        yield return fuseUI.LeaveScreen();
        yield return new WaitForSecondsRealtime(duration);
        profilePanel.gameObject.SetActive(false);
    }

    public void OpenPlayerInfo()
    {
        if (loadingUI || !User.me.haveData) return;

        TransformSelection(infoButton);

        StartCoroutine(OpenPlayerInfoIE());
    }

    private IEnumerator OpenPlayerInfoIE()
    {
        loadingUI = true;
        fusePanel.DOFade(0, duration).SetEase(Ease.OutQuad);
        yield return new WaitForSecondsRealtime(duration);
        OpenProfilePanel();
        fusePanel.gameObject.SetActive(false);
        yield return fuseUI.LeaveScreen();
        yield return inventoryUI.LoadScreen();
        infoPanel.gameObject.SetActive(true);
        infoPanel.DOFade(1, duration).SetEase(Ease.OutQuad);
        yield return new WaitForSecondsRealtime(duration);
        loadingUI = false;
    }

    public void OpenFuse()
    {
        if (loadingUI || !User.me.haveData) return;

        TransformSelection(fuseButton);

        StartCoroutine(OpenFuseIE());
    }

    private IEnumerator OpenFuseIE()
    {
        loadingUI = true;
        infoPanel.DOFade(0, duration).SetEase(Ease.OutQuad);
        yield return new WaitForSecondsRealtime(duration);
        OpenProfilePanel();
        infoPanel.gameObject.SetActive(false);
        yield return inventoryUI.LeaveScreen();
        yield return fuseUI.LoadScreen();
        fusePanel.gameObject.SetActive(true);
        fusePanel.DOFade(1, duration).SetEase(Ease.OutQuad);
        yield return new WaitForSecondsRealtime(duration);
        loadingUI = false;
    }

    private void OpenProfilePanel()
    {
        if (profilePanel.gameObject.activeSelf)
            return;

        (profilePanel.transform as RectTransform).DOAnchorMax(Vector2.one, duration).SetEase(Ease.OutQuad);
        (profilePanel.transform as RectTransform).DOAnchorMin(Vector2.zero, duration).SetEase(Ease.OutQuad);
        profilePanel.DOFade(1, duration).SetEase(Ease.OutQuad);
        profilePanel.gameObject.SetActive(true);
    }

    private void TransformSelection(RectTransform target)
    {
        if (selection == null || target == null) return;

        selection.DOAnchorMax(target.anchorMax, duration).SetEase(Ease.OutQuad);
        selection.DOAnchorMin(target.anchorMin, duration).SetEase(Ease.OutQuad);
    }
}
