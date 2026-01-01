using System.Collections;
using UnityEngine;

public class XayahBuff_W : MonoBehaviour, IOnHitEffect
{
    //=====================================================
    //                  Runtime
    //=====================================================
    private Chess owner;
    private float endTime;

    private float buffAtkSpeedMul = 1f;
    private float bonusDmgMul = 0f;
    private int bonusFlat = 0;

    private GameObject buffVfxPrefab;
    private GameObject hitVfxPrefab;
    private GameObject buffVfxInstance;

    //private float prevAtkSpeedMul = 1f;
    private Coroutine routine;

    //누적공속방지용
    private float baseBuffAtkSpeedMul = 1f;
    private bool baseCaptured = false;

    public void Begin(
        Chess owner,
        float duration,
        float atkSpeedMul,
        float bonusDamageMultiplier,
        int flatBonusDamage,
        GameObject buffVfxPrefab,
        GameObject hitVfxPrefab
    )
    {
        this.owner = owner;
        this.buffAtkSpeedMul = atkSpeedMul;
        this.bonusDmgMul = bonusDamageMultiplier;
        this.bonusFlat = flatBonusDamage;
        this.buffVfxPrefab = buffVfxPrefab;
        this.hitVfxPrefab = hitVfxPrefab;

        endTime = Time.time + duration;
        if (this.owner == null) return;

        if (!baseCaptured)
        {
            baseBuffAtkSpeedMul = this.owner.GetBuffAttackSpeedMultiplier();
            baseCaptured = true;
        }

        this.owner.SetBuffAttackSpeedMultiplier(baseBuffAtkSpeedMul * buffAtkSpeedMul);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        if (owner == null) yield break;

        if (buffVfxPrefab != null)
        {
            if (buffVfxInstance != null) Destroy(buffVfxInstance);
            buffVfxInstance = Instantiate(buffVfxPrefab, owner.transform.position, Quaternion.identity, owner.transform);
        }

        while (Time.time < endTime && owner != null && !owner.IsDead)
            yield return null;

        if (owner != null && !owner.IsDead)
            owner.SetBuffAttackSpeedMultiplier(baseBuffAtkSpeedMul);

        baseCaptured = false;

        if (buffVfxInstance != null)
            Destroy(buffVfxInstance);

        routine = null;
    }

    public void OnHit(Chess attacker, Chess target)
    {
        if (owner == null || attacker != owner) return;
        if (target == null || target.IsDead) return;

        //추가피해는 버프중일때만.
        if (Time.time > endTime) return;

        int bonus = Mathf.Max(1, Mathf.RoundToInt(owner.AttackDamage * bonusDmgMul) + bonusFlat);
        target.TakeDamage(bonus, owner);

        if (hitVfxPrefab != null)
            Instantiate(hitVfxPrefab, target.transform.position, Quaternion.identity);
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (buffVfxInstance != null)
        {
            Destroy(buffVfxInstance);
            buffVfxInstance = null;
        }

        if (owner != null && baseCaptured && !owner.IsDead)
        {
            owner.SetBuffAttackSpeedMultiplier(baseBuffAtkSpeedMul);
        }

        baseCaptured = false;
    }
}
