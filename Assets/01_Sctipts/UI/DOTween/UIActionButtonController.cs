using UnityEngine;

/// <summary>
/// XP 구매 버튼, 리롤 버튼, 전투 시작 버튼의
/// 활성/비활성 상태와 시각적 연출을 제어하는 컨트롤러.
/// 
/// - XP 구매 / 리롤 버튼은 라운드 단계와 무관
/// - 전투 시작 버튼만 Preparation 단계에서 활성
/// - GameManager의 RoundState 이벤트를 통해 상태를 갱신한다
/// </summary>
public class UIActionButtonController : Singleton<UIActionButtonController>
{
    [Header("Buttons")]
    [SerializeField] private UIGrayscaleButton expBuyButton;
    [SerializeField] private UIGrayscaleButton rerollButton;
    [SerializeField] private UIGrayscaleButton battleStartButton;

    // 현재 라운드 상태 캐싱
    private RoundState currentRoundState = RoundState.Preparation;

    private bool battleStartLocked = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged += HandleRoundStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged;
        }
    }

    private void HandleRoundStateChanged(RoundState newState)
    {
        currentRoundState = newState;

        if (newState == RoundState.Preparation)
        {
            battleStartLocked = false;
        }

        Refresh();
    }


    /// <summary>
    /// 현재 상태 기준으로 모든 버튼 상태를 갱신한다.
    /// </summary>
    public void Refresh()
    {
        RefreshExpButton();
        RefreshRerollButton();
        RefreshBattleButton();
    }

    // ====================================================
    // 개별 버튼 상태 계산
    // ====================================================

    private void RefreshExpButton()
    {
        if (expBuyButton == null) return;

        var shop = ShopManager.Instance;

        bool canUse =
            shop != null &&
            !shop.IsMaxLevel &&
            shop.CurrentGold >= 4;

        expBuyButton.SetInteractable(canUse);
    }

    private void RefreshRerollButton()
    {
        if (rerollButton == null) return;

        var shop = ShopManager.Instance;

        bool canUse =
            shop != null &&
            shop.CurrentGold >= 2;

        rerollButton.SetInteractable(canUse);
    }

    private void RefreshBattleButton()
    {
        if (battleStartButton == null)
            return;

        bool canUse =
            currentRoundState == RoundState.Preparation &&
            !battleStartLocked;

        battleStartButton.SetInteractable(canUse);
    }


    // 전투시작 진입 메서드 추가
    public void RequestBattleStartFromUI()
    {
        // 이미 눌린 상태면 무시
        if (battleStartLocked)
            return;

        battleStartLocked = true;

        Refresh();

        // 실제 전투 시작 로직 위임
        GameManager.Instance?.RequestStartBattle();
    }

}
