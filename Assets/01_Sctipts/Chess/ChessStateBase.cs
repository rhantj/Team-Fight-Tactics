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
    public ChessStatData BaseData => baseData;

    public int CurrentHP { get; protected set; }
    public int CurrentMana { get; protected set; }
    public int StarLevel { get; protected set; }
    public bool IsDead => CurrentHP <= 0;

    public bool IsTargetable { get; private set; } = true;

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

    // 글로벌 버프 보너스
    protected int bonusMaxHP_Buff;
    protected int bonusAttack_Buff;
    protected int bonusArmor_Buff;

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
    public float attackTimer; // protected -> public으로 변경
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
        deathHandled = false; // 사망 플래그 리셋 (풀링 재활성화 시 부활 가능하게)

        // 애니메이터 리셋 (Die 상태에서 빠져나오기)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
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

        bonusMaxHP_Buff = 0;
        bonusAttack_Buff = 0;
        bonusArmor_Buff = 0;

    CurrentHP = MaxHP;
        CurrentMana = 0;
        deathHandled = false; // 사망 플래그 리셋

        if (baseData.attackSpeed > 0f)
        {
            baseAttackInterval = 1f / baseData.attackSpeed;
            attackInterval = baseAttackInterval;
        }

        attackTimer = 0f; // attackInterval에서 0f로 변경 - 생성 시 즉시 공격 가능
    }
    //=====================================================
    //                  전투 시작 알림 25/12/22 add to Kwon
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
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(amount * damageMultiplier));

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

        //이미 풀마나면 또 쌓지 않음 (원하면 유지)
        if (CurrentMana >= baseData.mana) return;

        CurrentMana += amount;
        if (CurrentMana >= baseData.mana)
        {
            CurrentMana = baseData.mana;

            if (TryCastSkillInternal())
            {
                CurrentMana = 0;
            }
            else
            {
                CurrentMana = baseData.mana;
            }
        }
    }
    private bool TryCastSkillInternal()
    {
        var manager = GetComponent<SkillManager>();
        if (manager != null)
        {
            return manager.TryCastSkill();
        }


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
        Debug.Log($"[{gameObject.name}] SetAttackSpeedMultiplier: {attackSpeedMultiplier} -> {multiplier}, attackTimer={attackTimer}");

        attackSpeedMultiplier = Mathf.Max(0.1f, multiplier);

        if (baseAttackInterval > 0f)
        {
            float oldInterval = attackInterval;
            attackInterval = baseAttackInterval / attackSpeedMultiplier;

            // 타이머 비율 유지 (공속 변경 시 현재 진행도 보존)
            if (oldInterval > 0f && attackTimer > 0f)
            {
                float progress = attackTimer / oldInterval;
                attackTimer = attackInterval * progress;
            }
        }

        Debug.Log($"[{gameObject.name}] 결과: attackInterval={attackInterval}, attackTimer={attackTimer}");
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
        // 1. 아이템 보너스 적용
        bonusAttack_Item = attack;
        bonusArmor_Item = armor;
        bonusMaxHP_Item = hp;

        // 2. 아이템이 주는 HP만큼 즉시 회복
        CurrentHP += hp;

        // 3. MaxHP 초과 방지
        if (CurrentHP > MaxHP)
            CurrentHP = MaxHP;
    }


    // 12-22 ko
    // 글로벌 버프 적용
    public void GlobalBuffApply(float multiplier)
    {
        if (baseData.traits.Contains(TraitType.Melee))
        {
            bonusAttack_Buff = (int)(AttackDamage * (multiplier - 1f));
            bonusArmor_Buff = (int)(Armor * (multiplier - 1f));
            bonusMaxHP_Buff = (int)(MaxHP * (multiplier - 1f));

            CurrentHP = MaxHP;
        }
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

    // 12-22 ko
    // 글로벌 버프 초기화
    public void ClearAllBuffs()
    {
        SetAttackSpeedMultiplier(1f);
        bonusAttack_Buff = 0;
        bonusArmor_Buff = 0;
        bonusMaxHP_Buff = 0;

        CurrentHP = MaxHP;
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

    public void SetTargetable(bool value)
    {
        IsTargetable = value;
    }
}