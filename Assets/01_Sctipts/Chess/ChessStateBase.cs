using System.Collections.Generic;
using UnityEngine;

public abstract class ChessStateBase : MonoBehaviour
{
    //=====================================================
    //                  템플릿 / 기본 스탯
    //=====================================================
    [Header("템플릿")]
    [SerializeField]
    protected ChessStatData baseData;

    public int CurrentHP { get; protected set; }
    public int CurrentMana { get; protected set; }
    public int StarLevel { get; protected set; }
    public bool IsDead => CurrentHP <= 0;

    //=====================================================
    //                  전투 / 마나 설정
    //=====================================================
    [Header("전투 설정")]
    [Tooltip("공격 인터벌 (초당 공격 속도에서 자동 계산)")]
    [SerializeField] protected float attackInterval = 1.0f;

    [Tooltip("공격 시 획득 마나량")]
    [SerializeField] protected int manaOnHit = 10;

    [Tooltip("피격 시 얻는 마나량")]
    [SerializeField] protected int manaOnDamaged = 5;

    protected float baseAttackInterval;
    protected float attackSpeedMultiplier = 1f;
    protected float attackTimer;
    protected Animator animator;
    protected StateMachine stateMachine;

    //=====================================================
    //                  초기화
    //=====================================================
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        stateMachine = GetComponent<StateMachine>();
        InitFromSO();
    }

    public virtual void InitFromSO()
    {
        if (baseData == null)
        {
            return;
        }

        StarLevel = baseData.starLevel;
        CurrentHP = baseData.maxHP;
        CurrentMana = 0;

        //아래는 공격횟수 계산입니다. 초당 공속을 위해 인터벌로 계산해둔거에요
        if (baseData.attackSpeed > 0f)
        {
            baseAttackInterval = 1f / baseData.attackSpeed;
            attackInterval = baseAttackInterval;
        }

        attackTimer = attackInterval;
    }

    //=====================================================
    //                  전투 / 피격
    //=====================================================
    public virtual void TakeDamage(int amount, Chess attacker = null)
    {
        if (IsDead) return;

        int finalDamage = Mathf.Max(1, amount - baseData.armor);

        CurrentHP -= finalDamage;
        if (CurrentHP < 0) CurrentHP = 0;

        GainMana(manaOnDamaged);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    //=====================================================
    //                  마나 & 스킬
    //=====================================================
    public virtual void GainMana(int amount)
    {
        if (IsDead) return;

        CurrentMana += amount;
        if (CurrentMana >= baseData.mana)
        {
            CurrentMana = 0;
            CastSkill();
        }
    }

    protected virtual void CastSkill()
    {
        stateMachine?.SetSkill();
        animator?.SetTrigger("Skill");
    }

    //=====================================================
    //                  사망 처리
    //=====================================================
    protected virtual void Die()
    {
        if (!IsDead) return;

        Debug.Log("사망");
        stateMachine?.SetDie();
        animator?.SetTrigger("Die");
        gameObject.SetActive(false);
    }

    //=====================================================
    //                  특성 접근
    //=====================================================
    public IReadOnlyList<TraitType> Traits => baseData?.traits;

    public bool HasTrait(TraitType trait)
    {
        if (baseData == null || baseData.traits == null) return false;

        foreach (var t in baseData.traits)
        {
            if (t == trait) return true;
        }

        return false;
    }

    //=====================================================
    //                  공속 보정 (시너지 등)
    //=====================================================
    public void SetAttackSpeedMultiplier(float multiplier)
    {
        attackSpeedMultiplier = Mathf.Max(0.1f, multiplier);

        if (baseAttackInterval > 0f)
        {
            attackInterval = baseAttackInterval / attackSpeedMultiplier;
        }
    }


    //=====================================================
    //                  위치 변경
    //=====================================================
    public void SetPosition(Vector3 position)
    {
        position.y = 1.5f;
        transform.position = position;
    }
}
