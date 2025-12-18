using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardSettlement: MonoBehaviour
{
    [SerializeField] private ItemData[] rewardItems;

    private void OnEnable()
    {
        GameManager.Instance.OnRoundReward += Rewards;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRoundEnded -= Rewards;
    }

    private void Rewards(int currentRound, bool lastBattleWin)
    {
        //if (!lastBattleWin) return;
        ItemRewards(currentRound);
    }

    void ItemRewards(int currentRound)
    {
        int emptySlots = ItemSlotManager.Instance.EmptySlotCount;

        if (emptySlots <= 0) return;
        for (int i = 0; i < currentRound; ++i)
        {
            if (!ItemSlotManager.Instance) return;
            if (rewardItems == null || rewardItems.Length == 0) return;

            ItemData item = rewardItems[Random.Range(0, rewardItems.Length)];

            bool success = ItemSlotManager.Instance.AddItem(item);
            if (!success)
            {
                Debug.Log("ºó ½½·ÔÀÌ ¾ø½À´Ï´Ù.");
            }
        }
    }
}
