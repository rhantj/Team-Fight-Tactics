using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warmog : ItemBase
{
    private const float HP_PERCENT = 0.15f;

    public Warmog(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);
        chess.SetDynamicMaxHpPercent(HP_PERCENT);
    }

    public override void OnUnequip()
    {
        owner.SetDynamicMaxHpPercent(0f);
        base.OnUnequip();
    }
}
