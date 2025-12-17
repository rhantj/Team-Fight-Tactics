using UnityEngine;

/// <summary>
/// 월드 공간에 배치된 UI 요소가 항상 카메라를 향하도록 만드는 컴포넌트.
/// 
/// - 유닛의 HP 바, 마나 바 같은 오버헤드 UI에 사용된다.
/// - 카메라의 회전 방향을 기준으로 UI의 방향을 맞춘다.
/// </summary>
public class BillboardToCamera : MonoBehaviour
{
    /// <summary>
    /// 기준이 되는 메인 카메라 참조.
    /// </summary>
    private Camera cam;

    private void Awake()
    {
        // 메인 카메라 캐싱
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // UI가 항상 카메라 정면을 바라보도록 회전 보정
        transform.forward = cam.transform.forward;
    }
}
