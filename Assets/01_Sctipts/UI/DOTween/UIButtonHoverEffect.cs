using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI 버튼 / 슬롯 Hover + Press 연출
/// - Hover : 밝아짐 + 살짝 확대
/// - Press : 어두워짐 + 눌리는 느낌
/// </summary>
public class UIButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Target")]
    [SerializeField] private Image targetImage;
    [SerializeField] private RectTransform targetTransform;

    [Header("Color")]
    [SerializeField] private Color normalColor = new Color(0.78f, 0.78f, 0.78f, 1f); 
    [SerializeField] private Color hoverColor = Color.white;                          
    [SerializeField] private Color pressedColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Scale")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = Vector3.one * 1.01f;
    [SerializeField] private Vector3 pressedScale = Vector3.one * 0.99f;

    [Header("Tween")]
    [SerializeField] private float duration = 0.05f;
    [SerializeField] private float pressDuration = 0.07f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private bool isDisabled = false;


    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetTransform == null)
            targetTransform = transform as RectTransform;

        // 초기값 고정
        normalColor = targetImage.color;
        normalScale = targetTransform.localScale;
    }

    // =========================
    // Hover
    // =========================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDisabled) return;

        targetImage?.DOKill();
        targetTransform?.DOKill();

        targetImage?.DOColor(hoverColor, duration)
            .SetEase(ease);

        targetTransform?.DOScale(hoverScale, duration)
            .SetEase(ease);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDisabled) return;

        targetImage?.DOKill();
        targetTransform?.DOKill();

        targetImage?.DOColor(normalColor, duration)
            .SetEase(ease);

        targetTransform?.DOScale(normalScale, duration)
            .SetEase(ease);
    }

    // =========================
    // Press
    // =========================
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDisabled) return;

        targetImage?.DOKill();
        targetTransform?.DOKill();

        // 눌림 색상
        targetImage?.DOColor(pressedColor, pressDuration)
            .SetEase(Ease.OutQuad);

        // 눌림 스케일
        targetTransform?.DOScale(pressedScale, pressDuration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDisabled) return;

        targetImage?.DOKill();
        targetTransform?.DOKill();

        // Hover 상태로 복귀
        targetImage?.DOColor(hoverColor, pressDuration)
            .SetEase(Ease.OutQuad);

        targetTransform?.DOScale(hoverScale, pressDuration)
            .SetEase(Ease.OutBack);
    }

    // 외부에서 상태를 제어할 메서드 추가
    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;

        if (disabled)
        {
            // 즉시 연출 종료 + 원래 상태로 복귀
            targetImage?.DOKill();
            targetTransform?.DOKill();

            targetImage.color = normalColor;
            targetTransform.localScale = normalScale;
        }
    }

}
