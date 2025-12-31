using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianAngel : ItemBase
{
    private bool hpTriggered;

    // 수치
    private const float HP_THRESHOLD = 0.6f; 
    private const float UNTARGETABLE_DURATION = 4.0f;

    public GuardianAngel(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);

        hpTriggered = false;
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

    // =========================
    // HP 25% 이하 최초 도달 시
    // =========================
    private void HandleHPChanged(int currentHP, int maxHP)
    {
        if (hpTriggered) return;

        float ratio = (float)currentHP / maxHP;
        if (ratio > HP_THRESHOLD) return;

        hpTriggered = true;

        //Debug.Log($"[NightEdge] 지정 불가 발동 | {owner.name}");

        owner.StartCoroutine(UntargetableRoutine());
    }

    private IEnumerator UntargetableRoutine()
    {
        // 지정 불가 시작
        owner.SetTargetable(false);

        yield return new WaitForSeconds(UNTARGETABLE_DURATION);

        // 지정 가능 복구
        owner.SetTargetable(true);

        //Debug.Log($"[NightEdge] 지정 불가 종료 | {owner.name}");
    }
}
