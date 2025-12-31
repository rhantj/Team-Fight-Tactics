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
    public int LastReachedRound => currentRound;

    private int loseCount = 0;

    [Header("Round Info")]
    [SerializeField] private int startingRound = 1; //시작 라운드
    [SerializeField] private int maxRound = 5; //마지막 라운드
    [SerializeField] private int maxLoseCount = 3;  //게임 종료 패배 횟수
    public float battleTime = 30f; // 전투시간
    public float preparationTime = 60f; // 준비시간

    public event Action<int> OnRoundStarted;    //라운드 시작 이벤트
    public event Action<float> OnPreparationTimerUpdated;   //준비단계 타이머 이벤트
    public event Action<float> OnBattleTimerUpdated; //전투단계 타이머 이벤트
    public event Action<int, bool> OnRoundEnded;    //라운드 종료 이벤트 2
    public event Action<RoundState> OnRoundStateChanged;
    public event Action<int, bool> OnRoundReward; //정산때 보상이벤트 추가 12-16 Won Add
    public event Action OnGameOver;

    [SerializeField] private float battleStartDelay = 5f; //12.12 add Kim
    [SerializeField] private float winResultTime = 2.5f; //승리시 2.5초 춤추는거 볼 시간. (12.12 add Kim)
    [SerializeField] private float loseResultTime = 2.0f;
    private bool lastBattleWin = false;
    public event Action<float> OnTimerMaxTimeChanged; //12.18 add Kim
    private Coroutine battleCountdownCo; //12.18 add Kim
    private bool isReady = false;
    private Coroutine roundRoutineCo;

    // 필드에 스냅샷 저장소 추가
    private List<EndGameUnitSnapshot> lastBattleUnits = new();
    public IReadOnlyList<EndGameUnitSnapshot> LastBattleUnits => lastBattleUnits;

    //참조
    FieldGrid fieldGrid;
    EnemyGrid enemyGrid;
    BenchGrid benchGrid;
    /*
    public Player player;
    public BattleSystem battleSystem;
    public EnemyWaveDatabase waveDatabase; //라운드 별 적 데이터
    */

    //게임 시작
    [Header("등급 관련.")]
    [SerializeField] private float starStepMultiplier = 1.5f;

    public float GetStarMultiplier(int starLevel)
    {
        int lv = Mathf.Max(1, starLevel);
        return Mathf.Pow(starStepMultiplier, lv - 1);
    }
    private void Start()
    {
        StartGame();
        fieldGrid = StaticRegistry<FieldGrid>.Find();
        enemyGrid = StaticRegistry<EnemyGrid>.Find();
        benchGrid = StaticRegistry<BenchGrid>.Find();
    }
    public void StartGame()
    {
        gameState = GameState.Playing;
        roundState = RoundState.None; 

        currentRound = 1;
        loseCount = 0;

        //player.Initialize(); //골드/레벨/상점기물확률 초기화
    }

    //라운드 시작
    private void StartRound()
    {
        // 기존 라운드 코루틴이 살아있다면 종료
        if (roundRoutineCo != null)
        {
            StopCoroutine(roundRoutineCo);
            roundRoutineCo = null;
        }

        ClearAllVFX();
        ResetPlayerUnitsForNewRound();
        ResetEnemyUnitsForNewRound();
        UnitCountManager.Instance.Clear();

        SetRoundState(RoundState.Preparation);

        OnRoundStarted?.Invoke(currentRound);

        // 새 라운드 코루틴은 하나만 실행
        roundRoutineCo = StartCoroutine(RoundRoutine());
    }


    //라운드 흐름 코루틴
    private IEnumerator RoundRoutine()
    {
        // 준비단계
        isReady = false;
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

        // 전투 후 VFX 정리
        ClearAllVFX();

        //결과 계산
        SetRoundState(RoundState.Result);

        // 정산(보상) 타이밍 알림
        OnRoundReward?.Invoke(currentRound, lastBattleWin);

        // 연출시간.
        yield return new WaitForSeconds(lastBattleWin ? winResultTime : loseResultTime);

        CleanupDeadUnits();
        // 다음 라운드or게임 오버
        if (loseCount >= maxLoseCount)
        {
            EndGame();
            yield break;
        }

        // 현재라운드가 최대 라운드라면 종료
        if (currentRound >= maxRound)
        {
            EndGame();
            yield break;
        }

        currentRound++;
        StaticRegistry<EnemyGrid>.Find()?.SpawnEnemy(currentRound); //적 리스폰

        StartRound();
    }

    //전투시작 메서드
    private void StartBattle()
    {
        UnitCountManager.Instance.Clear();

        //var fieldGrid = FindAnyObjectByType<FieldGrid>();
        //var enemyGrid = FindAnyObjectByType<EnemyGrid>();

        if (fieldGrid != null)
        {
            foreach (var unit in fieldGrid.GetAllFieldUnits())
            {
                var chess = unit.GetComponent<Chess>();
                if (chess == null) continue;
                UnitCountManager.Instance.RegisterUnit(chess, chess.team == Team.Player);

                chess.NotifyBattleStart();
            }
        }

        if (enemyGrid != null)
        {
            foreach (var unit in enemyGrid.GetAllFieldUnits()) 
            {
                var chess = unit.GetComponent<Chess>(); 
                if (chess == null) continue;
                UnitCountManager.Instance.RegisterUnit(chess, chess.team == Team.Player);

                chess.NotifyBattleStart();
            }
        }

        //Debug.Log($"[StartBattle] Player={UnitCountManager.Instance.GetPlayerAliveCount()}, Enemy={UnitCountManager.Instance.GetEnemyAliveCount()}");

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

        SaveLastBattleUnits();

        OnRoundEnded?.Invoke(currentRound, win);

        if (!win) loseCount++;

        if (win)
        {
            //var fieldGrid = FindAnyObjectByType<FieldGrid>();
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
            SettingsUI.PlaySFX("Win", Vector3.zero, 1f, 1f);
        }
        else
        {
            SettingsUI.PlaySFX("Lose", Vector3.zero, 1f, 1f);
        }
    }


    //게임 종료 메서드
    private void EndGame()
    {
        //Debug.Log($"[CHECK] EndGame CALLED | frame={Time.frameCount}");
        gameState = GameState.GameOver;
        OnGameOver?.Invoke();
    }

    //게임 재시작 메서드 12-19 Won Add 아직 수정 덜함
    public void RestartGame()
    {
        // 마지막 라운드 전투 기물정보 초기화
        lastBattleUnits.Clear();

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

        // 켜있다면 SettingsUI 닫기
        var settingsUI = FindAnyObjectByType<SettingsUI>(
                           FindObjectsInactive.Include);

        if (settingsUI != null && settingsUI.gameObject.activeSelf)
        {
             settingsUI.Hide();
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

        // 라운드UI 초기화
        var roundUI = FindAnyObjectByType<RoundPrograssUI>();
        if (roundUI != null)
        {
            roundUI.ResetUI();
        }

        // ===== 적 정보(CSV 스탯) 재적용 =====
        if (enemyGrid != null)
        {
            foreach (var unit in enemyGrid.GetAllFieldUnits())
            {
                var enemy = unit as Enemy;
                if (enemy == null) continue;

                enemy.SetStats(1);
            }
        }

        // ===== 기물 합성 데이터 초기화 =====
        if (ChessCombineManager.Instance != null)
        {
            ChessCombineManager.Instance.ResetAll();
        }

        // ===== 풀 초기화 =====
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ResetAllPools();
        }



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
            //Debug.LogError("[ReturnToMainMenu] StartPanelUI not found");
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

        if (enemyGrid != null)
        {
            enemyGrid.SpawnEnemy(1);
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
        int lastSec = Mathf.CeilToInt(t);

        while (t > 0f)
        {
            int currentSec = Mathf.CeilToInt(t);
            if (currentSec != lastSec)
            {
                SettingsUI.PlaySFX("Start3Sec", Vector3.zero, 1f, 1f);
                lastSec = currentSec;
            }

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
        // 적 유닛
        //var enemyGrid = FindAnyObjectByType<EnemyGrid>();
        if (enemyGrid != null)
        {
            foreach (var unit in enemyGrid.GetAllFieldUnits())
            {
                var chess = unit.GetComponent<Chess>();
                if (chess != null && chess.IsDead)
                {
                    unit.gameObject.SetActive(false); 
                }
            }
        }

        //플레이어 
        //var fieldGrid = FindAnyObjectByType<FieldGrid>();
        if (fieldGrid != null)
        {
            foreach (var unit in fieldGrid.GetAllFieldUnits())
            {
                var chess = unit.GetComponent<Chess>();
                if (chess != null && chess.IsDead)
                {
                    unit.gameObject.SetActive(false);  
                }
            }
        }
    }


    private void ResetEnemyUnitsForNewRound()
    {
        //var enemyGrid = StaticRegistry<EnemyGrid>.Find();
        if (enemyGrid == null) return;

        foreach (var node in enemyGrid.FieldGrid)
        {
            if (node.ChessPiece == null) continue;

            var chess = node.ChessPiece.GetComponent<Chess>();
            if (chess == null) continue;

            if (!chess.gameObject.activeSelf)
                chess.gameObject.SetActive(true);

            chess.SetPosition(node.worldPosition);
            chess.SetOnField(true);
            chess.ResetForNewRound_Chess();
        }
    }
    private void ResetPlayerUnitsForNewRound()
    {
        //var fieldGrid = StaticRegistry<FieldGrid>.Find();
        if (fieldGrid != null)
        {
            foreach (var node in fieldGrid.FieldGrid)
            {
                if (node.ChessPiece == null) continue;

                var chess = node.ChessPiece.GetComponent<Chess>();
                if (chess == null) continue;

                if (!chess.gameObject.activeSelf)
                    chess.gameObject.SetActive(true);

                chess.SetPosition(node.worldPosition);
                chess.SetOnField(true);
                chess.ResetForNewRound_Chess();

                ChessCombineManager.Instance?.Register(chess);
            }
        }

        //var benchGrid = StaticRegistry<BenchGrid>.Find();
        if (benchGrid != null)
        {
            foreach (var node in benchGrid.FieldGrid)
            {
                if (node.ChessPiece == null) continue;

                var chess = node.ChessPiece.GetComponent<Chess>();
                if (chess == null) continue;

                if (!chess.gameObject.activeSelf)
                    chess.gameObject.SetActive(true);

                chess.SetPosition(node.worldPosition);
                chess.SetOnField(false);
                chess.ResetForNewRound_Chess();

                ChessCombineManager.Instance?.Register(chess);
            }
        }
    }

    // 마지막전투기물들을 저장하는 메서드
    private void SaveLastBattleUnits()
    {
        lastBattleUnits.Clear();

        //var fieldGrid = FindAnyObjectByType<FieldGrid>();
        if (fieldGrid == null) return;

        foreach (var unit in fieldGrid.GetAllFieldUnits())
        {
            var chess = unit.GetComponent<Chess>();
            if (chess == null) continue;
            if (chess.team != Team.Player) continue;

            lastBattleUnits.Add(new EndGameUnitSnapshot
            {
                portrait = chess.BaseData.gameOverPortrait,
                starLevel = chess.StarLevel,
                unitName = chess.BaseData.unitName
            });
        }
    }

    private void ClearAllVFX() => VFXManager.ClearAllVFX();
}
