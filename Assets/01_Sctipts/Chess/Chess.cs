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
    private float lastAttackAnimTime = -999f;

    [SerializeField] private float attackAnimMinInterval = 0.15f;
    private const string AtkSpeedParam = "AtkAnimSpeed";
    [SerializeField] private float atkSpeedBase = 1f;

    //타겟 쫓을때의 간격관련
    [SerializeField] private float approachRatio = 0.85f;
    [SerializeField] private float slotSpacing = 0.65f; //기물 간격

    [Header("배치간격")]
    [SerializeField] private float occupyRadius = 0.35f;   // 이 반경 안에 아군이 있으면 "자리 점유"로 판단
    [SerializeField] private float sideStep = 0.7f;        // 옆으로 비키는 간격
    [SerializeField] private int sideTries = 3;
    public ChessStateBase LastAttackTarget { get; private set; } //마지막공격 대상 추적 필드
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
                overrideState = false;
                if (!IsDead && animator != null)
                {
                    if (HasAnimParam("ToIdle"))
                        animator.SetTrigger("ToIdle");
                }
                ExitBattlePhase();
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
            if (HasAnimParam("Attack")) animator.ResetTrigger("Attack");
            if (HasAnimParam("UseSkill")) animator.ResetTrigger("UseSkill");
            if (HasAnimParam("ToIdle")) animator.ResetTrigger("ToIdle");
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
        if (IsDead) return;  
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
        //ApplyAtkAnimSpeed();
        if (currentTarget != null && !currentTarget.IsDead && currentTarget.IsTargetable)
        {
            FaceTarget(currentTarget.transform); //항상 현재 타겟을 바라보게 회전

            float dist = Vector3.Distance(
                transform.position,
                currentTarget.transform.position
            );
            attackTimer -= Time.deltaTime;
            if (dist > AttackRange)
            {
                Vector3 goal = GetApproachPoint(currentTarget);
                stateMachine?.SetMove();         //사거리 밖이면 이동 상태 유지
                MoveTowards(goal); //타겟 방향으로 계속 접근
                return;
            }

            stateMachine?.SetIdle();   //사거리 안이면 전투 상태로 

            if (attackTimer <= 0f && dist <= AttackRange)
            {
                attackTimer = attackInterval;    //다음 공격을 위해 쿨타임 초기화
                AttackOnce();
            }
        }
        else
        {
            currentTarget = null;
            stateMachine?.SetIdle();
        }
    }


    //=====================================================
    //                  이동 관련
    //=====================================================

    private bool IsAllyOccupying(Vector3 point)
    {
        var allies = (team == Team.Player)
            ? UnitCountManager.Instance.playerUnits
            : UnitCountManager.Instance.enemyUnits;

        Vector3 p = point; p.y = 0f;

        float r2 = occupyRadius * occupyRadius;

        for (int i = 0; i < allies.Count; i++)
        {
            var a = allies[i];
            if (a == null || a == this) continue;
            if (a.IsDead) continue;
            if (!a.isOnField) continue;

            Vector3 ap = a.transform.position; ap.y = 0f;

            if ((ap - p).sqrMagnitude <= r2)
                return true;
        }

        return false;
    }

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

    private Vector3 GetApproachPoint(Chess target)
    {
        Vector3 myPos = transform.position;
        Vector3 tPos = target.transform.position;
        myPos.y = 0f; tPos.y = 0f;

        Vector3 toTarget = (tPos - myPos);
        Vector3 dir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;
        dir.y = 0f;

        Vector3 basePoint = tPos - dir * (AttackRange * approachRatio);

        var list = (team == Team.Player) ? UnitCountManager.Instance.playerUnits : UnitCountManager.Instance.enemyUnits;

        int count = 0;
        int myIndex = 0;
        int myId = GetInstanceID();

        for (int i = 0; i < list.Count; i++)
        {
            var u = list[i];
            if (u == null || u.IsDead) continue;
            if (u.CurrentTarget != target) continue;

            count++;
            if (u.GetInstanceID() < myId) myIndex++;
        }

        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

        if (count > 1)
        {
            float center = (count - 1) * 0.5f;
            float offset = (myIndex - center) * slotSpacing;
            basePoint += right * offset;
        }

        Vector3 candidate = basePoint;

        if (IsAllyOccupying(candidate))
        {
            for (int i = 1; i <= sideTries; i++)
            {
                Vector3 left = basePoint - right * (sideStep * i);
                if (!IsAllyOccupying(left)) { candidate = left; break; }

                Vector3 rgt = basePoint + right * (sideStep * i);
                if (!IsAllyOccupying(rgt)) { candidate = rgt; break; }
            }
        }

        candidate.y = transform.position.y;
        return candidate;
    }


    //=====================================================
    //                  전투 관련
    //=====================================================
    private void ApplyAtkAnimSpeed()
    {
        if (animator == null) return;
        if (!HasAnimParam(AtkSpeedParam)) return;

        float denom = Mathf.Max(0.01f, atkSpeedBase);
        float speed = AttackSpeed / denom;     
        animator.SetFloat(AtkSpeedParam, speed);
    }
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

        // 데미지 / 온힛 / 마나 (기존 유지)
        int damage = GetAttackDamage();
        currentTarget.TakeDamage(damage, this);
        //공격 대상 캐싱
        LastAttackTarget = currentTarget;
        //기본공격 적중 이벤트
        NotifyBasicAttackHit();

        InvokeOnHitEffects(currentTarget);
        GainMana(manaOnHit);

        if (animator == null) return;

        //트리거 난사금지
        float minInterval = Mathf.Min(0.15f, attackInterval * 0.9f);
        if (Time.time - lastAttackAnimTime < minInterval) return;

        var st = animator.GetCurrentAnimatorStateInfo(0);
        float n = st.normalizedTime % 1f;

        bool isAttackState = st.IsName("Attack");

        //히트 프레임 이후면 재트리거 허용 0.3~0.5동적 조절 ㄱㄱ
        const float retriggerGate = 0.35f;
        if (isAttackState && n < retriggerGate) return;

        animator.ResetTrigger("Attack"); //없어도되긴함
        animator.SetTrigger("Attack");
        lastAttackAnimTime = Time.time;
    }
    public void ResetForNewRound_Chess()
    {
        ResetForNewRound();      
        currentTarget = null;  
        overrideState = false;

        stateMachine?.SetIdle();
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
        //int baseDamage = baseData.attackDamage;
        return AttackDamage * Mathf.Max(1, StarLevel);
    }

    protected override void Die()
    {
        if (!IsDead || deathHandled) return; //가드 추가했습니다. kim add 12.24

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