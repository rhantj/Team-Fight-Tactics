using UnityEngine;
using DG.Tweening;

/// <summary>
/// 이자 크리스탈 단일 오브젝트 연출 담당
/// - 항상 회전은 유지
/// - Show 시 : 스케일 확대 + 알파 증가
/// - Hide 시 : 스케일 축소 후 알파 감소
/// - 머티리얼 Color.a 를 직접 제어하는 방식 (가장 안전)
/// </summary>
public class InterestCrystal : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float appearScale = 2f;      // 보일 때 스케일
    [SerializeField] private float hiddenScale = 0.1f;    // 숨김 상태 스케일
    [SerializeField] private float scaleDuration = 0.35f; // 스케일 연출 시간

    [Header("Rotation Settings")]
    [SerializeField] private float rotateDuration = 10f;  // 한 바퀴 회전 시간

    [Header("Renderer")]
    [SerializeField] private Renderer crystalRenderer;    // 크리스탈 메쉬 렌더러

    private Tween scaleTween;       // 스케일 트윈 관리
    private Material runtimeMat;    // 런타임 전용 머티리얼 인스턴스
    private float currentAlpha = 0f;

    private void Awake()
    {
        // 런타임 전용 머티리얼 생성
        runtimeMat = crystalRenderer.material;

        // 초기 상태는 작고 투명
        transform.localScale = Vector3.one * hiddenScale;
        SetAlpha(0f);

        // 회전은 한번만 시작하고 절대 끊지 않는다
        transform
            .DORotate(new Vector3(0f, 360f, 0f), rotateDuration, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetEase(Ease.Linear);
    }

    /// <summary>
    /// 크리스탈 표시 연출
    /// - 스케일 확대
    /// - 알파값 증가
    /// </summary>
    public void Show()
    {
        // 기존 스케일 트윈 제거
        scaleTween?.Kill();

        // 스케일 확대 연출
        scaleTween = transform
            .DOScale(appearScale, scaleDuration)
            .SetEase(Ease.OutBack);

        // 알파값을 현재 값에서 1로 증가
        DOTween.To(
            () => currentAlpha,
            value => SetAlpha(value),
            1f,
            scaleDuration * 0.8f
        );
    }

    /// <summary>
    /// 크리스탈 숨김 연출
    /// - 스케일을 먼저 축소
    /// - 스케일이 완전히 줄어든 뒤 알파 감소
    /// 회전은 계속 유지됨
    /// </summary>
    public void Hide()
    {
        // 기존 스케일 트윈 제거
        scaleTween?.Kill();

        // 순서를 보장하기 위해 Sequence 사용
        Sequence seq = DOTween.Sequence();

        // 1단계 : 스케일을 먼저 hiddenScale까지 축소
        seq.Append(
            transform
                .DOScale(hiddenScale, scaleDuration)
                .SetEase(Ease.InCubic)
        );

        // 2단계 : 스케일이 완전히 줄어든 후 알파 감소
        seq.Append(
            DOTween.To(
                () => currentAlpha,
                value => SetAlpha(value),
                0f,
                scaleDuration * 0.6f
            )
        );
    }

    /// <summary>
    /// 머티리얼 Color 알파값 직접 설정
    /// 셰이더 의존성이 가장 적은 방식
    /// </summary>
    private void SetAlpha(float alpha)
    {
        currentAlpha = alpha;

        Color color = runtimeMat.color;
        color.a = alpha;
        runtimeMat.color = color;
    }
}
