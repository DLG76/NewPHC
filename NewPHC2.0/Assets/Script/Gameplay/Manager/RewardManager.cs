using System.Collections.Generic;
using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
    public void ClameReward(Reward reward)
    {
        if (reward == null) return;

        foreach (var itemDrop in reward.itemDrops)
        {
            var item = itemDrop.GetItem();
            if (item != null)
            {
                VoidInventoryUI.Instance.AddItem(item);
            }
        }
    }
}