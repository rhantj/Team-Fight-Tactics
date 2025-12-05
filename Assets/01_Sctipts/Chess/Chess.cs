using System;
using UnityEngine;

/*
- 전투 시스템
- 사망 시 비활성화 
    - 기물 조합 시 반환
- 마나에 따른 스킬 실행
- 조합 시 성급 상승
*/
public class Chess : ChessStateBase
{
    //=====================================================
    //                  타겟 / 이벤트
    //=====================================================
    private Chess currentTarget;

    public event Action<Chess> OnDead;
    public event Action<Chess> OnUsedAsMaterial;

    //=====================================================
    //                  업데이트 루프
    //=====================================================
    private void Update()
    {
        if (IsDead) return;

        if (currentTarget != null && !currentTarget.IsDead)
        {
            stateMachine?.SetBattle();

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                AttackOnce();
            }
        }
        else
        {
            stateMachine?.SetIdle();
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
            int index = UnityEngine.Random.Range(0, 2);
            animator.SetInteger("AttackIndex", index);
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
    public void CombineWith(Chess material)
    {
        if (material == null) return;
        if (material.baseData != baseData) return;

        StarLevel++;

        float hpMultiplier = 1.5f;
        CurrentHP = Mathf.RoundToInt(baseData.maxHP * Mathf.Pow(hpMultiplier, StarLevel - 1));
        CurrentMana = 0;

        OnUsedAsMaterial?.Invoke(material);
        material.gameObject.SetActive(false);

        Debug.Log($"조합됨");
    }

    public void SetPosition(Vector3 position)
    {
        position.y = 1.5f;
        transform.position = position;
    }
}
