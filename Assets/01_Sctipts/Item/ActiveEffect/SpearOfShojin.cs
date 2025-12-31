using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearOfShojin : ItemBase
{
    private const int MANA_ON_HIT = 5;

    public SpearOfShojin(ItemData data) : base(data)
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
        {
            owner.OnBasicAttackHit -= HandleBasicAttackHit;
        }
        base.OnUnequip();
    }

    //기본공격 적중 시
    private void HandleBasicAttackHit()
    {
        //마나 2 충전
        owner.GainMana(MANA_ON_HIT);

        //Debug.Log(
        //    $"[SpearOfShojin] 기본 공격 적중 | {owner.name} 마나 +{MANA_ON_HIT} (현재 {owner.CurrentMana})"
        //);
    }
}
