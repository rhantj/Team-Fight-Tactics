using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SynergyTooltipUI : Singleton<SynergyTooltipUI>
{
    [Header("Header")]
    [SerializeField] private Image synergyIcon;
    [SerializeField] private TMP_Text synergyNameText;

    [Header("Description")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text countText;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        gameObject.SetActive(false);
    }

    public void Show(
        Sprite icon,
        string name,
        TraitTooltipData data
    )
    {
        synergyIcon.sprite = icon;
        synergyNameText.text = name;

        descriptionText.text = data.description;
        countText.text = data.countDescription;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
