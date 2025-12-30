using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedbuffDebuff : MonoBehaviour
{
    private ChessStateBase target;

    private int burnDamagePerTick;
    private Coroutine routine;

    private void Awake()
    {
        target = GetComponent<ChessStateBase>();
    }

    public void Apply(float maxHpRatio, float duration)
    {
        if (target == null || target.IsDead)
            return;

        // 최초 적용 시에만 계산
        if (burnDamagePerTick == 0)
        {
            int maxHp = target.MaxHP;
            burnDamagePerTick = Mathf.Max(1, Mathf.RoundToInt(maxHp * maxHpRatio));
        }

        // 지속시간 갱신
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(BurnRoutine(duration));
    }

    private IEnumerator BurnRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;

            if (target == null || target.IsDead)
                break;

            ApplyTrueDamage(burnDamagePerTick);
        }

        Cleanup();
    }

    private void ApplyTrueDamage(int damage)
    {
        target.TakeTrueDamage(damage);
    }

    private void Cleanup()
    {
        routine = null;
        Destroy(this);
    }

    private void OnDestroy()
    {
        if (routine != null)
            StopCoroutine(routine);
    }
}
