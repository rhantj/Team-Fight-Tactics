using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTooltipUI : Singleton<SkillTooltipUI>
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;

    protected override void Awake()
    {
        base.Awake();

        // 싱글톤 중복 파괴 후 살아남은 인스턴스만 실행
        if (Instance != this) return;

        gameObject.SetActive(false);
    }

    public void Show(Sprite icon, string skillName, string description)
    {
        iconImage.sprite = icon;
        nameText.text = skillName;
        descText.text = description;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
