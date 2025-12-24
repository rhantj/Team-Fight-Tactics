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
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
    }
}
