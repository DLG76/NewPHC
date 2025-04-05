using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [SerializeField] private Transform rewardContent;
    [SerializeField] private InventorySlot inventorySlotModel;
    [SerializeField] private Button closeButton;

    public static void CreateRewardUI(JObject rewardData) => CreateRewardUI(rewardData, null);

    public static void CreateRewardUI(JObject rewardData, System.Action onClosingRewardUI)
    {
        var rewardCanvas = Instantiate(Resources.Load<RewardUI>("UI/RewardCanvas"));
        rewardCanvas.Setup(rewardData, onClosingRewardUI);
        DontDestroyOnLoad(rewardCanvas.gameObject);
    }

    public void Setup(JObject rewardData, System.Action onClosingRewardUI)
    {
        closeButton.onClick.AddListener(() => onClosingRewardUI?.Invoke());
        closeButton.onClick.AddListener(() => Destroy(gameObject));

        var rewardCanvasAnimator = GetComponentInChildren<Animator>();

        foreach (Transform c in rewardContent)
            Destroy(c.gameObject);

        StartCoroutine(ShowReward(rewardData));

        rewardCanvasAnimator.SetTrigger("Show");
    }

    private IEnumerator ShowReward(JObject rewardData)
    {
        yield return new WaitForSecondsRealtime(0.5f);

        var expSlot = Instantiate(inventorySlotModel, rewardContent);
        expSlot.SetItem(new InventoryItem
        {
            item = new Item(new JObject
            {
                ["_id"] = "exp",
                ["name"] = "Exp",
                ["description"] = "Experience points",
                ["canStack"] = true
            }),
            count = rewardData["exp"].ToObject<int>()
        });
        expSlot.IconImage.transform.localScale = Vector3.one * 0.7f;
        expSlot.transform.localScale = Vector3.zero;
        expSlot.transform.DOScale(Vector3.one, 0.4f);

        yield return new WaitForSecondsRealtime(0.2f);

        var itemSlot = Instantiate(inventorySlotModel, rewardContent);
        itemSlot.SetItem(new InventoryItem
        {
            item = new Item(rewardData["item"]?.ToObject<JObject>()),
            count = rewardData["itemCount"].ToObject<int>()
        });
        itemSlot.transform.localScale = Vector3.zero;
        itemSlot.transform.DOScale(Vector3.one, 0.4f);
    }
}
