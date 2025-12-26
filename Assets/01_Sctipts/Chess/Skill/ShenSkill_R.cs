using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShenSkill_R : SkillBase
{
    //=====================================================
    //                  Cast / Timing
    //=====================================================
    [Header("Cast")]
    [SerializeField, Tooltip("모션후 적용까지 시간")]
    private float windUpTime = 0.15f;

    //=====================================================
    //                  Target Rule
    //=====================================================
    [Header("Target")]
    [SerializeField, Tooltip("0이면 전체아군중에서 선택합니다")]
    private float searchRadius = 0f;

    [SerializeField, Tooltip("이건 자신도 포함되는지 여부")]
    private bool includeSelf = false;

    //=====================================================
    //                  Barrier (Shield)
    //=====================================================
    [Header("Barrier")]
    [SerializeField, Tooltip("실드 수치 = 대상 최대체력 * 배율")]
    private float shieldHpMultiplier = 0.35f;

    [SerializeField, Tooltip("실드 추가 고정 수치")]
    private int shieldFlat = 80;

    [SerializeField, Tooltip("실드 지속시간")]
    private float shieldDuration = 4.0f;

    //=====================================================
    //                  VFX (Optional)
    //=====================================================
    [Header("VFX")]
    [SerializeField, Tooltip("시전 VFX")]
    private GameObject castVfxPrefab;

    [SerializeField, Tooltip("대상에게 붙는 실드 VFX")]
    private GameObject shieldVfxPrefab;

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess shen = caster as Chess;
        if (shen == null) yield break;

        List<Chess> allies = (shen.team == Team.Player)
            ? UnitCountManager.Instance.playerUnits
            : UnitCountManager.Instance.enemyUnits;

        Chess target = FindLowestHpRatioAlly(shen, allies, searchRadius, includeSelf);
        if (target == null) yield break;

        var pos = transform.position + Vector3.up * 3f;
        if (castVfxPrefab != null)
        {
            var castVfx = PoolManager.Instance.Spawn("ShenCast");
            castVfx.transform.SetPositionAndRotation(pos, Quaternion.identity);
        }
        
        //쉔 R스킬 효과음 추가
        SettingsUI.PlaySFX("Shen_R_Use",shen.transform.position,1f,1f);

        if (windUpTime > 0f)
            yield return new WaitForSeconds(windUpTime);

        int shield = Mathf.Max(1, Mathf.RoundToInt(target.MaxHP * shieldHpMultiplier) + shieldFlat);
        target.AddShield(shield, shieldDuration);

        if (shieldVfxPrefab != null)
        {
            var shieldVfx = PoolManager.Instance.Spawn("ShenShield");
            shieldVfx.transform.SetPositionAndRotation(target.transform.position + Vector3.up * 3f, Quaternion.identity);

            float elapsed = shieldDuration;
            while (elapsed > 0f)
            {
                elapsed -= Time.deltaTime;
                shieldVfx.transform.position = pos;
                yield return null;
            }

            var pooled = shieldVfx.GetComponent<PooledObject>();
            pooled.ReturnToPool();
        }
    }

    //=====================================================
    //                  Target Helper
    //=====================================================
    private Chess FindLowestHpRatioAlly(Chess self, List<Chess> allies, float radius, bool includeSelf)
    {
        Chess best = null;
        float bestRatio = 999f;

        Vector3 selfPos = self.transform.position;

        for (int i = 0; i < allies.Count; i++)
        {
            Chess a = allies[i];
            if (a == null || a.IsDead) continue;
            if (!includeSelf && a == self) continue;

            if (radius > 0f)
            {
                float dist = Vector3.Distance(selfPos, a.transform.position);
                if (dist > radius) continue;
            }

            float ratio = (a.MaxHP <= 0) ? 1f : (float)a.CurrentHP / a.MaxHP;
            if (ratio < bestRatio)
            {
                bestRatio = ratio;
                best = a;
            }
        }

        return best;
    }
}
