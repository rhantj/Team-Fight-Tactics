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

    [Header("Stat Icons (3 slots)")]
    [SerializeField] private Image[] statIcons;

    [Header("Stat Sprites")]
    [SerializeField] private Sprite armorSprite;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite maxHPSprite;

    [Header("Synergy Tooltip Icons (Fixed)")]
    [SerializeField] private Sprite demaciaIcon;
    [SerializeField] private Sprite rangedIcon;
    [SerializeField] private Sprite meleeIcon;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 시너지 툴팁 표시
    /// - 툴팁 아이콘은 TraitType 기준 고정 스프라이트 사용
    /// - 시너지 UI 프리팹 아이콘과 분리된 구조
    /// </summary>
    public void Show(string synergyName, TraitTooltipData data)
    {
        // 기본 텍스트
        synergyNameText.text = synergyName;
        descriptionText.text = data.description;
        countText.text = data.countDescription;

        // 툴팁 전용 시너지 아이콘 설정
        synergyIcon.sprite = GetTooltipIcon(data.trait);

        // 시너지 타입에 따른 대표 스탯 아이콘 설정
        ApplyStatIcons(data.trait);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // =========================
    //      내부 처리 메서드
    // =========================

    private Sprite GetTooltipIcon(TraitType trait)
    {
        switch (trait)
        {
            case TraitType.Demacia:
                return demaciaIcon;

            case TraitType.Ranged:
                return rangedIcon;

            case TraitType.Melee:
                return meleeIcon;

            default:
                return null;
        }
    }

    private void ApplyStatIcons(TraitType trait)
    {
        Sprite statSprite = null;

        switch (trait)
        {
            case TraitType.Demacia:
                statSprite = armorSprite;
                break;

            case TraitType.Ranged:
                statSprite = attackSprite;
                break;

            case TraitType.Melee:
                statSprite = maxHPSprite;
                break;
        }

        foreach (var img in statIcons)
        {
            if (img == null) continue;

            if (statSprite != null)
            {
                img.sprite = statSprite;
                img.gameObject.SetActive(true);
            }
            else
            {
                img.gameObject.SetActive(false);
            }
        }
    }

    // 기존 호출부 호환용 (icon 파라미터 무시)
    public void Show(Sprite icon, string synergyName, TraitTooltipData data)
    {
        Show(synergyName, data);
    }

}
