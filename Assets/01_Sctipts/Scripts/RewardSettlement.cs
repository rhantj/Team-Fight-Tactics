using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardSettlement: MonoBehaviour
{
    [SerializeField] private ItemData[] rewardItems;
    private int rewardGold = 5;
    private int rewardItem = 1;

    private void OnEnable()
    {
        GameManager.Instance.OnRoundReward += Rewards;
    }

    private void Rewards(int currentRound, bool lastBattleWin)
    {
        //if (!lastBattleWin) return;
        ItemRewards(currentRound, lastBattleWin);
        GoldnExpReward(currentRound, lastBattleWin);
    }

    void ItemRewards(int round, bool isWin)
    {
        int emptySlots = ItemSlotManager.Instance.EmptySlotCount;
        rewardItem = isWin ? 3 : 1;

        if (rewardItem > emptySlots) rewardItem = emptySlots;
        for (int i = 0; i < rewardItem; ++i)
        {
            if (!ItemSlotManager.Instance) return;
            if (rewardItems == null || rewardItems.Length == 0) return;

            int rewardIdx = Random.Range(0, rewardItems.Length);
            ItemData item = rewardItems[rewardIdx];

            bool success = ItemSlotManager.Instance.AddItem(item);
            //if (!success)
            //{
            //    Debug.Log("빈 슬롯이 없습니다.");
            //}
        }
    }

    private void GoldnExpReward(int round, bool isWin)
    {
        int rewardExp = 2;
        var shop = StaticRegistry<ShopManager>.Find();

        // 현재 보유 골드 기준 이자 계산
        int interestGold = CalculateInterestGold(shop.CurrentGold); // ← 보유 골드 변수

        // 이번 라운드 최종 골드 보상
        int totalGoldReward = rewardGold + interestGold;

        // 지급
        shop.AddGold(totalGoldReward);
        shop. AddExp(rewardExp);

        //Debug.Log(
        //    $"[RoundReward] Round {round} | Win={isWin} | " +
        //    $"BaseReward={rewardGold}, Interest={interestGold}, TotalGold={totalGoldReward}"
        //);

        // 다음 라운드를 위한 연승 보상 갱신
        if (isWin)
        {
            rewardGold += 1;
        }
        else
        {
            rewardGold = 5;
        }
    }


    /// <summary>
    /// 이자 계산하는 메서드
    /// </summary>
    /// <param name="currentGold"></param>
    /// <returns></returns>
    private int CalculateInterestGold(int currentGold)
    {
        int interest = currentGold / 10;
        return Mathf.Min(interest, 5); //10원마다 나뉘어 떨어지며 상한 이자골드는 5원
    }
}
