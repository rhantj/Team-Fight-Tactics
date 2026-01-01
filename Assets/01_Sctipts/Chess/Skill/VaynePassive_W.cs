using UnityEngine;

public class VaynePassive_W : MonoBehaviour, IOnHitEffect
{
    //=====================================================
    //                  Settings
    //=====================================================
    [Header("Vayne W (Passive)")]
    [SerializeField, Tooltip("몇 타마다 발동할지")]
    private int procEveryHits = 3;

    [SerializeField, Tooltip("추가 피해 = 공격력 * 배율")]
    private float bonusDamageMultiplier = 1.0f;

    [SerializeField, Tooltip("추가 고정 피해")]
    private int flatBonusDamage = 20;

    [Header("VFX ")]
    [SerializeField, Tooltip("3타 발동 시  VFX")]
    private GameObject procVfxPrefab;
    Vector3 offset = Vector3.up * 3f;

    //=====================================================
    //                  Runtime
    //=====================================================
    private Chess lastTarget;
    private int hitCount;

    public void OnHit(Chess attacker, Chess target)
    {
        if (attacker == null || target == null || target.IsDead) return;

        //타겟이 중도에 바뀌면 스탯리셋
        if (target != lastTarget)
        {
            lastTarget = target;
            hitCount = 0;
        }

        hitCount++;

        if (hitCount >= procEveryHits)
        {
            hitCount = 0;

            int bonus = Mathf.Max(1, Mathf.RoundToInt(attacker.AttackDamage * bonusDamageMultiplier) + flatBonusDamage);
            target.TakeDamage(bonus, attacker); //일단 단순하게 추가피해..

            if (procVfxPrefab != null)
            {
                var hitVfx = PoolManager.Instance.Spawn("VayneHit");
                hitVfx.transform.SetPositionAndRotation(target.transform.position + offset, Quaternion.identity);
            }
        }
    }
}
