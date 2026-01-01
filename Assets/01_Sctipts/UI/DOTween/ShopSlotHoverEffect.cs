using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// ShopSlot 전용 Hover / Press 연출
/// - 기준 색상은 CostUIData에서 설정된 backgroundColor
/// - Hover : 기준색 기준 밝기 증가
/// - Press : 기준색 기준 어둡게
/// - 슬롯 코스트 색상 구조에 완전히 대응
/// </summary>
public class ShopSlotHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Target")]
    [SerializeField] private Image targetImage;
    [SerializeField] private RectTransform targetTransform;

    [Header("Color Multiplier")]
    [Tooltip("Hover 시 밝기 배율 (ex: 1.1 = 10% 밝아짐)")]
    [SerializeField] private float hoverMultiplier = 1.12f;

    [Tooltip("Press 시 밝기 배율 (ex: 0.9 = 어두워짐)")]
    [SerializeField] private float pressMultiplier = 0.9f;

    [Header("Scale")]
    [SerializeField] private Vector3 hoverScale = Vector3.one * 1.02f;
    [SerializeField] private Vector3 pressScale = Vector3.one * 0.97f;

    [Header("Tween")]
    [SerializeField] private float duration = 0.08f;
    [SerializeField] private float pressDuration = 0.06f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private Color baseColor;
    private Vector3 baseScale;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetTransform == null)
            targetTransform = transform as RectTransform;

        baseScale = targetTransform.localScale;
    }

    /// <summary>
    /// ShopSlot.Init 이후 반드시 호출되어야 함
    /// (CostUIData에서 색상 세팅한 뒤)
    /// </summary>
    public void CaptureBaseColor()
    {
        if (targetImage == null) return;
        baseColor = targetImage.color;
    }

    // =========================
    // Hover
    // =========================
    public void OnPointerEnter(PointerEventData eventData)
    { 
        if (targetImage == null) return;

        targetImage.DOKill();
        targetTransform.DOKill();

        Color hoverColor = baseColor * hoverMultiplier;
        hoverColor.a = baseColor.a; // 알파 보존

        targetImage.DOColor(hoverColor, duration).SetEase(ease);
        targetTransform.DOScale(hoverScale, duration).SetEase(ease);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetImage.DOKill();
        targetTransform.DOKill();

        targetImage.DOColor(baseColor, duration).SetEase(ease);
        targetTransform.DOScale(baseScale, duration).SetEase(ease);
    }

    // =========================
    // Press
    // =========================
    public void OnPointerDown(PointerEventData eventData)
    {
        targetImage.DOKill();
        targetTransform.DOKill();

        Color pressColor = baseColor * pressMultiplier;
        pressColor.a = baseColor.a;

        targetImage.DOColor(pressColor, pressDuration).SetEase(ease);
        targetTransform.DOScale(pressScale, pressDuration).SetEase(ease);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetImage.DOKill();
        targetTransform.DOKill();

        // Hover 상태로 복귀
        Color hoverColor = baseColor * hoverMultiplier;
        hoverColor.a = baseColor.a;

        targetImage.DOColor(hoverColor, pressDuration).SetEase(ease);
        targetTransform.DOScale(hoverScale, pressDuration).SetEase(Ease.OutBack);
    }
}
