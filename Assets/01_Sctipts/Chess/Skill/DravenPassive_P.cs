using UnityEngine;

public class DravenPassive_P : MonoBehaviour, IOnHitEffect
{
    //=====================================================
    //                  Stack
    //=====================================================
    [Header("Stack")]
    [SerializeField, Tooltip("평타시 얻는 스택량")]
    private int stacksPerHit = 1;

    [SerializeField, Tooltip("몇 스택 부터 환전될지.")]
    private int stacksPerCashout = 10;
    private int stacks = 0;

    //=====================================================
    //                  Gold
    //=====================================================
    [Header("패시브 관련")]
    [SerializeField, Tooltip("1성: 환전량")]
    private int goldPerCashout_1Star = 1;

    [SerializeField, Tooltip("2성: 환전량")]
    private int goldPerCashout_2Star = 3;

    [SerializeField, Tooltip("3성: 환전량")]
    private int goldPerCashout_3Star = 5;

    //InvokeOnHitEffects로 바꿧어요.
    public void OnHit(Chess attacker, Chess target)
    {
        if (attacker == null || target == null) return;

        if (attacker.gameObject != gameObject) return;

        stacks += stacksPerHit;

        int cashouts = stacksPerCashout > 0 ? (stacks / stacksPerCashout) : 0;
        if (cashouts <= 0) return;

        stacks -= cashouts * stacksPerCashout;

        // 드레이븐 패시브 효과음 추가
        SettingsUI.PlaySFX("Draven_Passive", Vector3.zero, 1f, 1f);

        int goldPer = GetGoldPerCashout(attacker.StarLevel);
        int gain = cashouts * goldPer;

        if (attacker.team == Team.Player && ShopManager.Instance != null)
            ShopManager.Instance.AddGold(gain);

        //Debug.Log($"[Draven P] HitStack cashout x{cashouts} (+{gain}g), remain stacks={stacks}");
    }

    private int GetGoldPerCashout(int starLevel)
    {
        if (starLevel >= 3) return goldPerCashout_3Star;
        if (starLevel == 2) return goldPerCashout_2Star;
        return goldPerCashout_1Star;
    }
}
