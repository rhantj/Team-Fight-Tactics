using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 선택된 기물(Chess)의 상세 정보를 화면에 표시하는 UI 매니저.
///
/// - 기물 기본 정보(아이콘, 이름, 코스트)
/// - 스탯 정보(방어력, 공격력, 공격속도)
/// - 스킬 아이콘 및 스킬 툴팁 연동
/// - 체력 / 마나 UI 이벤트 기반 갱신
/// - 시너지(특성) 아이콘 표시
///
/// 씬 내에서 단 하나만 존재하도록 Singleton 기반으로 설계됨
/// </summary>
public class ChessInfoUI : Singleton<ChessInfoUI>
{
    [Header("Root Panel")]
    [SerializeField] private GameObject panel;

    [Header("Basic Info")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image frameImage;

    [Header("Stats")]
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;

    [Header("Skill Icon")]
    [SerializeField] private Image skillIconImage;
    private SkillTooltipTrigger skillTooltipTrigger;

    [Header("Cost UI Data")]
    [SerializeField] private CostUIData costUIData;

    [Header("HP / Shield UI")]
    [SerializeField] private RectTransform hpFill;
    [SerializeField] private RectTransform shieldFill;
    [SerializeField] private float hpBarMaxWidth = 200f;
    [SerializeField] private TMP_Text hpText;

    [Header("Mana UI")]
    [SerializeField] private Image manaFillImage;
    [SerializeField] private TMP_Text manaText;

    [Header("Synergy UI")]
    [SerializeField] private Transform synergyContainer;
    [SerializeField] private GameObject synergyIconPrefab;
    [SerializeField] private TraitIconDatabase traitIconDB;

    [Header("Item Slots")]
    [SerializeField] private ChessInfoItemSlot[] itemSlots;

    private ChessStateBase currentChess;

    protected override void Awake()
    {
        base.Awake();
        skillTooltipTrigger = skillIconImage.GetComponent<SkillTooltipTrigger>();
        panel.SetActive(false);
    }

    //=====================================================
    //                EVENT BINDING
    //=====================================================
    private void OnEnable()
    {
        if (currentChess == null) return;

        currentChess.OnHPChanged += OnHPChanged;
        currentChess.OnBattleStart += RefreshAllUI;
        currentChess.OnStatChanged += RefreshAllUI;
    }

    private void OnDisable()
    {
        if (currentChess == null) return;

        currentChess.OnHPChanged -= OnHPChanged;
        currentChess.OnBattleStart -= RefreshAllUI;
        currentChess.OnStatChanged -= RefreshAllUI;
    }

    //=====================================================
    //                PUBLIC ENTRY
    //=====================================================
    public void ShowInfo(ChessStateBase chess)
    {
        if (chess == null)
        {
            Hide();
            return;
        }

        currentChess = chess;
        ChessStatData data = chess.BaseData;

        iconImage.sprite = data.icon;
        nameText.text = data.unitName;
        costText.text = data.cost.ToString();

        skillIconImage.sprite = data.skillIcon;
        skillTooltipTrigger?.SetData(data);

        if (costUIData != null)
        {
            var info = costUIData.GetInfo(data.cost);
            if (info != null && info.infoFrameSprite != null)
                frameImage.sprite = info.infoFrameSprite;
        }

        CreateSynergyUI(chess);
        SyncItemSlotsFromWorldUI(chess);

        panel.SetActive(true); // 여기서 OnEnable 호출됨
        RefreshAllUI();
    }

    public void Hide()
    {
        panel.SetActive(false); // 여기서 OnDisable 호출됨
        currentChess = null;
        ClearSynergyUI();
        SkillTooltipUI.Instance?.Hide();
    }

    //=====================================================
    //                UI REFRESH
    //=====================================================
    private void RefreshAllUI()
    {
        if (currentChess == null) return;

        UpdateStatUI();
        UpdateHPUI();
        UpdateManaUI();
    }

    private void UpdateStatUI()
    {
        armorText.text = currentChess.Armor.ToString();
        attackDamageText.text = currentChess.AttackDamage.ToString();
        attackSpeedText.text = currentChess.AttackSpeed.ToString("0.00");
    }

    private void UpdateHPUI()
    {
        int hp = currentChess.CurrentHP;
        int shield = currentChess.CurrentShield;
        int max = currentChess.MaxHP;

        hpText.text = $"{hp + shield} / {max}";

        float hpRatio = Mathf.Clamp01((float)hp / max);
        float hpWidth = hpBarMaxWidth * hpRatio;

        hpFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hpWidth);
        hpFill.anchoredPosition = Vector2.zero;

        if (shield <= 0)
        {
            shieldFill.gameObject.SetActive(false);
            return;
        }

        shieldFill.gameObject.SetActive(true);

        if (hp + shield <= max)
        {
            float shieldWidth = hpBarMaxWidth * ((float)shield / max);
            shieldFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, shieldWidth);
            shieldFill.anchoredPosition = new Vector2(hpWidth, 0f);
        }
        else
        {
            float total = hp + shield;
            float shieldWidth = hpBarMaxWidth * (shield / total);
            shieldFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, shieldWidth);
            shieldFill.anchoredPosition = new Vector2(hpBarMaxWidth - shieldWidth, 0f);
        }
    }

    private void UpdateManaUI()
    {
        int mana = currentChess.CurrentMana;
        int maxMana = currentChess.BaseData.mana;

        manaFillImage.fillAmount = (float)mana / maxMana;
        manaText.text = $"{mana} / {maxMana}";
    }

    private void OnHPChanged(int cur, int max)
    {
        UpdateHPUI();
    }

    //=====================================================
    //                SYNERGY
    //=====================================================
    private void ClearSynergyUI()
    {
        foreach (Transform child in synergyContainer)
            Destroy(child.gameObject);
    }

    private void CreateSynergyUI(ChessStateBase chess)
    {
        ClearSynergyUI();

        var traits = chess.BaseData.traits;
        if (traits == null) return;

        foreach (var trait in traits)
        {
            var icon = Instantiate(synergyIconPrefab, synergyContainer);

            var img = icon.transform.Find("TraitIcon")?.GetComponent<Image>();
            var txt = icon.transform.Find("TraitName")?.GetComponent<TMP_Text>();

            if (img != null)
                img.sprite = traitIconDB.GetIcon(trait);

            if (txt != null)
                txt.text = traitIconDB.GetDisplayName(trait);
        }
    }

    //=====================================================
    //                ITEMS
    //=====================================================
    private void SyncItemSlotsFromWorldUI(ChessStateBase chess)
    {
        foreach (var slot in itemSlots)
            slot.Clear();

        var handler = chess.GetComponent<ChessItemHandler>();
        if (handler == null) return;

        var items = handler.EquippedItems;
        for (int i = 0; i < items.Count && i < itemSlots.Length; i++)
            itemSlots[i].SetItem(items[i]);
    }

    public void RefreshItemUIOnly()
    {
        if (currentChess == null) return;
        SyncItemSlotsFromWorldUI(currentChess);
        RefreshAllUI();
    }

    public void NotifyChessSold(ChessStateBase soldChess)
    {
        if (currentChess == soldChess)
            Hide();
    }
}
