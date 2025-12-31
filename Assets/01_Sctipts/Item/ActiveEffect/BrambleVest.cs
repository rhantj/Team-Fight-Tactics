using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrambleVest : ItemBase
{
    private const float BONUS_HP_PERCENT = 0.09f; //추가체력
    private const float DAMAGE_REDUCTION = 0.05f; // 데미지 감소
    private const int THORN_DAMAGE = 10; //주변 피해 데미지
    private const float THORN_RADIUS = 1.5f; //주변 피해 범위

    private Chess ownerChess;

    private int bonusHp;

    public BrambleVest(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);

        // 퍼센트 체력 증가 (9%)
        chess.AddMaxHpPercent(BONUS_HP_PERCENT);

        // 기존 효과들 (예: 피해 감소, 반사 데미지)은 그대로
        chess.AddDamageReduction(0.05f);
        chess.OnDamagedByBasicAttack += HandleBasicAttackDamaged;
    }

    public override void OnUnequip()
    {
        owner.RemoveMaxHpPercent(BONUS_HP_PERCENT);

        owner.AddDamageReduction(-0.05f);
        owner.OnDamagedByBasicAttack -= HandleBasicAttackDamaged;

        base.OnUnequip();
    }

    private void HandleBasicAttackDamaged(Chess attacker)
    {
        DealThornDamage(attacker);
    }

    private void DealThornDamage(Chess attacker)
    {
        if (ownerChess == null) return;
        if (attacker == null) return;

        var hits = Physics.OverlapSphere(ownerChess.transform.position, THORN_RADIUS);

        for (int i = 0; i < hits.Length; i++)
        {
            Chess enemy = hits[i].GetComponentInParent<Chess>();
            if (enemy == null) continue;
            if (enemy == ownerChess) continue;
            if (enemy.IsDead) continue;
            if (enemy.team == ownerChess.team) continue;

            enemy.TakeDamage(THORN_DAMAGE, ownerChess);
        }

        //Debug.Log($"[BrambleVest] 가시 피해 발동 | {ownerChess.name}");
    }
    private void ApplyBonusHpAsBuff(int deltaHp)
    {
        // bonusMaxHP_Buff에 누적할 수 있는 전용 API가 없어서
        // 현재 구조에서는 GlobalBuffApply/ ClearAllBuffs 흐름을 피하면서
        // “bonusMaxHP_Buff”만 조작할 수 있는 메서드를 ChessStateBase에 추가하는 게 가장 깔끔하다.
        // 아래는 그 메서드가 있다고 가정한 호출 형태:

        owner.AddMaxHpBuff(deltaHp);
    }
}
