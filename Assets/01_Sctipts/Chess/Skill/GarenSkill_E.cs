using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarenSkill_E : SkillBase
{
    [Header("Spin Settings")]
    [Tooltip("지속시간")]
    [SerializeField] private float duration = 2.0f;

    [Tooltip("틱 간격")]
    [SerializeField] private float tickInterval = 0.25f;

    [Tooltip("피해 반경")]
    [SerializeField] private float radius = 2.0f;

    [Tooltip("피해량 [기본: 공격력 * 배율)]")]
    [SerializeField] private float damageMultiplier = 0.6f;

    [Header("VFX (Optional)")]
    [Tooltip("Spin Effect")]
    [SerializeField] private GameObject spinVfxPrefab;

    [Tooltip("Hit Effect")]
    [SerializeField] private GameObject hitVfxPrefab;

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess garen = caster as Chess;
        if (garen == null) yield break;
        //누굴 공격할지.
        List<Chess> enemyList = (garen.team == Team.Player)
            ? UnitCountManager.Instance.enemyUnits
            : UnitCountManager.Instance.playerUnits;

        //VFX
        GameObject spinVfx = null;
        if (spinVfxPrefab != null)
        {
            spinVfx = Object.Instantiate(spinVfxPrefab, garen.transform.position, Quaternion.identity, garen.transform);
        }

        float elapsed = 0f;
        float tickTimer = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickInterval)
            {
                tickTimer -= tickInterval;

                int dmg = Mathf.Max(1, Mathf.RoundToInt(garen.AttackDamage * damageMultiplier)); //피해량
                for (int i = 0; i < enemyList.Count; i++)//범위 적용
                {
                    Chess target = enemyList[i];
                    if (target == null || target.IsDead) continue;

                    float dist = Vector3.Distance(garen.transform.position, target.transform.position);
                    if (dist > radius) continue;

                    target.TakeDamage(dmg, garen);

                    if (hitVfxPrefab != null)
                        Object.Instantiate(hitVfxPrefab, target.transform.position, Quaternion.identity);
                }
            }

            yield return null;
        }

        if (spinVfx != null)
            Object.Destroy(spinVfx);
    }
}
