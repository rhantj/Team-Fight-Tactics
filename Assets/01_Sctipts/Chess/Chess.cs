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
    public Chess CurrentTarget => currentTarget; //바이 E 스킬때문에 넣었어요 12.17 add Kim
    private bool isOnField = false; // 필드에 배치되어 있는지 여부  // 12.17 add Kim


    //=====================================================
    //                  초기화
    //=====================================================
    protected override void Awake()
    {
        base.Awake();
        overrideState = false; // 초기화
    }
    private void Start()
    {
        GetComponentInChildren<ChessStatusUI>()?.Bind(this);
        TryRegisterGameManager();
        overrideState = false; // 명시적으로 false 설정
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        TryRegisterGameManager();
        overrideState = false; // 풀링 재활성화 시에도 false로 초기화
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

            HandleRoundStateChanged(GameManager.Instance.roundState);
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
                    if (HasAnimParam("ToIdle"))
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
        overrideState = false; // 전투 시작 시 강제로 false (어디선가 true로 설정한 경우 대비)
        attackTimer = 0f; // 전투 시작 시 즉시 평타 가능하도록 0으로 설정

        Debug.Log($"[{gameObject.name}] EnterBattlePhase - overrideState={overrideState}, attackTimer={attackTimer}");

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("UseSkill");
            animator.ResetTrigger("ToIdle");
        }

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
        if (overrideState)
        {
            return;
        }
        if (IsDead) return;
        if (!isInBattlePhase) return;

        if (!isOnField) return; //필드에 없던애들은 못싸우게.

        if (currentTarget != null && !currentTarget.IsDead && currentTarget.IsTargetable)
        {
            FaceTarget(currentTarget.transform); //항상 현재 타겟을 바라보게 회전

            float dist = Vector3.Distance(
                transform.position,
                currentTarget.transform.position
            );

            if (dist > AttackRange)
            {
                stateMachine?.SetMove();         //사거리 밖이면 이동 상태 유지
                MoveTowards(currentTarget.transform.position); //타겟 방향으로 계속 접근

            }
            else
            {
                //if (baseData != null && baseData.useBattleState)
                stateMachine?.SetIdle();   //사거리 안이면 전투 상태로 
            }

            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f && dist <= AttackRange)
            {
                attackTimer = attackInterval;    //다음 공격을 위해 쿨타임 초기화
                AttackOnce();
            }
        }
        else
        {
            currentTarget = null;
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
        if (Vector3.Distance(transform.position, currentTarget.transform.position) > AttackRange) return;
        Debug.Log($"[ATK] {name} t={Time.time:F2} interval={attackInterval:F3} AS={AttackSpeed:F2}");
        if (animator != null)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
        }
        //animator?.ResetTrigger("Attack");
        animator?.SetTrigger("Attack");

        int damage = GetAttackDamage();
        currentTarget.TakeDamage(damage, this);
        InvokeOnHitEffects(currentTarget);
        GainMana(manaOnHit);
    }


    private void InvokeOnHitEffects(Chess target)
    {
        var effects = GetComponents<IOnHitEffect>();
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].OnHit(this, target);
        }
    }

    private int GetAttackDamage()
    {
        int baseDamage = baseData.attackDamage;
        return baseDamage * Mathf.Max(1, StarLevel);
    }

    protected override void Die()
    {
        if (!IsDead) return;

        base.Die();
        OnDead?.Invoke(this);
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
        //material.gameObject.SetActive(false); 
    }
    //=====================================================
    //           게임 상태 따른 기물 State 변화
    //=====================================================
    public void ForceIdle()
    {
        overrideState = false;

        if (animator != null && HasAnimParam("ToIdle"))
        {
            animator.ResetTrigger("ToIdle");
            animator.SetTrigger("ToIdle");
        }

        stateMachine?.SetIdle(); // 애니 트리거가 없으면 로직이라도 Idle로 복귀
    }

    public void ForceBattle()
    {
        //overrideState = false;
        //animator?.SetInteger("State", 2);
        animator?.ResetTrigger("Attack");
        animator?.SetTrigger("Attack");

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
    //=====================================================
    //                  필드 배치 관리
    //=====================================================
    public void SetOnField(bool onField)
    {
        isOnField = onField;
    }

}