using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 시너지 아이콘 전용 Hover 연출
/// - Hover 시 색상만 살짝 밝아짐
/// - 크기 / 위치 변화 없음
/// - 기존 툴팁 로직과 완전히 독립
/// </summary>
public class SynergyIconHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Target")]
    [SerializeField] private Image targetImage;

    [Header("Color")]
    [Tooltip("Hover 시 밝기 배율 (1.1 ~ 1.15 권장)")]
    [SerializeField] private float hoverMultiplier = 1.12f;

    [Header("Tween")]
    [SerializeField] private float duration = 0.08f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private Color baseColor;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        // 기준 색상 캡처
        if (targetImage != null)
            baseColor = targetImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage == null) return;

        targetImage.DOKill();

        Color hoverColor = baseColor * hoverMultiplier;
        hoverColor.a = baseColor.a; // 알파 보존

        targetImage
            .DOColor(hoverColor, duration)
            .SetEase(ease);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage == null) return;

        targetImage.DOKill();

        targetImage
            .DOColor(baseColor, duration)
            .SetEase(ease);
    }
}
