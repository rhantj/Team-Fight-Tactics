using System.Collections;
using UnityEngine;

public class XayahSkill_W : SkillBase
{
    //=====================================================
    //                  Timing
    //=====================================================
    [Header("Cast")]
    [SerializeField, Tooltip("모션 시작후 적용까지.")]
    private float windUpTime = 0.1f;

    //=====================================================
    //                  Buff
    //=====================================================
    [Header("Buff")]
    [SerializeField, Tooltip("버프 지속시간(초)")]
    private float duration = 4.0f;

    [SerializeField, Tooltip("공속 배율 ex: 1.3 = 30% 증가")]
    private float attackSpeedMultiplier = 1.3f;

    [SerializeField, Tooltip("추가 피해 = 공격력 * 배율")]
    private float bonusDamageMultiplier = 0.4f;

    [SerializeField, Tooltip("추가 고정 피해")]
    private int flatBonusDamage = 10;

    //=====================================================
    //                  VFX (Optional)
    //=====================================================
    [Header("VFX")]
    [SerializeField, Tooltip("시전시 VFX")]
    private GameObject castVfxPrefab;

    [SerializeField, Tooltip("버프 VFX")]
    private GameObject buffVfxPrefab;

    [SerializeField, Tooltip("히트 VFX")]
    private GameObject hitVfxPrefab;

    Vector3 offset = Vector3.up * 3f;

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess xayah = caster as Chess;
        if (xayah == null) yield break;

        if (castVfxPrefab != null)
            Object.Instantiate(castVfxPrefab, xayah.transform.position + offset, Quaternion.identity);

        // 자야 스킬 효과음 추가
        SettingsUI.PlaySFX("Xayah_W", Vector3.zero, 1f, 1f);

        if (windUpTime > 0f)
            yield return new WaitForSeconds(windUpTime);

        var buff = xayah.GetComponent<XayahBuff_W>();
        if (buff == null) buff = xayah.gameObject.AddComponent<XayahBuff_W>();

        buff.Begin(
            xayah,
            duration,
            attackSpeedMultiplier,
            bonusDamageMultiplier,
            flatBonusDamage,
            buffVfxPrefab,
            hitVfxPrefab
        );

        yield return null;
    }
}
