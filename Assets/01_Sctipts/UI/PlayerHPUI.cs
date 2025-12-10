using UnityEngine;
using TMPro;

public class PlayerHPUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;

    private int maxHP = 3;
    private int currentHP = 3;

    private void Start()
    {
        // 초기 HP UI 세팅
        currentHP = maxHP;
        UpdateHPUI();
    }
    private void OnEnable()
    {
        // 이벤트 구독
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundEnded += HandleRoundEnd;
    }

    private void HandleRoundEnd(int round, bool win)
    {
        // 라운드 패배시 체력 -1
        if (!win)
        {
            currentHP--;
            UpdateHPUI();
        }
    }

    private void UpdateHPUI()
    {
        hpText.text = currentHP.ToString();
    }

    private void OnDisable()
    {
        // 이벤트 해제
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundEnded -= HandleRoundEnd;
    }
}
