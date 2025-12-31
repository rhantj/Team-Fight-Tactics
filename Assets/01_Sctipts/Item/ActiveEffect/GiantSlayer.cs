using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantSlayer : ItemBase
{
    private ChessStateBase owner;

    public GiantSlayer(ItemData data) : base(data) { }

    public override void OnEquip(ChessStateBase target)
    {
        owner = target;
        owner.OnBasicAttackHit += HandleBasicAttackHit;
    }

    public override void OnUnequip()
    {
        if (owner != null)
            owner.OnBasicAttackHit -= HandleBasicAttackHit;
    }

    private void HandleBasicAttackHit()
    {
        
        var chess = owner as Chess;
        if (chess == null) return;

        ChessStateBase target = chess.LastAttackTarget;
        if (target == null) return;

        // 난동꾼 시너지 체크
        if (!target.HasTrait(TraitType.Melee))
            return;

        int bonusDamage = Mathf.RoundToInt(owner.AttackDamage * 0.15f);

        // 추가 피해는 별도 처리 예정
        target.TakeDamage(bonusDamage, chess);

        //Debug.Log($"[GiantSlayer] 난동꾼 대상 추가 피해 {bonusDamage}");
    }
}
