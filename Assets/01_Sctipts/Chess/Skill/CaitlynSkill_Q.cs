using UnityEngine;
using System.Collections;

public class CaitlynSkill_Q : SkillBase
{
    [Header("타이밍")]
    [SerializeField] private float windUpTime = 0.2f;

    [Header("VFX 관련")]
    [SerializeField] private GameObject castVfxPrefab;//캐스팅 이펙트
    [Tooltip("캐스팅 이펙트")]
    [SerializeField] private GameObject projectilePrefab;//투사체 관련
    [Tooltip("투사체")]
    [SerializeField] private Transform firePoint;//총구 위치
    [Tooltip("스킬 시전 위치")]

    public override IEnumerator Execute(ChessStateBase caster)
    {
        if (castVfxPrefab != null)
            Object.Instantiate(castVfxPrefab, caster.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(windUpTime);

        if (projectilePrefab != null && firePoint != null)
            Object.Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        else
            Debug.Log("발사체가 아직 없습니다.");
    }
}
