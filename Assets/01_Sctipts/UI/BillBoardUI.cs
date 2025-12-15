using UnityEngine;

// HP UI가 카메라를 바라보도록 하는 스크립트
public class BillboardToCamera : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // 카메라를 정면으로 바라보게 회전
        transform.forward = cam.transform.forward;
    }
}
