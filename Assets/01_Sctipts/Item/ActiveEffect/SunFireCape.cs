using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunFireCape : ItemBase
{
    private const float BONUS_HP_PERCENT = 0.08f; //최대체력 8%의 추가체력
    private const float BURN_RADIUS = 10.0f; // 불태우기 범위
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

        owner = chess;

        owner.AddMaxHpPercent(BONUS_HP_PERCENT);

        chess.OnBattleStart += OnBattleStart;
    }

    public override void OnUnequip()
    {
        if (owner != null)
        {
            owner.RemoveMaxHpPercent(BONUS_HP_PERCENT);

            owner.OnBattleStart -= OnBattleStart;

            if (burnRoutine != null)
            {
                owner.StopCoroutine(burnRoutine);
                burnRoutine = null;
            }
        }

        base.OnUnequip(); ;
    }

    private void OnBattleStart()
    {
        if (burnRoutine != null)
            owner.StopCoroutine(burnRoutine);

        burnRoutine = owner.StartCoroutine(BurnAuraRoutine());
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
            Chess target = hit.GetComponentInParent<Chess>();
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
