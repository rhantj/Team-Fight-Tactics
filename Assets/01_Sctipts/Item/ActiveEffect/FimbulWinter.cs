using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//======================== 수호자의 맹세 =======================
// 1. 전투 시작시 마나 +20
// 2. 체력 40% 이하 최초 도달 시 
//      ㄴ 마나 +15
//      ㄴ 최대체력 20% 보호막 획득
//==============================================================
public class FimbulWinter : ItemBase
{
    private bool hpTriggered;

    public FimbulWinter(ItemData data) : base(data)
    { 
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);

        hpTriggered = false;

        owner.OnBattleStart += HandleBattleStart;
        owner.OnHPChanged += HandleHPChanged;
    }

    public override void OnUnequip()
    {
        if(owner != null)
        {
            owner.OnBattleStart -= HandleBattleStart;
            owner.OnHPChanged -= HandleHPChanged;
        }
        base.OnUnequip();
    }
    // =========================
    // 효과 1 : 전투 시작 시 마나 +20
    // =========================
    private void HandleBattleStart()
    {
        owner.GainMana(5);
    }
    // =========================
    // 효과 2 : HP 40% 이하 최초 도달 시
    // =========================
    private void HandleHPChanged(int currentHP, int maxHP)
    {
        if (hpTriggered) return;

        float ratio = (float)currentHP /maxHP;
        if (ratio > 0.4f) return;

        hpTriggered = true;

        // 마나 +15
        owner.GainMana(15);

        // 보호막 = 최대 체력의 20%
        int shieldAmount = Mathf.RoundToInt(maxHP * 0.2f);
        owner.AddShield(shieldAmount, 0f);
    }
}
