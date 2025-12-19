using UnityEngine;

public class DravenPassive_P : MonoBehaviour, IOnKillEffect
{
    //=====================================================
    //                  Stack
    //=====================================================
    [Header("Stack")]
    [SerializeField, Tooltip("킬 당 얻는 스택")]
    private int stacksPerKill = 1;

    [SerializeField, Tooltip("몇 스택마다 골드로 환전되는지")]
    private int stacksPerCashout = 11;

    [SerializeField, Tooltip("현재 스택")]
    private int stacks = 0;

    //=====================================================
    //                  Gold
    //=====================================================
    [Header("Gold Per 11 Stacks")]
    [SerializeField, Tooltip("1성: 11스택당 골드")]
    private int goldPerCashout_1Star = 1;

    [SerializeField, Tooltip("2성: 11스택당 골드")]
    private int goldPerCashout_2Star = 3;

    [SerializeField, Tooltip("3성: 11스택당 골드")]
    private int goldPerCashout_3Star = 5;

    public void OnKill(Chess killer, Chess victim)
    {
        if (killer == null || victim == null) return;
        if (killer.gameObject != gameObject) return;

        stacks += stacksPerKill;

        int cashouts = stacks / stacksPerCashout;
        if (cashouts <= 0) return;

        stacks -= cashouts * stacksPerCashout;

        int goldPer = GetGoldPerCashout(killer.StarLevel);
        int gain = cashouts * goldPer;

        if (killer.team == Team.Player && ShopManager.Instance != null)
        {
            ShopManager.Instance.AddGold(gain);
        }

        Debug.Log($"[Draven P] Cashout x{cashouts} (+{gain}g), remain stacks={stacks}");
    }

    private int GetGoldPerCashout(int starLevel)
    {
        if (starLevel >= 3) return goldPerCashout_3Star;
        if (starLevel == 2) return goldPerCashout_2Star;
        return goldPerCashout_1Star;
    }
}
