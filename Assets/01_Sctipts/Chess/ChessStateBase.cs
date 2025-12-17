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
    public ChessStatData BaseData => baseData;

    public int CurrentHP { get; protected set; }
    public int CurrentMana { get; protected set; }
    public int StarLevel { get; protected set; }
    public bool IsDead => CurrentHP <= 0;
    //=====================================================
    //                  보너스(시너지,아이템) 스탯 
    //=====================================================
    protected int bonusMaxHP;
    protected int bonusAttack;
    protected int bonusArmor;

    public int MaxHP => (baseData != null ? baseData.maxHP : 0) + bonusMaxHP;
    public int AttackDamage => (baseData != null ? baseData.attackDamage : 0) + bonusAttack;
    public int Armor => (baseData != null ? baseData.armor : 0) + bonusArmor;

    //=====================================================
    //                  전투 / 마나 설정
    //=====================================================
    [Header("전투 설정")]
    [Tooltip("공격 인터벌 (초당 공속 계산)")]
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

    public virtual void SetBaseData(ChessStatData newData)
    {
        baseData = newData;
        InitFromSO();
    }
    public virtual void InitFromSO()
    {
        if (baseData == null)
            return; //SO없으면 리턴

        StarLevel = baseData.starLevel;

        bonusMaxHP = 0;
        bonusAttack = 0;
        bonusArmor = 0;

        CurrentHP = MaxHP;   
        CurrentMana = 0;

        if (baseData.attackSpeed > 0f)
        {
            baseAttackInterval = 1f / baseData.attackSpeed;
            attackInterval = baseAttackInterval;
        }

        attackTimer = attackInterval; //첫 전투시작시 공속리셋
    }


    //=====================================================
    //                  전투 / 피격
    //=====================================================
    public virtual void TakeDamage(int amount, Chess attacker = null)
    {
        if (IsDead) return; 

        int finalDamage = Mathf.Max(1, amount - baseData.armor); //최소 1뎀 

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
        var runner = GetComponent<SkillRunner>();
        if (runner != null)
        {
            runner.RequestCast();
            return;
        }

        stateMachine?.SetSkill();
        animator?.SetTrigger("UseSkill");
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
        //gameObject.SetActive(false);
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
    //                  보너스 스탯 적용
    //=====================================================
    public void AddBonusStats(int attack, int armor, int hp)
    {
        bonusAttack = attack;
        bonusArmor = armor;

        int oldMaxHP = MaxHP;
        bonusMaxHP = hp;
        int newMaxHP = MaxHP;

        if (newMaxHP > oldMaxHP && !IsDead)
        {
            int hpIncrease = newMaxHP - oldMaxHP;
            CurrentHP += hpIncrease;
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

    //애니메이션 설정

    public void OnDieAnimationEnd()
    {
        // 풀링 오브젝트면 풀로 반환, 아니면 비활성화
        var pooled = GetComponentInParent<PooledObject>();
        if (pooled != null) pooled.ReturnToPool();
        else gameObject.SetActive(false);
    }

    // 공속 읽기전용 프로퍼티 하나 추가할게요 Won Add
    public float AttackSpeed
    {
        get
        {
            if (attackInterval <= 0f) return 0f;
            return 1f / attackInterval;
        }
    }

    //필드에서 내려갈때 호출 용도로 추가했습니다
    public void ResetSynergyStats()
    {
        SetAttackSpeedMultiplier(1f);
        ClearBonusStats();
    }

    /// <summary>
    /// 시너지 및 기타 버프로 인해 추가된 보너스 스탯을 모두 초기화한다.
    /// 필드를 벗어나거나 시너지 해제 시 호출된다.
    /// </summary>
    public void ClearBonusStats()
    {
        bonusAttack = 0;
        bonusArmor = 0;
        bonusMaxHP = 0;
    }


}
