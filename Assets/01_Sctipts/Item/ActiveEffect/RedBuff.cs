using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBuff : ItemBase
{
    private const float BURN_DURATION = 5f;
    private const float BURN_MAXHP_RATIO = 0.01f;

    public RedBuff(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);
        owner.OnBasicAttackHit += HandleBasicAttackHit;
    }
    public override void OnUnequip()
    {
        if (owner != null)
            owner.OnBasicAttackHit -= HandleBasicAttackHit;

        base.OnUnequip();
    }



    private void HandleBasicAttackHit()
    {
        if (owner is not Chess chess)
            return;

        Chess target = chess.LastAttackTarget as Chess;
        if (target == null || target.IsDead)
            return;

        ApplyBurn(target);
    }
    private void ApplyBurn(Chess target)
    {
        var burn = target.GetComponent<RedbuffDebuff>();
        if (burn == null)
            burn = target.gameObject.AddComponent<RedbuffDebuff>();

        burn.Apply(BURN_MAXHP_RATIO, BURN_DURATION);
    }
}
