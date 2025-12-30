using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidStaffDebuff : MonoBehaviour
{
    private ChessStateBase target;

    private int baseArmorSnapshot;   //최초 기준 방어력
    private int reducedArmor;
    private Coroutine routine;

    private void Awake()
    {
        target = GetComponent<ChessStateBase>();
    }

    public void Apply(float ratio, float duration)
    {
        if (target == null || target.IsDead)
            return;

        // 최초 1회만 기준 방어력 저장
        if (reducedArmor == 0)
        {
            baseArmorSnapshot = target.Armor;
            reducedArmor = Mathf.RoundToInt(baseArmorSnapshot * ratio);

            target.AddBonusStats(
                0,
                -reducedArmor,
                0
            );
        }

        // 지속시간만 갱신
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ExpireAfter(duration));
    }

    private IEnumerator ExpireAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        Remove();
    }

    private void Remove()
    {
        if (target != null && reducedArmor != 0)
        {
            target.AddBonusStats(
                0,
                reducedArmor,
                0
            );
        }

        reducedArmor = 0;
        routine = null;
        Destroy(this);
    }

    private void OnDestroy()
    {
        if (target != null && reducedArmor != 0)
        {
            target.AddBonusStats(
                0,
                reducedArmor,
                0
            );
        }
    }
}
