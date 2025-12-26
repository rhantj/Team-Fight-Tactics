using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class BlueBuff : ItemBase
{
    private const float ATTACK_PERCENT = 0.10f;

    public BlueBuff(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);
        chess.AddAttackPercent(ATTACK_PERCENT);
    }

    public override void OnUnequip()
    {
        if (owner != null)
        {
            owner.RemoveAttackPercent(ATTACK_PERCENT);
        }
        base.OnUnequip();
    }
}
