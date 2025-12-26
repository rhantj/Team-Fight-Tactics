using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalioSkill_W : SkillBase
{
    [Header("Cast")]
    [SerializeField, Tooltip("모션유지 시간")]
    private float channelTime = 1.0f;

    //=====================================================
    //                  Damage
    //=====================================================
    [Header("Damage")]
    [SerializeField, Tooltip("범위 반경")]
    private float radius = 2.5f;

    [SerializeField, Tooltip("피해 배율)")]
    private float damageMultiplier = 1.0f;

    [SerializeField, Tooltip("추가 피해")]
    private int flatBonusDamage = 10;

    //=====================================================
    //                  Barrier (Shield)
    //=====================================================
    [Header("Barrier")]
    [SerializeField, Tooltip("실드 배율")]
    private float shieldHpMultiplier = 0.25f;

    [SerializeField, Tooltip("실드량")]
    private int shieldFlat = 50;

    [SerializeField, Tooltip("실드 지속시간")]
    private float shieldDuration = 4.0f;

    //=====================================================
    //                  VFX (꼭 다 할당안해도 됩니다.)
    //=====================================================
    [Header("VFX")]
    [SerializeField, Tooltip("채널링 VFX")]
    private GameObject channelVfxPrefab;

    [SerializeField, Tooltip("히트 vfx")]
    private GameObject blastVfxPrefab;

    [SerializeField, Tooltip("실드 VFX")]
    private GameObject shieldVfxPrefab;

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess galio = caster as Chess;
        if (galio == null) yield break;

        Vector3 pos = galio.transform.position;
        pos.y = 3f;

        GameObject channelVfx = null;
        if (channelVfxPrefab != null)
        {
            channelVfx = PoolManager.Instance.Spawn("GalioChannel");
            channelVfx.transform.SetPositionAndRotation(pos, Quaternion.identity);
        }
        // 갈리오 스킬 효과음 추가
        SettingsUI.PlaySFX("Galio W",galio.transform.position,1f,1f);

        if (channelTime > 0f)
            yield return new WaitForSeconds(channelTime);

        if (channelVfx != null)
        {
            var pooled = channelVfx.GetComponent<PooledObject>();
            pooled.ReturnToPool();
        }

        // VFX
        if (blastVfxPrefab != null)
        {
            var blastVfx = PoolManager.Instance.Spawn("GalioBlast");
            blastVfx.transform.SetPositionAndRotation(pos, Quaternion.identity);
        }

        //범위피해
        List<Chess> enemies = (galio.team == Team.Player)
            ? UnitCountManager.Instance.enemyUnits
            : UnitCountManager.Instance.playerUnits;

        int dmg = Mathf.Max(1, Mathf.RoundToInt(galio.AttackDamage * damageMultiplier) + flatBonusDamage);

        for (int i = 0; i < enemies.Count; i++)
        {
            Chess t = enemies[i];
            if (t == null || t.IsDead) continue;

            float dist = Vector3.Distance(pos, t.transform.position);
            if (dist > radius) continue;

            t.TakeDamage(dmg, galio);
        }

        int shield = Mathf.Max(1, Mathf.RoundToInt(galio.MaxHP * shieldHpMultiplier) + shieldFlat);
        galio.AddShield(shield, shieldDuration);

        //VFX
        if (shieldVfxPrefab != null)
        {
            var shieldVfx = PoolManager.Instance.Spawn("GalioShield");
            shieldVfx.transform.SetPositionAndRotation(pos, Quaternion.identity);

            float elapsed = shieldDuration;
            while (elapsed > 0f)
            {
                elapsed -= Time.deltaTime;
                shieldVfx.transform.position = transform.position + Vector3.up * 1.5f;
                yield return null;
            }
            
            var pooled = shieldVfx.GetComponent<PooledObject>();
            pooled.ReturnToPool();
        }
    }
}
