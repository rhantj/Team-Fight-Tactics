using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class SteraksGage : ItemBase
{
    private bool triggered;


    private const float HP_THRESHOLD = 0.6f;   
    private const float SHIELD_RATIO = 0.4f;   
    private const float SHIELD_DURATION = 4f;  

    public SteraksGage(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);

        triggered = false;
        owner.OnHPChanged += HandleHPChanged;
    }

    public override void OnUnequip()
    {
        if (owner != null)
        {
            owner.OnHPChanged -= HandleHPChanged;
        }

        base.OnUnequip();
    }

    private void HandleHPChanged(int currentHP, int maxHP)
    {
        if (triggered) return;

        float ratio = (float)currentHP / maxHP;
        if (ratio > HP_THRESHOLD) return;

        triggered = true;

        int shieldAmount = Mathf.RoundToInt(maxHP * SHIELD_RATIO);
        owner.AddShield(shieldAmount, SHIELD_DURATION);

        //Debug.Log(
        //    $"[Steraks] 발동 | {owner.name} HP {currentHP}/{maxHP} → 보호막 {shieldAmount} ({SHIELD_DURATION}s)"
        //);
    }
}
