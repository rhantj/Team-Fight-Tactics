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

    [SerializeField] private float winResultTime = 2.5f; //승리시 2.5초 춤추는거 볼 시간. (12.12 add Kim)
    [SerializeField] private float loseResultTime = 2.0f;
    private bool lastBattleWin = false;

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

        while (!isReady)
        {
            yield return null;  
        }

        //전투단계
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

        Debug.Log("[GameManger] Battle start requested");
        isReady = true;

    }

}
