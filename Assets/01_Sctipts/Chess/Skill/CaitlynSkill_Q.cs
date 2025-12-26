using UnityEngine;
using System.Collections;

public class CaitlynSkill_Q : SkillBase
{
    [Header("Cast Timing")]
    [SerializeField, Tooltip("모션 시작 ~ 타격 적용까지 시간")]
    private float windUpTime = 0.2f;

    [Header("Damage")]
    [SerializeField, Tooltip("피해 배율 (AttackDamage * 배율)")]
    private float damageMultiplier = 1.5f;

    [SerializeField, Tooltip("추가 고정 피해")]
    private int flatBonusDamage = 0;

    [Header("VFX")]
    [SerializeField] private GameObject castVfxPrefab;       // 캐스팅 이펙트
    [SerializeField] private GameObject projectilePrefab;    // 투사체(연출용)
    [SerializeField] private Transform firePoint;// 총구 위치

    public override IEnumerator Execute(ChessStateBase caster)
    {
        Chess cait = caster as Chess;
        if (cait == null) yield break;

        // 타겟 없으면 스킬 취소(원하면 전방 발사로 바꿀 수 있음)
        Chess target = cait.CurrentTarget;
        if (target == null || target.IsDead) yield break;

        

        // 캐스팅 VFX
        if (castVfxPrefab != null)
        {
            var castVfx = PoolManager.Instance.Spawn("CaitlynCast");
            castVfx.transform.SetPositionAndRotation(caster.transform.position, Quaternion.identity);
        }

        if (windUpTime > 0f)
            yield return new WaitForSeconds(windUpTime);

        // 테스트용 케이틀린 효과음
        SettingsUI.PlaySFX("Caitlyn_QSkillSound",caster.transform.position,1f,1f);


        // 투사체(연출용)
        if (projectilePrefab != null && firePoint != null)
        {
            var projectileVfx = PoolManager.Instance.Spawn("CaitlynProjectile");
            projectileVfx.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        }

        // 실제 데미지
        int dmg = Mathf.Max(1, Mathf.RoundToInt(cait.AttackDamage * damageMultiplier) + flatBonusDamage);
        target.TakeDamage(dmg, cait);
    }
}
