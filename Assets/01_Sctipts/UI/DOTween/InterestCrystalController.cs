using UnityEngine;

public class InterestCrystalController : MonoBehaviour
{
    [SerializeField] private InterestCrystal[] crystals; // Slot1 ~ Slot5 (아래 → 위)

    private int currentActiveCount = 0;

    private void Start()
    {
        // 시작 골드 기준으로 반드시 1회 동기화
        int startGold = ShopManager.Instance != null
            ? ShopManager.Instance.CurrentGold
            : 0;

        RefreshByGold(startGold);
    }

    /// <summary>
    /// 현재 골드를 기준으로 이자 크리스탈 갱신
    /// </summary>
    public void RefreshByGold(int gold)
    {
        int targetCount = Mathf.Clamp(gold / 10, 0, crystals.Length);

        if (targetCount == currentActiveCount)
            return;

        // 증가 (아래 → 위)
        if (targetCount > currentActiveCount)
        {
            for (int i = currentActiveCount; i < targetCount; i++)
            {
                crystals[i]?.Show();
            }
        }
        // 감소 (위 → 아래)
        else
        {
            for (int i = currentActiveCount - 1; i >= targetCount; i--)
            {
                crystals[i]?.Hide();
            }
        }

        currentActiveCount = targetCount;
    }
}
