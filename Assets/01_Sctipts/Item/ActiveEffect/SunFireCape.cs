using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunFireCape : ItemBase
{
    private const float BONUS_HP_PERCENT = 0.08f; //최대체력 8%의 추가체력
    private const float BURN_RADIUS = 2.0f; // 불태우기 범위
    private const float BURN_DURATION = 10.0f; //불태우기 지속시간
    private const float BURN_INTERVAL = 2.0f; //불태우기 갱신 시간 
    private const float BURN_DAMAGE_PERCENT = 0.01f; //불태우기 고정피해 데미지

    private Coroutine burnRoutine;

    public SunFireCape(ItemData data) : base(data)
    {
    }

    public override void OnEquip(ChessStateBase chess)
    {
        base.OnEquip(chess);

        chess.AddMaxHpPercent(BONUS_HP_PERCENT);

        burnRoutine = chess.StartCoroutine(BurnAuraRoutine());
    }

    public override void OnUnequip()
    {
        owner.RemoveMaxHpPercent(BONUS_HP_PERCENT);

        if (burnRoutine != null)
        {
            owner.StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        base.OnUnequip();
    }

    private IEnumerator BurnAuraRoutine()
    {
        while (true)
        {
            ApplyBurnToNearbyEnemies();
            yield return new WaitForSeconds(BURN_INTERVAL);
        }
    }

    private void ApplyBurnToNearbyEnemies()
    {
        if (owner == null || owner.IsDead) return;

        var hits = Physics.OverlapSphere(
            owner.transform.position,
            BURN_RADIUS
        );

        foreach (var hit in hits)
        {
            Chess target = hit.GetComponent<Chess>();
            if (target == null) continue;
            if (target.team == (owner as Chess).team) continue;
            if (target.IsDead) continue;


            SunFireCapeDebuff.Apply(
               target,
               BURN_DAMAGE_PERCENT,
               BURN_DURATION
           );
        }
    }
}
