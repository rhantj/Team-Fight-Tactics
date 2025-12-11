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
    Battle,
    Result          //라운드 승/패 연출타임, 정산
}

public enum BattleResult //전투 결과
{ 
    None,
    PlayerWin,
    PlayerLose
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        float timer = preparationTime;

        while(timer > 0f)
        {
            timer -= Time.deltaTime;

            //준비시간 갱신 UI전달
            OnPreparationTimerUpdated?.Invoke(timer);

            yield return null;
        }

        //전투단계
        StartBattle();

        float battleTimer = battleTime;

        while(true)
        {
            battleTimer -=Time.deltaTime;
            OnBattleTimerUpdated?.Invoke(battleTimer);

            bool playerAllDead = UnitCountManager.Instance.ArePlayerAllDead();
            bool enemyAllDead = UnitCountManager.Instance.AreEnemyAllDead();

            //if(playerAllDead || enemyAllDead) //임시임 이거 있으면 겜매니저 안돌아서 나중에 적 기물 만들고 실험해봐야할듯
            //{
            //    bool playerWin = enemyAllDead && !playerAllDead;
            //    EndBattle(playerWin);
            //    break;
            //}

            if(battleTimer <= 0f)
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


        //다음 라운드or게임 오버
        if(loseCount >= maxLoseCount)
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
        var fieldGrid = FindAnyObjectByType<FieldGrid>();
        var fieldUnits = fieldGrid.GetAllFieldUnits();

        foreach(var unit in fieldUnits)
        {
            Chess chess = unit.GetComponent<Chess>();
            if (chess == null) continue;

            bool isPlayer = chess.team == Team.Player; //팀 분류 기준
            UnitCountManager.Instance.RegisterUnit(chess, isPlayer);
        }

        SetRoundState(RoundState.Battle);
    }

    private void EndBattle(bool win)
    {
        EndRound(win);
    }
    //라운드 종료 메서드
    private void EndRound(bool win)
    {
        OnRoundEnded?.Invoke(currentRound, win);

        if(!win)
        {
            loseCount++;
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
        roundState = newState;
        OnRoundStateChanged?.Invoke(newState);
    }
}
