using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState //게임 상태
{
    None,
    Playing,
    GameOver
}

public enum RoundState //라운드 상태
{
    None,
    Preparation,    //준비단계 - 상점, 배치, 아이템
    Battle,         //전투단계 - 상점과 벤치간 상호작용만 가능, 필드 위 기물은 건드릴 수 없음
    Result          //라운드 승/패 연출타임, 정산
}

public enum BattleResult //전투 결과
{ 
    None,
    PlayerWin,
    PlayerLose
}


public class GameManager : Singleton<GameManager>
{

    protected override void Awake()
    {
        base.Awake();
    }

    public GameState gameState { get; private set; }
    public RoundState roundState { get; private set; }
    public int currentRound { get; private set; }

    private int loseCount = 0;

    [Header("Round Info")]
    //[SerializeField] private int startingRound = 1; //시작 라운드
    //[SerializeField] private int maxRound = 5; //마지막 라운드
    [SerializeField] private int maxLoseCount = 3;  //게임 종료 패배 횟수
    public float battleTime = 30f; // 전투시간
    public float preparationTime = 60f; // 준비시간

    public event Action<int> OnRoundStarted;    //라운드 시작 이벤트
    public event Action<float> OnPreparationTimerUpdated;   //준비단계 타이머 이벤트
    public event Action<float> OnBattleTimerUpdated; //전투단계 타이머 이벤트
    public event Action<int, bool> OnRoundEnded;    //라운드 종료 이벤트 2
    public event Action<RoundState> OnRoundStateChanged;
    public event Action<int, bool> OnRoundReward; //정산때 보상이벤트 추가 12-16 Won Add

    [SerializeField] private float battleStartDelay = 5f; //12.12 add Kim
    [SerializeField] private float winResultTime = 2.5f; //승리시 2.5초 춤추는거 볼 시간. (12.12 add Kim)
    [SerializeField] private float loseResultTime = 2.0f;
    private bool lastBattleWin = false;
    public event Action<float> OnTimerMaxTimeChanged; //12.18 add Kim
    private Coroutine battleCountdownCo; //12.18 add Kim
    private bool isReady = false;
    //참조
    /*
    public Player player;
    public BattleSystem battleSystem;
    public EnemyWaveDatabase waveDatabase; //라운드 별 적 데이터
    */

    //게임 시작

    private void Start()
    {
        StartGame();
    }
    public void StartGame()
    {
        gameState = GameState.Playing;
        roundState = RoundState.None;

        currentRound = 1;
        loseCount = 0;

        //player.Initialize(); //골드/레벨/상점기물확률 초기화

        StartRound();
    }

    //라운드 시작
    private void StartRound()
    {
        UnitCountManager.Instance.Clear();

        SetRoundState(RoundState.Preparation);

        OnRoundStarted?.Invoke(currentRound);

        StartCoroutine(RoundRoutine());
    }

    //라운드 흐름 코루틴
    private IEnumerator RoundRoutine()
    {
        // 준비단계
        isReady = false;

        //while (!isReady)
        //{
        //    yield return null;  
        //}
        //float t = battleStartDelay;
        //while (t > 0f)
        //{
        //    OnPreparationTimerUpdated?.Invoke(t); // TimeUI가 여기로 받으면 됨
        //    t -= Time.deltaTime;
        //    yield return null;
        //}
        while (!isReady)
            yield return null;
        StartBattle();

        float battleTimer = battleTime;

        while (true)
        {
            battleTimer -= Time.deltaTime;
            OnBattleTimerUpdated?.Invoke(battleTimer);

            bool playerAllDead = UnitCountManager.Instance.ArePlayerAllDead();
            bool enemyAllDead = UnitCountManager.Instance.AreEnemyAllDead();
            if (enemyAllDead)
            {
                EndBattle(true);  
                break;
            }

            if (playerAllDead)
            {
                EndBattle(false);  
                break;
            }

            if (battleTimer <= 0f)
            {
                int playerAlive = UnitCountManager.Instance.GetPlayerAliveCount();
                int enemyAlive = UnitCountManager.Instance.GetEnemyAliveCount();

                bool playerWin = playerAlive > enemyAlive; 
                EndBattle(playerWin);
                break;
            }

            yield return null;
        }


        //결과 계산
        SetRoundState(RoundState.Result);

        // 정산(보상) 타이밍 알림
        OnRoundReward?.Invoke(currentRound, lastBattleWin);

        //연출시간.
        yield return new WaitForSeconds(lastBattleWin ? winResultTime : loseResultTime);

        CleanupDeadUnits();
        //다음 라운드or게임 오버
        if (loseCount >= maxLoseCount)
        {
            EndGame();
            yield break;
        }

        currentRound++;
        StartRound();
    }

    //전투시작 메서드
    private void StartBattle()
    {
        UnitCountManager.Instance.Clear();

        var fieldGrid = FindAnyObjectByType<FieldGrid>();
        var enemyGrid = FindAnyObjectByType<EnemyGrid>();

        if (fieldGrid != null)
        {
            foreach (var unit in fieldGrid.GetAllFieldUnits())
            {
                var chess = unit.GetComponent<Chess>();
                if (chess == null) continue;
                UnitCountManager.Instance.RegisterUnit(chess, chess.team == Team.Player);
            }
        }

        if (enemyGrid != null)
        {
            foreach (var unit in enemyGrid.GetAllFieldUnits()) 
            {
                var chess = unit.GetComponent<Chess>(); 
                if (chess == null) continue;
                UnitCountManager.Instance.RegisterUnit(chess, chess.team == Team.Player);
            }
        }

        Debug.Log($"[StartBattle] Player={UnitCountManager.Instance.GetPlayerAliveCount()}, Enemy={UnitCountManager.Instance.GetEnemyAliveCount()}");

        SetRoundState(RoundState.Battle);
    }


    private void EndBattle(bool win)
    {
        EndRound(win);
    }
    //라운드 종료 메서드
    private void EndRound(bool win)
    {
        lastBattleWin = win;
        OnRoundEnded?.Invoke(currentRound, win);

        if (!win) loseCount++;

        if (win)
        {
            var fieldGrid = FindAnyObjectByType<FieldGrid>();
            if (fieldGrid != null)
            {
                var units = fieldGrid.GetAllFieldUnits();
                foreach (var u in units)
                {
                    var c = u.GetComponent<Chess>();
                    if (c == null) continue;
                    if (c.team != Team.Player) continue;
                    if (c.IsDead) continue;

                    c.ForceVictory();
                }
            }
        }
    }


    //게임 종료 메서드
    private void EndGame()
    {
        gameState = GameState.GameOver;
    }

    //게임 재시작 메서드 12-19 Won Add 아직 수정 덜함
    public void RestartGame()
    {
        // ===== 라운드 관련 상태 초기화 =====
        currentRound = 1;
        loseCount = 0;

        // 전투/라운드 관련 코루틴 중지
        if (battleCountdownCo != null)
        {
            StopCoroutine(battleCountdownCo);
            battleCountdownCo = null;
        }

        // GameManager에서 실행 중인 모든 코루틴 정지
        StopAllCoroutines();

        // 현재 게임 상태를 "완전히 종료된 상태"로 되돌림
        gameState = GameState.Playing;
        roundState = RoundState.Preparation;

        // 필드 위 아군 기물 전부 풀로 반환
        var fieldGrid = FindAnyObjectByType<FieldGrid>();
        if (fieldGrid != null)
        {
            var fieldUnits = fieldGrid.GetAllFieldUnits();

            foreach (var unit in fieldUnits)
            {
                if (unit == null) continue;

                // 노드 참조 제거 (CountOfPiece 자동 감소)
                fieldGrid.ClearChessPiece(unit);

                // 풀 반환
                var pooled = unit.GetComponentInParent<PooledObject>();
                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    unit.gameObject.SetActive(false);
            }
        }

        // 필드 위 적 기물 전부 풀로 반환
        var enemyGrid = FindAnyObjectByType<EnemyGrid>();
        if (enemyGrid != null)
        {
            var enemyUnits = enemyGrid.GetAllFieldUnits();

            foreach (var unit in enemyUnits)
            {
                if (unit == null) continue;

                // Grid 노드 참조 제거 (CountOfPiece 감소)
                enemyGrid.ClearChessPiece(unit);

                // 필드 플래그 해제 (안 해도 되지만 안전)
                if (unit is Chess chess)
                {
                    chess.SetOnField(false);
                }

                // 풀 반환
                var pooled = unit.GetComponentInParent<PooledObject>();
                if (pooled != null)
                {
                    pooled.ReturnToPool();
                }
                else
                {
                    unit.gameObject.SetActive(false);
                }
            }
        }

        // 벤치 위 기물 전부 풀로 반환
        var benchGrid = FindAnyObjectByType<BenchGrid>();
        if (benchGrid != null)
        {
            foreach (var node in benchGrid.FieldGrid)
            {
                if (node.ChessPiece == null) continue;

                var unit = node.ChessPiece;

                // 노드 참조 제거
                node.ChessPiece = null;

                // 벤치 상태 명시
                if (unit is Chess chess)
                {
                    chess.SetOnField(false);
                }

                // 풀 반환
                var pooled = unit.GetComponentInParent<PooledObject>();
                if (pooled != null)
                {
                    pooled.ReturnToPool();
                }
                else
                {
                    unit.gameObject.SetActive(false);
                }
            }
        }

        // 각 Grid(Field / Enemy / Bench)의 노드에 남아있는 ChessPiece 참조 제거
        var allGrids = FindObjectsOfType<GridDivideBase>();
        foreach (var grid in allGrids)
        {
            foreach (var node in grid.FieldGrid)
            {
                node.ChessPiece = null;
            }
        }

        // Grid의 CountOfPiece 값 초기화
        foreach (var grid in allGrids)
        {
            while (grid.CountOfPiece > 0)
            {
                grid.DecreasePieceCount();
            }
        }

        // 시너지 매니저 내부 상태 초기화 (현재 활성 시너지 제거)
        if (SynergyManager.Instance != null)
        {
            SynergyManager.Instance.ResetAll();
        }

        // 상점 레벨 / 경험치 / 확률 / 리롤 상태 초기값으로 복구
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ResetShopProgress();
        }

        // 상점 기물 목록 초기화
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.RefreshShop();
        }

        // 플레이어 골드 초기값으로 복구
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ResetGold();
        }


        // 아이템 인벤토리 비우기
        if (ItemSlotManager.Instance != null)
        {
            ItemSlotManager.Instance.ClearAllSlots();
        }

        // 아이템 관련 UI 초기화
        var allItemUIs = FindObjectsOfType<ChessItemUI>();
        foreach (var itemUI in allItemUIs)
        {
            itemUI.ClearAll();
        }

        // SettingsUI 닫기
        var settingsUI = FindAnyObjectByType<SettingsUI>(
            FindObjectsInactive.Include
        );

        if (settingsUI != null)
        {
            settingsUI.ToggleSettingsUI();
        }

        // 기물정보UI 닫기
        if (ChessInfoUI.Instance != null)
        {
            ChessInfoUI.Instance.Hide();
        }

        // 플레이어 HP 초기화
        var playerHPUI = FindAnyObjectByType<PlayerHPUI>(
            FindObjectsInactive.Include
        );

        if (playerHPUI != null)
        {
            playerHPUI.ResetHP();
        }

        // 라운드 완성되면 라운드 UI랑 연동 잘되는지 체크
        // 재시작후 일부 기물이 합성하거나 구매할때 사라지는 버그가 있음

    }

    public void ReturnToMainMenu()
    {
        // 항상 안전하게 복구
        Time.timeScale = 1f;

        // 게임 플레이 상태 완전 초기화
        RestartGame();

        // 메인 메뉴(StartPanel) 표시 (비활성 포함 탐색)
        var startPanel = FindAnyObjectByType<StartPanelUI>(
            FindObjectsInactive.Include
        );

        if (startPanel != null)
        {
            startPanel.Open();
        }
        else
        {
            Debug.LogError("[ReturnToMainMenu] StartPanelUI not found");
        }

        // 인트로 BGM 재생
        SoundSystem.SoundPlayer?.PlaySound(
            "BGM3",
            Vector3.zero,
            1f,
            0f
        );
    }


    public void StartGameFromMainMenu()
    {
        gameState = GameState.Playing;
        roundState = RoundState.Preparation;

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.RefreshShop();
        }

        // 라운드 시작
        StartRound();

        SoundSystem.SoundPlayer?.PlaySound(
            "BGM1",
            Vector3.zero,
            1f,
            0f
        );
    }

    //라운드 상태 변경 메서드
    private void SetRoundState(RoundState newState)
    {
        Debug.Log($"RoundState => {newState}");
        roundState = newState;
        OnRoundStateChanged?.Invoke(newState);
    }

    // 전투시작버튼이 호출할 메서드
    public void RequestStartBattle()
    {
        if (roundState != RoundState.Preparation) return;
        if (isReady) return; //이미 준비 완료면 무시

        if (battleCountdownCo != null)
        {
            StopCoroutine(battleCountdownCo);
            battleCountdownCo = null;
        }

        battleCountdownCo = StartCoroutine(BattleCountdownRoutine(battleStartDelay));
    }


    private IEnumerator BattleCountdownRoutine(float wait)
    {
        OnTimerMaxTimeChanged?.Invoke(wait);

        float t = wait;
        while (t > 0f)
        {
            OnPreparationTimerUpdated?.Invoke(t);
            t -= Time.deltaTime;
            yield return null;
        }

        OnPreparationTimerUpdated?.Invoke(0f);
        isReady = true;

        battleCountdownCo = null;
    }

    private void CleanupDeadUnits()
    {
        //적 유닛 정리
        var enemyGrid = FindAnyObjectByType<EnemyGrid>();
        if (enemyGrid != null)
        {
            var enemies = enemyGrid.GetAllFieldUnits();
            foreach (var unit in enemies)
            {
                var chess = unit.GetComponent<Chess>();
                if (chess != null && chess.IsDead)
                {
                    var pooled = unit.GetComponentInParent<PooledObject>();
                    if (pooled != null)
                        pooled.ReturnToPool();
                    else
                        unit.gameObject.SetActive(false);
                }
            }
        }

        //플레이어 유닛도 죽은 것 정리
        var fieldGrid = FindAnyObjectByType<FieldGrid>();
        if (fieldGrid != null)
        {
            var players = fieldGrid.GetAllFieldUnits();
            foreach (var unit in players)
            {
                var chess = unit.GetComponent<Chess>();
                if (chess != null && chess.IsDead)
                {
                    var pooled = unit.GetComponentInParent<PooledObject>();
                    if (pooled != null)
                        pooled.ReturnToPool();
                    else
                        unit.gameObject.SetActive(false);
                }
            }
        }
    }

}
