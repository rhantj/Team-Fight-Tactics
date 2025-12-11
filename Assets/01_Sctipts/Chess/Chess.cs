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
    public Team team;
    //=====================================================
    //                  타겟 / 이벤트
    //=====================================================
    private Chess currentTarget;
    public bool overrideState = false; //외부제어용 플래그
    private bool isInBattlePhase = false; 

    public event Action<Chess> OnDead;
    public event Action<Chess> OnUsedAsMaterial;
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
        TryRegisterGameManager();
    }
    private void OnEnable()
    {
        TryRegisterGameManager();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged;
        }
    }
    private void TryRegisterGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStateChanged -= HandleRoundStateChanged;
            GameManager.Instance.OnRoundStateChanged += HandleRoundStateChanged;
        }
    }
    //=====================================================
    //                  라운드 상태 처리
    //=====================================================
    private void HandleRoundStateChanged(RoundState newState)
    {
        switch (newState)
        {
            case RoundState.Preparation:
                ExitBattlePhase();
                break;

            case RoundState.Battle:
                EnterBattlePhase();
                break;

            case RoundState.Result:
                ExitBattlePhase();
                break;
        }
    }

    private void EnterBattlePhase()
    {
        isInBattlePhase = true;

        //Battle 상태가 필요한 유닛만 전환시킵니다
        if (baseData != null && baseData.useBattleState)
        {
            stateMachine?.SetBattle();
        }
    }

    private void ExitBattlePhase()
    {
        isInBattlePhase = false;
        currentTarget = null;
        attackTimer = attackInterval;
        stateMachine?.SetIdle();
    }
    //=====================================================
    //                  업데이트 루프
    //=====================================================
    private void Update()
    {
        if (overrideState) return;
        if (IsDead) return;
        if (!isInBattlePhase) return;

        if (currentTarget != null && !currentTarget.IsDead)
        {
            if (baseData != null && baseData.useBattleState) //케틀
            {
                stateMachine?.SetBattle();
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                AttackOnce();
            }
        }
    }
    //=====================================================
    //                  전투 관련
    //=====================================================
    public void SetTarget(Chess target)
    {
        currentTarget = target;
    }

    private void AttackOnce()
    {
        if (currentTarget == null || currentTarget.IsDead) return;


        if (animator != null)
        {
            //int index = UnityEngine.Random.Range(0, 2);
            //animator.SetInteger("AttackIndex", index);
            animator.SetTrigger("Attack");
        }

        int damage = GetAttackDamage();
        currentTarget.TakeDamage(damage, this);

        GainMana(manaOnHit);
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
            return;
        if (StarLevel >= 3)
            return;
        StarLevel = Mathf.Min(StarLevel + 1, 3);
        float hpMultiplier = 1.5f;
        CurrentHP = Mathf.RoundToInt(baseData.maxHP * Mathf.Pow(hpMultiplier, StarLevel - 1));
        CurrentMana = 0;

        //재료 소모쪽.
        ConsumeMaterial(material1);
        ConsumeMaterial(material2);

        Debug.Log($"조합됨 (현재 성급: {StarLevel})");
    }

    private void ConsumeMaterial(Chess material)
    {
        OnUsedAsMaterial?.Invoke(material);
        material.gameObject.SetActive(false);
    }
    //=====================================================
    //           게임 상태 따른 기물 State 변화
    //=====================================================
    public void ForceIdle()
    {
        overrideState = true;
        animator?.SetInteger("State", 0);
    }

    public void ForceBattle()
    {
        overrideState = true;
        animator?.SetInteger("State", 2);
    }
}
