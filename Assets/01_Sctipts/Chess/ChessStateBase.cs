using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ChessStateBase : MonoBehaviour
{
    //=====================================================
    //                  템플릿 / 기본 스탯
    //=====================================================
    [Header("템플릿")]
    [SerializeField]
    protected ChessStatData baseData;
    protected SkillManager skillManager;
    public ChessStatData BaseData => baseData;

    public int CurrentHP { get; protected set; }
    public int CurrentMana { get; protected set; }
    public int StarLevel { get; protected set; }
    public bool IsDead => CurrentHP <= 0;

    public bool IsTargetable { get; private set; } = true;

    public float BaseAttackSpeed => baseData.attackSpeed;
    public float FinalAttackSpeed => baseData.attackSpeed * attackSpeedMultiplier;


    //=====================================================
    //          보너스 스탯 (시너지 / 아이템 분리)
    //=====================================================

    // 시너지 보너스
    protected int bonusMaxHP_Synergy;
    protected int bonusAttack_Synergy;
    protected int bonusArmor_Synergy;
    protected float bonusAttackSpeed_Synergy = 1f;

    // 아이템 보너스
    protected int bonusMaxHP_Item;
    protected int bonusAttack_Item;
    protected int bonusArmor_Item;
    protected float bonusAttackSpeed_Item = 1f;

    // 글로벌 버프 보너스
    protected int bonusMaxHP_Buff;
    protected int bonusAttack_Buff;
    protected int bonusArmor_Buff;
    protected float bonusAttackSpeed_Buff = 1f;

    // ================== 최종 스탯 계산 ==================
    public int MaxHP =>
        (baseData != null ? baseData.maxHP : 0)
        + bonusMaxHP_Synergy
        + bonusMaxHP_Item
        + bonusMaxHP_Buff;

    public int AttackDamage =>
        (baseData != null ? baseData.attackDamage : 0)
        + bonusAttack_Synergy
        + bonusAttack_Item
        + bonusAttack_Buff;

    public int Armor =>
        (baseData != null ? baseData.armor : 0)
        + bonusArmor_Synergy
        + bonusArmor_Item
        + bonusArmor_Buff;

    //=====================================================
    //                  전투 이벤트 addtoKwon
    //=====================================================
    public event System.Action OnBattleStart;
    public event System.Action<int, int> OnHPChanged;
    public event System.Action OnStatChanged;
    public event System.Action OnBasicAttackHit;
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
    public float AttackSpeedMultiplier => attackSpeedMultiplier;

    public float attackTimer;

    protected Animator animator;
    protected StateMachine stateMachine;

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

            shieldCoroutine = StartCoroutine(
                ShieldExpireRoutine(shieldVersion, duration)
            );
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

        if (version != shieldVersion)
            yield break;

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
        skillManager = GetComponent<SkillManager>();
        InitFromSO();
    }

    protected virtual void OnEnable()
    {
        currentShield = 0;
        deathHandled = false;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public virtual void SetBaseData(ChessStatData newData)
    {
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
        bonusAttackSpeed_Synergy = 1f;

        bonusMaxHP_Item = 0;
        bonusAttack_Item = 0;
        bonusArmor_Item = 0;
        bonusAttackSpeed_Item = 1f;

        bonusMaxHP_Buff = 0;
        bonusAttack_Buff = 0;
        bonusArmor_Buff = 0;
        bonusAttackSpeed_Buff = 1f;

        CurrentHP = MaxHP;
        CurrentMana = 0;
        deathHandled = false;

        if (baseData.attackSpeed > 0f)
        {
            baseAttackInterval = 1f / baseData.attackSpeed;
            attackInterval = baseAttackInterval;
        }

        attackTimer = 0f;
        RecalculateAttackSpeed();
        OnStatChanged?.Invoke();
    }

    //=====================================================
    //                  전투 시작 알림
    //=====================================================
    public void NotifyBattleStart()
    {
        OnBattleStart?.Invoke();
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

        float damageMultiplier = 100f / (100f + Armor);
        int finalDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(amount * damageMultiplier)
        );

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

            OnHPChanged?.Invoke(CurrentHP, MaxHP);
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
        if (baseData == null) return;
        if (skillManager != null && skillManager.IsCasting)
            return;
        if (CurrentMana >= baseData.mana) return;

        CurrentMana += amount;

        if (CurrentMana >= baseData.mana)
        {
            CurrentMana = baseData.mana;

            if (TryCastSkillInternal())
                CurrentMana = 0;
        }
    }

    private bool TryCastSkillInternal()
    {
        if (skillManager != null)
            return skillManager.TryCastSkill();

        if (HasAnimParam("UseSkill"))
        {
            animator.ResetTrigger("UseSkill");
            animator.SetTrigger("UseSkill");
            return true;
        }

        return false;
    }

    protected virtual void CastSkill()
    {
        TryCastSkillInternal();
    }

    //=====================================================
    //                  사망 처리
    //=====================================================
    protected virtual void Die()
    {
        if (!IsDead || deathHandled) return;
        deathHandled = true;

        var statusUI = GetComponentInChildren<ChessStatusUI>();
        if (statusUI != null)
        {
            statusUI.ForceRefreshHP();
        }

        if (lastAttacker != null && lastAttacker != (this as Chess))
        {
            var effects = lastAttacker.GetComponents<IOnKillEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].OnKill(lastAttacker, this as Chess);
            }
        }

        if (HasAnimParam("Die"))
        {
            animator.ResetTrigger("Die");
            animator.SetTrigger("Die");
        }
    }

    //=====================================================
    //                  특성 접근
    //=====================================================
    public IReadOnlyList<TraitType> Traits => baseData?.traits;

    public bool HasTrait(TraitType trait)
    {
        if (baseData == null || baseData.traits == null)
            return false;

        foreach (var t in baseData.traits)
            if (t == trait) return true;

        return false;
    }

    //=====================================================
    //                  공속 계산 핵심
    //=====================================================
    private void RecalculateAttackSpeed()
    {
        attackSpeedMultiplier =
            bonusAttackSpeed_Synergy
            * bonusAttackSpeed_Item
            * bonusAttackSpeed_Buff;

        attackSpeedMultiplier = Mathf.Max(0.1f, attackSpeedMultiplier);

        if (baseAttackInterval > 0f)
        {
            attackInterval = baseAttackInterval / attackSpeedMultiplier;
        }
        if (animator != null && HasAnimParam("AtkAnimSpeed"))
            animator.SetFloat("AtkAnimSpeed", attackSpeedMultiplier);

    }

    // ================================
    // 기존 코드 호환용 API (절대 삭제 X)
    // ================================
    public void SetAttackSpeedMultiplier(float multiplier)
    {
        // 기존 호출부는 "최종 배수"를 넣는다고 생각함
        // 그 값을 시너지 공속으로 흡수
        bonusAttackSpeed_Synergy = Mathf.Max(0.1f, multiplier);
        RecalculateAttackSpeed();
        OnStatChanged?.Invoke();

        if (animator != null && HasAnimParam("AtkAnimSpeed"))
        {
            animator.SetFloat("AtkAnimSpeed", attackSpeedMultiplier);
        }
    }


    //=====================================================
    //                  보너스 스탯 API
    //=====================================================
    public void AddBonusStats(int attack, int armor, int hp, float attackSpeed = 1f)
    {
        SetSynergyBonusStats(attack, armor, hp, attackSpeed);
    }

    public void SetSynergyBonusStats(int attack, int armor, int hp, float attackSpeed = 1f)
    {
        float ratio = MaxHP > 0 ? (float)CurrentHP / MaxHP : 1f;

        bonusAttack_Synergy = attack;
        bonusArmor_Synergy = armor;
        bonusMaxHP_Synergy = hp;
        bonusAttackSpeed_Synergy = attackSpeed;

        CurrentHP = Mathf.RoundToInt(MaxHP * ratio);
        RecalculateAttackSpeed();
        OnStatChanged?.Invoke();
    }

    public void SetItemBonusStats(int attack, int armor, int hp, float attackSpeed = 1f)
    {
        bonusAttack_Item = attack;
        bonusArmor_Item = armor;
        bonusMaxHP_Item = hp;
        bonusAttackSpeed_Item *= attackSpeed;

        CurrentHP += hp;
        if (CurrentHP > MaxHP)
            CurrentHP = MaxHP;

        RecalculateAttackSpeed();
        OnStatChanged?.Invoke();
    }

    public void GlobalBuffApply(float multiplier)
    {
        if (true)//(baseData.traits.Contains(TraitType.Melee))
        {
            bonusAttack_Buff = (int)(AttackDamage * (multiplier - 1f));
            bonusArmor_Buff = (int)(Armor * (multiplier - 1f));
            bonusMaxHP_Buff = (int)(MaxHP * (multiplier - 1f));
            bonusAttackSpeed_Buff *= multiplier;

            CurrentHP += bonusMaxHP_Buff;
            RecalculateAttackSpeed();
            OnStatChanged?.Invoke();
        }
    }

    //=====================================================
    //                  리셋
    //=====================================================
    public void ResetSynergyStats()
    {
        bonusAttack_Synergy = 0;
        bonusArmor_Synergy = 0;
        bonusMaxHP_Synergy = 0;
        bonusAttackSpeed_Synergy = 1f;

        RecalculateAttackSpeed();
        OnStatChanged?.Invoke();
    }

    public void ClearAllBuffs()
    {
        bonusAttack_Buff = 0;
        bonusArmor_Buff = 0;
        bonusMaxHP_Buff = 0;
        bonusAttackSpeed_Buff = 1f;

        CurrentHP = MaxHP;
        RecalculateAttackSpeed();
        OnStatChanged?.Invoke();
    }

    //=====================================================
    //                  위치 / 기타
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
            if (p.name == param) return true;

        return false;
    }

    public void SetTargetable(bool value)
    {
        IsTargetable = value;
    }

    //공격 적중 알림용 메서드
    protected void NotifyBasicAttackHit()
    {
        OnBasicAttackHit?.Invoke();
    }
}
