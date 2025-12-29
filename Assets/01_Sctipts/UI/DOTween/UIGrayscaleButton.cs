using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIGrayscaleButton : MonoBehaviour
{
    [Header("Target Images")]
    [SerializeField] private Image[] grayscaleTargets;

    [Header("Text Targets")]
    [SerializeField] private TMP_Text[] grayscaleTexts;

    [Header("Materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material grayscaleMaterial;

    [Header("Text Colors")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color disabledTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private Button button;
    private UIButtonHoverEffect hoverEffect;

    private void Awake()
    {
        button = GetComponent<Button>();
        hoverEffect = GetComponent<UIButtonHoverEffect>();
    }

    public void SetInteractable(bool canInteract)
    {
        button.interactable = canInteract;
        SetGrayscale(!canInteract);

        if (hoverEffect != null)
            hoverEffect.SetDisabled(!canInteract);
    }

    private void SetGrayscale(bool value)
    {
        // 이미지 처리
        foreach (var img in grayscaleTargets)
        {
            if (img == null) continue;

            if (value)
            {
                if (grayscaleMaterial != null)
                    img.material = grayscaleMaterial;
            }
            else
            {
                if (defaultMaterial != null)
                    img.material = defaultMaterial;
            }
        }

        // 텍스트 처리
        foreach (var text in grayscaleTexts)
        {
            if (text == null) continue;

            text.color = value ? disabledTextColor : normalTextColor;
        }
    }

}
