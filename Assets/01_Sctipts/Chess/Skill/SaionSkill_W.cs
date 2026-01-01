using System.Collections;
using UnityEngine;

public class SionSkill_W : SkillBase
{
    //=====================================================
    //                  Cast / Timing
    //=====================================================
    [Header("Cast")]
    [SerializeField, Tooltip("모션 시작 ~ 실드 적용까지 시간(없으면 0)")]
    private float windUpTime = 0.05f;

    //=====================================================
    //                  Barrier (Shield)
    //=====================================================
    [Header("Barrier")]
    [SerializeField, Tooltip("실드 배율")]
    private float shieldHpMultiplier = 0.35f;

    [SerializeField, Tooltip("실드 추가 수치")]
    private int shieldFlat = 80;

    [SerializeField, Tooltip("실드 지속시간")]
    private float shieldDuration = 4.0f;

    //=====================================================
    //                  VFX 
    //=====================================================
    [Header("VFX")]
    [SerializeField, Tooltip("실드 VFX")]
    private GameObject shieldVfxPrefab;

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess sion = caster as Chess;
        if (sion == null) yield break;

        if (windUpTime > 0f)
            yield return new WaitForSeconds(windUpTime);

        //사이온 W스킬 효과음
        SettingsUI.PlaySFX("Sion_WSkillSound",sion.transform.position,1f,1f);

        int shield = Mathf.Max(1, Mathf.RoundToInt(sion.MaxHP * shieldHpMultiplier) + shieldFlat);
        sion.AddShield(shield, shieldDuration);

        if (shieldVfxPrefab != null)
        {
            var pos = transform.position + Vector3.up * 3f;
            var shieldVfx = PoolManager.Instance.Spawn("SionShield");
            shieldVfx.transform.SetPositionAndRotation(pos, Quaternion.identity);

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
}
