using System.Collections;
using UnityEngine;

public class ChessStarVFXPlayer : MonoBehaviour
{
    [SerializeField] private Chess owner;

    [Header("VFX Pool IDs")]
    [SerializeField] private string twoStarVFX;
    [SerializeField] private string threeStarVFX;

    [Header("SFX Keys")]
    [SerializeField] private string twoStarSFX;
    [SerializeField] private string threeStarSFX;

    [SerializeField] private Vector3 offset = Vector3.up * 1.5f;

    // ===== 핵심 버퍼 상태 =====
    private int pendingStarLevel = -1;
    private Coroutine pendingRoutine;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponent<Chess>();
    }

    private void OnEnable()
    {
        if (owner != null)
            owner.OnStarUp += OnStarUpBuffered;
    }

    private void OnDisable()
    {
        if (owner != null)
            owner.OnStarUp -= OnStarUpBuffered;
    }

    // StarUp 이벤트 수신
    private void OnStarUpBuffered(int starLevel)
    {
        // 항상 가장 높은 성급만 유지
        if (starLevel > pendingStarLevel)
            pendingStarLevel = starLevel;

        // 이미 대기 중이면 새로 시작하지 않음
        if (pendingRoutine == null)
            pendingRoutine = StartCoroutine(PlayBufferedStarEffect());
    }

    // 몇 프레임 대기 후 "최종 성급"만 실행
    private IEnumerator PlayBufferedStarEffect()
    {
        yield return null;
        yield return null;

        int finalStar = pendingStarLevel;

        pendingStarLevel = -1;
        pendingRoutine = null;

        // ===== VFX =====
        string vfxId = finalStar switch
        {
            2 => twoStarVFX,
            3 => threeStarVFX,
            _ => null
        };

        if (!string.IsNullOrEmpty(vfxId))
        {
            var vfx = PoolManager.Instance.Spawn(vfxId);
            if (vfx != null)
            {
                vfx.transform.SetPositionAndRotation(
                    transform.position + offset,
                    Quaternion.identity
                );
            }
        }

        // ===== SFX =====
        string sfxKey = finalStar switch
        {
            2 => twoStarSFX,
            3 => threeStarSFX,
            _ => null
        };

        if (!string.IsNullOrEmpty(sfxKey))
        {
            SettingsUI.PlaySFX(
                sfxKey,
                Vector3.zero,
                1f,
                1f
            );
        }
    }
}
