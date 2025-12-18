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
    //          보너스 스탯 (시너지 / 아이템 분리)
    //=====================================================

    // 시너지 보너스
    protected int bonusMaxHP_Synergy;
    protected int bonusAttack_Synergy;
    protected int bonusArmor_Synergy;

    // 아이템 보너스
    protected int bonusMaxHP_Item;
    protected int bonusAttack_Item;
    protected int bonusArmor_Item;

    // ================== 최종 스탯 계산 ==================
    public int MaxHP =>
        (baseData != null ? baseData.maxHP : 0)
        + bonusMaxHP_Synergy
        + bonusMaxHP_Item;

    public int AttackDamage =>
        (baseData != null ? baseData.attackDamage : 0)
        + bonusAttack_Synergy
        + bonusAttack_Item;

    public int Armor =>
        (baseData != null ? baseData.armor : 0)
        + bonusArmor_Synergy
        + bonusArmor_Item;

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
    public float AttackSpeedMultiplier => attackSpeedMultiplier;

    //=====================================================
    //                  Shield (Barrier)
    //=====================================================
    [Header("Shield")]
    [SerializeField, Tooltip("현재 실드(베리어) 수치")]
    protected int currentShield;

    public int CurrentShield => currentShield;

    private Coroutine shieldCoroutine;
    private int shieldVersion = 0;

    //=====================================================
    //                  Shield API
    //=====================================================
    public void AddShield(int amount, float duration)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        currentShield += amount;
        if (duration > 0f)
        {
            shieldVersion++;

            if (shieldCoroutine != null)
                StopCoroutine(shieldCoroutine);

            shieldCoroutine = StartCoroutine(ShieldExpireRoutine(shieldVersion, duration));
        }
    }

    public void ClearShield()
    {
        currentShield = 0;

        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
    }

    private System.Collections.IEnumerator ShieldExpireRoutine(int version, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (version != shieldVersion) yield break;

        currentShield = 0;
        shieldCoroutine = null;
    }

    //=====================================================
    //                  초기화
    //=====================================================
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        stateMachine = GetComponent<StateMachine>();
        InitFromSO();
    }

    protected virtual void OnEnable()
    {
        currentShield = 0;
    }

    public virtual void SetBaseData(ChessStatData newData)
    {
        // 같은 SO면 재초기화 방지 (이동 / 드롭 보호)
        if (baseData == newData)
            return;

        baseData = newData;
        InitFromSO();
    }

    public virtual void InitFromSO()
    {
        if (baseData == null)
            return;

        StarLevel = baseData.starLevel;

        bonusMaxHP_Synergy = 0;
        bonusAttack_Synergy = 0;
        bonusArmor_Synergy = 0;

        bonusMaxHP_Item = 0;
        bonusAttack_Item = 0;
        bonusArmor_Item = 0;

        CurrentHP = MaxHP;
        CurrentMana = 0;

        if (baseData.attackSpeed > 0f)
        {
            baseAttackInterval = 1f / baseData.attackSpeed;
            attackInterval = baseAttackInterval;
        }

        attackTimer = attackInterval;
    }

    //=====================================================
    //                  Kill Tracking
    //=====================================================
    protected Chess lastAttacker;
    private bool deathHandled = false;

    //=====================================================
    //                  전투 / 피격
    //=====================================================
    public virtual void TakeDamage(int amount, Chess attacker = null)
    {
        if (IsDead) return;
        if (attacker != null) lastAttacker = attacker;

        int finalDamage = Mathf.Max(1, amount - Armor);

        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, finalDamage);
            currentShield -= absorbed;
            finalDamage -= absorbed;
        }

        if (finalDamage > 0)
        {
            CurrentHP -= finalDamage;
            if (CurrentHP < 0) CurrentHP = 0;
        }

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
        var manager = GetComponent<SkillManager>();
        if (manager != null)
        {
            manager.TryCastSkill();
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
        if (!IsDead || deathHandled) return;
        deathHandled = true;

        if (lastAttacker != null && lastAttacker != (this as Chess))
        {
            var effects = lastAttacker.GetComponents<IOnKillEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].OnKill(lastAttacker, this as Chess);
            }
        }

        stateMachine?.SetDie();
        animator?.SetTrigger("Die");
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
    //                  보너스 스탯 API (외부 호환 유지)
    //=====================================================

    // 기존 SynergyManager 호출용
    public void AddBonusStats(int attack, int armor, int hp)
    {
        SetSynergyBonusStats(attack, armor, hp);
    }

    public void SetSynergyBonusStats(int attack, int armor, int hp)
    {
        float ratio = MaxHP > 0 ? (float)CurrentHP / MaxHP : 1f;

        bonusAttack_Synergy = attack;
        bonusArmor_Synergy = armor;
        bonusMaxHP_Synergy = hp;

        CurrentHP = Mathf.RoundToInt(MaxHP * ratio);
    }

    public void SetItemBonusStats(int attack, int armor, int hp)
    {
        float ratio = MaxHP > 0 ? (float)CurrentHP / MaxHP : 1f;

        bonusAttack_Item = attack;
        bonusArmor_Item = armor;
        bonusMaxHP_Item = hp;

        CurrentHP = Mathf.RoundToInt(MaxHP * ratio);
    }

    //=====================================================
    //                  시너지 리셋 (기존 API 유지)
    //=====================================================
    public void ResetSynergyStats()
    {
        SetAttackSpeedMultiplier(1f);
        bonusAttack_Synergy = 0;
        bonusArmor_Synergy = 0;
        bonusMaxHP_Synergy = 0;
    }

    //=====================================================
    //                  위치 변경
    //=====================================================
    public void SetPosition(Vector3 position)
    {
        position.y = 1.5f;
        transform.position = position;
    }

    public void OnDieAnimationEnd()
    {
        var pooled = GetComponentInParent<PooledObject>();
        if (pooled != null) pooled.ReturnToPool();
        else gameObject.SetActive(false);
    }

    // 공속 읽기전용
    public float AttackSpeed
    {
        get
        {
            if (attackInterval <= 0f) return 0f;
            return 1f / attackInterval;
        }
    }

    protected bool HasAnimParam(string param)
    {
        if (animator == null) return false;

        foreach (var p in animator.parameters)
        {
            if (p.name == param)
                return true;
        }

        return false;
    }

}
