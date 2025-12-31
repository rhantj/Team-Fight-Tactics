using System.Collections;
using UnityEngine;

public class ChessStarVFXPlayer : MonoBehaviour
{
    [SerializeField] private Chess owner;

    [Header("VFX Pool IDs")]
    [SerializeField] private string twoStarVFX;
    [SerializeField] private string threeStarVFX;

    [SerializeField] private Vector3 offset = Vector3.up * 1.5f;

    // ===== 핵심 상태 =====
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

    // StarUp 이벤트를 즉시 재생하지 않고 버퍼링
    private void OnStarUpBuffered(int starLevel)
    {
        // 항상 가장 높은 성급만 기억
        if (starLevel > pendingStarLevel)
            pendingStarLevel = starLevel;

        // 이미 대기중이면 그대로 둠
        if (pendingRoutine == null)
            pendingRoutine = StartCoroutine(PlayBufferedVFX());
    }

    // 몇 프레임 대기 후 최종 성급만 재생
    private IEnumerator PlayBufferedVFX()
    {
        yield return null;
        yield return null;

        int finalStar = pendingStarLevel;

        pendingStarLevel = -1;
        pendingRoutine = null;

        string vfxId = finalStar switch
        {
            2 => twoStarVFX,
            3 => threeStarVFX,
            _ => null
        };

        if (string.IsNullOrEmpty(vfxId))
            yield break;

        var vfx = PoolManager.Instance.Spawn(vfxId);
        if (vfx == null)
            yield break;

        vfx.transform.SetPositionAndRotation(
            transform.position + offset,
            Quaternion.identity
        );
    }
}
