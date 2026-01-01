using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunFireCapeDebuff : MonoBehaviour
{
    private float damagePercent;
    private float duration;

    private Coroutine tickRoutine;
    private Coroutine durationRoutine;

    // =========================
    // 정적 Apply 진입점
    // =========================
    public static void Apply(
        Chess target,
        float maxHpPercent,
        float duration
    )
    {
        if (target == null || target.IsDead) return;

        var debuff = target.GetComponent<SunFireCapeDebuff>();
        if (debuff == null)
            debuff = target.gameObject.AddComponent<SunFireCapeDebuff>();

        debuff.Refresh(maxHpPercent, duration);
    }

    // =========================
    // 디버프 갱신
    // =========================
    private void Refresh(float percent, float duration)
    {
        this.damagePercent = percent;
        this.duration = duration;

        if (tickRoutine == null)
            tickRoutine = StartCoroutine(TickRoutine());

        if (durationRoutine != null)
            StopCoroutine(durationRoutine);

        durationRoutine = StartCoroutine(DurationRoutine());
    }

    // =========================
    // 매초 피해
    // =========================
    private IEnumerator TickRoutine()
    {
        while (true)
        {
            DealDamage();
            yield return new WaitForSeconds(1f);
        }
    }

    private void DealDamage()
    {
        var chess = GetComponent<ChessStateBase>();
        if (chess == null || chess.IsDead) return;

        int damage = Mathf.RoundToInt(chess.MaxHP * damagePercent);
        damage = Mathf.Max(1, damage);

        // 방어력 무시 고정 피해
        chess.TakeTrueDamage(damage);

    }

    // =========================
    // 지속시간 종료
    // =========================
    private IEnumerator DurationRoutine()
    {
        yield return new WaitForSeconds(duration);
        Destroy(this);
    }

    private void OnDestroy()
    {
        if (tickRoutine != null)
            StopCoroutine(tickRoutine);

        if (durationRoutine != null)
            StopCoroutine(durationRoutine);
    }
}
