using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChessInfoUI : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private GameObject panel;

    [Header("Basic Info")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;

    [Header("Stats")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;
    [SerializeField] private TMP_Text manaText;

    [Header("Skill")]
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescText;

    [Header("Skill Icon (SO에 추가되면 사용)")]
    [SerializeField] private Image skillIconImage;
    // TODO: ChessStatData에 skillIcon(Sprite) 추가되면 아래 라인 활성화
    // skillIconImage.sprite = data.skillIcon;

    [Header("Traits")]
    [SerializeField] private TMP_Text traitText;


    private void Awake()
    {
        panel.SetActive(false);
    }


    // ============================================================
    //            핵심: ChessStatData(SO)를 그대로 UI에 반영
    // ============================================================
    public void ShowInfo(ChessStatData data)
    {
        if (data == null)
        {
            Hide();
            return;
        }

        // -------- 기본 정보 --------
        iconImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        // -------- 스탯 --------
        hpText.text = data.maxHP.ToString();
        armorText.text = data.armor.ToString();
        attackDamageText.text = data.attackDamage.ToString();
        attackSpeedText.text = data.attackSpeed.ToString("0.00");
        manaText.text = data.mana.ToString();

        // -------- 스킬 --------
        skillNameText.text = data.skillName;
        skillDescText.text = data.skillDescription;

        // 스킬 아이콘 자리만 준비된 상태 (SO 수정 필요)
        // skillIconImage.sprite = data.skillIcon;

        // -------- 특성 --------
        traitText.text = GetTraitString(data.traits);

        panel.SetActive(true);
    }


    public void Hide()
    {
        panel.SetActive(false);
    }


    private string GetTraitString(TraitType[] traits)
    {
        if (traits == null || traits.Length == 0)
            return "";

        return string.Join(" / ", traits);
    }
}
