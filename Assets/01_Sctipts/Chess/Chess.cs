using System;
using UnityEngine;

/*
- 전투 시스템
- 사망 시 비활성화 
    - 기물 조합 시 반환
- 마나에 따른 스킬 실행
- 조합 시 성급 상승
*/

public enum Team
{
    Player,
    Enemy
}

public class Chess : ChessStateBase
{
    //=====================================================
    //                  타겟 / 이벤트
    //=====================================================

    [SerializeField] private float rotateSpeed = 720f; //회전속도
    public Team team; //유닛 소속을 정합니다 플레이어기물인지 적 기물인지.
    private Chess currentTarget; //현재 공격대상입니다
    public bool overrideState = false; //외부에서 제어중이라면 Update 전투로직을 막기위해 만들어뒀습니다.
    private bool isInBattlePhase = false; //현재 라운드가 Battle인지 여부를 체크합니다.

    public event Action<Chess> OnDead; //사망시 매니저에게 알리기위한 이벤트입니다
    public event Action<Chess> OnUsedAsMaterial; //조합에 사용되는 처리용 이벤트입니다. 풀반환이라던가,벤치 정리등.
    public float AttackRange => (baseData != null && baseData.attackRange > 0f) ? baseData.attackRange : 1.8f; //사거리
    public float MoveSpeed => (baseData != null) ? baseData.moveSpeed : 0f;   
                                                                              

    //=====================================================
    //                  초기화
    //=====================================================
    protected override void Awake()
    {
        base.Awake();

        //if (GameManager.Instance != null)
        //{
        //    GameManager.Instance.OnRoundStateChanged += HandleRoundStateChanged;
        //}
    }
    private void Start()
    {
        TryRegisterGameManager(); //라운드 이벤트 구독
    }
    private void OnEnable()
    {
        TryRegisterGameManager(); //풀링 재활성화할때 구독끊기는거 방지용으로 재등록하게했습니다
    }

    private void OnDestroy()
    {
        //씬이 끝나거나 파괴할때 이벤트 누수가 되는걸 방지합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged;
        }
    }
    private void TryRegisterGameManager()
    {
        //중복구독을 방지하기 위해서 해제를 먼저하고 다시 등록합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged; //해제
            GameManager.Instance.OnRoundStateChanged += HandleRoundStateChanged; //등록
        }
    }
    //=====================================================
    //                  라운드 상태 처리
    //=====================================================
    private void HandleRoundStateChanged(RoundState newState)
    {
        //라운드에 따른 전투루프와 상태를 관리합니다.
        switch (newState)
        {
            case RoundState.Preparation:
                overrideState = false; //결과,연출로 강제 상태였다면 정상적으로 복귀
                if (animator != null)
                {
                    animator.SetTrigger("ToIdle"); //Animator있을때만
                }
                ExitBattlePhase(); //타겟타이머 초기화시키고 Idle로 복귀시킵니다.
                break;

            case RoundState.Battle:
                EnterBattlePhase(); //Battle일때 ,Update 루프를 활성화시킵니다.
                break;

            case RoundState.Result:
                ExitBattlePhase(); //타겟제거 및 복귀
                break;
        }
    }

    private void EnterBattlePhase()
    {
        isInBattlePhase = true; //Update가 돌 수있도록 플래그

        ////Battle 상태가 필요한 유닛만 전환시킵니다
        //if (baseData != null && baseData.useBattleState)
        //{
        //    stateMachine?.SetBattle(); //이건 Battle상태머신이 필요할떄 쓰려고 둔건데..
        //}
    }

    private void ExitBattlePhase()
    {
        isInBattlePhase = false;//전투중단
        currentTarget = null; //이전타겟 제거
        attackTimer = attackInterval; //공격타이머 초기화

        if (overrideState) return; //외부연출중이라면 덮어쓰기 방지.
        stateMachine?.SetIdle(); //기본상태복귀
    }

    //=====================================================
    //                  업데이트 루프
    //=====================================================
    private void Update()
    {
        if (overrideState) return; //마찬가지로 연출상태라면 내부로직 중단.
        if (IsDead) return;//사망했다면 로직중단
        if (!isInBattlePhase) return; //Battle라운드가 아니면 중단.

        if (currentTarget != null && !currentTarget.IsDead)
        {
            FaceTarget(currentTarget.transform);
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist > AttackRange)
            {
                stateMachine?.SetMove(); // Move는 State(int) 기반이라 “상태가 바뀔 때만” 적용됨 (CurrentState 가드로 안전)

                MoveTowards(currentTarget.transform.position); // 실제 이동
                return; // 이동 중엔 공격 로직 실행 X
            }
            if (baseData != null && baseData.useBattleState) //케틀
            {
                stateMachine?.SetBattle();
            }

            attackTimer -= Time.deltaTime;/*공격주기입니다*/
            if (attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                AttackOnce();
            }
        }
    }
    //=====================================================
    //                  이동 관련
    //=====================================================
    private void MoveTowards(Vector3 targetPos)
    {
        // y 고정이 필요한 프로젝트면 targetPos.y도 고정
        targetPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            MoveSpeed * Time.deltaTime
        );
    }

    //=====================================================
    //                  전투 관련
    //=====================================================

    private void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f; 

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    public void SetTarget(Chess target)
    {
        currentTarget = target; //TargetManager가 지정하는 공격대상.
    }

    private void AttackOnce()
    {
        if (currentTarget == null || currentTarget.IsDead) return;
        if (Vector3.Distance(transform.position, currentTarget.transform.position) > AttackRange) return; //사거리 밖이면 공격 
        Debug.Log($"[{name}]Attack once,Interval ={attackInterval}");
        if (currentTarget == null || currentTarget.IsDead) return; //유효한지 체크


        if (animator != null)
        {
            //int index = UnityEngine.Random.Range(0, 2);
            //animator.SetInteger("AttackIndex", index);
            animator.SetTrigger("Attack");
        }

        int damage = GetAttackDamage();
        currentTarget.TakeDamage(damage, this);

        GainMana(manaOnHit); //마나획득
    }


    private int GetAttackDamage()
    {
        int baseDamage = baseData.attackDamage;
        return baseDamage * Mathf.Max(1, StarLevel);
    }

    protected override void Die()
    {
        if (!IsDead) return;

        OnDead?.Invoke(this);
        base.Die();
    }

    //=====================================================
    //                  조합 & 성급 상승
    //=====================================================
    public void CombineWith(Chess material1, Chess material2)
    {
        if (material1 == null || material2 == null) return;

        if (material1.baseData != baseData || material2.baseData != baseData)
            return; //동일 유닛끼리만 조합이 되게.
        if (StarLevel >= 3)
            return; //3성이상은 조합안되게


        StarLevel = Mathf.Min(StarLevel + 1, 3);
        float hpMultiplier = 1.5f; 
        CurrentHP = Mathf.RoundToInt(baseData.maxHP * Mathf.Pow(hpMultiplier, StarLevel - 1));
        CurrentMana = 0; //조합후 마나 초기화

        //재료 소모쪽.
        ConsumeMaterial(material1);
        ConsumeMaterial(material2);

        Debug.Log($"조합됨 ({StarLevel}성)");
    }

    private void ConsumeMaterial(Chess material)
    {
        OnUsedAsMaterial?.Invoke(material); //외부에서 후처리 가능하게.
        material.gameObject.SetActive(false); //풀링반환용
    }
    //=====================================================
    //           게임 상태 따른 기물 State 변화
    //=====================================================
    public void ForceIdle()
    {
        overrideState = false;
        animator.ResetTrigger("ToIdle");
        animator.SetTrigger("ToIdle");
    }


    public void ForceBattle()
    {
        overrideState = true;
        //animator?.SetInteger("State", 2);
        stateMachine?.SetBattle();
    }

    public void ForceVictory()
    {
        overrideState = true;      
        animator?.ResetTrigger("Attack"); 
        animator?.SetTrigger("Victory");  
    }

    // =============== 기즈모 =============== //
    private void OnDrawGizmosSelected()
    {
        if (baseData == null)
        {
            Gizmos.color = Color.red;                 
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.3f);
            return;
        }

        Gizmos.color = Color.green;                   //사거리 표시
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }







}
