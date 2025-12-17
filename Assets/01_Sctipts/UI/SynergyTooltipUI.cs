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
    [SerializeField] private Image[] statIcons; // ← 3개 모두 연결

    [Header("Stat Sprites")]
    [SerializeField] private Sprite armorSprite;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite maxHPSprite;

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
        // 기본 정보
        synergyIcon.sprite = icon;
        synergyNameText.text = name;
        descriptionText.text = data.description;
        countText.text = data.countDescription;

        // 시너지 타입에 따른 대표 스탯 스프라이트 선택
        Sprite statSprite = null;

        switch (data.trait)
        {
            case TraitType.Demacia:
                statSprite = armorSprite;
                break;

            case TraitType.Ranged:   // 정찰대
                statSprite = attackSprite;
                break;

            case TraitType.Melee: // 난동꾼
                statSprite = maxHPSprite;
                break;
        }

        // 3개 StatIcon 모두 동일한 스프라이트로 교체
        if (statSprite != null)
        {
            foreach (var img in statIcons)
            {
                if (img == null) continue;
                img.sprite = statSprite;
                img.gameObject.SetActive(true);
            }
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
