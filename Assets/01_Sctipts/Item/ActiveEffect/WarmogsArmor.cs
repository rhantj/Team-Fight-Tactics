using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarmogsArmor : ItemBase
{
    private const float BONUS_HP_PERCENT = 0.15f;

    public WarmogsArmor(ItemData data) : base(data) { }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);
        chess.AddMaxHpPercent(BONUS_HP_PERCENT);
    }

    public override void OnUnequip()
    {
        owner.RemoveMaxHpPercent(BONUS_HP_PERCENT);
        base.OnUnequip();
    }
}