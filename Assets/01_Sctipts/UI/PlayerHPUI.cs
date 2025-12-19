using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어의 남은 체력을 UI로 표시하는 컴포넌트.
///
/// - 라운드 결과에 따라 플레이어 체력을 감소시킨다.
/// - GameManager의 라운드 종료 이벤트를 구독하여 동작한다.
/// - 전투 로직과 분리된 UI 전용 스크립트이다.
/// </summary>
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

    /// <summary>
    /// 라운드 종료 시 호출되는 이벤트 핸들러.
    /// 패배한 경우 플레이어 체력을 1 감소시킨다.
    /// </summary>
    private void HandleRoundEnd(int round, bool win)
    {
        // 라운드 패배시 체력 -1
        if (!win)
        {
            currentHP--;
            UpdateHPUI();
        }
    }

    /// <summary>
    /// 현재 체력을 UI 텍스트에 반영한다.
    /// </summary>
    private void UpdateHPUI()
    {
        hpText.text = currentHP.ToString();
    }

    public void ResetHP()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }


    private void OnDisable()
    {
        // 이벤트 해제
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundEnded -= HandleRoundEnd;
    }
}
