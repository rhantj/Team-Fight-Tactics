using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViSkill_E : SkillBase
{
    [Header("Timing")]
    [SerializeField, Tooltip("모션시작에서 타격적용까지 시간.")]
    private float windUpTime = 0.15f;

    [Header("Damage")]
    [SerializeField, Tooltip("주대상 피해 배율")]
    private float mainDamageMultiplier = 1.2f;

    [SerializeField, Tooltip("주대상 피해량")]
    private int mainFlatBonusDamage = 20;

    [SerializeField, Tooltip("스플래시 대상 피해")]
    private float splashDamageMultiplier = 0.8f;

    [SerializeField, Tooltip("스플래시 길이")]
    private float coneRange = 3.0f;

    [SerializeField, Tooltip("스플래시 각도..인데 90으로하면 좌우 45도로 되요")]
    private float coneAngle = 90f;

    [Header("VFX")]
    [SerializeField, Tooltip("시전 이펙트")]
    private GameObject castVfxPrefab;

    [SerializeField, Tooltip("Hit Effect)")]
    private GameObject hitVfxPrefab;

    Vector3 offset = Vector3.up * 3f;

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess vi = caster as Chess;
        if (vi == null) yield break;

        Chess mainTarget = vi.CurrentTarget;
        if (mainTarget == null || mainTarget.IsDead) yield break;

        //적 리스트 가져오기
        List<Chess> enemies = (vi.team == Team.Player)
            ? UnitCountManager.Instance.enemyUnits
            : UnitCountManager.Instance.playerUnits;

        //캐스팅 VFX
        if (castVfxPrefab != null)
        {
            var castVfx = PoolManager.Instance.Spawn("ViCast");
            castVfx.transform.SetPositionAndRotation(vi.transform.position + offset, Quaternion.identity);
        }

        //모션 타이밍
        if (windUpTime > 0f)
            yield return new WaitForSeconds(windUpTime);

        //바이 E스킬 사운드 추가
        SettingsUI.PlaySFX("Vi_E_Hit",vi.transform.position,1f,1f);

        //방향
        Vector3 dir = (mainTarget.transform.position - vi.transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) yield break;
        dir.Normalize();

        //Main Target
        int mainDmg = Mathf.Max(1, Mathf.RoundToInt(vi.AttackDamage * mainDamageMultiplier) + mainFlatBonusDamage);
        mainTarget.TakeDamage(mainDmg, vi);

        if (hitVfxPrefab != null)
        {
            var hitVfx = PoolManager.Instance.Spawn("ViHit");
            hitVfx.transform.SetPositionAndRotation(mainTarget.transform.position + offset, Quaternion.identity);
        }

        //Cone Splash 
        float halfAngle = coneAngle * 0.5f;

        for (int i = 0; i < enemies.Count; i++)
        {
            Chess t = enemies[i];
            if (t == null || t.IsDead) continue;
            if (t == mainTarget) continue;

            Vector3 to = t.transform.position - vi.transform.position;
            to.y = 0f;

            float dist = to.magnitude;
            if (dist > coneRange) continue;

            float angle = Vector3.Angle(dir, to.normalized);
            if (angle > halfAngle) continue;

            int splashDmg = Mathf.Max(1, Mathf.RoundToInt(vi.AttackDamage * splashDamageMultiplier));
            t.TakeDamage(splashDmg, vi);
        }
    }
}
