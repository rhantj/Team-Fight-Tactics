using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 타겟 탐색 관리
/// - 가장 가까운 적 탐색
/// </summary>
public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance { get; private set; }
    private List<Chess> playerUnits = new List<Chess>();
    private List<Chess> enemyUnits = new List<Chess>();

    private float updateTimer = 0f;

    [Header("타겟 설정")]
    [SerializeField] private float targetUpdateInterval = 0.5f;
    [SerializeField] private float maxDetectionRange = 20f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged += OnRoundStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged -= OnRoundStateChanged;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.roundState != RoundState.Battle) return;
        updateTimer += Time.deltaTime;
        if (updateTimer >= targetUpdateInterval)
        {
            updateTimer = 0f;
            RegisterAllUnits();
            AssignTargets();
        }
    }
    //================================================
    //               라운드 상태 처리
    //================================================
    private void OnRoundStateChanged(RoundState newState)
    {
        switch (newState)
        {
            case RoundState.Battle:
                //RegisterAllUnits(); //유닛목록 수집합니다
                //AssignTargets(); //타겟 할당 코드입니다.
                StartCoroutine(BattleSetup()); //12.18 kim Add
                break;
            case RoundState.Preparation:
            case RoundState.Result:
                ClearAllTargets(); //타겟 초기화
                break;
            //호출
            //RegisterAllUnits ==
            //AssignTargets ==
            //ClearAllTargets();
        }
    }

    private IEnumerator BattleSetup() //12.18 kim Add
    {
        yield return null;
        RegisterAllUnits();
        AssignTargets();
    }

    //================================================
    //                  유닛 등록
    //================================================
    private void RegisterAllUnits()
    {
        playerUnits.Clear();
        enemyUnits.Clear();

        var fieldGrid = StaticRegistry<FieldGrid>.Find();
        if (fieldGrid != null)
        {
            foreach (var unit in fieldGrid.GetAllFieldUnits())
            {
                if (unit == null) continue;
                var chess = unit.GetComponent<Chess>();
                if (chess == null || chess.IsDead) continue;
                playerUnits.Add(chess);
            }
        }

        var enemyGrid = StaticRegistry<EnemyGrid>.Find();
        if (enemyGrid != null)
        {
            foreach (var unit in enemyGrid.GetAllFieldUnits())
            {
                if (unit == null) continue;
                var chess = unit.GetComponent<Chess>();
                if (chess == null || chess.IsDead) continue;
                enemyUnits.Add(chess);
            }
        }
    }

    //================================================
    //                  타겟 할당
    //================================================
    private void AssignTargets()
    {
        foreach (var player in playerUnits) //가장 가까운 적을 찾습니다.
        {
            if (player == null || player.IsDead) continue;

            Chess target = FindNearestEnemy(player, enemyUnits);
            if (target != null)
            {
                player.SetTarget(target);
            }
        }

        foreach (var enemy in enemyUnits) //반대로 유닛이 플레이어를 가장가까운 애부터 찾습니다.
        {
            if (enemy == null || enemy.IsDead) continue;
            Chess target = FindNearestEnemy(enemy, playerUnits);
            if (target != null)
            {
                enemy.SetTarget(target);
            }
        }
    }

    private Chess FindNearestEnemy(Chess attacker, List<Chess> enemyList)//기준유닛
    {
        Chess nearest = null;
        float nearestDist = maxDetectionRange;

        foreach (var enemy in enemyList)
        {
            if (enemy == null || enemy.IsDead||!enemy.IsTargetable) continue;
            float dist = Vector3.Distance(attacker.transform.position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }
    //================================================
    //                  타겟 초기화
    //================================================
    private void ClearAllTargets()
    {
        foreach (var chess in playerUnits)
        {
            if (chess != null) chess.SetTarget(null);

        }
        foreach (var chess in enemyUnits)
        {
            if (chess != null) chess.SetTarget(null);
        }

        playerUnits.Clear();
        enemyUnits.Clear();
    }
    //================================================
    //                  수동갱신용
    //================================================
    public void RefreshTargets() //외부호출용,
    {
        RegisterAllUnits();
        AssignTargets();
    }
}
