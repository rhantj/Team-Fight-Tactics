using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveItem : ItemBase
{
    public PassiveItem(ItemData data) : base(data)
    {

    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);

        ApplyStat(chess);
    }

    public override void OnUnequip()
    {
        if(owner != null)
        {
            RemoveStat(owner);
        }
        base.OnUnequip();
    }

    private void ApplyStat(ChessStateBase chess)
    {
        chess.AddBonusStats(
            Data.addAttack,
            Data.addDefense,
            Data.addHp);
    }

    private void RemoveStat(ChessStateBase chess)
    {
        chess.AddBonusStats(
            -Data.addAttack,
            -Data.addDefense,
            -Data.addHp
        );
    }
}
