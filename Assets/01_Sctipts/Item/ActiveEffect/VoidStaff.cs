using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidStaff : ItemBase
{
    private const float ARMOR_REDUCE_RATIO = 0.3f;
    private const float DEBUFF_DURATION = 5f;

    public VoidStaff(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);

        owner.OnBasicAttackHit += HandleBasicAttackHit;
    }

    public override void OnUnequip()
    {
        if(owner!= null)
        {
            owner.OnBasicAttackHit -= HandleBasicAttackHit;
        }
        base.OnUnequip();
    }

    private void HandleBasicAttackHit()
    {
        if (owner is Chess chess)
        {
            Chess target = chess.LastAttackTarget as Chess;
            if (target == null || target.IsDead) return;

            ApplyArmorShred(target);
        }
    }

    private void ApplyArmorShred(Chess target)
    {
        var debuff = target.GetComponent<VoidStaffDebuff>();
        if (debuff == null)
        {
            debuff = target.gameObject.AddComponent<VoidStaffDebuff>();
        }

        debuff.Apply(ARMOR_REDUCE_RATIO, DEBUFF_DURATION);
    }
}
