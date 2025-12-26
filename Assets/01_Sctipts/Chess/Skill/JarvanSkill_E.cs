using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JarvanSkill_E : SkillBase
{
    [Header("Cast")]
    [SerializeField, Tooltip("모션시작에서 타격적용까지 시간.")]
    private float windUpTime = 0.1f;

    [SerializeField, Tooltip("설치 사거리")]
    private float castRange = 4.5f;

    [Header("Damage")]
    [SerializeField, Tooltip("범위 반경")]
    private float radius = 2.2f;

    [SerializeField, Tooltip("피해 배율")]
    private float damageMultiplier = 1.0f;

    [SerializeField, Tooltip("추가 고정 피해")]
    private int flatBonusDamage = 0;

    [Header("VFX")]
    [SerializeField, Tooltip("깃발 프리팹")]
    private GameObject flagPrefab;

    [SerializeField, Tooltip("히트 이펙트")]
    private GameObject hitVfxPrefab;

    private void Awake()
    {
        endByAnimEvent = false; 
    }

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess jarvan = caster as Chess;
        if (jarvan == null) yield break;

        Vector3 pos = jarvan.transform.position + jarvan.transform.forward * castRange;
        pos.y = 1.5f;

        if (jarvan.CurrentTarget != null && !jarvan.CurrentTarget.IsDead)
        {
            pos = jarvan.CurrentTarget.transform.position;
            pos.y = 1.5f;
        }

        if (flagPrefab != null)
            Object.Instantiate(flagPrefab, pos, Quaternion.identity);

        if (windUpTime > 0f)
            yield return new WaitForSeconds(windUpTime);

        SettingsUI.PlaySFX("Jarvan_E",Vector3.zero, 1f,1f);

        List<Chess> enemies = (jarvan.team == Team.Player)
            ? UnitCountManager.Instance.enemyUnits
            : UnitCountManager.Instance.playerUnits;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(jarvan.AttackDamage * damageMultiplier) + flatBonusDamage);

        for (int i = 0; i < enemies.Count; i++)
        {
            Chess t = enemies[i];
            if (t == null || t.IsDead) continue;

            float dist = Vector3.Distance(pos, t.transform.position);
            if (dist > radius) continue;

            t.TakeDamage(dmg, jarvan);

            if (hitVfxPrefab != null)
                Object.Instantiate(hitVfxPrefab, t.transform.position, Quaternion.identity);
        }
    }
}
