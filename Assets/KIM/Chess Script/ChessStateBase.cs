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
    [Tooltip("공격 인터벌")]
    [SerializeField] protected float attackInterval = 1.0f;

    [Tooltip("공격 시 획득 마나량")]
    [SerializeField] protected int manaOnHit = 10;

    [Tooltip("피격 시 얻는 마나량")]
    [SerializeField] protected int manaOnDamaged = 5;

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
}
